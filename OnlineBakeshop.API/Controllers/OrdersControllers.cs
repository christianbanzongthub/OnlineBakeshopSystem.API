using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : Controller
{
    private readonly IOrderRepository _orderRepository;

    public OrdersController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpPost]
    [Route("CreateOrder")]
    public async Task<IActionResult> CreateOrder(OrderModel order)
    {
        order.UserId = 1;    
        order.ProductId = 1; 

        var response = await _orderRepository.CreateOrder(order);
        return Ok(response);
    }

    [HttpGet]
    [Route("GetOrder/{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var response = await _orderRepository.GetOrderById(id);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetAllOrders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var response = await _orderRepository.GetAllOrders();
        return Ok(response);
    }

    [HttpPut]
    [Route("UpdateOrder/{id}")]
    public async Task<IActionResult> UpdateOrder(int id, OrderModel order)
    {
        order.OrderId = id;
        await _orderRepository.UpdateOrder(order);
        return Ok(new { Message = "Order Status Updated" });
    }

    [HttpDelete]
    [Route("DeleteOrder/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await _orderRepository.DeleteOrder(id);
        return Ok(new { Message = "Order deleted successfully" });
    }
}