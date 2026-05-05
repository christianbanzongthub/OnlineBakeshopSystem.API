using Dapper;
using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class CustomOrderClass : ICustomOrderRepository
    {
        private readonly SqlConnection conn;
        private readonly IWebHostEnvironment _env;

        public CustomOrderClass(IConfiguration config, IWebHostEnvironment env)
        {
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
            _env = env;
        }

        public async Task<ServiceResponse<List<CustomOrderModel>>> GetAllCustomOrders()
        {
            ServiceResponse<List<CustomOrderModel>> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALL");
                var result = (await conn.QueryAsync<CustomOrderModel>(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure)).ToList();
                service.Status = 200;
                service.Data = result;
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<CustomOrderModel>> GetCustomOrderById(int customOrderId)
        {
            ServiceResponse<CustomOrderModel> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "GETBYID");
                var result = await conn.QueryFirstOrDefaultAsync<CustomOrderModel>(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                if (result != null) { service.Status = 200; service.Data = result; }
                else { service.Status = 404; service.Message = "Custom Order Not Found"; }
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<List<CustomOrderModel>>> GetCustomOrdersByUserId(int userId)
        {
            ServiceResponse<List<CustomOrderModel>> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@statementType", "GETBYUSER");
                var result = (await conn.QueryAsync<CustomOrderModel>(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure)).ToList();
                service.Status = 200;
                service.Data = result;
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> CreateCustomOrder(CustomOrderModel order)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", order.UserId);
                param.Add("@orderType", order.OrderType);
                param.Add("@flavor", order.Flavor);
                param.Add("@size", order.Size);
                param.Add("@colorTheme", order.ColorTheme);
                param.Add("@messageOnCake", order.MessageOnCake);
                param.Add("@numberOfLayers", order.NumberOfLayers);
                param.Add("@referenceImage", order.ReferenceImage);
                param.Add("@specialNotes", order.SpecialNotes);
                param.Add("@deliveryDate", order.DeliveryDate);
                param.Add("@deliveryTime", order.DeliveryTime);
                param.Add("@deliveryAddress", order.DeliveryAddress);
                param.Add("@statementType", "CREATE");

                var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);

                int newId = (int)(result?.customOrderId ?? 0);
                service.Status = 200;
                service.Message = "Custom Order Created Successfully";
                service.Data = new { customOrderId = newId };
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> ApproveCustomOrder(int customOrderId, decimal quotedPrice)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@quotedPrice", quotedPrice);
                param.Add("@statementType", "APPROVE");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Order Approved Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> PlaceCustomOrder(
            int customOrderId, string paymentMethod,
            string deliveryDate, string deliveryTime,
            string deliveryAddress, string fulfillmentType, string? meetupPlace)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@paymentMethod", paymentMethod);
                param.Add("@deliveryDate", deliveryDate);
                param.Add("@deliveryTime", deliveryTime);
                param.Add("@deliveryAddress", deliveryAddress);
                param.Add("@fulfillmentType", fulfillmentType);
                param.Add("@meetupPlace", meetupPlace);
                param.Add("@statementType", "PLACEORDER");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Order Placed Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> UpdateReceipt(int customOrderId, string receiptImagePath)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@receiptImage", receiptImagePath);
                param.Add("@statementType", "UPDATERECEIPT");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Receipt Updated Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> ConfirmPayment(int customOrderId)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "CONFIRMPAYMENT");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Payment Confirmed Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> UpdateStatus(int customOrderId, string orderStatus)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@orderStatus", orderStatus);
                param.Add("@statementType", "UPDATESTATUS");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Status Updated Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> MarkAsPaid(int customOrderId)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "MARKPAID");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Payment Confirmed Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> RejectCustomOrder(int customOrderId)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "REJECT");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Custom Order Rejected Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> DeleteCustomOrder(int customOrderId)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "DELETE");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Custom Order Deleted Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        public async Task<ServiceResponse<object>> SetPrice(int customOrderId, decimal quotedPrice)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@quotedPrice", quotedPrice);
                param.Add("@statementType", "SETPRICE");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Price Set Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }

        // NEW
        public async Task<ServiceResponse<object>> CancelCustomOrder(int customOrderId)
        {
            ServiceResponse<object> service = new();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "CANCEL");
                await conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_CUSTOMORDERS", param,
                    commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Custom Order Cancelled Successfully";
            }
            catch (Exception ex) { service.Status = 500; service.Message = ex.Message; }
            return service;
        }
    }
}