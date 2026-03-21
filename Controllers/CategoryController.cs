// Location: OnlineBakeshop.API/Controllers/CategoryController.cs

using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : Controller
    {
        ICategoryRepository categoryRepository;

        public CategoryController(ICategoryRepository category)
        {
            categoryRepository = category;
        }


        [HttpGet]
        [Route("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var response = await categoryRepository.GetCategories();
            return Ok(response);
        }

        [HttpGet]
        [Route("GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            var response = await categoryRepository.GetCategoryById(categoryId);
            return Ok(response);
        }

        [HttpPost]
        [Route("CreateCategory")]
        public async Task<IActionResult> CreateCategory(CategoryModel category)
        {
            var response = await categoryRepository.CreateCategory(category);
            return Ok(response);
        }

 
        [HttpPut]
        [Route("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory(CategoryModel category)
        {
            var response = await categoryRepository.UpdateCategory(category);
            return Ok(response);
        }


        [HttpDelete]
        [Route("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var response = await categoryRepository.DeleteCategory(categoryId);
            return Ok(response);
        }
    }
}