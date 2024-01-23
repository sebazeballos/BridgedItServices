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
using System.Collections.Generic;
using Amazon.Runtime.Internal;

namespace BridgetItService.Services
{
    public class MagentoService : IMagentoService
    {
        private readonly IOptions<MagentoSettings> _options;
        private readonly IMap<InfinityPosProducts, MagentoProducts> _infinityToMagentoProductMap;
        private readonly IMap<MagentoProduct, PutMagentoProduct> _magentoPostToPut;
        private readonly IInfinityPOSClient _infinityPOSClient;
        private readonly IMap<MagentoOrder, Invoices> _magentoTransactionsMap;
        private readonly IMap<MagentoRefund, Invoices> _magentoRefundMap;
        private readonly HttpClient _client;
        private readonly string MEDIA_TYPE = "application/json";
        private readonly ILogger<MagentoService> _logger;
        public MagentoService(IServiceProvider serviceProvider, IInfinityPOSClient infinityPOSClient, ILogger<MagentoService> logger)
        {   
            _options = serviceProvider.GetService<IOptions<MagentoSettings>>();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));

            _infinityToMagentoProductMap = serviceProvider.GetService<IMap<InfinityPosProducts, MagentoProducts>>();
            _magentoTransactionsMap = serviceProvider.GetService<IMap<MagentoOrder, Invoices>>();
            _magentoRefundMap = serviceProvider.GetService<IMap<MagentoRefund, Invoices>>();
            _magentoPostToPut = serviceProvider.GetService<IMap<MagentoProduct, PutMagentoProduct>>();

