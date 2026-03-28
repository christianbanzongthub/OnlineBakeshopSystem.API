using Dapper;
using OnlineBakeshop.API.Model.Response;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var param = new DynamicParameters();
            param.Add("@fullname", fullname);
            param.Add("@email", email);
            param.Add("@password", hashedPassword);
            param.Add("@address", address);
            param.Add("@contactno", contactNo);

            var result = conn.Query("SP_ONLINEBAKESHOPDB_USERREGISTER", param, commandType: CommandType.StoredProcedure).ToList();

            if (result.Count > 0)
            {
                service.Status = 200;
                service.Data = result;
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
}