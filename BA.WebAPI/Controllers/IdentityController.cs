
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BA.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly ILogger<IdentityController> logger;
        private readonly IHttpClientFactory httpFactory;
        public IdentityController(
            ILogger<IdentityController> logger,
            IHttpClientFactory httpFactory)
        {
            this.logger = logger;
            this.httpFactory = httpFactory;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(
                from c in User.Claims select new { c.Type, c.Value }
                );
        }
    }
}
