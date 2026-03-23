using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        IValidationRepository validationRepository;

        public ProductsController(IProductRepository repository, IValidationRepository validation)
        {
            _repository = repository;
            validationRepository = validation;
        }

        [HttpGet]
        [Route("GetProducts")]
        public async Task<IActionResult> GetProducts()
        {
            var response = await _repository.GetProducts();

            if (response == null)
                return NotFound();

            return Ok(response);
        }

        [HttpPost]
        [Route("CreateProduct")]
        public async Task<IActionResult> CreateProduct(ProductModel product)
        {
            if (product == null)
                return BadRequest();

            var validation = await validationRepository.ValidateProduct(
                product.ProductName, product.Price
            );

            if (validation.Data == null || !validation.Data.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = validation.Data?.Message ?? "Validation failed"
                });

            var response = await _repository.CreateProduct(
                product.ProductName,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.IsAvailable
            );

            return Ok(response);
        }
    }
}