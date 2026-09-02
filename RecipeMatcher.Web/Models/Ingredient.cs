using System.ComponentModel.DataAnnotations;

namespace RecipeMatcher.Web.Models;

public class Ingredient
{
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = "";
}

