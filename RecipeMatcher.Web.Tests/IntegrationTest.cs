using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests
{
    public abstract class IntegrationTest: IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _Factory;
        protected readonly HttpClient _Client;

        public IntegrationTest(CustomWebApplicationFactory factory)
        {
            _Factory = factory;
            _Client = _Factory.CreateClient();
            InitializeDB();
        }
        protected void InitializeDB()
        {
            using var db = CreateDbContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        protected AppDbContext CreateDbContext()
        {
            var scope = _Factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }
    }
}
