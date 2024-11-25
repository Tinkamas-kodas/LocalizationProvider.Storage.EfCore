using System.Globalization;
using DbLocalizationProvider.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalizationProvider.Storage.EfCore;

public class EfCoreResourceRepository(IServiceProvider serviceProvider) : IResourceRepository
{
    public IEnumerable<CultureInfo> GetAvailableLanguages(bool includeInvariant)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        var languages = dbContext.LocalizationResourceTranslations.Select(l => new CultureInfo(l.Language))
            .Distinct().ToList();
        if (includeInvariant)
        {
            languages.Add(CultureInfo.InvariantCulture);
        }

        return languages;
    }

    public LocalizationResource GetByKey(string resourceKey)
    {
        if (resourceKey == null)
        {
            throw new ArgumentNullException(nameof(resourceKey));
        }

        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        var resource = dbContext.LocalizationResources
            .Include(t => t.Translations)
            .AsNoTracking()
            .FirstOrDefault(r => r.ResourceKey == resourceKey);

        return resource!;
    }

    public void InsertResource(LocalizationResource resource)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResources.Add(resource);
        dbContext.SaveChanges();
    }

    public void AddTranslation(LocalizationResource resource, LocalizationResourceTranslation translation)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResourceTranslations.Add(translation);
        dbContext.SaveChanges();
    }

    public void UpdateTranslation(LocalizationResource resource, LocalizationResourceTranslation translation)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResourceTranslations.Update(translation);
        dbContext.SaveChanges();
    }

    public void UpdateResource(LocalizationResource resource)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResources.Update(resource);
        dbContext.SaveChanges();
    }

    public void DeleteAllResources()
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResources.ExecuteDelete();
    }

    public void DeleteResource(LocalizationResource resource)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResources.Remove(resource);
        dbContext.SaveChanges();
    }

    public IEnumerable<LocalizationResource> GetAll()
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        return dbContext.LocalizationResources
            .Include(t => t.Translations)
            .AsNoTracking()
            .ToList();
    }

    public void DeleteTranslation(LocalizationResource resource, LocalizationResourceTranslation translation)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResourceTranslations.Remove(translation);
        dbContext.SaveChanges();
    }

    public void ResetSyncStatus()
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        dbContext.LocalizationResources.ExecuteUpdate(t => t.SetProperty(x => x.FromCode, false));
    }

    public void RegisterDiscoveredResources(ICollection<DiscoveredResource> discoveredResources,
        IEnumerable<LocalizationResource> allResources,
        bool flexibleRefactoringMode)
    {
        using var ctx = serviceProvider.CreateScope();
        using var dbContext = ctx.ServiceProvider.GetRequiredService<LocalizationDbContext>();
        //find all discovered resources not existing in allResources
        var newResources = discoveredResources.Where(d => allResources.All(r => r.ResourceKey != d.Key)).ToList();
        foreach (var newResource in newResources)
        {
            var resource = new LocalizationResource
            {
                ResourceKey = newResource.Key,
                Author = "type-scanner",
                FromCode = true,
                IsHidden = newResource.IsHidden,
                IsModified = false,
                ModificationDate = DateTime.UtcNow,
                Notes = ""
            };
            foreach (var translation in newResource.Translations)
            {
                resource.Translations.Add(new LocalizationResourceTranslation
                {
                    Language = translation.Culture,
                    Value = translation.Translation,
                    ModificationDate = DateTime.UtcNow
                });
            }

            dbContext.LocalizationResources.Add(resource);
        }

        dbContext.SaveChanges();

        //find all   discovered resources  existing in allResources
        var existingResources =
            discoveredResources.Where(d => allResources.Any(r => r.ResourceKey == d.Key)).ToList();
        foreach (var existingResource in existingResources)
        {
            var existingEntity = allResources.First(r => r.ResourceKey == existingResource.Key);
            dbContext.LocalizationResources.Attach(existingEntity);

            existingEntity.IsHidden = existingResource.IsHidden;
            foreach (var existingTranslation in existingResource.Translations)
            {
                var translationEntity =
                    existingEntity.Translations.FirstOrDefault(t => t.Language == existingTranslation.Culture);
                //if translation exists and is different from existing one, update it
                //update only if translation is not modified by user
                if (translationEntity != null && translationEntity.Value != existingTranslation.Translation &&
                    (!existingEntity.IsModified.HasValue || !existingEntity.IsModified.Value))
                {
                    translationEntity.Value = existingTranslation.Translation;
                    translationEntity.ModificationDate = DateTime.UtcNow;
                }
                else if (translationEntity == null)
                {
                    existingEntity.Translations.Add(new LocalizationResourceTranslation
                    {
                        Language = existingTranslation.Culture,
                        Value = existingTranslation.Translation,
                        ModificationDate = DateTime.UtcNow
                    });
                }

            }
        }

        dbContext.SaveChanges();
    }
}