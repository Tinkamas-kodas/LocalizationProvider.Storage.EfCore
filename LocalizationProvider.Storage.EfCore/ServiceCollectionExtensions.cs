using DbLocalizationProvider;
using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Sync;

namespace LocalizationProvider.Storage.EfCore;

public static class ServiceCollectionExtensions
{
    

    /// <summary>
    /// Add EF Core storage for localization provider.
    /// </summary>
    /// <typeparam name="T">Localization DB schema updater.</typeparam>
    /// <param name="configurationContext">Localization DB configuration context to inject IResourceRepository and UpdateSchema.Command.</param>
    /// <returns></returns>
    public static void AddEfCoreStorage<T>(this ConfigurationContext configurationContext)
        where T : ICommandHandler<UpdateSchema.Command>
    {
        configurationContext.TypeFactory.AddTransient<IResourceRepository, EfCoreResourceRepository>();
        configurationContext.TypeFactory.ForQuery<UpdateSchema.Command>().SetHandler<T>();
    }


    /// <summary>
    /// Add EF Core storage for localization provider with default EfCoreSchemaUpdater.
    /// </summary>
    /// <param name="configurationContext">Localization DB configuration context to inject IResourceRepository and UpdateSchema.Command.</param>
    /// <returns></returns>
    public static void AddEfCoreStorage(this ConfigurationContext configurationContext)
    {
        AddEfCoreStorage<EfCoreSchemaUpdater>(configurationContext);
    }
}