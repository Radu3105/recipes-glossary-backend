﻿using RecipesGlossary.Business.DTOs;
using RecipesGlossary.DataAccess.Abstractions;
using RecipesGlossary.DataAccess.Models;
using RecipesGlossary.DataAccess.Repositories;
using RecipesGlossary.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.Business.Services
{
    public class RecipeService
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipeService(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        public async Task<Recipe> GetRecipeByIdAsync(string id)
        {
            return await _recipeRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> GetPaginatedRecipesAsync(int pageNumber, string sortBy, string sortOrder)
        {
            return await _recipeRepository.GetPaginatedAsync(pageNumber, sortBy, sortOrder);
        }
            
        public async Task<IEnumerable<RecipeDisplayDTO>> SearchRecipesByNameAsync(int pageNumber, string searchQuery)
        {
            return await _recipeRepository.SearchByNameAsync(pageNumber, searchQuery);
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> FilterRecipesByIngredientsAsync (int pageNumber, List<string> ingredients)
        {
            return await _recipeRepository.FilterByIngredientsAsync(pageNumber, ingredients);
        }

        public async Task<IEnumerable<AuthorDisplayDTO>> GetRecipesByAuthorAsync (string authorName, int pageNumber)
        {
            return await _recipeRepository.GetAllByAuthorAsync(authorName, pageNumber);
        }

        public async Task<int> GetTotalRecipesAsync()
        {
            return await _recipeRepository.CountRecipesAsync();
        }

        public async Task<int> GetTotalRecipesByAuthorAsync(string authorName)
        {
            return await _recipeRepository.CountRecipesByAuthorAsync(authorName);
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> Get5MostComplexRecipes()
        {
            return await _recipeRepository.Get5MostComplexRecipes();
        }
    }
}
