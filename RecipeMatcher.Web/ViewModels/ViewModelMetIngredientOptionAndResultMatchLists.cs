namespace RecipeMatcher.Web.ViewModels
{
    public class ViewModelMetIngredientOptionAndResultMatchLists
    {
        public IReadOnlyList<IngredientOptionViewModel> IngredientOption { get; set; } = [];
        public IReadOnlyList<ResultMatchViewModel> ResultMatch { get; set; } = [];
        public bool HasSearched { get; set; } = false;
}
}

