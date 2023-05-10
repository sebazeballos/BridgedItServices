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
            services.AddSingleton<IMap<InfinityPOSProduct, Product>, InfinityToShopifyProductMap>();
        }
    }
}
