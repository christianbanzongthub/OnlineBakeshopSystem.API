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

                // 1.1) Create per-user inbox records
                await _conn.ExecuteAsync(
                    "SP_ONLINEBAKESHOPDB_CREATE_USER_NOTIFICATION_RECIPIENTS",
                    new
                    {
                        notificationId,
                        targetUserIdsCsv = string.Join(",", userIds)
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

        public async Task<ServiceResponse<object>> GetMyNotificationsAsync(int userId, int page, int pageSize)
        {
            var service = new ServiceResponse<object>();

            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 20;

                var rows = (await _conn.QueryAsync<UserNotificationRow>(
                    "SP_ONLINEBAKESHOPDB_GET_USER_NOTIFICATIONS",
                    new { userId, page, pageSize },
                    commandType: CommandType.StoredProcedure)).ToList();

                service.Status = 200;
                service.Message = "Notifications retrieved successfully.";
                service.Data = rows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for userId={UserId}", userId);
                service.Status = 500;
                service.Message = ex.Message;
            }

            return service;
        }

        public async Task<ServiceResponse<object>> MarkAsReadAsync(int userId, long notificationId)
        {
            var service = new ServiceResponse<object>();

            try
            {
                var affected = await _conn.ExecuteScalarAsync<int>(
                    "SP_ONLINEBAKESHOPDB_MARK_NOTIFICATION_AS_READ",
                    new { userId, notificationId },
                    commandType: CommandType.StoredProcedure);

                if (affected <= 0)
                {
                    service.Status = 404;
                    service.Message = "Notification not found.";
                    return service;
                }

                service.Status = 200;
                service.Message = "Notification marked as read.";
                service.Data = new { NotificationId = notificationId, AffectedRows = affected };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error marking notification as read userId={UserId} notificationId={NotificationId}",
                    userId, notificationId);

                service.Status = 500;
                service.Message = ex.Message;
            }

            return service;
        }

        public async Task<ServiceResponse<object>> MarkAllAsReadAsync(int userId)
        {
            var service = new ServiceResponse<object>();

            try
            {
                var affected = await _conn.ExecuteScalarAsync<int>(
                    "SP_ONLINEBAKESHOPDB_MARK_ALL_NOTIFICATIONS_AS_READ",
                    new { userId },
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Message = "All notifications marked as read.";
                service.Data = new { AffectedRows = affected };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for userId={UserId}", userId);
                service.Status = 500;
                service.Message = ex.Message;
            }

            return service;
        }

        public async Task<ServiceResponse<object>> DeleteForUserAsync(int userId, long notificationId)
        {
            var service = new ServiceResponse<object>();

            try
            {
                var affected = await _conn.ExecuteScalarAsync<int>(
                    "SP_ONLINEBAKESHOPDB_DELETE_NOTIFICATION_FOR_USER",
                    new { userId, notificationId },
                    commandType: CommandType.StoredProcedure);

                if (affected <= 0)
                {
                    service.Status = 404;
                    service.Message = "Notification not found.";
                    return service;
                }

                service.Status = 200;
                service.Message = "Notification removed successfully.";
                service.Data = new { NotificationId = notificationId, AffectedRows = affected };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting notification for user userId={UserId} notificationId={NotificationId}",
                    userId, notificationId);

                service.Status = 500;
                service.Message = ex.Message;
            }

            return service;
        }

        public async Task<ServiceResponse<object>> SendFromUserAsync(
            int senderUserId,
            SendUserNotificationRequest request,
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

                var userIds = request.TargetUserIds.Where(x => x > 0).Distinct().ToList();
                if (userIds.Count == 0)
                {
                    service.Status = 400;
                    service.Message = "At least one target user is required.";
                    return service;
                }

                // If your SP supports senderUserId, add it here.
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

                await _conn.ExecuteAsync(
                    "SP_ONLINEBAKESHOPDB_CREATE_USER_NOTIFICATION_RECIPIENTS",
                    new
                    {
                        notificationId,
                        targetUserIdsCsv = string.Join(",", userIds)
                    },
                    commandType: CommandType.StoredProcedure);

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
                        SenderUserId = senderUserId,
                        TargetedDeviceCount = 0,
                        SuccessCount = 0,
                        FailureCount = 0,
                        DeactivatedDeviceCount = 0
                    };
                    return service;
                }

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
                    SenderUserId = senderUserId,
                    TargetedDeviceCount = devices.Count,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    DeactivatedDeviceCount = deactivatedCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending user notification. senderUserId={SenderUserId}", senderUserId);
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

        public async Task<ServiceResponse<object>> GetMyNotificationByIdAsync(int userId, long notificationId)
        {
            var service = new ServiceResponse<object>();

            try
            {
                var row = await _conn.QueryFirstOrDefaultAsync<UserNotificationRow>(
                    "SP_ONLINEBAKESHOPDB_GET_USER_NOTIFICATION_BY_ID",
                    new { userId, notificationId },
                    commandType: CommandType.StoredProcedure);

                if (row == null)
                {
                    service.Status = 404;
                    service.Message = "Notification not found.";
                    return service;
                }

                service.Status = 200;
                service.Message = "Notification retrieved successfully.";
                service.Data = row;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving notification detail userId={UserId} notificationId={NotificationId}",
                    userId, notificationId);

                service.Status = 500;
                service.Message = ex.Message;
            }

            return service;
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

        private sealed class UserNotificationRow
        {
            public long NotificationId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public string? Type { get; set; }
            public string? DataJson { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
            public DateTime? ReadAt { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime? DeletedAt { get; set; }
            public int? SenderUserId { get; set; }
        }
    }
}