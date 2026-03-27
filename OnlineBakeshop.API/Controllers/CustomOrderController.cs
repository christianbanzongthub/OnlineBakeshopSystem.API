using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomOrderController : Controller
    {
        ICustomOrderRepository customOrderRepository;

        public CustomOrderController(ICustomOrderRepository customOrder)
        {
            customOrderRepository = customOrder;
        }

        [HttpGet]
        [Route("GetAllCustomOrders")]
        public async Task<IActionResult> GetAllCustomOrders()
        {
            var response = await customOrderRepository.GetAllCustomOrders();
            return Ok(response);
        }

        [HttpGet]
        [Route("GetCustomOrderById")]
        public async Task<IActionResult> GetCustomOrderById(int customOrderId)
        {
            var response = await customOrderRepository.GetCustomOrderById(customOrderId);
            return Ok(response);
        }

        [HttpPost]
        [Route("CreateCustomOrder")]
        public async Task<IActionResult> CreateCustomOrder(CustomOrderModel order)
        {
            var response = await customOrderRepository.CreateCustomOrder(order);
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int customOrderId, string orderStatus)
        {
            var response = await customOrderRepository.UpdateStatus(customOrderId, orderStatus);
            return Ok(response);
        }

        [HttpPut]
        [Route("MarkAsPaid")]
        public async Task<IActionResult> MarkAsPaid(int customOrderId)
        {
            var response = await customOrderRepository.MarkAsPaid(customOrderId);
            return Ok(response);
        }

        [HttpPut]
        [Route("SetPrice")]
        public async Task<IActionResult> SetPrice(int customOrderId, decimal quotedPrice)
        {
            var response = await customOrderRepository.SetPrice(customOrderId, quotedPrice);
            return Ok(response);
        }

        [HttpPut]
        [Route("RejectCustomOrder")]
        public async Task<IActionResult> RejectCustomOrder(int customOrderId)
        {
            var response = await customOrderRepository.RejectCustomOrder(customOrderId);
            return Ok(response);
        }

        [HttpDelete]
        [Route("DeleteCustomOrder")]
        public async Task<IActionResult> DeleteCustomOrder(int customOrderId)
        {
            var response = await customOrderRepository.DeleteCustomOrder(customOrderId);
            return Ok(response);
        }
    }
}