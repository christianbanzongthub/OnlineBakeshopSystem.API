using Dapper;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model.Request;
using OnlineBakeshop.API.Model.Response;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class DeviceClass : IDeviceRepository
    {
        private readonly SqlConnection _conn;

        public DeviceClass(IConfiguration config)
        {
            _conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        public async Task<ServiceResponse<object>> RegisterDeviceAsync(int userId, RegisterDeviceRequest request)
        {
            var service = new ServiceResponse<object>();

            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@fcmToken", request.FcmToken);
                param.Add("@platform", request.Platform);
                param.Add("@deviceIdentifier", request.DeviceIdentifier);
                param.Add("@deviceName", request.DeviceName);

                var deviceId = await _conn.ExecuteScalarAsync<int>(
                    "SP_ONLINEBAKESHOPDB_REGISTER_DEVICE",
                    param,
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Message = "Device registered successfully.";
                service.Data = new { DeviceId = deviceId };
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }

            return service;
        }
    }
}