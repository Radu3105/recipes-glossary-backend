using Neo4j.Driver;
using RecipesGlossary.Business.DTOs;
using RecipesGlossary.DataAccess.Abstractions;
using RecipesGlossary.DataAccess.Models;
using RecipesGlossary.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Repositories
{
    public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(IDriver driver) : base(driver)
        {
        }

        public override async Task<Recipe> GetByIdAsync(string id)
        {
            using var session = _driver.AsyncSession();
            try
            {
                string query = @"
                    MATCH (recipe: Recipe {id: $id})
                    OPTIONAL MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    WITH recipe, COLLECT(ingredient.name) AS ingredients
                    OPTIONAL MATCH (recipe)-[:COLLECTION]->(collection: Collection)
                    WITH recipe, ingredients, COLLECT(collection.name) AS collections
                    OPTIONAL MATCH (recipe)-[:KEYWORD]->(keyword: Keyword)
                    WITH recipe, ingredients, collections, COLLECT(keyword.name) AS keywords
                    OPTIONAL MATCH (recipe)-[:DIET_TYPE]->(dietType: DietType)
                    RETURN recipe, ingredients, collections, keywords, COLLECT(dietType.name) AS dietTypes;
                ";

                var result = await session.RunAsync(query, new { id });
                return await result.SingleAsync(record =>
                {
                    var node = record["recipe"].As<INode>();
                    var ingredients = record["ingredients"].As<List<string>>();
                    var collections = record["collections"].As<List<string>>();
                    var keywords = record["keywords"].As<List<string>>();
                    var dietTypes = record["dietTypes"].As<List<string>>();

                    return new Recipe
                    {
                        Id = node.Properties["id"].As<string>(),
                        Name = node.Properties["name"].As<string>(),
                        Description = node.Properties["description"].As<string>(),
                        CookingTime = node.Properties["cookingTime"].As<int>(),
                        PreparationTime = node.Properties["preparationTime"].As<int>(),
                        Ingredients = ingredients.ToArray(),
                        Collections = collections.ToArray(),
                        Keywords = keywords.ToArray(),
                        DietTypes = dietTypes.ToArray()
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> GetPaginatedAsync(int pageNumber, string sortBy, string sortOrder)
        {
            using var session = _driver.AsyncSession();
            try
            {
                const int PAGE_SIZE = 20;
                int skipNumber = (pageNumber - 1) * PAGE_SIZE;
                string orderByQueryLine;
                if (sortBy == "name")
                {
                    orderByQueryLine = $"ORDER BY lTrim(recipe.{sortBy}) {sortOrder}";
                }
                else if (sortBy == "skillLevel")
                {
                    orderByQueryLine = $@"
                        ORDER BY 
                            CASE recipe.skillLevel 
                                WHEN 'Easy' THEN 1 
                                WHEN 'More effort' THEN 2 
                                WHEN 'A challenge' THEN 3 
                            END 
                        {sortOrder}
                    ";
                }
                else
                {
                    orderByQueryLine = $"ORDER BY {sortBy} {sortOrder}";
                }
                string query = $@"
                    MATCH (author: Author)-[:WROTE]->(recipe: Recipe)
                    OPTIONAL MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    WITH recipe, author, COLLECT(ingredient) AS ingredients
                    WITH recipe, author, SIZE(ingredients) AS ingredientCount
                    {orderByQueryLine}
                    RETURN recipe, author, ingredientCount
                    SKIP {skipNumber}
                    LIMIT {PAGE_SIZE};
                ";

                var result = await session.RunAsync(query);
                return await result.ToListAsync(record =>
                {
                    var recipeNode = record["recipe"].As<INode>();
                    var authorNode = record["author"].As<INode>();

                    return new RecipeDisplayDTO
                    {
                        RecipeId = recipeNode.Properties["id"].As<string>(),
                        RecipeName = recipeNode.Properties["name"].As<string>(),
                        AuthorName = authorNode.Properties["name"].As<string>(),
                        IngredientCount = record["ingredientCount"].As<int>(),
                        SkillLevel = recipeNode.Properties["skillLevel"].As<string>()
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> SearchByNameAsync(int pageNumber, string searchQuery)
        {
            using var session = _driver.AsyncSession();
            try
            {
                const int PAGE_SIZE = 20;
                int skipNumber = (pageNumber - 1) * PAGE_SIZE;

                string query = $@"
                    MATCH (author: Author)-[:WROTE]->(recipe: Recipe)
                    OPTIONAL MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    WITH recipe, author, COLLECT(ingredient) AS ingredients
                    WITH recipe, author, SIZE(ingredients) AS ingredientCount
                    WHERE recipe.name CONTAINS $searchQuery
                    RETURN recipe, author, ingredientCount
                    SKIP {skipNumber}
                    LIMIT {PAGE_SIZE};
                ";

                var result = await session.RunAsync(query, new { searchQuery });
                return await result.ToListAsync(record =>
                {
                    var recipeNode = record["recipe"].As<INode>();
                    var authorNode = record["author"].As<INode>();

                    return new RecipeDisplayDTO
                    {
                        RecipeId = recipeNode.Properties["id"].As<string>(),
                        RecipeName = recipeNode.Properties["name"].As<string>(),
                        AuthorName = authorNode.Properties["name"].As<string>(),
                        IngredientCount = record["ingredientCount"].As<int>(), 
                        SkillLevel = recipeNode.Properties["skillLevel"].As<string>() 
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> FilterByIngredientsAsync(int pageNumber, List<string> ingredientList)
        {
            using var session = _driver.AsyncSession();
            try
            {
                const int PAGE_SIZE = 20;
                int skipNumber = (pageNumber - 1) * PAGE_SIZE;

                string query = $@"
                    MATCH (author: Author)-[:WROTE]->(recipe: Recipe)
                    MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    WITH recipe, author, ingredient, COLLECT(ingredient) AS initialIngredients
                    WITH recipe, author, ingredient, SIZE(initialIngredients) AS ingredientCount
                    WHERE ingredient.name IN $ingredientList
                    WITH recipe, author, ingredientCount, COLLECT(ingredient.name) AS ingredients
                    WHERE ALL(ing IN $ingredientList WHERE ing IN ingredients)
                    RETURN recipe, author, ingredientCount
                    ORDER BY recipe.name
                    SKIP {skipNumber}
                    LIMIT {PAGE_SIZE};
                ";

                var result = await session.RunAsync(query, new { ingredientList });
                return await result.ToListAsync(record =>
                {
                    var recipeNode = record["recipe"].As<INode>();
                    var authorNode = record["author"].As<INode>();

                    return new RecipeDisplayDTO
                    {
                        RecipeId = recipeNode.Properties["id"].As<string>(),
                        RecipeName = recipeNode.Properties["name"].As<string>(),
                        AuthorName = authorNode.Properties["name"].As<string>(),
                        IngredientCount = record["ingredientCount"].As<int>(), 
                        SkillLevel = recipeNode.Properties["skillLevel"].As<string>() 
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<AuthorDisplayDTO>> GetAllByAuthorAsync(string authorName, int pageNumber)
        {
            using var session = _driver.AsyncSession();
            try
            {
                const int PAGE_SIZE = 20;
                int skipNumber = (pageNumber - 1) * PAGE_SIZE;

                string query = @"
                    MATCH (author: Author {name: $authorName})-[:WROTE]->(recipe: Recipe)
                    WITH author, recipe
                    ORDER BY recipe.name
                    RETURN author, recipe
                    SKIP $skipNumber
                    LIMIT $PAGE_SIZE;
                ";

                var result = await session.RunAsync(query, new { authorName, skipNumber, PAGE_SIZE });
                return await result.ToListAsync(record =>
                {
                    var recipeNode = record["recipe"].As<INode>();
                    var authorNode = record["author"].As<INode>();

                    return new AuthorDisplayDTO
                    {
                        RecipeId = recipeNode.Properties["id"].As<string>(),
                        RecipeName = recipeNode.Properties["name"].As<string>(),
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<int> CountRecipesAsync()
        {
            using var session = _driver.AsyncSession();
            try
            {
                var query = @"
                    MATCH (recipe: Recipe)
                    RETURN COUNT(recipe) AS total;
                ";

                var result = await session.RunAsync(query);
                var record = await result.SingleAsync();
                return record["total"].As<int>();
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<int> CountRecipesByAuthorAsync(string authorName)
        {
            using var session = _driver.AsyncSession();
            try
            {
                var query = @"
                    MATCH (author: Author {name: $authorName})-[:WROTE]->(recipe: Recipe)
                    RETURN COUNT(recipe) AS total;
                ";

                var result = await session.RunAsync(query, new { authorName });
                var record = await result.SingleAsync();
                return record["total"].As<int>();
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> Get5MostComplexRecipes()
        {
            using var session = _driver.AsyncSession();
            try
            {
                var query = @"
                    MATCH (author: Author)-[:WROTE]->(recipe: Recipe) 
                    OPTIONAL MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    WITH recipe, author, COLLECT(ingredient) AS ingredients
                    WITH recipe, author, SIZE(ingredients) AS ingredientCount
                    ORDER BY ingredientCount DESC
                    RETURN author, recipe, ingredientCount
                    LIMIT 5;
                ";

                var result = await session.RunAsync(query);
                return await result.ToListAsync(record =>
                {
                    var recipeNode = record["recipe"].As<INode>();
                    var authorNode = record["author"].As<INode>();

                    return new RecipeDisplayDTO
                    {
                        RecipeId = recipeNode.Properties["id"].As<string>(),
                        RecipeName = recipeNode.Properties["name"].As<string>(),
                        AuthorName = authorNode.Properties["name"].As<string>(),
                        IngredientCount = record["ingredientCount"].As<int>(),
                        SkillLevel = recipeNode.Properties["skillLevel"].As<string>()
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
