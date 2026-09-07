using System.ComponentModel.DataAnnotations;

namespace RecipeMatcher.Web.ViewModels;

public class EditRecipeViewModel
{
    [Required]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = "";

    [Range(1, 480)]
    public int PreparationMinutes { get; set; }

    public IReadOnlyList<IngredientOptionViewModel> Ingredients { get; set; } = [];
}