            _infinityPOSClient = infinityPOSClient;
            _logger = logger;
        }
        public async Task PublishProducts(InfinityPosProducts products)
        {
            if (products != null)
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
                MagentoProducts magentoProducts = _infinityToMagentoProductMap.Map(products);
                if (magentoProducts.Product.Count > 0)
                {
                    await SendProducts(magentoProducts.Product);
                }
                if (magentoProducts.DeletedProduct.Count > 0)
                {
                    await DeleteProducts(magentoProducts.DeletedProduct);
                }
            }
        }

        public async Task<InfinityPosProducts> GetProductsInInfinity(InfinityPosProducts infinityProducts)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
            var page = 1;

            MagentoProducts products = new MagentoProducts();
              products.Product = new List<MagentoProduct>();
            MagentoProducts productsInMagento = new MagentoProducts();
            InfinityPosProducts infinityPosProducts = new InfinityPosProducts();
            infinityPosProducts.Products = new List<InfinityPOSProduct>();
            //MagentoProducts infinityMProducts = _infinityToMagentoProductMap.Map(infinityProducts);
            var bodyError = "";
            while (page <= 513)
            {
                var parameters = $"?searchCriteria[currentPage]={page}&searchCriteria[pageSize]=20";
                try
                {
                    var response = await _client.GetAsync(_options.Value.BaseUrl + _options.Value.CreateProduct + parameters);
                    if (!response.IsSuccessStatusCode)
                    {
                        bodyError = await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var productsResponse = Deserialize<MagentoGetResponse>(content);
                    foreach( var product in productsResponse.Items)
                    {
                        products.Product.Add(product);
                    }
                    page++;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError("Exception = " + ex.StatusCode.ToString()
                                + $" Using Endpoint Method Get {_options.Value.BaseUrl + _options.Value.Orders + parameters} Message = {bodyError}");
                }
            }

            infinityPosProducts.Products = (IList<InfinityPOSProduct>)infinityProducts.Products
            .Where(p => products.Product.Any(iProducts => iProducts.Sku == p.ProductCode))
            .ToList();

            return infinityPosProducts;
        }

        public async Task<MagentoProducts> GetProductsNotFoundInInfinity(InfinityPosProducts infinityProducts)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
            var page = 1;
            MagentoProducts products = new MagentoProducts();
            products.Product = new List<MagentoProduct>();
            MagentoProducts productsInMagento = new MagentoProducts();
            MagentoProducts infinityMProducts = _infinityToMagentoProductMap.Map(infinityProducts);
            var bodyError = "";
            while (page <= 501)
            {
                var parameters = $"?searchCriteria[currentPage]={page}&searchCriteria[pageSize]=20";
                try
                {
                    var response = await _client.GetAsync(_options.Value.BaseUrl + _options.Value.CreateProduct + parameters);
                    if (!response.IsSuccessStatusCode)
                    {
                        bodyError = await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var productsResponse = Deserialize<MagentoGetResponse>(content);
                    foreach (var product in productsResponse.Items)
                    {
                        products.Product.Add(product);
                    }
                    page++;
                }
                catch(HttpRequestException ex) {

                    _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Method Get {_options.Value.BaseUrl + _options.Value.Orders + parameters} Message = {bodyError}");

                }
            }
            productsInMagento.Product = products.Product
            .Where(p => !infinityMProducts.DeletedProduct.Any(iProducts => iProducts == p.Sku))
            .ToList();
            return products;
        }
        private async Task SendProduct(MagentoProduct product)
        {
            var bodyError = "";
            MagentoRequest request = new MagentoRequest();
            request.Product = product;
            var body = SerializeBody(request);
            try
            {
                HttpResponseMessage response;
                if (product.Visibility == null)
                {
                    await PutProduct(product);
                }
                else
                {
                    response = await _client.PostAsync($"{_options.Value.BaseUrl + _options.Value.CreateProduct}", body);
                    bodyError = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    _logger.LogInformation("Product Published" + " With Body = " + TransformToRawString(product)
                        + $" Using Endpoint {_options.Value.BaseUrl + _options.Value.CreateProduct} Message = {bodyError}");
                }



            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Exception = " + ex.StatusCode.ToString() + " With Body = " + TransformToRawString(product)
                        + $" Using Endpoint {_options.Value.BaseUrl + _options.Value.CreateProduct} Message = {bodyError}");
                
            }
        }

        private async Task SendProducts(IList<MagentoProduct> products)
        {
            var sent = 0;
            foreach (MagentoProduct product in products)
            {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
                    var bodyError = "";
                    MagentoRequest request = new MagentoRequest();
                    request.Product = product;
                    var body = SerializeBody(request);
                    try
                    {
                        HttpResponseMessage response;
                        await PutProduct(product);

                        sent++;

                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.StatusCode == HttpStatusCode.BadRequest)
                        {
                            if (product.Status == 2)
                            {
                            }
                            else
                            {
                                await PutProduct(product);
                            }
                        }
                        else
                        {
                            _logger.LogError("Exception = " + ex.StatusCode.ToString() + " With Body = " + TransformToRawString(product)
                                + $" Using Endpoint {_options.Value.BaseUrl + _options.Value.CreateProduct} Message = {bodyError}");
                        }
                    }
            }
        }

        private async Task PutProduct(MagentoProduct product)
        {
            PutProductRequest putMagentoProduct = new PutProductRequest();
            putMagentoProduct.Product = _magentoPostToPut.Map(product);
            var putBody = SerializeBody(putMagentoProduct);
            var bodyError = "";
            try
            {
                var response = await _client.PutAsync($"{_options.Value.BaseUrl + _options.Value.CreateProduct}/" + putMagentoProduct.Product.Sku, putBody);
                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }
                bodyError = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Product Updated" + " With Body = " + TransformToRawString(putMagentoProduct.Product)
                   + $" Using Endpoint {_options.Value.BaseUrl + _options.Value.CreateProduct + "/" + putMagentoProduct.Product.Sku} Message = {bodyError}");
            }
            catch (HttpRequestException ex2)
            {
                if (ex2.StatusCode == HttpStatusCode.BadRequest)
                {
                    if (product.Status != 2  )
                    {
                        product.Visibility = 1;
                        await SendProduct(product);
                    }

                }
                else {
                    _logger.LogError("Exception = " + ex2.StatusCode.ToString() + " With Body = " + TransformToRawString(putMagentoProduct.Product)
                    + $" Using Endpoint {_options.Value.BaseUrl + _options.Value.CreateProduct + "/" + putMagentoProduct.Product.Sku} Message = {bodyError}");
                }
                
            }
        }

        private async Task DeleteProduct(string sku)
        {
            var bodyError = "";
                try
                {
                    var response = await _client.DeleteAsync($"{_options.Value.BaseUrl + _options.Value.CreateProduct}/{sku}");
                    if (!response.IsSuccessStatusCode)
                    {
                        bodyError = await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError("Exception = " + e.StatusCode.ToString() +
                        $" Trying to Delete using endpoint = {_options.Value.BaseUrl + _options.Value.CreateProduct}/{sku} Message = {bodyError}");
                }
        }

        private async Task DeleteProducts(IList<string> skuList)
        {
            var bodyError = "";
            foreach (string sku in skuList)
            {
                try
                {
                    var response = await _client.DeleteAsync($"{_options.Value.BaseUrl + _options.Value.CreateProduct}/{sku}");
                    if (!response.IsSuccessStatusCode)
                    {
                        bodyError = await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError("Exception = " + e.StatusCode.ToString() + 
                        $" Trying to Delete using endpoint = { _options.Value.BaseUrl + _options.Value.CreateProduct}/{sku} Message = {bodyError}");
                }
            }
        }

        public async Task GetOrder()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
            var bodyError = "";
            try
            {
                var response = await _client.GetAsync($"{_options.Value.BaseUrl + "/rest/V1/orders/000006326"}");
                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var magentoOrder = Deserialize<MagentoOrder>(content);
                Invoices invoices = _magentoTransactionsMap.Map(magentoOrder);
                if (invoices.Invoice.Count > 0)
                {
                    foreach (Invoice invoice in invoices.Invoice)
                    {
                        await _infinityPOSClient.PostTransaction(invoice);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Orders Method Get {_options.Value.BaseUrl + _options.Value.Orders } Message = {bodyError}");
            }
        }

        public async Task GetOrders(string startDate)
        {
            var endDate = DateTime.Now.AddHours(-13).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var starttDate = DateTime.Now.AddHours(-13).Subtract(TimeSpan.FromMinutes(15)).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var parameters = "?searchCriteria[filter_groups][0][filters][0][field]=status&searchCriteria[filter_groups][0][filters][0][value]=complete" +
                $"&searchCriteria[filter_groups][1][filters][0][field]=updated_at&searchCriteria[filter_groups][1][filters][0][value]={startDate}" +
                "&searchCriteria[filter_groups][1][filters][0][condition_type]=gteq&searchCriteria[filter_groups][2][filters][0][field]=updated_at" +
                $"&searchCriteria[filter_groups][2][filters][0][value]={endDate}&searchCriteria[filter_groups][2][filters][0][condition_type]=lteq&searchCriteria[sortOrders][0][direction]=ASC";


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
            var bodyError = "";
            try
            {
                var response = await _client.GetAsync($"{_options.Value.BaseUrl + "/rest/V1/orders" + parameters}");
                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }
                 response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var magentoOrder = Deserialize<MagentoOrder>(content);
                Invoices invoices = _magentoTransactionsMap.Map(magentoOrder);

                if (invoices.Invoice.Count > 0)
                {
                    foreach (Invoice invoice in invoices.Invoice)
                    {
                        await _infinityPOSClient.PostTransaction(invoice);
                    }
                }
            }
            catch (HttpRequestException ex) {
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Orders Method Get {_options.Value.BaseUrl + _options.Value.Orders + parameters} Message = {bodyError}");
            }
        }
        public async Task GetRefunds(string startDate)
        {
            var endDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var starttDate = DateTime.Now.AddHours(-13).Subtract(TimeSpan.FromMinutes(15)).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var parameters = "?searchCriteria[filter_groups][0][filters][0][field]=status&searchCriteria[filter_groups][0][filters][0][value]=closed" +
                "&searchCriteria[filter_groups][0][filters][0][condition_type]=eqS&searchCriteria[filter_groups][1][filters][0][field]=updated_at&searchCriteria[filter_groups]" +
                $"[1][filters][0][value]={startDate}&searchCriteria[filter_groups][1][filters][0][condition_type]=gteq&searchCriteria[filter_groups][2][filters][0][field]=updated_at" +
                $"&searchCriteria[filter_groups][2][filters][0][value]={endDate}&searchCriteria[filter_groups][2][filters][0][condition_type]=lteq";
             _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuth());
            var bodyError = "";
            try
            {
                var response = await _client.GetAsync($"{_options.Value.BaseUrl + _options.Value.Orders + parameters}");
                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var magentoRefund = Deserialize<MagentoRefund>(content);
                Invoices invoices = _magentoRefundMap.Map(magentoRefund);
                if (invoices.Invoice.Count > 0)
                {
                    foreach (Invoice invoice in invoices.Invoice)
                    {
                        await _infinityPOSClient.PostTransaction(invoice);
                    }
                }
            }
            catch (HttpRequestException ex) {
                _logger.LogError("Exception = " + ex.StatusCode.ToString()
                            + $" Using Endpoint Refunds Method Get {_options.Value.BaseUrl + _options.Value.Orders + parameters} Message = {bodyError}");
            }
        }
        public async Task<string?> GetAuth()
        {
            var auth = new MagentoAuth
            {
                Username = _options.Value.User,
                Password = _options.Value.Password
            };
            var credentials = SerializeBody(auth);
            var bodyError = "";
            try
            {
                var response = await _client.PostAsync($"{_options.Value.BaseUrl + _options.Value.AuthUrl}", credentials);
                
                if (!response.IsSuccessStatusCode)
                {
                    bodyError = await response.Content.ReadAsStringAsync();
                }
                response.EnsureSuccessStatusCode();
                return Deserialize<string>(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex) {
                _logger.LogError("Exception = " + ex.StatusCode.ToString() + " With Body = " + TransformToRawString(auth) + $" Using Endpoint {_options.Value.BaseUrl + _options.Value.AuthUrl} Message = {bodyError}");
                return null;
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
            return new StringContent(stringPayload, Encoding.UTF8, MEDIA_TYPE);
        }

        private string TransformToRawString(object body)
        {

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                WriteIndented = true
            };
            return JsonSerializer.Serialize(body, serializeOptions).ToString();
        }

        private T Deserialize<T>(string jsonElement) where T : class =>
            JsonSerializer.Deserialize<T>(jsonElement, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = new SnakeCaseNamingPolicy()
            });
    }
}
