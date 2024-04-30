using Neo4j.Driver;
using RecipesGlossary.DataAccess.Abstractions;
using RecipesGlossary.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Repositories
{
    public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
    {
        public IngredientRepository(IDriver driver) : base(driver)
        {
        }

        public override Task<Ingredient> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            using var session = _driver.AsyncSession();
            try
            {
                string query = @"
                    MATCH (ingredient: Ingredient)
                    RETURN ingredient
                    ORDER BY ingredient.name;
                ";

                var result = await session.RunAsync(query);
                return await result.ToListAsync(record =>
                {
                    var ingredientNode = record["ingredient"].As<INode>();

                    return new Ingredient
                    {
                        Name = ingredientNode.Properties["name"].As<string>(),
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }
    }
}
