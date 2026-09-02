using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Models;
using RecipeMatcher.Web.Data;
namespace RecipeMatcher.Web.Controllers;

public class IngredientsController(AppDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var ingredients = await dbContext.Ingredients.OrderBy(ingredient => ingredient.Name).ToListAsync();
        return View(ingredients);
    }
    //Create GET
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    //Create POST
    [HttpPost]
    public async Task<IActionResult> Create(Ingredient ingredient)
    {
        if (!ModelState.IsValid) return View(ingredient);
        var ie = await isExist(ingredient);
        if (ie) return View(ingredient);
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    public async Task<bool> isExist(Ingredient ingredient, int? excludeId = null)
    {
        bool nameExists = await dbContext.Ingredients.AnyAsync(i => i.Name == ingredient.Name && (i.Id != excludeId));

        if (nameExists)
        {
            ModelState.AddModelError("Name", "Error: Already exist");
            return true;
        }
        else return false;
    }
    //Edit GET
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var existingIngredient = await dbContext.Ingredients.FindAsync(id);
        if (existingIngredient == null) return NotFound();


        return View(existingIngredient);
    }
    //Edit POST
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Ingredient ingredient)
    {
        if (ingredient.Id != id) return NotFound();
        if (!ModelState.IsValid) return View(ingredient);

        var existingIngredient = await dbContext.Ingredients.FindAsync(id);
        if (existingIngredient == null) return NotFound();

        var ie = await isExist(ingredient, id);
        if (ie) return View(ingredient);

        existingIngredient.Name = ingredient.Name;
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    //Delete GET
    [HttpGet]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteGet(int id)
    {
        var existingIngredient = await dbContext.Ingredients.FindAsync(id);
        if (existingIngredient == null) return NotFound();

        return View(existingIngredient);
    }
    //Delete POST
    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeletePostConfirmed(int id)
    {
        var existingIngredient = await dbContext.Ingredients.FindAsync(id);
        if (existingIngredient == null) return NotFound();
        else
        {
            dbContext.Ingredients.Remove(existingIngredient);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    //Details GET
    public async Task<IActionResult> Details(int id)
    {
        var ingredient = await dbContext.Ingredients.FindAsync(id);
        if (ingredient is null) return NotFound();

        return View(ingredient);
    }
}


  
