using RecipesGlossary.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Abstractions
{
    public interface IIngredientRepository : IGenericRepository<Ingredient>
    {
        Task<IEnumerable<Ingredient>> GetAllAsync();
    }
}
