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

        public CustomOrderClass(IConfiguration config)
        {
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        // =============================================
        // GET ALL CUSTOM ORDERS
        // =============================================
        public async Task<ServiceResponse<object>> GetAllCustomOrders()
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALL");

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (result.Count > 0)
                {
                    service.Status = 200;
                    service.Data = result;
                }
                else
                {
                    service.Status = 404;
                    service.Message = "No Custom Orders Found";
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // GET CUSTOM ORDER BY ID
        // =============================================
        public async Task<ServiceResponse<object>> GetCustomOrderById(int customOrderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "GETBYID");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                if (result != null)
                {
                    service.Status = 200;
                    service.Data = result;
                }
                else
                {
                    service.Status = 404;
                    service.Message = "Custom Order Not Found";
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // CREATE CUSTOM ORDER
        // =============================================
        public async Task<ServiceResponse<object>> CreateCustomOrder(CustomOrderModel order)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
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

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Custom Order Created Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // UPDATE STATUS
        // =============================================
        public async Task<ServiceResponse<object>> UpdateStatus(int customOrderId, string orderStatus)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@orderStatus", orderStatus);
                param.Add("@statementType", "UPDATESTATUS");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Status Updated Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // MARK AS PAID
        // =============================================
        public async Task<ServiceResponse<object>> MarkAsPaid(int customOrderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "MARKPAID");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Payment Confirmed Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        
        public async Task<ServiceResponse<object>> RejectCustomOrder(int customOrderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "REJECT");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Custom Order Rejected Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

       
        public async Task<ServiceResponse<object>> DeleteCustomOrder(int customOrderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@statementType", "DELETE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Custom Order Deleted Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> SetPrice(int customOrderId, decimal quotedPrice)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@customOrderId", customOrderId);
                param.Add("@quotedPrice", quotedPrice);
                param.Add("@statementType", "SETPRICE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CUSTOMORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Price Set Successfully";
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