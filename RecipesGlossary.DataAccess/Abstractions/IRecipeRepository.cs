using RecipesGlossary.Business.DTOs;
using RecipesGlossary.DataAccess.Models;
using RecipesGlossary.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Abstractions
{
    public interface IRecipeRepository : IGenericRepository<Recipe>
    {
        Task<IEnumerable<RecipeDisplayDTO>> GetPaginatedAsync(int pageNumber, string sortBy, string sortOrder);
        Task<SearchByNameDTO> SearchByNameAsync(int pageNumber, string searchQuery);
        Task<FilterByIngredientRecipeDTO> FilterByIngredientsAsync(int pageNumber, List<string> ingredients);
        Task<IEnumerable<AuthorDisplayDTO>> GetAllByAuthorAsync(string authorName, int pageNumber);
        Task<IEnumerable<CommonIngredientDTO>> GetTop5MostCommonIngredients();
        Task<IEnumerable<ProlificAuthorDTO>> GetTop5MostProlificAuthors();
        Task<IEnumerable<RecipeDisplayDTO>> GetTop5MostComplexRecipes();
        Task<int> CountRecipesAsync();
        Task<int> CountRecipesByAuthorAsync(string authorName);
    }
}
