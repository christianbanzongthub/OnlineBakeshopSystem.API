using Dapper;
using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class OrderClass : IOrderRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;

        public OrderClass(IConfiguration config)
        {
            _configuration = config;
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        public async Task<ServiceResponse<object>> CreateOrder(OrderModel order)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", order.UserId);
                param.Add("@productId", order.ProductId);
                param.Add("@quantity", order.Quantity);
                param.Add("@totalPrice", order.TotalPrice);
                param.Add("@deliveryDate", order.DeliveryDate);
                param.Add("@deliveryTime", order.DeliveryTime);
                param.Add("@deliveryAddress", order.DeliveryAddress);
                param.Add("@specialNotes", order.SpecialNotes);
                param.Add("@statementType", "CREATE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Order Created Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> GetAllOrders()
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALL");

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
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
                    service.Message = "No Orders Found";
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> GetOrderById(int orderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@orderId", orderId);
                param.Add("@statementType", "GETBYID");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
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
                    service.Message = "Order Not Found";
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> GetOrdersByUserId(int userId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@statementType", "GETBYUSER");

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                service.Status = 200;
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> UpdateOrder(OrderModel order)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@orderId", order.OrderId);
                param.Add("@OrderStatus", order.OrderStatus);
                param.Add("@statementType", "UPDATESTATUS");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Order Updated Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> RejectOrder(int orderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@orderId", orderId);
                param.Add("@statementType", "REJECT");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Order Rejected Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> DeleteOrder(int orderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@orderId", orderId);
                param.Add("@statementType", "DELETE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Order Deleted Successfully";
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