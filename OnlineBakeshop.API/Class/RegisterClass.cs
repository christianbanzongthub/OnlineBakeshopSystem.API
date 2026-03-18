using Dapper;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineBakeshop.API.Class;

public class RegisterClass : IRegisterRepository
{
    private readonly IConfiguration _configuration;
    private readonly SqlConnection conn;

    public RegisterClass(IConfiguration config)
    {
        _configuration = config;
        conn = new SqlConnection(config["ConnectionStrings:OnlineBakeshopdb"]);
    }

    public async Task<ServiceResponse<object>> GetRegister(string fullname, string email, string password, string address, string contactNo)
    {
        var service = new ServiceResponse<object>();

        try
        {
            var param = new DynamicParameters();
            param.Add("@fullname", fullname);
            param.Add("@email", email);
            param.Add("@password", password);
            param.Add("@address", address);
            param.Add("@contactno", contactNo);

            var result = conn.Query("SP_ONLINEBAKESHOPDB_USERREGISTER", param, commandType: CommandType.StoredProcedure).ToList();

            if (result.Count > 0)
            {
                // GENERATE TOKEN AFTER REGISTER
                string token = GenerateToken(email);

                service.Status = 200;
                service.Data = result;
                service.Token = token;
            }
            else
            {
                service.Status = 400;
                service.Message = "No Record Found";
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

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}