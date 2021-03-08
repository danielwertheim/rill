using System;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests
{
    internal static class TestEnvironment
    {
        private static readonly Lazy<IConfiguration> LazyConfiguration = new(CreateConfiguration);

        internal static IConfiguration Configuration => LazyConfiguration.Value;

        private static IConfiguration CreateConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("rill-appsettings-integrationtests.json", true, false)
                .AddJsonFile("rill-appsettings.local.json", true, false)
                .AddEnvironmentVariables("Rill_")
                .Build();
    }
}
