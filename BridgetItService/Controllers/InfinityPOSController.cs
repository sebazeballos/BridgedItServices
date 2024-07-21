using BridgetItService.Contracts;
using BridgetItService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BridgetItService.Controllers
{
    [ApiController]
    [Route("infinityPOS")]
    public class InfinityPOSController : ControllerBase
    {
        private readonly IInfinityPOSClient _infinityPOSService;
        private readonly IShopifyServiceAPI _shopifyService;
        private readonly ApiHandler _apiHandler;
        public InfinityPOSController(IInfinityPOSClient infinityPOSService, IShopifyServiceAPI shopifyService, ApiHandler apiHandler)
        {
            _infinityPOSService = infinityPOSService;
            _shopifyService = shopifyService;
            _apiHandler = apiHandler;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var result = await _infinityPOSService.GetAuthentication();
            return Ok(result);
        }

        [HttpPost("Syncronize/{startDate}")]
        public async Task<IActionResult> SyncronizeAsync(string startDate)
        {
                await _apiHandler.SyncronizePlatformsAsync(startDate);
            return Ok($"the date is {startDate}");
        }

        [HttpPost("Order/{incrementalId}")]
        public async Task<IActionResult> SyncronizeOrderAsync(string incrementalId)
        {
            await _apiHandler.PublishOrder(incrementalId);
            return Ok($"the date is {incrementalId}");
        }

        [HttpPost("/AddProductsInShopify")]
        public async Task<IActionResult> PublishProduct(string startDate)
        {
            var products = await _infinityPOSService.GetProducts(startDate);
            return Ok(products);
        }
    }
}