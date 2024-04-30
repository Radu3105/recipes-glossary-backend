using RecipesGlossary.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.Shared.DTOs
{
    public class FilterByIngredientRecipeDTO
    {
        public IEnumerable<RecipeDisplayDTO> Recipes { get; set; }
        public int TotalCount { get; set; }
    }
}
