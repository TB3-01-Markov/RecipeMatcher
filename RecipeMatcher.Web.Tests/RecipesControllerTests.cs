using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
using Xunit;

namespace RecipeMatcher.Web.Tests;

public class RecipesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RecipesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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
}