using Dapper;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model.Response;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBakeshop.API.Class
{
    public class AuthClass : IAuthRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;

        public AuthClass(IConfiguration config)
        {
            _configuration = config;
            conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
        }

        public async Task<ServiceResponse<object>> RefreshToken(string refreshToken)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@refreshToken", refreshToken);
                param.Add("@statementType", "GETBYTOKEN");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_REFRESHTOKENS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                {
                    service.Status = 401;
                    service.Message = "Invalid Refresh Token";
                    return service;
                }

                if (result.isRevoked == true)
                {
                    service.Status = 401;
                    service.Message = "Refresh Token has been revoked";
                    return service;
                }

                if (result.expiryDate < DateTime.Now)
                {
                    service.Status = 401;
                    service.Message = "Refresh Token has expired. Please login again.";
                    return service;
                }

                var revokeParam = new DynamicParameters();
                revokeParam.Add("@refreshToken", refreshToken);
                revokeParam.Add("@statementType", "REVOKE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_REFRESHTOKENS",
                    revokeParam,
                    commandType: CommandType.StoredProcedure
                );

                // FIX: pass userId when regenerating token on refresh
                string email = result.email;
                int userId = (int)result.userId;

                string newJwtToken = GenerateToken(email, userId);
                string newRefreshToken = GenerateRefreshToken();

                var saveParam = new DynamicParameters();
                saveParam.Add("@userId", userId);
                saveParam.Add("@email", email);
                saveParam.Add("@refreshToken", newRefreshToken);
                saveParam.Add("@expiryDate", DateTime.Now.AddDays(
                    Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpiryDays"])
                ));
                saveParam.Add("@statementType", "INSERT");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_REFRESHTOKENS",
                    saveParam,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Token Refreshed Successfully";
                service.Token = newJwtToken;
                service.RefreshToken = newRefreshToken;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        public async Task<ServiceResponse<object>> RevokeToken(string refreshToken)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@refreshToken", refreshToken);
                param.Add("@statementType", "REVOKE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_REFRESHTOKENS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                service.Status = 200;
                service.Message = "Token Revoked Successfully. User Logged Out.";
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = ex.Message;
            }
            return service;
        }

        //  FIX: Added userId parameter so it gets embedded in the JWT
        public string GenerateToken(string email, int userId)
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
                new Claim(ClaimTypes.Email, email),
                new Claim("userId", userId.ToString())   //  THIS IS THE KEY FIX
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

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            // ✅ URL-safe: replaces +, /, = so token survives HTTP transport
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}