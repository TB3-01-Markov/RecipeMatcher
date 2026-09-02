using RecipeMatcher.Web.ViewModels;

public class EditRecipeViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int PreparationMinutes { get; set; }

    public IReadOnlyList<IngredientOptionViewModel> Ingredients { get; set; } = [];
}