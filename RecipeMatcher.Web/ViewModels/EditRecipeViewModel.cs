using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace RecipeMatcher.Web.ViewModels;

public class EditRecipeViewModel
{
    [Required]
    [Range (1, int.MaxValue)] 
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [Range(1, 480)]
    public int PreparationMinutes { get; set; }

    [Required]
    public IReadOnlyList<IngredientOptionViewModel> Ingredients { get; set; } = [];
}