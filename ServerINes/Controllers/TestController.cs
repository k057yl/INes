using INest.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TestController(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                lang = Thread.CurrentThread.CurrentCulture.Name,
                msg = _localizer["RequiredAuthFields"]
            });
        }
    }
}
