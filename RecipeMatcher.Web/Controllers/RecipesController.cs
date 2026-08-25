using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Models;
using RecipeMatcher.Web.Data;
namespace RecipeMatcher.Web.Controllers;

public class RecipesController(AppDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        //var recipes =  new List<Recipe>{ new() { Id = 1, Name = "Pancakes", PreparationMinutes = 20 }, new() { Id = 2, Name = "Tomato Soup", PreparationMinutes = 30 }};
        var recipes = await dbContext.Recipes.OrderBy(recipe => recipe.Name).ToListAsync();

        return View(recipes);
    }
}
