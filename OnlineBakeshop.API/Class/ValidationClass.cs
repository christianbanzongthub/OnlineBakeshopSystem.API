using Dapper;
using System.Data.SqlClient;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;
using System.Data;

namespace OnlineBakeshop.API.Class
{
    public class ValidationClass : IValidationRepository
    {
        private readonly SqlConnection conn;

        public ValidationClass(IConfiguration config)
        {
            conn = new SqlConnection(config["ConnectionString:BakeshopDB"]);
        }

        public async Task<ServiceResponse<ValidationModel>> ValidateOrder(int userId, int productId, int quantity)
        {
            var service = new ServiceResponse<ValidationModel>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@productId", productId);
                param.Add("@quantity", quantity);

                var result = await conn.QueryFirstOrDefaultAsync<ValidationModel>( "SP_ONLINEBAKESHOPDB_VALIDATEORDER", param, commandType: CommandType.StoredProcedure);

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

        public async Task<ServiceResponse<ValidationModel>> ValidateProduct(string productName, decimal price)
        {
            var service = new ServiceResponse<ValidationModel>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@productName", productName);
                param.Add("@price", price);

                var result = await conn.QueryFirstOrDefaultAsync<ValidationModel>( "SP_ONLINEBAKESHOPDB_VALIDATEPRODUCTS", param, commandType: CommandType.StoredProcedure);

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

        public async Task<ServiceResponse<ValidationModel>> ValidateUser(string fullName, string email, string password, string contactNo)
        {
            var service = new ServiceResponse<ValidationModel>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@fullName", fullName);
                param.Add("@email", email);
                param.Add("@password", password);
                param.Add("@contactNo", contactNo);

                var result = await conn.QueryFirstOrDefaultAsync<ValidationModel>("SP_ONLINEBAKESHOPDB_VALIDATEUSERS", param,  commandType: CommandType.StoredProcedure);

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
    }
}