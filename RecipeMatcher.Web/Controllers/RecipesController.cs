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
    //Create GET
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    //Create POST
    [HttpPost]
    public async Task<IActionResult> Create(Recipe recipe)
    {
        if (!ModelState.IsValid) return View(recipe);
        
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    //Edit GET
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var existingRecipe = await dbContext.Recipes.FindAsync(id);
        if (existingRecipe == null) return NotFound();

        return View(existingRecipe);
    }
    //Edit POST
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Recipe recipe)
    {
        if (recipe.Id != id) return NotFound();
        if (!ModelState.IsValid)return View(recipe);
       
        var existingRecipe = await dbContext.Recipes.FindAsync(id);
        if (existingRecipe==null) return NotFound();
     
        existingRecipe.Name = recipe.Name;
        existingRecipe.PreparationMinutes = recipe.PreparationMinutes;
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    //Delete GET
    [HttpGet]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteGet(int id)
    {
        var existingRecipe = await dbContext.Recipes.FindAsync(id);
        if (existingRecipe == null) return NotFound();

        //return View("Delete recipe:" + existingRecipe.Name + "?");
        return View(existingRecipe);
    }
    //Delete POST
    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeletePostConfirmed(int id)
    {
        var existingRecipe = await dbContext.Recipes.FindAsync(id);
        if (existingRecipe == null) return NotFound();
        else
        {
            dbContext.Recipes.Remove(existingRecipe);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    //Details GET
    public async Task<IActionResult> Details(int id)
    {
        var recipe = await dbContext.Recipes.FindAsync(id);
        if (recipe is null) return NotFound();

        return View(recipe);
    }
}
