using BridgetItService.Contracts;
using BridgetItService.MapperFactory;
using BridgetItService.Models;
using ShopifySharp;

namespace BridgetItService.Settings
{
    public static class SettingsConfig
    {
        public static void AddSettingsConfig(this IServiceCollection services, IConfiguration configuration) {
            services.AddOptions();
            services.Configure<InfinityPOSSettings>(configuration.GetSection("InfinityPOSSettings"));
            services.Configure<IntervalMinutes>(configuration.GetSection("IntervalMinutes"));
            services.Configure<ShopifySettings>(configuration.GetSection("ShopifySettings"));
            services.AddSingleton<IMap<InfinityPOSProduct, Product>, InfinityToShopifyProductMap>();
        }
    }
}
