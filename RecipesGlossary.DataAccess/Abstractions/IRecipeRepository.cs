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
        Task<RecipeDisplayWithTotalCountDTO> GetRecipesAsync(int pageNumber, string sortBy, string sortOrder, string searchQuery, List<string> ingredientFilters);
        Task<IEnumerable<AuthorDisplayDTO>> GetAllByAuthorAsync(string authorName, int pageNumber);
        Task<IEnumerable<CommonIngredientDTO>> GetTop5MostCommonIngredientsAsync();
        Task<IEnumerable<ProlificAuthorDTO>> GetTop5MostProlificAuthorsAsync();
        Task<IEnumerable<RecipeDisplayDTO>> GetTop5MostComplexRecipesAsync();
        Task<int> CountRecipesByAuthorAsync(string authorName);
    }
}
