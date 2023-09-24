using BridgetItService.Contracts;
using BridgetItService.Models.Inifnity;
using BridgetItService.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Excel = Microsoft.Office.Interop.Excel;
using System.Linq;
using ShopifySharp;
using System.Net.Http;
using System.Net;
using Microsoft.Office.Interop.Excel;
using System;

namespace BridgetItService.Services
{
    public class InfinityPOSClient : IInfinityPOSClient
    {

        private readonly HttpClient _client;
        private readonly IOptions<InfinityPOSSettings> _options;
        private const string Client = "infinityPOS";
        private readonly string MEDIA_TYPE = "application/json";
        private readonly string GUID = Guid.NewGuid().ToString();
        private readonly ILogger<MagentoService> _logger;

        public InfinityPOSClient(IHttpClientFactory clientFactory, IServiceProvider provider, ILogger<MagentoService> logger)
        {
            _options = provider.GetService<IOptions<InfinityPOSSettings>>();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
            _logger = logger;
        }
        public async Task<string> GetAuthentication()
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(_options.Value.ClientId), "client_id");
            formData.Add(new StringContent(_options.Value.ClientSecret), "client_secret");
            formData.Add(new StringContent("client_credentials"), "grant_type");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseEndpoint + _options.Value.AuthorizationEndpoint);
            requestMessage.Content = formData;
            HttpResponseMessage response = null;
            var bodyError = "";
            try
            {
                response = await _client.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }
                response.EnsureSuccessStatusCode();
                var auth = Deserialize<Auth>(await response.Content.ReadAsStringAsync());
                return auth.accessToken;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint  {HttpMethod.Post + _options.Value.BaseEndpoint + _options.Value.AuthorizationEndpoint} Message = {bodyError}");
                return null;
                //throw new ServiceException($"REQUEST {HttpMethods.Post} to {_options.Value.AuthorizationEndpoint} FAILD with body: {formData}", ex.ToString());
            }
        }

        public async Task<InfinityPosProducts> GetProducts(string startDate)
        {
            var bodyError = "";
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/products?updated_since={startDate}");

                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var products = Deserialize<InfinityPosProducts>(content);


                return products;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Method Get {_options.Value.BaseEndpoint}/products?updated_since={startDate} Message = {bodyError}");
                }
                return null;
            }
        }

        public async Task<PutProducts> SetProductAsFalse(string startDate)
        {
            var bodyError = "";
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/products?updated_since={startDate}");

                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var products = Deserialize<PutProducts>(content);

                if (products.Products.Count > 0)
                {
                    foreach (var product in products.Products)
                    {
                        if (product.CustomFields != null)
                        {
                            if (!product.CustomFields.Any(cf => cf.FieldName == "Web Enabled"))
                            {
                                product.CustomFields.Add(new CustomFields
                                {
                                    FieldName = "Web Enabled",
                                    FieldValue = "False"
                                });
                            }
                            else
                            {
                                product.CustomFields.FirstOrDefault(cf => cf.FieldName == "Web Enabled").FieldValue = "False";
                            }
                        }
                        else
                        {
                            product.CustomFields = new List<CustomFields?>();
                            product.CustomFields.Add(new CustomFields
                            {
                                FieldName = "Web Enabled",
                                FieldValue = "False"
                            });
                        }
                        //await _infinityPOSClient.PutProductInInfinity(product);
                    }

                    await PutProductListInInfinity(products);

                    foreach (var product in products.Products)
                    {
                        if (product.CustomFields != null)
                        {
                            if (!product.CustomFields.Any(cf => cf.FieldName == "Web Enabled"))
                            {
                                product.CustomFields.Add(new CustomFields
                                {
                                    FieldName = "Web Enabled",
                                    FieldValue = "True"
                                });
                            }
                            else
                            {
                                product.CustomFields.FirstOrDefault(cf => cf.FieldName == "Web Enabled").FieldValue = "True";
                            }
                        }
                        else
                        {
                            product.CustomFields = new List<CustomFields?>();
                            product.CustomFields.Add(new CustomFields
                            {
                                FieldName = "Web Enabled",
                                FieldValue = "True"
                            });
                        }
                        //await _infinityPOSClient.PutProductInInfinity(product);
                    }
                }


                return products;
            }
            catch (HttpRequestException ex)
            {
                //if (ex.StatusCode != HttpStatusCode.NotFound)
                //{
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                        + $" Using Endpoint Method Get {_options.Value.BaseEndpoint}/products?updated_since={startDate} Message = {bodyError}");
                //}
                return null;
            }
        }

        public async Task<InfinityPosProducts> AddStock(InfinityPosProducts products, string startDate)
        {
            var bodyError = "";
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/product_inventory?updated_since={startDate}");

                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var inventories = Deserialize<InventoryResponse>(content);
                if (inventories != null)
                {
                    var onlyLastUpdated = GetLastUpdatedInventory(inventories);

                    return await SetProducts(onlyLastUpdated, products);
                }
                return products;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Method Get {_options.Value.BaseEndpoint}/product_inventory?updated_since={startDate} Message = {bodyError}");
                }
                return null;
            }
        }

        private List<Inventory> GetLastUpdatedInventory(InventoryResponse inventories) => inventories.Inventory.Where(inv => inv.SiteCode == 1)
                                                                                            .GroupBy(p => p.ProductCode)
                                                                                            .Select(pr => pr.OrderByDescending(obj => obj.Updated).First())
                                                                                            .ToList();

        private async Task<InfinityPosProducts> SetProducts(List<Inventory> inventories, InfinityPosProducts? products = null)
        {
            if (products != null)
            {
                HashSet<string> productCodes = new HashSet<string>(products.Products.Select(p => p.ProductCode));

                List<Inventory> nonRepeatedInventories = inventories.Where(i => !productCodes.Contains(i.ProductCode)).ToList();

                if (nonRepeatedInventories.Count > 0)
                {
                    foreach (Inventory inv in nonRepeatedInventories)
                    {
                        products.Products.Add(await GetProduct(inv.ProductCode));
                    }
                }
            }
            else
            {
                await CreateListOfProductsFromInventory(inventories);
            }


            foreach (var inventory in inventories)
            {
                var matchingProduct = products.Products.FirstOrDefault(p => p?.ProductCode == inventory.ProductCode);
                if (matchingProduct != null)
                {
                    matchingProduct.SellableQuantity = Convert.ToInt64(inventory.SellableQuantity);
                }
            }
            return products;
        }

        private async Task<InfinityPosProducts> CreateListOfProductsFromInventory(List<Inventory> inventories)
        {
            InfinityPosProducts products = new InfinityPosProducts();
            products.Products = new List<InfinityPOSProduct?>();
            foreach (Inventory inv in inventories)
            {
                products.Products.Add(await GetProduct(inv.ProductCode));
            }
            return products;
        }

        private async Task<InfinityPOSProduct> GetProduct(string productCode)
        {
            var bodyError = "";
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/products/{productCode}");

                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var product = Deserialize<InfinityPOSProduct>(content);
                return product;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Method Get {_options.Value.BaseEndpoint}/products/{productCode} Message = {bodyError}");
                return null;
            }
        }

        public async Task PutProductInInfinity(PutProductInInfinity product)
        {
            var bodyError = "";
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                string guid = Guid.NewGuid().ToString();
                if (_client.DefaultRequestHeaders.Contains("x-request-id"))
                {
                    _client.DefaultRequestHeaders.Remove("x-request-id");
                }
                _client.DefaultRequestHeaders.Add("x-request-id", guid);
                var infinityProduct = SerializeBody(product);
                var response = await _client.PostAsync($"{_options.Value.BaseEndpoint}/products", infinityProduct);
                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Method Put {_options.Value.BaseEndpoint}/products Message = {bodyError}");
            }
        }

        public async Task PutProductListInInfinity(PutProducts products)
        {
            var responseBody = "";
            if (products != null)
            {
                foreach (var product in products.Products)
                {
                    try
                    {
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                        string guid = Guid.NewGuid().ToString();
                        if (_client.DefaultRequestHeaders.Contains("x-request-id"))
                        {
                            _client.DefaultRequestHeaders.Remove("x-request-id");
                        }
                        _client.DefaultRequestHeaders.Add("x-request-id", guid);
                        var infinityProducts = SerializeBody(product);
                        var response = await _client.PutAsync($"{_options.Value.BaseEndpoint}/products?site_specific_override=always", infinityProducts);

                        if (!response.IsSuccessStatusCode)
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                        }

                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException ex)
                    {
                        if(responseBody.Contains("The supplier code does not exist"))
                        {
                            await PutProductInInfinity(product);
                        }
                        _logger.LogError("Exception = " + ex.StatusCode.ToString()
                                    + $" Using Endpoint Method Put {_options.Value.BaseEndpoint}/products Message = {responseBody}");
                    }
                }
                
            }
        }

        public async Task PostTransaction(Invoice invoice)
        {
            var bodyError = "";
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                if (_client.DefaultRequestHeaders.Contains("x-request-id"))
                {
                    _client.DefaultRequestHeaders.Remove("x-request-id");
                }
                _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
                _client.DefaultRequestHeaders.Add("x-logging-id", Guid.NewGuid().ToString());
                _client.DefaultRequestHeaders.Add("user-agent", "Infinity API Synchronisation Service");
                var infinityInvoices = SerializeBody(invoice);
                var response = await _client.PostAsync(_options.Value.BaseEndpoint + _options.Value.PostInvoicesEndpoint, infinityInvoices);

                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                        + $" Using Endpoint Method Get {_options.Value.BaseEndpoint + _options.Value.PostInvoicesEndpoint}" +
                        $" Body = {TransformToRawString(invoice)}  Message = {bodyError}");
            }
        }

        private HttpContent SerializeBody(object body)
        {

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                WriteIndented = true
            };

            var stringPayload = JsonSerializer.Serialize(body, serializeOptions);
            return new StringContent(stringPayload, Encoding.UTF8, "application/json");
        }
        private T Deserialize<T>(string jsonElement) where T : class =>
            JsonSerializer.Deserialize<T>(jsonElement, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = new SnakeCaseNamingPolicy()
            });
        private string TransformToRawString(object body)
        {

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                WriteIndented = true
            };
            return JsonSerializer.Serialize(body, serializeOptions);
        }
    }
}