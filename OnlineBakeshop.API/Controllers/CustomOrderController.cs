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
        IWebHostEnvironment _env;

        public CustomOrderController(ICustomOrderRepository customOrder, IWebHostEnvironment env)
        {
            customOrderRepository = customOrder;
            _env = env;
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

        [HttpGet]
        [Route("GetCustomOrdersByUserId")]
        public async Task<IActionResult> GetCustomOrdersByUserId(int userId)
        {
            var response = await customOrderRepository.GetCustomOrdersByUserId(userId);
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
        [Route("ApproveCustomOrder")]
        public async Task<IActionResult> ApproveCustomOrder(int customOrderId, decimal quotedPrice)
        {
            var response = await customOrderRepository.ApproveCustomOrder(customOrderId, quotedPrice);
            return Ok(response);
        }

        [HttpPut]
        [Route("PlaceCustomOrder")]
        public async Task<IActionResult> PlaceCustomOrder(
            int customOrderId, string paymentMethod,
            string deliveryDate, string deliveryTime,
            string deliveryAddress, string fulfillmentType,
            string? meetupPlace = null)
        {
            var response = await customOrderRepository.PlaceCustomOrder(
                customOrderId, paymentMethod,
                deliveryDate, deliveryTime,
                deliveryAddress, fulfillmentType, meetupPlace);
            return Ok(response);
        }

        [HttpPost]
        [Route("UploadReceipt")]
        public async Task<IActionResult> UploadReceipt(
            [FromForm] int customOrderId,
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

            string fileName = $"receipt_custom_{customOrderId}_{Guid.NewGuid()}{ext}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            string imageUrl = $"/images/receipts/{fileName}";
            var response = await customOrderRepository.UpdateReceipt(customOrderId, imageUrl);
            return Ok(new { Status = 200, Message = "Receipt uploaded.", ImageUrl = imageUrl });
        }

        [HttpPut]
        [Route("ConfirmPayment")]
        public async Task<IActionResult> ConfirmPayment(int customOrderId)
        {
            var response = await customOrderRepository.ConfirmPayment(customOrderId);
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

        // NEW
        [HttpPut]
        [Route("CancelCustomOrder")]
        public async Task<IActionResult> CancelCustomOrder(int customOrderId)
        {
            var response = await customOrderRepository.CancelCustomOrder(customOrderId);
            return Ok(response);
        }
    }
}