using BridgetItService.Contracts;
using BridgetItService.Settings;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using ShopifySharp;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Net;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;

namespace BridgetItService.Services
{
    public class MagentoService : IMagentoService
    {
        private readonly IOptions<MagentoSettings> _options;
        private readonly IMap<InfinityPosProducts, MagentoProducts> _infinityToMagentoProductMap;
        private readonly IInfinityPOSClient _infinityPOSClient;
        private readonly IMap<MagentoOrder, Invoice> _magentoTransactionsMap;
        private readonly HttpClient _client;
        private readonly string MEDIA_TYPE = "application/json";
        public MagentoService(IServiceProvider serviceProvider, IInfinityPOSClient infinityPOSClient)
        {
            _options = serviceProvider.GetService<IOptions<MagentoSettings>>();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
            _infinityToMagentoProductMap = serviceProvider.GetService<IMap<InfinityPosProducts, MagentoProducts>>();
            _magentoTransactionsMap = serviceProvider.GetService<IMap<MagentoOrder, Invoice>>();
            _infinityPOSClient = infinityPOSClient;
        }
        public async Task PublishProducts(InfinityPosProducts products)
        {
            if (products != null)
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
                MagentoProducts magentoProducts = _infinityToMagentoProductMap.Map(products);
                foreach (MagentoProduct product in magentoProducts.Product)
                {
                    MagentoRequest request = new MagentoRequest();
                    request.Product = product;
                    var body = SerializeBody(request);
                    try
                    {
                        var response = await _client.PostAsync($"{_options.Value.BaseUrl + _options.Value.CreateProduct}", body);
                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.StatusCode == HttpStatusCode.BadRequest)
                        {
                            try
                            {
                                var response = await _client.PutAsync($"{_options.Value.BaseUrl + _options.Value.CreateProduct}" + request.Product.Sku, body);
                                response.EnsureSuccessStatusCode();
                            }
                            catch (HttpRequestException ex2)
                            {
                                //log exception
                            }
                        }
                    }
                }
            }
        }
        public async Task GetOrders()
        {
            var parameters = "?searchCriteria[filter_groups][0][filters][0][field]=status&searchCriteria[filter_groups][0][filters][0][value]=complete" +
                "&searchCriteria[filter_groups][0][filters][0][condition_type]=eq&searchCriteria[filter_groups][1][filters][0][field]=created_at&searchCriteria[filter_groups]" +
                "[1][filters][0][value]=2023-07-2T00:00:00Z&searchCriteria[filter_groups][1][filters][0][condition_type]=gteq&searchCriteria[filter_groups][2][filters][0][field]=created_at" +
                "&searchCriteria[filter_groups][2][filters][0][value]=2023-07-30T18:00:00Z&searchCriteria[filter_groups][2][filters][0][condition_type]=lteq";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
            
            try
            {
                var response = await _client.GetAsync($"{_options.Value.BaseUrl + _options.Value.Orders + parameters}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var magentoOrder = Deserialize<MagentoOrder>(content);
                Invoice invoice = _magentoTransactionsMap.Map(magentoOrder);
                await _infinityPOSClient.PostTransaction(invoice);
            }
            catch (HttpRequestException ex) { }
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
            return Deserialize<string>(await response.Content.ReadAsStringAsync());
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
