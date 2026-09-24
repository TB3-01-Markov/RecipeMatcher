using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
using RecipeMatcher.Web.ViewModels;

namespace RecipeMatcher.Web.Controllers;

[Route("matcher")]
public class MatcherController(AppDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(new ViewModelMetIngredientOptionAndResultMatchLists
        {
            IngredientOption = await GetIngredientOptionList([]),
            ResultMatch = [],
            HasSearched = false
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(int[]? ingredientIds)
    {
        if (ingredientIds == null) ingredientIds = [];
        var ingredientsSelect = await GetIngredientOptionList(ingredientIds);
        List<Recipe> allRecipesOrdered = await dbContext.Recipes
                                          .AsNoTracking()
                                          .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient)
                                          .OrderBy(recipe => recipe.RecipeIngredients.Count(ri => !ingredientIds.Contains(ri.IngredientId)))
                                          .ThenBy(recipe => recipe.Name)
                                          .ToListAsync();
        var listMatcherResultViewModel = new List<ResultMatchViewModel>();
        foreach (var recipe in allRecipesOrdered)
        {
            listMatcherResultViewModel.Add(EenResultMatchViewModel(recipe, ingredientIds));
        } 
        return View(new ViewModelMetIngredientOptionAndResultMatchLists {
                                    IngredientOption = ingredientsSelect, 
                                    ResultMatch = listMatcherResultViewModel,
                                    HasSearched = true});
    }
    private ResultMatchViewModel EenResultMatchViewModel(Recipe recipe, int[] ingredientIds)
    {
        List<string> missingIngredients = recipe.RecipeIngredients
                                      .Where(ri => !ingredientIds.Contains(ri.IngredientId))
                                      .Select(ri => ri.Ingredient.Name)
                                      .Where(name => !string.IsNullOrWhiteSpace(name))
                                      .OrderBy(name => name)
                                      .ToList();
        return new ResultMatchViewModel()
        {
            RecipeId = recipe.Id,
            Name = recipe.Name,
            PreparationMinutes = recipe.PreparationMinutes,
            IngredientNames    = recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name).OrderBy(name => name).ToList(),
            MissingIngredients = missingIngredients,
            MissingCount = recipe.RecipeIngredients.Count(ri => !ingredientIds.Contains(ri.IngredientId))
        };  
    }
    private async Task<List<IngredientOptionViewModel>> GetIngredientOptionList(int[] ingredientIds)
    {
        var ingredients = await dbContext.Ingredients.OrderBy(ingredient => ingredient.Name).ToListAsync();
        return ingredients.Select(
                                i => new IngredientOptionViewModel
                                { Id = i.Id, Name = i.Name, Selected = ingredientIds.Contains(i.Id) }
                                ).ToList();
    }
}
