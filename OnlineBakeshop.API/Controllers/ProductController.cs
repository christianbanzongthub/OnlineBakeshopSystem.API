using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        IProductRepository productRepository;

        public ProductController(IProductRepository product)
        {

            productRepository = product;
        }

        [HttpGet]
        [Route("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var response = await productRepository.GetAllProducts();
            return Ok(response);
        }

        [HttpGet]
        [Route("GetProductById")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var response = await productRepository.GetProductById(productId);
            return Ok(response);
        }

        [HttpPost]
        [Route("CreateProduct")]
        public async Task<IActionResult> CreateProduct(ProductModel product)
        {
            var response = await productRepository.CreateProduct(product);
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(ProductModel product)
        {
            var response = await productRepository.UpdateProduct(product);
            return Ok(response);
        }

        [HttpDelete]
        [Route("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var response = await productRepository.DeleteProduct(productId);
            return Ok(response);
        }
    }
}