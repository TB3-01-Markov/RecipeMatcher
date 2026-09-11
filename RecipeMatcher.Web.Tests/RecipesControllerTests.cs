using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests;

public class RecipesControllerTests(CustomWebApplicationFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Post_Create_With_Empty_Name_Does_Not_Save_And_Shows_Validation_Error()
    {
        var formData = new Dictionary<string, string>
        {
            ["Name"] = "",
            ["PreparationMinutes"] = "15"
        };
        var response = await _Client.PostAsync("/recipes/create", new FormUrlEncodedContent(formData));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("field-validation-error", content);
        using (var db = CreateDbContext())
        {
            Assert.DoesNotContain(db.Recipes, r => r.Name == "");
        }
    }

    [Fact]
    public async Task Post_Edit_Updates_Recipe_Name_With_New_Name_And_PreparationMinutes()
    {
        int id;
        int oldPreparationMinutes = 45;
        int newPreparationMinutes = 40;
        string oldRecipeName = "Old Name";
        string newRecipeName = "New Name";
        using (var db = CreateDbContext())
        {
            var recipe = new Recipe { Name = oldRecipeName, PreparationMinutes = oldPreparationMinutes};
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            id = recipe.Id;
            Assert.Contains(db.Recipes, n => n.Name == oldRecipeName);
            Assert.Contains(db.Recipes, p => p.PreparationMinutes == oldPreparationMinutes);
        }

        var formData = new Dictionary<string, string>
        {
            ["Name"] = newRecipeName,
            ["PreparationMinutes"] = newPreparationMinutes.ToString(),
            ["Id"] = id.ToString()
        };
   
        var response= await _Client.PostAsync($"/recipes/edit/{id}", new FormUrlEncodedContent(formData));
        response.EnsureSuccessStatusCode();
        using (var db = CreateDbContext())
        {
            Assert.Contains(db.Recipes, i => i.Name == newRecipeName);
            Assert.Contains(db.Recipes, i => i.PreparationMinutes == newPreparationMinutes);
        }
    }
    [Fact]
    public async Task Post_Edit_Updates_Recipe_Name_With_Dezelfde_Name()
    {
        int id1, id2;
        int oldPreparationMinutes1 = 45;
        int oldPreparationMinutes2 = 50;
        string oldRecipeName1 = "Recipe Oliviersalade";
        string oldRecipeName2 = "Recipe Appeltaart";
        using (var db = CreateDbContext())
        {
            var recipe = new Recipe { Name = oldRecipeName1 };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();

            id1 = recipe.Id;
            Assert.Contains(db.Recipes, i => i.Name == oldRecipeName1);
        }
        using (var db = CreateDbContext())
        {
            var recipe = new Recipe { Name = oldRecipeName2 };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();

            id2 = recipe.Id;
            Assert.Contains(db.Recipes, i => i.Name == oldRecipeName2);
        }
        var formData1 = new Dictionary<string, string>
        {
            ["Name"] = oldRecipeName1,
            ["PreparationMinutes"] = oldPreparationMinutes1.ToString(),
            ["Id"] = id1.ToString()
        };
        var formData2 = new Dictionary<string, string>
        {
            ["Name"] = oldRecipeName2,
            ["PreparationMinutes"] = oldPreparationMinutes2.ToString(),
            ["Id"] = id2.ToString()
        };
        var response1 = await _Client.PostAsync($"/recipes/edit/{id1}", new FormUrlEncodedContent(formData1));
        response1.EnsureSuccessStatusCode();
        using (var db = CreateDbContext())
        {
            Assert.Contains(db.Recipes, i => i.Name == oldRecipeName1);
            Assert.Contains(db.Recipes, i => i.PreparationMinutes == oldPreparationMinutes1);
        }
        var response2 = await _Client.PostAsync($"/recipes/edit/{id1}", new FormUrlEncodedContent(formData2));
        response2.EnsureSuccessStatusCode();
        using (var db = CreateDbContext())
        {
            Assert.Contains(db.Recipes, i => i.Name == oldRecipeName1);
            Assert.Contains(db.Recipes, i => i.PreparationMinutes == oldPreparationMinutes1);
            Assert.Contains(db.Recipes, i => i.Name == oldRecipeName2);
            Assert.Contains(db.Recipes, i => i.PreparationMinutes == oldPreparationMinutes2);
        }
    }
    
    [Fact]
    public async Task Get_Recipes_Returns_Ok_And_Shows_Recipe()
    {
        using (var db = CreateDbContext())
        {
            db.Recipes.Add(new Recipe
            {
                Name = "Test Pancakes",
                PreparationMinutes = 15
            });
            await db.SaveChangesAsync();
        }
        var response = await _Client.GetAsync("/recipes");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Pancakes", content);
    }

    [Fact]
    public async Task Get_Details_With_Existing_Id_Returns_Ok()
    {
        int id;
        using (var db = CreateDbContext())
        {
            var recipe = new Recipe
            {
                Name = "Details Test Recipe",
                PreparationMinutes = 10
            };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            id = recipe.Id;
        }
        var response = await _Client.GetAsync($"/recipes/details/{id}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Details Test Recipe", content);
    }

    [Fact]
    public async Task Get_Details_With_Unknown_Id_Returns_NotFound()
    {
        var response = await _Client.GetAsync("/recipes/details/999999");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Delete_byId_Unknown_Id_Returns_NotFound()
    {
        var response = await _Client.GetAsync("/recipes/delete/999999");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task Get_Delete_byId_With_Existing_Id_Returns_Ok()
    {
        using (var db = CreateDbContext())
        {
            var newRecipe = new Recipe
            {
                Name = "Get Delete Test Recipe",
                PreparationMinutes = 15
            };
            db.Recipes.Add(newRecipe);
            await db.SaveChangesAsync();
            var response = await _Client.GetAsync($"/recipes/delete/{newRecipe.Id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Get Delete Test Recipe", content);
        }
    }

    [Fact]
    public async Task Post_Delete_byId_With_Existing_Id_Returns_Ok()
    {
        using (var db = CreateDbContext())
        {
            var newRecipe = new Recipe
            {
                Name = "Post Delete Test Recipe",
                PreparationMinutes = 15
            };
            db.Recipes.Add(newRecipe);

            await db.SaveChangesAsync();

            var formData = new Dictionary<string, string> { { "Id", newRecipe.Id.ToString() } };
            var content = new FormUrlEncodedContent(formData);
            var responsePost = await _Client.PostAsync($"/recipes/delete/{newRecipe.Id}", content);

            var deletedresponse = await _Client.GetAsync($"/recipes/delete/{newRecipe.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, deletedresponse.StatusCode);
        }
    }
}