using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.Shared.DTOs
{
    public class SimilarRecipeDTO
    {
        public string RecipeId { get; set; }
        public string RecipeName { get; set; }
        public int Similarity { get; set; }
    }
}
