using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
using RecipeMatcher.Web.ViewModels;

namespace RecipeMatcher.Web.Controllers;

public class MatcherController(AppDbContext dbContext) : Controller
{
    // MME: Only one view, and hence two controller actions (Index GET & Index POST) needed.
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
                                ).ToList();
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
            var ditMatcherResultViewModel = new MatcherResultViewModel()
            {
                Id = recipe.Id,
                Name = recipe.Name,
                PreparationMinutes = recipe.PreparationMinutes,
                IngredientNames = recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name).ToList()
            };
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

    public NearMatchResultViewModel NearMatchResult(Recipe recipe, int[] ingredientIds)
    {
        List<int> ingredientsIdsInThisRecipe = recipe.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
        var ingredientsWhatWeHadNot = new List<string>();
        foreach (var ingredient in ingredientsIdsInThisRecipe)
        {
            if (!ingredientIds.Contains(ingredient))
            {
                Ingredient recipeIngedient = recipe.RecipeIngredients.Where(ri => ingredient == ri.Ingredient.Id).Select(ri => ri.Ingredient).First();
                string recipeIngredientName = recipeIngedient.Name;
                if (!string.IsNullOrWhiteSpace(recipeIngredientName)) ingredientsWhatWeHadNot.Add(recipeIngredientName);
            }
        }
        var ditNearMatcherResultViewModel = new NearMatchResultViewModel()
        {
            RecipeId = recipe.Id,
            Name = recipe.Name,
            PreparationMinutes = recipe.PreparationMinutes,
            MissingIngredients = ingredientsWhatWeHadNot,
            MissingCount = recipe.RecipeIngredients.Count(ri => !ingredientIds.Contains(ri.IngredientId))
        };
        return ditNearMatcherResultViewModel;
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
}