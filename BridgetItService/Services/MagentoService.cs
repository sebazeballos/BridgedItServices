using BridgetItService.Contracts;
using BridgetItService.Models;
using BridgetItService.Settings;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using ShopifySharp;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Net;

namespace BridgetItService.Services
{
    public class MagentoService : IMagentoService
    {
        private readonly IOptions<MagentoSettings> _options;
        private readonly IMap<InfinityPosProducts, MagentoProducts> _infinityToMagentoProductMap;
        private readonly HttpClient _client;
        private readonly string MEDIA_TYPE = "application/json";
        public MagentoService(IServiceProvider serviceProvider)
        {
            _options = serviceProvider.GetService<IOptions<MagentoSettings>>();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
            _infinityToMagentoProductMap = serviceProvider.GetService<IMap<InfinityPosProducts, MagentoProducts>>();
        }
        public async Task PublishProducts(InfinityPosProducts products)
        {
            if (products != null)
            {
                try
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
                    MagentoProducts magentoProducts = _infinityToMagentoProductMap.Map(products);
                    foreach (MagentoProduct product in magentoProducts.Product)
                    {
                        MagentoRequest request = new MagentoRequest();
                        request.Product = product;
                        var body = SerializeBody(request);
                        var response = await _client.PostAsync($"{_options.Value.BaseUrl + _options.Value.CreateProduct}", body);
                        response.EnsureSuccessStatusCode();
                    }
                }
                catch (Exception ex)
                {
                    //Log a exception
                }
            }
        }
        public async Task<string> GetAuth()
        {
            var auth = new MagentoAuth
            {
                Username = _options.Value.User,
                Password = _options.Value.Password
            };
            var credentials = SerializeBody(auth);
            var response = await _client.PostAsync($"{_options.Value.BaseUrl + _options.Value.AuthUrl}", credentials);
            response.EnsureSuccessStatusCode();
            var a = Deserialize<string>(await response.Content.ReadAsStringAsync());
            return a;
        }

        private HttpContent SerializeBody(object body)
        {

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                WriteIndented = true
            };
            var stringPayload = JsonSerializer.Serialize(body, serializeOptions);
            return new StringContent(stringPayload, Encoding.UTF8, MEDIA_TYPE);
        }
        private T Deserialize<T>(string jsonElement) where T : class =>
            JsonSerializer.Deserialize<T>(jsonElement, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = new SnakeCaseNamingPolicy()
            });
    }
}
