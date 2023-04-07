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
        public InfinityPOSController(IInfinityPOSClient infinityPOSService)
        {
            _infinityPOSService = infinityPOSService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var result = await _infinityPOSService.GetAuthentication();
            return Ok(result);
        }
    }
}