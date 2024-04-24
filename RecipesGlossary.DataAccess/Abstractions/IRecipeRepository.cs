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
        Task<IEnumerable<RecipeDisplayDTO>> SearchByNameAsync(int pageNumber, string searchQuery);
        Task<IEnumerable<RecipeDisplayDTO>> FilterByIngredientsAsync(int pageNumber, List<string> ingredients);
        Task<IEnumerable<AuthorDisplayDTO>> GetAllByAuthorAsync(string authorName, int pageNumber);
        Task<int> CountRecipesAsync();
        Task<int> CountRecipesByAuthorAsync(string authorName);
        Task<IEnumerable<RecipeDisplayDTO>> Get5MostComplexRecipes();
    }
}
