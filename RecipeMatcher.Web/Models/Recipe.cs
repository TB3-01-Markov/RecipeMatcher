using System.ComponentModel.DataAnnotations;

namespace RecipeMatcher.Web.Models;

public class Recipe
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Range(1, 480)]
    public int PreparationMinutes { get; set; }
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}

