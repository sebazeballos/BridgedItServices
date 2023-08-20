using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BridgetItService.Contracts;
using BridgetItService.Services;
using Microsoft.OpenApi.Models;
using BridgetItService.Settings;
using BridgetItService.MapperFactory;
using ShopifySharp;
using BridgetItService.Models.Inifnity;

namespace BridgetItService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add the IHttpClientFactory service
            services.AddHttpClient();
            services.AddSettingsConfig(Configuration);
            services.AddSingleton<ApiHandler>();
            services.AddSingleton<IInfinityPOSClient, InfinityPOSClient>();
            services.AddSingleton<IShopifyServiceAPI, ShopifyServiceAPI>();
            services.AddSingleton<IMagentoService, MagentoService>();
            services.AddSingleton<UpdateService>();
            services.AddSingleton<IMap<InfinityPOSProduct, Product>, InfinityToShopifyProductMap>();

            services.AddLogging((logging) =>
            {
                logging.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
            });

            services.AddMvc();
            services.AddControllers();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API de BridgetIt",
                    Version = "v1",
                    Description = "API para BridgetItService"
                });
            });
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
    (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de BridgetIt V1");
                });
            }

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddAWSProvider();
            });


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
