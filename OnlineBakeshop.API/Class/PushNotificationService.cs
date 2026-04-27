using Dapper;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model.Request;
using OnlineBakeshop.API.Model.Response;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;

namespace OnlineBakeshop.API.Class
{
    public class PushNotificationService : IPushNotificationService
    {
        private const int FcmMaxTokensPerMulticast = 500;
        private readonly SqlConnection _conn;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(IConfiguration config, ILogger<PushNotificationService> logger)
        {
            _conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
            _logger = logger;
        }

        public async Task<ServiceResponse<object>> SendToUsersAsync(
            SendPushNotificationRequest request,
            CancellationToken cancellationToken = default)
        {
            var service = new ServiceResponse<object>();

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    service.Status = 500;
                    service.Message = "Firebase is not initialized.";
                    return service;
                }

                var userIds = request.TargetUserIds.Distinct().ToList();
                if (userIds.Count == 0)
                {
                    service.Status = 400;
                    service.Message = "At least one target user is required.";
                    return service;
                }

                // 1) Create notification row
                var notificationId = await _conn.ExecuteScalarAsync<long>(
                    "SP_ONLINEBAKESHOPDB_CREATE_NOTIFICATION",
                    new
                    {
                        targetUserId = userIds.Count == 1 ? userIds[0] : (int?)null,
                        title = request.Title,
                        body = request.Body,
                        notificationType = request.Type,
                        dataJson = request.Data == null ? null : JsonSerializer.Serialize(request.Data),
                        isBroadcast = false
                    },
                    commandType: CommandType.StoredProcedure);

                // 2) Fetch active device tokens by users
                var devices = (await _conn.QueryAsync<DeviceTokenRow>(
                    "SP_ONLINEBAKESHOPDB_GET_ACTIVE_DEVICE_TOKENS",
                    new { targetUserIdsCsv = string.Join(",", userIds) },
                    commandType: CommandType.StoredProcedure)).ToList();

                if (devices.Count == 0)
                {
                    await _conn.ExecuteAsync(
                        "SP_ONLINEBAKESHOPDB_MARK_NOTIFICATION_SENT",
                        new { notificationId },
                        commandType: CommandType.StoredProcedure);

                    service.Status = 200;
                    service.Message = "No active devices found for target users.";
                    service.Data = new
                    {
                        NotificationId = notificationId,
                        TargetedDeviceCount = 0,
                        SuccessCount = 0,
                        FailureCount = 0,
                        DeactivatedDeviceCount = 0
                    };
                    return service;
                }

                // 3) Create pending delivery rows
                foreach (var d in devices)
                {
                    await _conn.ExecuteAsync(
                        "SP_ONLINEBAKESHOPDB_CREATE_NOTIFICATION_DELIVERY",
                        new
                        {
                            notificationId,
                            userDeviceId = d.DeviceId,
                            status = "Pending",
                            responseMessage = (string?)null
                        },
                        commandType: CommandType.StoredProcedure);
                }

                var successCount = 0;
                var failureCount = 0;
                var deactivatedCount = 0;

                var dataPayload = request.Data == null
                    ? new Dictionary<string, string>()
                    : new Dictionary<string, string>(request.Data);

                if (!string.IsNullOrWhiteSpace(request.Type))
                    dataPayload["type"] = request.Type;

                // 4) Send in chunks and update delivery status
                foreach (var chunk in devices.Chunk(FcmMaxTokensPerMulticast))
                {
                    var chunkList = chunk.ToList();

                    var message = new MulticastMessage
                    {
                        Tokens = chunkList.Select(x => x.FcmToken).ToList(),
                        Notification = new Notification
                        {
                            Title = request.Title,
                            Body = request.Body
                        },
                        Data = dataPayload.Count == 0 ? null : dataPayload
                    };

                    var batch = await FirebaseMessaging.DefaultInstance
                        .SendEachForMulticastAsync(message, cancellationToken);

                    for (var i = 0; i < batch.Responses.Count; i++)
                    {
                        var device = chunkList[i];
                        var sendResponse = batch.Responses[i];

                        if (sendResponse.IsSuccess)
                        {
                            successCount++;
                            await _conn.ExecuteAsync(
                                "SP_ONLINEBAKESHOPDB_CREATE_NOTIFICATION_DELIVERY",
                                new
                                {
                                    notificationId,
                                    userDeviceId = device.DeviceId,
                                    status = "Sent",
                                    responseMessage = Truncate(sendResponse.MessageId, 2048)
                                },
                                commandType: CommandType.StoredProcedure);
                        }
                        else
                        {
                            failureCount++;
                            var error = sendResponse.Exception?.Message;

                            await _conn.ExecuteAsync(
                                "SP_ONLINEBAKESHOPDB_CREATE_NOTIFICATION_DELIVERY",
                                new
                                {
                                    notificationId,
                                    userDeviceId = device.DeviceId,
                                    status = "Failed",
                                    responseMessage = Truncate(error, 2048)
                                },
                                commandType: CommandType.StoredProcedure);

                            if (sendResponse.Exception is FirebaseMessagingException fcmEx &&
                                ShouldDeactivateDevice(fcmEx))
                            {
                                await _conn.ExecuteAsync(
                                    "SP_ONLINEBAKESHOPDB_DEACTIVATE_DEVICE_TOKEN",
                                    new { deviceId = device.DeviceId, fcmToken = (string?)null },
                                    commandType: CommandType.StoredProcedure);

                                deactivatedCount++;
                            }
                        }
                    }
                }

                await _conn.ExecuteAsync(
                    "SP_ONLINEBAKESHOPDB_MARK_NOTIFICATION_SENT",
                    new { notificationId },
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Message = "Push notification send completed.";
                service.Data = new
                {
                    NotificationId = notificationId,
                    TargetedDeviceCount = devices.Count,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    DeactivatedDeviceCount = deactivatedCount
                };

                _logger.LogInformation(
                    "Push {NotificationId}: devices={Count}, success={Success}, failure={Failure}, deactivated={Deactivated}",
                    notificationId, devices.Count, successCount, failureCount, deactivatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending push notifications.");
                service.Status = 500;
                service.Message = ex.Message;
            }

            return service;
        }

        private static bool ShouldDeactivateDevice(FirebaseMessagingException ex)
        {
            return ex.MessagingErrorCode switch
            {
                MessagingErrorCode.Unregistered => true,
                MessagingErrorCode.SenderIdMismatch => true,
                MessagingErrorCode.InvalidArgument =>
                    ex.Message.Contains("registration-token-not-registered", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.Contains("invalid-registration-token", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        private static string? Truncate(string? value, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return value.Length <= maxLen ? value : value[..maxLen];
        }

        private sealed class DeviceTokenRow
        {
            public int DeviceId { get; set; }
            public int UserId { get; set; }
            public string FcmToken { get; set; } = string.Empty;
            public string Platform { get; set; } = string.Empty;
            public string? DeviceIdentifier { get; set; }
            public string? DeviceName { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}