using RecipesGlossary.DataAccess.Abstractions;
using RecipesGlossary.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.Business.Services
{
    public class IngredientService
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientService(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredientsAsync()
        {
            return await _ingredientRepository.GetAllAsync();
        }
    }
}
