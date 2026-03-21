// Location: OnlineBakeshop.API/Class/CategoryClass.cs

using Dapper;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class CategoryClass : ICategoryRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;

        public CategoryClass(IConfiguration config)
        {
            _configuration = config;
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        public async Task<ServiceResponse<object>> GetCategories()
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_GETCATEGORIES",
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
                    service.Message = "No Categories Found";
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }


        public async Task<ServiceResponse<object>> GetCategoryById(int categoryId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@CategoryId", categoryId);

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_GETCATEGORYBYID",
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
                    service.Message = "Category Not Found";
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
        // CREATE CATEGORY
        // =============================================
        public async Task<ServiceResponse<object>> CreateCategory(CategoryModel category)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@CategoryName", category.CategoryName);
                param.Add("@Description", category.Description);

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_CREATECATEGORY",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Category Created Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // UPDATE CATEGORY
        // =============================================
        public async Task<ServiceResponse<object>> UpdateCategory(CategoryModel category)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@CategoryId", category.CategoryId);
                param.Add("@CategoryName", category.CategoryName);
                param.Add("@Description", category.Description);

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_UPDATECATEGORY",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Category Updated Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // DELETE CATEGORY
        // =============================================
        public async Task<ServiceResponse<object>> DeleteCategory(int categoryId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@CategoryId", categoryId);

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_DELETECATEGORY",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Category Deleted Successfully";
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