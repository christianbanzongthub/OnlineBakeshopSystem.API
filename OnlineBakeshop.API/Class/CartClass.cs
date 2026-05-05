using Dapper;
using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class CartClass : ICartRepository
    {
        private readonly SqlConnection conn;

        public CartClass(IConfiguration config)
        {
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        public async Task<ServiceResponse<object>> GetCartByUser(int userId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@statementType", "GETBYUSER");

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_CART",
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

        public async Task<ServiceResponse<object>> UpsertCartItem(CartModel cart)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", cart.UserId);
                param.Add("@productId", cart.ProductId);
                param.Add("@quantity", cart.Quantity);
                param.Add("@deliveryDate", cart.DeliveryDate);
                param.Add("@deliveryTime", cart.DeliveryTime);
                param.Add("@deliveryAddress", cart.DeliveryAddress);
                param.Add("@specialNotes", cart.SpecialNotes);
                param.Add("@paymentMethod", cart.PaymentMethod);
                param.Add("@fulfillmentType", cart.FulfillmentType);
                param.Add("@meetupPlace", cart.MeetupPlace);
                param.Add("@statementType", "UPSERT");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_CART",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = result?.Message ?? "Cart Updated";
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> UpdateCartItemQty(int cartId, int userId, int quantity)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@cartId", cartId);
                param.Add("@userId", userId);
                param.Add("@quantity", quantity);
                param.Add("@statementType", "UPDATEQTY");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CART",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Quantity Updated";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> RemoveCartItem(int cartId, int userId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@cartId", cartId);
                param.Add("@userId", userId);
                param.Add("@statementType", "DELETE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CART",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Item Removed";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> ClearCart(int userId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@statementType", "CLEARALL");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CART",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Cart Cleared";
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