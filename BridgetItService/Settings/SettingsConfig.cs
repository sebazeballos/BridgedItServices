using BridgetItService.Contracts;
using BridgetItService.MapperFactory;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;
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
            services.Configure<EcommerceSettings>(configuration.GetSection("EcommerceSettings"));
            services.Configure<MagentoSettings>(configuration.GetSection("MagentoSettings"));

            services.AddSingleton<IMap<InfinityPOSProduct, Product>, InfinityToShopifyProductMap>();
            services.AddSingleton<IMap<InfinityPosProducts, MagentoProducts>, InfinityToMagentoProductMap>();
            services.AddSingleton<IMap<MagentoOrder, Invoice>, MagentoTransactionsMap>();
            services.AddSingleton<IMap<MagentoRefund, Invoice>, MagentoRefundsMapping>();
        }
    }
}
