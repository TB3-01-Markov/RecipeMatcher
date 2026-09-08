using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
using RecipeMatcher.Web.ViewModels;

namespace RecipeMatcher.Web.Controllers;

public class MatcherController(AppDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var ingredients = await dbContext.Ingredients.OrderBy(ingredient => ingredient.Name).ToListAsync();
        List<IngredientOptionViewModel> ingredientsSelect = ingredients.Select(i => new IngredientOptionViewModel { Id = i.Id, Name = i.Name, Selected = false }).ToList();
        return View(ingredientsSelect);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int[]? ingredientIds)
    {
        var ingredients = await dbContext.Ingredients.OrderBy(ingredient => ingredient.Name).ToListAsync();
        // MME: either use `var` or collection initialization, don't repeat the type twice.
        List<IngredientOptionViewModel> ingredientsSelect = new List<IngredientOptionViewModel>();
        if (ingredientIds == null) ingredientIds = [];

        ingredientsSelect = ingredients.Select(i => new IngredientOptionViewModel { Id = i.Id, Name = i.Name, Selected = ingredientIds.Contains(i.Id) }).ToList();

        return View(ingredientsSelect);
    }

    [HttpPost]
    public async Task<IActionResult> MatchRecipeIngredients(int[]? ingredientIds)
    {
        if (ingredientIds == null) ingredientIds = [];
        var matchingRecipes = await dbContext.Recipes
                                .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient)
                                .Where(recipe => !recipe.RecipeIngredients.Any(ri => !ingredientIds.Contains(ri.IngredientId)))
                                .ToListAsync();

        // MME: Use explanatory variable names. lmrvm ???
        List<MatcherResultViewModel> lmrvm = new List<MatcherResultViewModel>();
        foreach (var recipe in matchingRecipes)
        {
            // MME: Use explanatory variable names.
            MatcherResultViewModel mrvm = new MatcherResultViewModel();
            mrvm.Id = recipe.Id;
            mrvm.Name = recipe.Name;
            mrvm.PreparationMinutes = recipe.PreparationMinutes;
            mrvm.IngredientNames = recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name).ToList();
            lmrvm.Add(mrvm);
        }
        return View(lmrvm);
    }
    public bool MatchResult(Recipe recipe, int[] ingredientIds)
    {
        // MME: remove dead code comments
        //List<string> IngredientsInThisRecipe = recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name).ToList();
        // MME: local vars start with lower case
        List<int> IngredientsIdsInThisRecipe = recipe.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
        // MME: either use `var` or collection initialization.
        // MME: local vars start with lower case
        List<string> IngredientsWhatWeHadNot = new List<string>();
        bool CanWeKookDitRecipe = true;
        // MME: remove dead code comments
        //string WhatWeHadNot = "";
        foreach (var ingredient in IngredientsIdsInThisRecipe)
        {
            if (ingredientIds.Contains(ingredient) == false)
            {
                CanWeKookDitRecipe = false;
                // MME: ingred ???
                string ingred = recipe.RecipeIngredients.Where(ri => ingredient == ri.Ingredient.Id).Select(ri => ri.Ingredient.Name).FirstOrDefault() ?? "";
                if (!string.IsNullOrWhiteSpace(ingred)) IngredientsWhatWeHadNot.Add(ingred);
            }
        }
        return CanWeKookDitRecipe;
    }
    public NearMatchResultViewModel NearMatchResult(Recipe recipe, int[] ingredientIds)
    {
        // MME: remove dead code comments
        //List<string> IngredientsInThisRecipe = recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name).ToList();
        List<int> IngredientsIdsInThisRecipe = recipe.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
        // MME: remove dead code comments
        //List<string> IngredientsWhatWeHadNot = new List<string>();
        // MME: either use `var` or collection initialization.
        List<Ingredient> IngredientsWhatWeHadNot = new List<Ingredient>();
        // MME: nmrvm ???
        NearMatchResultViewModel nmrvm = new NearMatchResultViewModel();
        nmrvm.RecipeId = recipe.Id;
        nmrvm.Name = recipe.Name;
        nmrvm.PreparationMinutes = recipe.PreparationMinutes;

        //string WhatWeHadNot = "";
        foreach (var ingredient in IngredientsIdsInThisRecipe)
        {
            if (ingredientIds.Contains(ingredient) == false)
            {
                // MME: ingred => recipeIngredient
                string ingred = recipe.RecipeIngredients.Where(ri => ingredient == ri.Ingredient.Id).Select(ri => ri.Ingredient.Name).FirstOrDefault() ?? "";
                if (!string.IsNullOrWhiteSpace(ingred)) nmrvm.MissingIngredients.Add(ingred);

            }
        }
        //nmrvm.MissingCount = nmrvm.MissingIngredients.Count();
        nmrvm.MissingCount = recipe.RecipeIngredients.Count(ri => !ingredientIds.Contains(ri.IngredientId));
        return nmrvm;
    }
}