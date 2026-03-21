using Dapper;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class ProductClass : IProductRepository
    {
        private readonly string _connectionString;

        public ProductClass(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("OnlineBakeshopdb");
        }

        public async Task<ServiceResponse<object>> GetProducts()
        {
            var service = new ServiceResponse<object>();
            var conn = new SqlConnection(_connectionString);

            try
            {
                conn.Open();

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_GETPRODUCTS",
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
            finally
            {
                conn.Close();
            }

            return service;
        }

        public async Task<ServiceResponse<object>> CreateProduct(
            string productName,
            string description,
            decimal price,
            string imageUrl,
            bool isAvailable)
        {
            var service = new ServiceResponse<object>();
            var conn = new SqlConnection(_connectionString);

            try
            {
                var param = new DynamicParameters();
                param.Add("@ProductName", productName);
                param.Add("@Description", description);
                param.Add("@Price", price);
                param.Add("@ImageUrl", imageUrl);
                param.Add("@IsAvailable", isAvailable);

                conn.Open();

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_CREATEPRODUCTS",
                    param,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                service.Status = 200;
                service.Data = result;
                service.Message = "Product created successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            finally
            {
                conn.Close();
            }

            return service;
        }
    }
}