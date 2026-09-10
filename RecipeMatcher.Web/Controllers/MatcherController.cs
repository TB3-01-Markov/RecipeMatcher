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
        List<IngredientOptionViewModel> ingredientsSelect = ingredients.Select(i => new IngredientOptionViewModel 
                                                            { Id = i.Id, Name = i.Name, Selected = false }).ToList();
        return View(ingredientsSelect);
    }

    [HttpPost]
    public async Task<IActionResult> Index(int[]? ingredientIds)
    {
        var ingredients = await dbContext.Ingredients.OrderBy(ingredient => ingredient.Name).ToListAsync();
        if (ingredientIds == null) ingredientIds = [];
        var ingredientsSelect = ingredients.Select(
                                i => new IngredientOptionViewModel
                                { Id = i.Id, Name = i.Name, Selected = ingredientIds.Contains(i.Id) }
                                ).ToList(); // List<IngredientOptionViewModel>
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
        var listMatcherResultViewModel = new List<MatcherResultViewModel>();
        foreach (var recipe in matchingRecipes)
        {
            MatcherResultViewModel ditMatcherResultViewModel = new MatcherResultViewModel();
            ditMatcherResultViewModel.Id = recipe.Id;
            ditMatcherResultViewModel.Name = recipe.Name;
            ditMatcherResultViewModel.PreparationMinutes = recipe.PreparationMinutes;
            ditMatcherResultViewModel.IngredientNames = recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name).ToList();
            listMatcherResultViewModel.Add(ditMatcherResultViewModel);
        }
        return View(listMatcherResultViewModel);
    }
    [HttpPost]
    public async Task<IActionResult> NearMatchRecipeIngredients(int[]? ingredientIds)
    {
        if (ingredientIds == null) ingredientIds = [];
       
        List<Recipe> AllRecipesOredered = await dbContext.Recipes
                                          .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient)
                                          .OrderBy(recipe => recipe.RecipeIngredients.Count(ri => !ingredientIds.Contains(ri.IngredientId)))
                                          .ThenBy(recipe => recipe.Name)
                                          .ToListAsync();
        var listMatcherResultViewModel = new List<NearMatchResultViewModel>();
        foreach (var recipe in AllRecipesOredered)
        {
            listMatcherResultViewModel.Add(NearMatchResult(recipe, ingredientIds));
        }
        return View(listMatcherResultViewModel);
    }
    
    public bool MatchResult(Recipe recipe, int[] ingredientIds)
    {
        List<int> ingredientsIdsInThisRecipe = recipe.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
        var ingredientsWhatWeHadNot = new List<string>();
        bool canWeKookDitRecipe = true;
        foreach (var ingredient in ingredientsIdsInThisRecipe)
        {
            if (ingredientIds.Contains(ingredient) == false)
            {
                canWeKookDitRecipe = false;
                string ditIngredientWeHadNot = recipe.RecipeIngredients.Where(ri => ingredient == ri.Ingredient.Id).Select(ri => ri.Ingredient.Name).FirstOrDefault() ?? "";
                if (!string.IsNullOrWhiteSpace(ditIngredientWeHadNot)) ingredientsWhatWeHadNot.Add(ditIngredientWeHadNot);
            }
        }
        return canWeKookDitRecipe;
    }
    
    public NearMatchResultViewModel NearMatchResult(Recipe recipe, int[] ingredientIds)
    {
        List<int> ingredientsIdsInThisRecipe = recipe.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
        var ingredientsWhatWeHadNot = new List<string>();
        var ditNearMatcherResultViewModel = new NearMatchResultViewModel();
        ditNearMatcherResultViewModel.RecipeId = recipe.Id;
        ditNearMatcherResultViewModel.Name = recipe.Name;
        ditNearMatcherResultViewModel.PreparationMinutes = recipe.PreparationMinutes;

        foreach (var ingredient in ingredientsIdsInThisRecipe)
        {
            if (ingredientIds.Contains(ingredient) == false)
            {
                Ingredient recipeIngedient = recipe.RecipeIngredients.Where(ri => ingredient == ri.Ingredient.Id).Select(ri => ri.Ingredient).First();
                string recipeIngredientName = recipeIngedient.Name;
                if (!string.IsNullOrWhiteSpace(recipeIngredientName)) ingredientsWhatWeHadNot.Add(recipeIngredientName); 
            }
        }
        ditNearMatcherResultViewModel.MissingIngredients.AddRange(ingredientsWhatWeHadNot);
        ditNearMatcherResultViewModel.MissingCount = recipe.RecipeIngredients.Count(ri => !ingredientIds.Contains(ri.IngredientId));
        return ditNearMatcherResultViewModel;
    }
}