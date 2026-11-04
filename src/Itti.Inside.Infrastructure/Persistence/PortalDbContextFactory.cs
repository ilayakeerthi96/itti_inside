using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Itti.Inside.Infrastructure.Persistence;

/// <summary>Lets the dotnet-ef tool create migrations without starting the website.</summary>
public sealed class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
{
    public PortalDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<PortalDbContext>().UseSqlServer().Options);
}
