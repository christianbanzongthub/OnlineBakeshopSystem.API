using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : Controller
    {
        IOrderRepository orderRepository;

        public OrdersController(IOrderRepository order)
        {
            orderRepository = order;
        }


        [HttpPost]
        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder(OrderModel order)
        {
            var response = await orderRepository.CreateOrder(order);
            return Ok(response);
        }

    
        [HttpGet]
        [Route("GetAllOrders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var response = await orderRepository.GetAllOrders();
            return Ok(response);
        }

  
        [HttpGet]
        [Route("GetOrderById")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var response = await orderRepository.GetOrderById(orderId);
            return Ok(response);
        }

 
        [HttpPut]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(OrderModel order)
        {
            var response = await orderRepository.UpdateOrder(order);
            return Ok(response);
        }

        [HttpDelete]
        [Route("DeleteOrder")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var response = await orderRepository.DeleteOrder(orderId);
            return Ok(response);
        }
    }
}