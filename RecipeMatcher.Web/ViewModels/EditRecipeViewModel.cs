using System.ComponentModel.DataAnnotations;

namespace RecipeMatcher.Web.ViewModels;

public class EditRecipeViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [Range(1, 480)]
    public int PreparationMinutes { get; set; }

    public IReadOnlyList<IngredientOptionViewModel> Ingredients { get; set; } = [];
}