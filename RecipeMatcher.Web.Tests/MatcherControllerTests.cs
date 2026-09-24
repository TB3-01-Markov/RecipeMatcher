using System.Net;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests
{
    public class MatcherControllerTests(CustomWebApplicationFactory factory) : IntegrationTest(factory)
    {
        [Fact]
        public async Task Get_Matcher_Shows_Ingredients_And_No_Results()
        {
            SeedData();

            var response = await _Client.GetAsync("/matcher");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Egg", html);
            Assert.Contains("Milk", html);
            Assert.Contains("name=\"ingredientIds\"", html);
            Assert.DoesNotContain("Boiled egg", html);
        }

        [Fact]
        public async Task Post_Matcher_Shows_You_Have_Everything_For_Full_Match()
        {
            int eggId = SeedData();

            var html = await PostMatcher(eggId);

            Assert.Contains("Boiled egg", html);
            Assert.Contains("Alles ingredient was found.", html);
        }

        [Fact]
        public async Task Post_Matcher_Shows_Missing_Ingredient()
        {
            int eggId = SeedData();

            var html = await PostMatcher(eggId);

            Assert.Contains("Pancakes", html);
            Assert.Contains("Missing: Milk", html);
        }

        [Fact]
        public async Task Post_Matcher_Shows_Full_Match_Before_Near_Match()
        {
            int eggId = SeedData();

            var html = await PostMatcher(eggId);

            Assert.Contains("Boiled egg", html);
            Assert.Contains("Pancakes", html);
            var beforePancakes = html.Split("Pancakes")[0];
            Assert.Contains("Boiled egg", beforePancakes);
        }

        [Fact]
        public async Task Post_Matcher_Without_Selection_Does_Not_Crash()
        {
            SeedData();

            var response = await _Client.PostAsync("/matcher", new FormUrlEncodedContent(new Dictionary<string, string>()));
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Boiled egg", html);
            Assert.Contains("Pancakes", html);
        }

        private async Task<string> PostMatcher(int ingredientId)
        {
            var form = new Dictionary<string, string> { ["ingredientIds"] = ingredientId.ToString() };
            var response = await _Client.PostAsync("/matcher", new FormUrlEncodedContent(form));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.Content.ReadAsStringAsync();
        }

        private int SeedData()
        {
            using var db = CreateDbContext();
            var egg = new Ingredient { Name = "Egg" };
            var milk = new Ingredient { Name = "Milk" };

            db.Recipes.Add(new Recipe
            {
                Name = "Boiled egg",
                PreparationMinutes = 10,
                RecipeIngredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = egg }
                }
            });
            db.Recipes.Add(new Recipe
            {
                Name = "Pancakes",
                PreparationMinutes = 20,
                RecipeIngredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = egg },
                    new RecipeIngredient { Ingredient = milk }
                }
            });
            db.SaveChanges();

            return egg.Id;
        }
    }
}