using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LocalizationProvider.Storage.EfCore.Tests;

/// <summary>
/// used for dotnet ef migrations
/// </summary>
public class LocalizationDbContextFactory : IDesignTimeDbContextFactory<LocalizationDbContext>
{
    public LocalizationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LocalizationDbContext>();
        var cnnString = "Host=localhost;Database=payments;Username=postgres;Password=admin";
        optionsBuilder.UseNpgsql(cnnString, builder => builder.MigrationsAssembly("LocalizationProvider.Storage.EfCore.Tests"));
        return new LocalizationDbContext(optionsBuilder.Options);
    }
}