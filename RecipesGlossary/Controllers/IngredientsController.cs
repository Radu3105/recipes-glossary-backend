using Microsoft.AspNetCore.Mvc;
using RecipesGlossary.Business.Services;

namespace RecipesGlossary.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IngredientsController : Controller
    {
        private readonly IngredientService _ingredientService;

        public IngredientsController(IngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _ingredientService.GetAllIngredientsAsync();
            return Ok(ingredients);
        }
    }
}
