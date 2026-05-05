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
        IValidationRepository validationRepository;
        IWebHostEnvironment _env;

        public OrdersController(IOrderRepository order, IValidationRepository validation, IWebHostEnvironment env)
        {
            orderRepository = order;
            validationRepository = validation;
            _env = env;
        }

        [HttpPost]
        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder(OrderModel order)
        {
            var validation = await validationRepository.ValidateOrder(
                order.UserId, order.ProductId, order.Quantity
            );
            if (validation.Data == null || !validation.Data.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = validation.Data?.Message ?? "Validation failed"
                });

            var response = await orderRepository.CreateOrder(order);
            return Ok(response);
        }

        [HttpPost]
        [Route("UploadReceipt")]
        public async Task<IActionResult> UploadReceipt(
            [FromForm] int orderId,
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Ok(new { Status = 400, Message = "No file uploaded." });

            string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
                return Ok(new { Status = 400, Message = "Only jpg, jpeg, png, webp allowed." });

            string folder = Path.Combine(_env.WebRootPath, "images", "receipts");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"receipt_{orderId}_{Guid.NewGuid()}{ext}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            string imageUrl = $"/images/receipts/{fileName}";
            var response = await orderRepository.UpdateReceipt(orderId, imageUrl);
            return Ok(new { Status = 200, Message = "Receipt uploaded.", ImageUrl = imageUrl });
        }

        [HttpPut]
        [Route("ConfirmPayment")]
        public async Task<IActionResult> ConfirmPayment(int orderId)
        {
            var response = await orderRepository.ConfirmPayment(orderId);
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

        [HttpGet]
        [Route("GetOrdersByUserId")]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            var response = await orderRepository.GetOrdersByUserId(userId);
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(OrderModel order)
        {
            var response = await orderRepository.UpdateOrder(order);
            return Ok(response);
        }

        [HttpPut]
        [Route("RejectOrder")]
        public async Task<IActionResult> RejectOrder(int orderId)
        {
            var response = await orderRepository.RejectOrder(orderId);
            return Ok(response);
        }

        [HttpDelete]
        [Route("DeleteOrder")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var response = await orderRepository.DeleteOrder(orderId);
            return Ok(response);
        }

        // NEW
        [HttpPut]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var response = await orderRepository.CancelOrder(orderId);
            return Ok(response);
        }
    }
}