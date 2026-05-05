using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : Controller
    {
        ICartRepository cartRepository;

        public CartController(ICartRepository cart)
        {
            cartRepository = cart;
        }

        [HttpGet]
        [Route("GetCartByUser")]
        public async Task<IActionResult> GetCartByUser(int userId)
        {
            var response = await cartRepository.GetCartByUser(userId);
            return Ok(response);
        }

        [HttpPost]
        [Route("UpsertCartItem")]
        public async Task<IActionResult> UpsertCartItem(CartModel cart)
        {
            var response = await cartRepository.UpsertCartItem(cart);
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateCartItemQty")]
        public async Task<IActionResult> UpdateCartItemQty(int cartId, int userId, int quantity)
        {
            var response = await cartRepository.UpdateCartItemQty(cartId, userId, quantity);
            return Ok(response);
        }

        [HttpDelete]
        [Route("RemoveCartItem")]
        public async Task<IActionResult> RemoveCartItem(int cartId, int userId)
        {
            var response = await cartRepository.RemoveCartItem(cartId, userId);
            return Ok(response);
        }

        [HttpDelete]
        [Route("ClearCart")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var response = await cartRepository.ClearCart(userId);
            return Ok(response);
        }
    }
}