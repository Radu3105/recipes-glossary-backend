using RecipesGlossary.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Models
{
    public class SimilarRecipe
    {
        public string RecipeId { get; set; }
        public string RecipeName { get; set; }
        // public string AuthorName { get; set; }
        public int SimilarityScore { get; set; }
    }
}
