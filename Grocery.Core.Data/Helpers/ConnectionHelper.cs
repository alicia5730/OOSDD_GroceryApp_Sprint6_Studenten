using Microsoft.Extensions.Configuration;

namespace Grocery.Core.Data.Helpers
{
    public static class ConnectionHelper
    {
        public static string ConnectionStringValue(string name)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(basePath, "appsettings.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuratiebestand niet gevonden: {configPath}");
            }

            var builder = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true);
            var config = builder.Build();
            return config.GetConnectionString(name)!;
        }

    }
}
