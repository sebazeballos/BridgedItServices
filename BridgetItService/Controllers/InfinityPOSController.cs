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
        public InfinityPOSController(IInfinityPOSClient infinityPOSService, IShopifyServiceAPI shopifyService)
        {
            _infinityPOSService = infinityPOSService;
            _shopifyService = shopifyService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var result = await _infinityPOSService.GetAuthentication();
            return Ok(result);
        }

        [HttpGet("/products")]
        public async Task<IActionResult> GetAsync(string startDate)
        {
            var result = await _infinityPOSService.GetProducts(startDate);
            return Ok(result);
        }
        [HttpPost("/AddProductsInShopify")]
        public async Task<IActionResult> PublishProduct(string startDate)
        {
            var products = await _infinityPOSService.GetProducts(startDate);
            return Ok(products);
        }
    }
}