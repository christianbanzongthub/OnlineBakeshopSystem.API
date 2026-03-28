using Dapper;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;
using System.Data;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineBakeshop.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;

        public LoginClass(IConfiguration config)
        {
            _configuration = config;
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        public async Task<ServiceResponse<object>> GetLogin(string email, string password)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();

            try
            {
                var param = new DynamicParameters();
                param.Add("@email", email);

                var result = conn.QueryFirstOrDefault("SP_ONLINEBAKESHOPDB_GETUSERLOGIN", param, commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                   
                    string hashedPassword = result.password;
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);

                    if (!isPasswordValid)
                    {
                        service.Status = 400;
                        service.Message = "Invalid Email or Password";
                        return service;
                    }

                    string token = GenerateToken(email);
                    service.Status = 200;
                    service.Data = new
                    {
                        result.userId,
                        result.fullName,
                        result.email,
                        result.address,
                        result.contactNo,
                        result.dateCreated
                    };
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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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