namespace RecipeMatcher.Web.ViewModels;

public class ResultMatchViewModel
{
    public int RecipeId { get; set; }
    public string Name { get; set; } = "";
    public int PreparationMinutes { get; set; }
    public IReadOnlyList<string> IngredientNames { get; set; } = [];
    public IReadOnlyList<string> MissingIngredients { get; set; } = [];
    public int MissingCount { get; set; } 
}
