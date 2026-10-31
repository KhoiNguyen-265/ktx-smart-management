using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KTX.Data;

public class KtxDesignTimeDbContextFactory : IDesignTimeDbContextFactory<KtxDbContext>
{
    public KtxDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KtxDbContext>();
        
        // Default design-time connection string to MSSQLLocalDB
        var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=KTX_SmartDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new KtxDbContext(optionsBuilder.Options);
    }
}
