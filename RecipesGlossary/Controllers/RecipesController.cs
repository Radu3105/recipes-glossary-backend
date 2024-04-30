using Microsoft.AspNetCore.Mvc;
using RecipesGlossary.Business.Services;
using RecipesGlossary.DataAccess.Abstractions;
using RecipesGlossary.DataAccess.Repositories;

namespace RecipesGlossary.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly RecipeService _recipeService;

        public RecipesController(RecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetRecipe(string id)
        {
            var recipes = await _recipeService.GetRecipeByIdAsync(id);
            return Ok(recipes);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecipes(int pageNumber, string sortBy = "name", string sortOrder = "ASC")
        {
            var recipes = await _recipeService.GetPaginatedRecipesAsync(pageNumber, sortBy, sortOrder);
            return Ok(recipes);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchRecipesByName(int pageNumber, string searchQuery)
        {
            var recipes = await _recipeService.SearchRecipesByNameAsync(pageNumber, searchQuery);
            return Ok(recipes);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterRecipesByIngredients(int pageNumber, [FromQuery] List<string> ingredients)
        {
            var recipes = await _recipeService.FilterRecipesByIngredientsAsync(pageNumber, ingredients);
            return Ok(recipes);
        }

        [HttpGet("{authorName}/{pageNumber}")]
        public async Task<IActionResult> GetRecipesByAuthor(string authorName, int pageNumber)
        {
            var recipes = await _recipeService.GetRecipesByAuthorAsync(authorName, pageNumber);
            return Ok(recipes);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetTotalRecipes()
        {
            var total = await _recipeService.GetTotalRecipesAsync();
            return Ok(total);
        }

        [HttpGet("count/{authorName}")]
        public async Task<IActionResult> GetTotalRecipesByAuthor(string authorName)
        {
            var total = await _recipeService.GetTotalRecipesByAuthorAsync(authorName);
            return Ok(total);
        }
        
        [HttpGet("top-5-most-common-ingredients/")]
        public async Task<IActionResult> GetTop5MostCommonIngredients()
        {
            var recipes = await _recipeService.GetTop5MostCommonIngredients();
            return Ok(recipes);
        }

        [HttpGet("top-5-most-prolific-authors/")]
        public async Task<IActionResult> GetTop5MostProlificAuthors()
        {
            var recipes = await _recipeService.GetTop5MostProlificAuthors();
            return Ok(recipes);
        }

        [HttpGet("top-5-most-complex-recipes/")]
        public async Task<IActionResult> Get5MostComplexRecipes()
        {
            var recipes = await _recipeService.GetTop5MostComplexRecipes();
            return Ok(recipes);
        }
    }

}
