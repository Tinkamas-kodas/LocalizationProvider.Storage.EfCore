using DbLocalizationProvider.Abstractions;

namespace LocalizationProvider.Storage.EfCore.Tests;

[LocalizedResource(KeyPrefix = "Test")]
public class TestLocalization
{                     
    public string Title { get; set; } = "Localized title";
}