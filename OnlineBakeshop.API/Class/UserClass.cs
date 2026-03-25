using Dapper;
using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class UserClass : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;

        public UserClass(IConfiguration config)
        {
            _configuration = config;
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        // =============================================
        // GET ALL USERS
        // =============================================
        public async Task<ServiceResponse<object>> GetAllUsers()
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALL");

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_USERS",
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
                    service.Message = "No Users Found";
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
        // GET USER BY ID
        // =============================================
        public async Task<ServiceResponse<object>> GetUserById(int userId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@statementType", "GETBYID");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_USERS",
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
                    service.Message = "User Not Found";
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
        // UPDATE USER
        // =============================================
        public async Task<ServiceResponse<object>> UpdateUser(UserModel user)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", user.UserId);
                param.Add("@fullName", user.FullName);
                param.Add("@email", user.Email);
                param.Add("@address", user.Address);
                param.Add("@contactNo", user.ContactNo);
                param.Add("@statementType", "UPDATE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_USERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "User Updated Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // DELETE USER
        // =============================================
        public async Task<ServiceResponse<object>> DeleteUser(int userId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@userId", userId);
                param.Add("@statementType", "DELETE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_USERS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "User Deleted Successfully";
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