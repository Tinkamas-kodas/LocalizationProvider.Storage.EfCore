# Purpose

Add efcore repository provider for awesome [LocalizationProvider](https://github.com/valdisiljuconoks/LocalizationProvider) library.

## Why?
Currenctly LocalizationProvider supports only SQL server, PostgreSQL and AzureTables as storage providers. This package adds support for other databases using efcore.

## Versioning

Library versions corresponds Efcore version. So version 8.0.11 is linked to efcore 8.0.11. 

# How to use
## Isntall package

Add the [package](https://www.nuget.org/packages/LocalizationProvider.Storage.EfCore)

```sh
Install-Package LocalizationProvider.Storage.EfCore
```

.NET Core command line interface:

```sh
dotnet add package LocalizationProvider.Storage.EfCore
```

## Configure

On registering `LocalizationProvider`, add efcore repository provider.

```csharp
serviceCollection
        .AddDbLocalizationProvider(context =>
        {
            ...
            //register efcore repository provider with default schema updater
            context.AddEfCoreStorage();
            //or use custom schema updater if needed
            //context.AddEfCoreStorage<NullSchemaUpdater>();
        });
```

Register `LocalizationDbContext` in your application.

```csharp
serviceCollection.AddDbContext<LocalizationDbContext>(o =>
            {

                //configure specific db provider
                o.UseSqlServer("<<your connection string>>",
                //enable migrations
                builder => builder.MigrationsAssembly("<<your specific migration assembly>>"));
            });
```

## Create localization DB migration

Run `Add-Migration` command to create migration for localization DB.
Note: library is dataabse agnostic, therefore it does not provide any specific migration logic. You have to create your own migrations for database type of your choise.



