using Dapper;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;
using System.Data;
using System.Data.SqlClient;

namespace OnlineBakeshop.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;
        private readonly AuthClass _authClass;

        public LoginClass(IConfiguration config)
        {
            _configuration = config;
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
            _authClass = new AuthClass(config);
        }

        public async Task<ServiceResponse<object>> GetLogin(string email, string password)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@email", email);

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_GETUSERLOGIN",
                    param,
                    commandType: CommandType.StoredProcedure
                );

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

                    // ✅ FIX: pass userId so it gets embedded inside the JWT
                    int userId = (int)result.userId;
                    string token = _authClass.GenerateToken(email, userId);
                    string refreshToken = _authClass.GenerateRefreshToken();

                    var rtParam = new DynamicParameters();
                    rtParam.Add("@userId", userId);
                    rtParam.Add("@email", email);
                    rtParam.Add("@refreshToken", refreshToken);
                    rtParam.Add("@expiryDate", DateTime.Now.AddDays(
                        Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpiryDays"])
                    ));
                    rtParam.Add("@statementType", "INSERT");

                    conn.Execute(
                        "SP_ONLINEBAKESHOPDB_REFRESHTOKENS",
                        rtParam,
                        commandType: CommandType.StoredProcedure
                    );

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
                    service.RefreshToken = refreshToken;
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
    }
}