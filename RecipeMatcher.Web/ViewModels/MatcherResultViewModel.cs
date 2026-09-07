namespace RecipeMatcher.Web.ViewModels;

public class MatcherResultViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int PreparationMinutes { get; set; }
    public List<string> IngredientNames { get; set; } = [];
}