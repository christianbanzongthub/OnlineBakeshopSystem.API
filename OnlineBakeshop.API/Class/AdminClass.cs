using Dapper;
using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using System.Data;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineBakeshop.API.Class
{
    public class AdminClass : IAdminRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;

        public AdminClass(IConfiguration config)
        {
            _configuration = config;
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        // =============================================
        // ADMIN LOGIN
        // =============================================
        public async Task<ServiceResponse<object>> AdminLogin(AdminLoginModel admin)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@email", admin.Email);
                param.Add("@password", admin.Password);
                param.Add("@statementType", "LOGIN");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_ADMIN",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                if (result != null)
                {
                    string token = GenerateToken(admin.Email);
                    service.Status = 200;
                    service.Data = result;
                    service.Token = token;
                }
                else
                {
                    service.Status = 400;
                    service.Message = "Invalid Email or Password";
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
        // GET ALL ADMINS
        // =============================================
        public async Task<ServiceResponse<object>> GetAllAdmins()
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALL");

                var result = conn.Query(
                    "SP_ONLINEBAKESHOPDB_ADMIN",
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
                    service.Message = "No Admins Found";
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
        // GET ADMIN BY ID
        // =============================================
        public async Task<ServiceResponse<object>> GetAdminById(int adminId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@adminId", adminId);
                param.Add("@statementType", "GETBYID");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_ADMIN",
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
                    service.Message = "Admin Not Found";
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
        // CREATE ADMIN
        // =============================================
        public async Task<ServiceResponse<object>> CreateAdmin(AdminModel admin)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@fullName", admin.FullName);
                param.Add("@email", admin.Email);
                param.Add("@password", admin.Password);
                param.Add("@statementType", "CREATE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_ADMIN",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Admin Created Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // UPDATE ADMIN
        // =============================================
        public async Task<ServiceResponse<object>> UpdateAdmin(AdminModel admin)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@adminId", admin.AdminId);
                param.Add("@fullName", admin.FullName);
                param.Add("@email", admin.Email);
                param.Add("@password", admin.Password);
                param.Add("@statementType", "UPDATE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_ADMIN",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Admin Updated Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // DELETE ADMIN
        // =============================================
        public async Task<ServiceResponse<object>> DeleteAdmin(int adminId)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@adminId", adminId);
                param.Add("@statementType", "DELETE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_ADMIN",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Admin Deleted Successfully";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        // =============================================
        // TOKEN GENERATOR
        // =============================================
        private string GenerateToken(string email)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])
            );
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", "admin")
            };
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(jwtSettings["ExpiryMinutes"])
                ),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}