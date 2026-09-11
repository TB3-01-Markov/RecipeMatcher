using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;



namespace RecipeMatcher.Web.Tests;

public class RecipesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RecipesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_Create_With_Empty_Name_Does_Not_Save_And_Shows_Validation_Error()
    {
        var client = _factory.CreateClient();

        var formData = new Dictionary<string, string>
        {
            ["Name"] = "",
            ["PreparationMinutes"] = "15"
        };

        var response = await client.PostAsync("/recipes/create", new FormUrlEncodedContent(formData));

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("field-validation-error", content);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            //var count = await dbContext.Recipes.CountAsync();
            //Assert.Equal(0, count);
            Assert.DoesNotContain(dbContext.Recipes, r => r.Name == "");
        }
    }

    [Fact]
    public async Task Get_Recipes_Returns_Ok_And_Shows_Recipe()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureCreatedAsync();

            dbContext.Recipes.Add(new Recipe
            {
                Name = "Test Pancakes",
                PreparationMinutes = 15
            });

            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/recipes");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Test Pancakes", content);
    }

    [Fact]
    public async Task Get_Details_With_Existing_Id_Returns_Ok()
    {
        int id;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var recipe = new Recipe
            {
                Name = "Details Test Recipe",
                PreparationMinutes = 10
            };

            dbContext.Recipes.Add(recipe);
            await dbContext.SaveChangesAsync();

            id = recipe.Id;
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/recipes/details/{id}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Details Test Recipe", content);
    }

    [Fact]
    public async Task Get_Details_With_Unknown_Id_Returns_NotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/recipes/details/999999");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Delete_byId_Unknown_Id_Returns_NotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/recipes/delete/999999");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task Get_Delete_byId_With_Existing_Id_Returns_Ok()
    {
        var client = _factory.CreateClient();


        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var newRecipe = new Recipe
            {
                Name = "Get Delete Test Recipe",
                PreparationMinutes = 15
            };
            dbContext.Recipes.Add(newRecipe);
            await dbContext.SaveChangesAsync();

            var response = await client.GetAsync($"/recipes/delete/{newRecipe.Id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Get Delete Test Recipe", content);
        }
    }

    [Fact]
    public async Task Post_Delete_byId_With_Existing_Id_Returns_Ok()
    {
        var client = _factory.CreateClient();
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var newRecipe = new Recipe
            {
                Name = "Post Delete Test Recipe",
                PreparationMinutes = 15
            };
            dbContext.Recipes.Add(newRecipe);

            await dbContext.SaveChangesAsync();

            var formData = new Dictionary<string, string> { { "Id", newRecipe.Id.ToString() } };
            var content = new FormUrlEncodedContent(formData);
            var responsePost = await client.PostAsync($"/recipes/delete/{newRecipe.Id}", content);

            var deletedresponse = await client.GetAsync($"/recipes/delete/{newRecipe.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, deletedresponse.StatusCode);
        }
    }
}