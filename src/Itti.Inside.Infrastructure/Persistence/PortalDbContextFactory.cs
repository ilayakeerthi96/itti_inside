using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Itti.Inside.Infrastructure.Persistence;

/// <summary>Lets the dotnet-ef tool create migrations without starting the website.</summary>
public sealed class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
{
    // Never opened: creating a migration only needs to know the database type.
    private const string DesignTimeConnection = "Server=localhost;Database=ittiinside";

    public PortalDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<PortalDbContext>().UseMySQL(DesignTimeConnection).Options);
}
