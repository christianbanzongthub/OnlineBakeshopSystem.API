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
        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
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

            var response = await _repository.CreateProduct(
                product.ProductName,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.IsAvailable
            );
                return Ok("New Product Released");
        }
    }
}
