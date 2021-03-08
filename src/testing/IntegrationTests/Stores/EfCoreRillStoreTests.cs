using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rill.Stores.EfCore;

namespace IntegrationTests.Stores
{
    public class EfCoreRillStoreTests : RillStoreTests<EfCoreRillStore>
    {
        private static EfCoreRillStore CreateRillStore(IConfiguration configuration)
        {
            var pass = configuration["SqlServer:Pass"];
            if (string.IsNullOrWhiteSpace(pass))
                throw new System.Exception("Password is missing.");

            var dbContextOptions = new DbContextOptionsBuilder<RillDbContext>()
                .UseSqlServer($@"Server=.,1401;Database=RillTests;User=sa;Password={pass};MultipleActiveResultSets=True;TrustServerCertificate=true")
                .Options;

            using var dbContext = new RillDbContext(dbContextOptions);

            if (!dbContext.Database.EnsureCreated())
            {
                dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.RillEvent;");
                dbContext.Database.ExecuteSqlRaw("DELETE FROM dbo.RillCommit;");
                dbContext.Database.ExecuteSqlRaw("DELETE FROM dbo.Rill;");
            }
            dbContext.Database.CloseConnection();

            return new EfCoreRillStore(dbContextOptions);
        }

        public EfCoreRillStoreTests() : base(() => CreateRillStore(TestEnvironment.Configuration))
        {
        }
    }
}
