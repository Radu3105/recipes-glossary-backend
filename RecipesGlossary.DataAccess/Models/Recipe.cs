using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Models
{
    public class Recipe : GenericModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CookingTime { get; set; }
        public int PreparationTime { get; set; }
        public string[] Ingredients { get; set; }
        public string[] Collections { get; set; }
        public string[] Keywords { get; set; }
        public string[] DietTypes { get; set; }
        public List<SimilarRecipe> SimilarRecipes { get; set; }
    }
}
