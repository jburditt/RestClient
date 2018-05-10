using Microsoft.Extensions.Configuration;
using System;

namespace RestClient.Tests
{
    /// <summary>
    ///   Application Settings
    /// </summary>
    /// <nugets>
    ///   Microsoft.Extensions.Configuration
    ///   Microsoft.Extensions.Configuration.Json
    ///   Microsoft.Extensions.Configuration.Binder
    /// </nugets>
    public static class ApplicationSettings
    {
        public static bool EnableTestServiceAddTracing { get; private set; }

        private static IConfigurationRoot Configuration { get; set; }

        static ApplicationSettings()
        {
            Build();
        }

        public static void Build()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            EnableTestServiceAddTracing = Configuration.GetValue<bool>("EnableTestServiceAddTracing");
        }
    }
}
