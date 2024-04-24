using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.Business.DTOs
{
    public class RecipeDisplayDTO
    {
        public string RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string AuthorName { get; set; }
        public int IngredientCount { get; set; }
        public string SkillLevel { get; set; }
    }
}
