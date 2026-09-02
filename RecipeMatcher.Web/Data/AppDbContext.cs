using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ingredient>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<RecipeIngredient>().HasKey(ri => new { ri.RecipeId, ri.IngredientId });
    }
}