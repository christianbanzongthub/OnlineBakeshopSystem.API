using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlineBakeshop.API.Model;

public class OrderClass : IOrderRepository
{
    private readonly IConfiguration _config;
    private readonly SqlConnection _conn;

    public OrderClass(IConfiguration config)
    {
        _config = config;
        _conn = new SqlConnection(_config["ConnectionStrings:OnlineBakeshopdb"]);
    }

    public async Task<int> CreateOrder(OrderModel order)
    {
        var param = new DynamicParameters();
        param.Add("@UserId", order.UserId);
        param.Add("@Quantity", order.Quantity);
        param.Add("@TotalPrice", order.TotalPrice);
        param.Add("@OrderStatus", order.OrderStatus ?? "Pending");
        param.Add("@statementType", "CREATE");

        return await _conn.ExecuteScalarAsync<int>( "SP_ONLINEBAKESHOPDB_ORDERS",param,commandType: CommandType.StoredProcedure );
    }

    public async Task<List<OrderModel>> GetAllOrders()
    {
        var param = new DynamicParameters();
        param.Add("@statementType", "GETALL");

        var result = await _conn.QueryAsync<OrderModel>("SP_ONLINEBAKESHOPDB_ORDERS", param, commandType: CommandType.StoredProcedure);
        return result.ToList();
    }


    public async Task<OrderModel> GetOrderById(int orderId)
    {
        var param = new DynamicParameters();
        param.Add("@OrderId", orderId);
        param.Add("@statementType", "GETBYID");

        return await _conn.QueryFirstOrDefaultAsync<OrderModel>("SP_ONLINEBAKESHOPDB_ORDERS", param, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateOrder(OrderModel order)
    {
        var param = new DynamicParameters();
        param.Add("@OrderId", order.OrderId);
        param.Add("@OrderStatus", order.OrderStatus);
        param.Add("@statementType", "UPDATESTATUS");

        await _conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_ORDERS", param, commandType: CommandType.StoredProcedure );
    }
    public async Task DeleteOrder(int orderId)
    {
        var param = new DynamicParameters();
        param.Add("@OrderId", orderId);
        param.Add("@statementType", "DELETE");

        await _conn.ExecuteAsync("SP_ONLINEBAKESHOPDB_ORDERS", param, commandType: CommandType.StoredProcedure);
    }
}