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

        public async Task<RecipeDisplayWithTotalCountDTO> GetRecipesAsync(int pageNumber, string sortBy, string sortOrder, string searchQuery, List<string> ingredientFilters)
        {
            using var session = _driver.AsyncSession();
            try
            {
                const int PAGE_SIZE = 20;
                int skipNumber = (pageNumber - 1) * PAGE_SIZE;

                var whereClauses = new List<string>();
                if (ingredientFilters != null && ingredientFilters.Any()) {
                    whereClauses.Add("ingredient.name IN $ingredientFilters");
                }
                if (!string.IsNullOrEmpty(searchQuery)) {
                    whereClauses.Add($"toLower(recipe.name) CONTAINS toLower($searchQuery)");
                }
                var whereClause = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";
                
                var whereAllClause = ingredientFilters != null && ingredientFilters.Any() ? "WHERE ALL(ing IN $ingredientFilters WHERE ing IN ingredients)" : "";

                string orderByClause;
                if (sortBy == "name")
                {
                    orderByClause = $"ORDER BY lTrim(recipe.{sortBy}) {sortOrder}";
                }
                else if (sortBy == "skillLevel")
                {
                    orderByClause = $@"
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
                    orderByClause = $"ORDER BY {sortBy} {sortOrder}";
                }

                var query = $@"
                    MATCH (author: Author)-[:WROTE]->(recipe: Recipe)
                    MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    WITH recipe, author, COLLECT(DISTINCT ingredient.name) AS allIngredients
                    WHERE {(!string.IsNullOrEmpty(searchQuery) ? $"toLower(recipe.name) CONTAINS toLower($searchQuery) AND " : "")}
                    {(ingredientFilters != null && ingredientFilters.Any() ? "ALL(ing IN $ingredientFilters WHERE ing IN allIngredients)" : "true")}
                    WITH recipe, author, allIngredients
                    RETURN recipe, author, SIZE(allIngredients) AS ingredientCount
                    {orderByClause}
                    SKIP $skipNumber
                    LIMIT $PAGE_SIZE;
                ";
                
                var result = await session.RunAsync(query, new { ingredientFilters, searchQuery, skipNumber, PAGE_SIZE });
                var recipes = await result.ToListAsync(record =>
                {
                    var recipeNode = record["recipe"].As<INode>();
                    var authorNode = record["author"].As<INode>();

                    return new RecipeDisplayDTO
                    {
                        RecipeId = recipeNode.Properties["id"].As<string>(),
                        RecipeName = recipeNode.Properties["name"].As<string>(),
                        AuthorName = authorNode.Properties["name"].As<string>(),
                        IngredientCount = record["ingredientCount"].As<int>(), 
                        SkillLevel = recipeNode.Properties["skillLevel"].As<string>(),
                    };
                });

                var countQuery = $@"
                    MATCH (author: Author)-[:WROTE]->(recipe: Recipe)
                    MATCH (recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    {whereClause}
                    WITH recipe, author, COLLECT(DISTINCT ingredient.name) AS ingredients
                    {whereAllClause}
                    RETURN count(DISTINCT recipe) AS total;
                ";
                var countResult = await session.RunAsync(countQuery, new { ingredientFilters, searchQuery });
                var totalCount = (await countResult.SingleAsync())["total"].As<int>();

                return new RecipeDisplayWithTotalCountDTO
                {
                    Recipes = recipes,
                    TotalCount = totalCount
                };
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

        public async Task<IEnumerable<CommonIngredientDTO>> GetTop5MostCommonIngredients()
        {
            using var session = _driver.AsyncSession();
            try
            {
                var query = @"
                    MATCH (recipe: Recipe)-[:CONTAINS_INGREDIENT]->(ingredient: Ingredient)
                    RETURN ingredient, SIZE(COLLECT(recipe)) AS recipeCount
                    ORDER BY recipeCount DESC
                    LIMIT 5;
                ";

                var result = await session.RunAsync(query);
                return await result.ToListAsync(record =>
                {
                    var ingredientNode = record["ingredient"].As<INode>();

                    return new CommonIngredientDTO
                    {
                        Name = ingredientNode["name"].As<string>(),
                        RecipeCount = record["recipeCount"].As<int>()
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<ProlificAuthorDTO>> GetTop5MostProlificAuthors()
        {
            using var session = _driver.AsyncSession();
            try
            {
                var query = @"
                    MATCH (author: Author)-[:WROTE]->(recipe: Recipe)
                    RETURN author, SIZE(COLLECT(recipe)) AS recipeCount
                    ORDER BY recipeCount DESC
                    LIMIT 5;
                ";

                var result = await session.RunAsync(query);
                return await result.ToListAsync(record =>
                {
                    var authorNode = record["author"].As<INode>();

                    return new ProlificAuthorDTO
                    {
                        AuthorName = authorNode.Properties["name"].As<string>(),
                        RecipeCount = record["recipeCount"].As<int>()
                    };
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> GetTop5MostComplexRecipes()
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
    }
}
