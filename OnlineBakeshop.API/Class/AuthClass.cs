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

        // =============================================
        // REFRESH TOKEN — get new JWT using refresh token
        // =============================================
        public async Task<ServiceResponse<object>> RefreshToken(string refreshToken)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            try
            {
                // Step 1: Find the refresh token in DB
                var param = new DynamicParameters();
                param.Add("@refreshToken", refreshToken);
                param.Add("@statementType", "GETBYTOKEN");

                var result = conn.QueryFirstOrDefault(
                    "SP_ONLINEBAKESHOPDB_REFRESHTOKENS",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                // Step 2: Validate — must exist, not revoked, not expired
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

                // Step 3: Revoke old refresh token
                var revokeParam = new DynamicParameters();
                revokeParam.Add("@refreshToken", refreshToken);
                revokeParam.Add("@statementType", "REVOKE");

                conn.Execute(
                    "SP_ONLINEBAKESHOPDB_REFRESHTOKENS",
                    revokeParam,
                    commandType: CommandType.StoredProcedure
                );

                // Step 4: Generate new JWT and new Refresh Token
                string email = result.email;

                string newJwtToken = GenerateToken(email);
                string newRefreshToken = GenerateRefreshToken();

                // Step 5: Save new refresh token to DB
                var saveParam = new DynamicParameters();
                saveParam.Add("@userId", result.userId);
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

        // =============================================
        // REVOKE TOKEN — logout / invalidate refresh token
        // =============================================
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

        // =============================================
        // GENERATE JWT TOKEN (email only, no role)
        // =============================================
        public string GenerateToken(string email)
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
                new Claim(ClaimTypes.Email, email)
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

        // =============================================
        // GENERATE REFRESH TOKEN (random secure string)
        // =============================================
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}