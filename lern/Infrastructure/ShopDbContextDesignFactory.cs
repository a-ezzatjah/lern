using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace lern.Infrastructure;

// EF schema generation must not execute the web application's startup/seeding code.
public sealed class ShopDbContextDesignFactory : IDesignTimeDbContextFactory<ShopDbContext>
{
    public ShopDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ShopDbContext>()
        .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=lern_design_only;Integrated Security=True;Encrypt=True")
        .Options);
}
