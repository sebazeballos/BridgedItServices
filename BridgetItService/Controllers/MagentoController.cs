using BridgetItService.Contracts;
using BridgetItService.Models;
using Microsoft.AspNetCore.Mvc;
using ShopifySharp;

namespace BridgetItService.Controllers
{
    [Route("api/magento")]
    [ApiController]
    public class MagentoController : ControllerBase
    {
        private readonly IMagentoService _magentoService;
        public MagentoController(IMagentoService magentoService)
        {
            _magentoService = magentoService;
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
