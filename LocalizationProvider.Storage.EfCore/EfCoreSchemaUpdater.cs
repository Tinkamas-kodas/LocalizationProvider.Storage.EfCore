using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalizationProvider.Storage.EfCore;

/// <summary>
/// Default EF Core schema updater. It will use standard EF Core migration mechanism to update schema. The migrations assembly is expected to be in the different assembly as the DbContext.
/// </summary>
/// <param name="serviceProvider"></param>
public class EfCoreSchemaUpdater(IServiceProvider serviceProvider) : ICommandHandler<UpdateSchema.Command>
{
    public void Execute(UpdateSchema.Command command)
    {
        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.Database.Migrate();
    }
}