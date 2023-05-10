using BridgetItService.Contracts;
using BridgetItService.Models;
using Microsoft.AspNetCore.Mvc;
using ShopifySharp;

namespace BridgetItService.Controllers
{
    [Route("api/shopify")]
    [ApiController]
    public class ShopifyController : ControllerBase
    {
        private readonly IShopifyServiceAPI _shopifyService;
        public ShopifyController(IShopifyServiceAPI shopifyService)
        {
            _shopifyService = shopifyService;
        }
        //[HttpPost("/products")]
        //public async Task<IActionResult> PostProducts(InfinityPosProducts products)
        //{
        //    return Ok(await _shopifyService.PublishProducts(products.Products);
        //}

        private IActionResult Ok(object v)
        {
            throw new NotImplementedException();
        }
    }
}
