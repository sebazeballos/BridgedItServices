using BridgetItService.Contracts;
using BridgetItService.Models;
using BridgetItService.Settings;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Communication;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BridgetItService.Services
{
    public class InfinityPOSClient : IInfinityPOSClient
    {

        private readonly HttpClient _client;
        private readonly IOptions<InfinityPOSSettings> _options;
        private const string Client = "infinityPOS";
        private readonly string MEDIA_TYPE = "application/json";

        public InfinityPOSClient(IHttpClientFactory clientFactory, IServiceProvider provider)
        {
            _options = provider.GetService<IOptions<InfinityPOSSettings>>();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
        }
        public async Task<string> GetAuthentication()
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(_options.Value.ClientId), "client_id");
            formData.Add(new StringContent(_options.Value.ClientSecret),"client_secret");
            formData.Add(new StringContent("client_credentials"),"grant_type");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseEndpoint + _options.Value.AuthorizationEndpoint);
            requestMessage.Content = formData;
            HttpResponseMessage response = null;
            try
            {
                response = await _client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                var auth = Deserialize<Auth>( await response.Content.ReadAsStringAsync());
                return auth.accessToken;
            }
            catch (HttpRequestException ex) {

                throw new ServiceException($"REQUEST {HttpMethods.Post} to {_options.Value.AuthorizationEndpoint} FAILD with body: {formData}", ex.ToString());
            }
        }

        public async Task<InfinityPosProducts> GetProducts(string startDate)
        {
            try {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/products?updated_since={startDate}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var products = Deserialize<InfinityPosProducts>(content);
                return products;
            }
            catch (HttpRequestException ex)
            {
                return null;
                //throw new ServiceException($"REQUEST {HttpMethods.Post} to {_options.Value.AuthorizationEndpoint} FAILD with body:", ex.ToString());
            }
        }

        public async Task<IList<InfinityPOSProduct>> AddStock(string startDate)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/product_inventory?updated_since={startDate}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var inventories = Deserialize<InventoryResponse>(content);
                InfinityPosProducts products = new InfinityPosProducts();
                products.Products = new List<InfinityPOSProduct>();
                foreach (var inventory in inventories.Inventory)
                {
                    products.Products.Add(await UpdateInventory(inventory));
                }
                return products.Products;
            }
            catch (HttpRequestException ex)
            {
                return null;
                //throw new ServiceException($"REQUEST {HttpMethods.Post} to {_options.Value.AuthorizationEndpoint} FAILD with body:", ex.ToString());
            }
        }
        public async Task<InfinityPOSProduct?> UpdateInventory(Inventory inventory)
        {
            try
            {
                
                if (long.TryParse(inventory.ProductCode, out var productCode))
                {
                    var product = await GetProduct(productCode);
                    if(inventory.SellableQuantity == 0){
                        product.SellableQuantity = 0;
                    }
                    else
                    {
                        product.SellableQuantity = Convert.ToInt64(inventory.SellableQuantity);
                    }
                    return product;
                }
                return null;
            }
            catch (HttpRequestException ex)
            {
                return null;
            }
        }

        public async Task<InfinityPOSProduct> GetProduct(long productCode)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/products/{productCode}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var product = Deserialize<InfinityPOSProduct>(content);
                return product;
            }
            catch (HttpRequestException ex)
            {
                return null;
                //throw new ServiceException($"REQUEST {HttpMethods.Post} to {_options.Value.AuthorizationEndpoint} FAILD with body:", ex.ToString());
            }
            
        }

        public async Task PutProductInInfinity (InfinityPOSProduct product)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                string guid = Guid.NewGuid().ToString();
                _client.DefaultRequestHeaders.Add("x-request-id", guid);
                var infinityProduct = SerializeBody(product);
                var response = await _client.PutAsync($"{_options.Value.BaseEndpoint}/products?site_specific_override=always", infinityProduct);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException($"REQUEST {HttpMethods.Put} to {_options.Value.BaseEndpoint} /products?site_specific_override=always FAILD with body:", ex.Data.ToString());
            }
        }

        private HttpContent SerializeBody(object body) {

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