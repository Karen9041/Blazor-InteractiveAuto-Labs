using Microsoft.AspNetCore.Mvc;


namespace TestPrototype.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("apple-callback")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AppleCallback([FromForm] string code)
        {
           return Redirect($"/AppleAuthSuccess?code={code}");
        }
    }
}
