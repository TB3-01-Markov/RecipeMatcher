namespace RecipeMatcher.Web.ViewModels;

public class NearMatchResultViewModel
{
    public int RecipeId { get; set; }
    public string Name { get; set; } = "";
    public int PreparationMinutes { get; set; }
    public List<string> MissingIngredients { get; set; } = [];
    public int MissingCount { get; set; } 
}
