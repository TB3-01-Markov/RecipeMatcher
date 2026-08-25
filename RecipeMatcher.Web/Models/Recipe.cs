namespace RecipeMatcher.Web.Models;

public class Recipe
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int PreparationMinutes { get; set; }
}