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

                await conn.ExecuteAsync(
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

        public async Task<ServiceResponse<List<OrderModel>>> GetAllOrders()
        {
            ServiceResponse<List<OrderModel>> service = new ServiceResponse<List<OrderModel>>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALL");

                var result = (await conn.QueryAsync<OrderModel>(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

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

        public async Task<ServiceResponse<OrderModel>> GetOrderById(int orderId)
        {
            ServiceResponse<OrderModel> service = new ServiceResponse<OrderModel>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@orderId", orderId);
                param.Add("@statementType", "GETBYID");

                var result = await conn.QueryFirstOrDefaultAsync<OrderModel>(
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

        public async Task<ServiceResponse<List<OrderModel>>> GetOrdersByUserId(int userId)
        {
            ServiceResponse<List<OrderModel>> service = new ServiceResponse<List<OrderModel>>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@statementType", "GETBYUSER");

                var result = (await conn.QueryAsync<OrderModel>(
                    "SP_ONLINEBAKESHOPDB_ORDERS",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

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

                await conn.ExecuteAsync(
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

                await conn.ExecuteAsync(
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

                await conn.ExecuteAsync(
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