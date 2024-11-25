using Microsoft.Extensions.DependencyInjection;
using DbLocalizationProvider.AspNetCore;
using Testcontainers.PostgreSql;
using System.Globalization;
using DbLocalizationProvider.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LocalizationProvider.Storage.EfCore.Tests
{

    public class EfCoreLocalizationTests
    {
        #region run docker postgre sql container
        private static readonly Lazy<PostgreSqlContainer> Container = new(() =>
        {
            var container = new PostgreSqlBuilder()
                .WithDatabase(DatabaseName)
                .WithUsername(Username)
                .WithPassword(Password)
                .WithImage(ImageName)
                .WithPortBinding(PostgreSqlPort, true)
                .Build();

            container.StartAsync().Wait();
            return container;
        });


        private static readonly string DatabaseName = "paymentsDb";
        private static readonly string Username = "postgres";
        private static readonly string Password = "admin";
        private static readonly string ImageName = "postgres:16";
        private static readonly int PostgreSqlPort = 5432;
        public static string GetConnectionString(string databaseName)
        {
            var properties = new Dictionary<string, string>
            {
                { "Server", Container.Value.Hostname },
                { "Port", Container.Value.GetMappedPublicPort(PostgreSqlPort).ToString() },
                { "Database", databaseName },
                { "User Id", Username },
                { "Pwd", Password }
            };
            return string.Join(";", properties.Select(property => string.Join("=", property.Key, property.Value)));
        }
        #endregion
        
        [Fact]
        public void RegisterEfCoreProviderTest()
        {
            var defaultCulture = CultureInfo.GetCultureInfo("en-US");
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddLogging(o => o.AddDebug())
                .AddDbLocalizationProvider(context =>
                {
                    context.DefaultResourceCulture = defaultCulture;
                    context.EnableInvariantCultureFallback = false;
                    context.ScanAllAssemblies = false;

                    context.PopulateCacheOnStartup = false;
                    context.DiagnosticsEnabled = true;
                    context.DiscoverAndRegisterResources = false;
                    //register efcore repository provider with default schema updater
                    context.AddEfCoreStorage();
                    //or use custom schema updater if needed
                    //context.AddEfCoreStorage<NullSchemaUpdater>();
                });

            //config localization db context
            serviceCollection.AddDbContext<LocalizationDbContext>(o =>
            {

                o.UseNpgsql(GetConnectionString("localizationDb"),
                    //enable migrations
                    builder => builder.MigrationsAssembly("LocalizationProvider.Storage.EfCore.Tests"));
            });

            //configure net core localization services
            serviceCollection.AddLocalization();

            using var scope = serviceCollection.BuildServiceProvider().CreateScope();

            var sync = scope.ServiceProvider.GetRequiredService<Synchronizer>();
            sync.UpdateStorageSchema();
            sync.SyncResources(true);

            //use net core localization services
            var stringLocalizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<TestLocalization>>();

            Assert.Equal("Localized title", stringLocalizer.GetString(t => t.Title));

        }
    }
}