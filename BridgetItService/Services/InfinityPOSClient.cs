using BridgetItService.Contracts;
using BridgetItService.Models;
using BridgetItService.Settings;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Communication;
using ShopifySharp;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public async Task<string> GetProducts(string startDate)
        {
            try {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthentication());
                var response = await _client.GetAsync($"{_options.Value.BaseEndpoint}/products?updated_since={startDate}");

                response.EnsureSuccessStatusCode();

                var products = await response.Content.ReadAsStringAsync();
                return products;
            }
            catch (HttpRequestException ex)
            {

                throw new ServiceException($"REQUEST {HttpMethods.Post} to {_options.Value.AuthorizationEndpoint} FAILD with body:", ex.ToString());
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