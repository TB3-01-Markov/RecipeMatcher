using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests;

// MME: DRY (Don't Repeat Yourself), use a baseclass for the tests
// See IntegrationTests class in BookTracker
public class IngredientsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IngredientsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_Create_With_Empty_Name_Does_Not_Save_And_Shows_Validation_Error()
    {
        var client = _factory.CreateClient();

        var formData = new Dictionary<string, string> { ["Name"] = "" };

        var response = await client.PostAsync("/ingredients/create", new FormUrlEncodedContent(formData));

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("field-validation-error", content);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.DoesNotContain(dbContext.Ingredients, i => i.Name == "");
        }
    }

    [Fact]
    public async Task Post_Create_With_Name()
    {
        
        var client = _factory.CreateClient();
        var formData = new Dictionary<string, string> { ["Name"] = "Melk" };
        var response = await client.PostAsync("/ingredients/create", new FormUrlEncodedContent(formData));
        response.EnsureSuccessStatusCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Contains(dbContext.Ingredients, i => i.Name == "Melk");
        }
    }
    [Fact]
    public async Task Post_Create_With_Dezelfde_Name()
    {
        var client = _factory.CreateClient();

        var formData1 = new Dictionary<string, string> { ["Name"] = "Salt" };
        var response1 = await client.PostAsync("/ingredients/create", new FormUrlEncodedContent(formData1));
        response1.EnsureSuccessStatusCode();

        var formData2 = new Dictionary<string, string> { ["Name"] = "Salt" };
        var response2 = await client.PostAsync("/ingredients/create", new FormUrlEncodedContent(formData2));
        response2.EnsureSuccessStatusCode();

        var content2 = await response2.Content.ReadAsStringAsync();
        Assert.Contains("field-validation-error", content2);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var count = await dbContext.Ingredients.CountAsync(i => i.Name == "Salt");
            Assert.Equal(1, count);
        }
    }
    [Fact]
    public async Task Post_Edit_Updates_Ingredient_Name_With_New_Name()
    {
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ingredient = new Ingredient { Name = "Old Name" };
            dbContext.Ingredients.Add(ingredient);
            await dbContext.SaveChangesAsync();

            id = ingredient.Id;
            Assert.Contains(dbContext.Ingredients, i => i.Name == "Old Name");
        }

        var client = _factory.CreateClient();

        var formData = new Dictionary<string, string>
        {
            ["Name"] = "New Name",
            ["Id"] = id.ToString()
        };
        var response = await client.PostAsync($"/ingredients/edit/{id}", new FormUrlEncodedContent(formData));
        response.EnsureSuccessStatusCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Contains(dbContext.Ingredients, i => i.Name == "New Name");
        }

    }
    [Fact]
    public async Task Post_Edit_Updates_Ingredient_Name_With_Dezelfde_Name()
    {
        int id1, id2;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ingredient = new Ingredient { Name = "Old Name @Salt" };
            dbContext.Ingredients.Add(ingredient);
            await dbContext.SaveChangesAsync();

            id1 = ingredient.Id;
            Assert.Contains(dbContext.Ingredients, i => i.Name == "Old Name @Salt");
        }
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ingredient = new Ingredient { Name = "Old Name @LactosaVrijMelk" };
            dbContext.Ingredients.Add(ingredient);
            await dbContext.SaveChangesAsync();

            id2 = ingredient.Id;
            Assert.Contains(dbContext.Ingredients, i => i.Name == "Old Name @LactosaVrijMelk");
        }
        var client = _factory.CreateClient();

        var formData1 = new Dictionary<string, string>
        {
            ["Name"] = "Old Name @Salt",
            ["Id"] = id1.ToString()
        };
        var formData2 = new Dictionary<string, string>
        {
            ["Name"] = "Old Name @LactosaVrijMelk",
            ["Id"] = id2.ToString()
        };
        var response1 = await client.PostAsync($"/ingredients/edit/{id1}", new FormUrlEncodedContent(formData1));
        response1.EnsureSuccessStatusCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Contains(dbContext.Ingredients, i => i.Name == "Old Name @Salt");
        }

        var response2 = await client.PostAsync($"/ingredients/edit/{id1}", new FormUrlEncodedContent(formData2));
        response2.EnsureSuccessStatusCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Contains(dbContext.Ingredients, i => i.Name == "Old Name @Salt");
            Assert.Contains(dbContext.Ingredients, i => i.Name == "Old Name @LactosaVrijMelk");
        }
    }

    [Fact]
    public async Task Get_Ingredients_Returns_Ok_And_Shows_Ingredient()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.EnsureCreatedAsync();

            dbContext.Ingredients.Add(new Ingredient { Name = "Test melk" });

            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/ingredients");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Test melk", content);
    }

    [Fact]
    public async Task Get_Details_With_Existing_Id_Returns_Ok()
    {
        int id;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var ingredient = new Ingredient { Name = "Details Test Ingredient" };

            dbContext.Ingredients.Add(ingredient);
            await dbContext.SaveChangesAsync();

            id = ingredient.Id;
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/ingredients/details/{id}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Details Test Ingredient", content);
    }

    [Fact]
    public async Task Get_Details_With_Unknown_Id_Returns_NotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/ingredients/details/999999");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Delete_byId_Unknown_Id_Returns_NotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/ingredients/delete/999999");
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

            var newIngredient = new Ingredient { Name = "Get Delete Test Ingredient" };
            dbContext.Ingredients.Add(newIngredient);
            await dbContext.SaveChangesAsync();

            var response = await client.GetAsync($"/ingredients/delete/{newIngredient.Id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Get Delete Test Ingredient", content);
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

            var newIngredient = new Ingredient { Name = "Post Delete Test Ingredient" };
            dbContext.Ingredients.Add(newIngredient);

            await dbContext.SaveChangesAsync();

            var formData = new Dictionary<string, string> { { "Id", newIngredient.Id.ToString() } };
            var content = new FormUrlEncodedContent(formData);
            var responsePost = await client.PostAsync($"/ingredients/delete/{newIngredient.Id}", content);

            var deletedresponse = await client.GetAsync($"/ingredients/delete/{newIngredient.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, deletedresponse.StatusCode);
        }
    }
}