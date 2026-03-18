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
        private readonly IConfiguration _config;
        private readonly SqlConnection _conn;

        public OrderClass(IConfiguration config)
        {
            _config = config;
            _conn = new SqlConnection(_config["ConnectionStrings:OnlineBakeshopdb"]);
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
                param.Add("@OrderStatus", order.OrderStatus ?? "Pending");
                param.Add("@statementType", "CREATE");

                await _conn.ExecuteAsync(
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

                var result = await _conn.QueryAsync(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Data = result.ToList();
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

                var result = await _conn.QueryAsync(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Data = result.ToList();
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

                await _conn.ExecuteAsync(
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

 
        public async Task<ServiceResponse<object>> DeleteOrder(int orderId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@orderId", orderId);
                param.Add("@statementType", "DELETE");

                await _conn.ExecuteAsync(
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