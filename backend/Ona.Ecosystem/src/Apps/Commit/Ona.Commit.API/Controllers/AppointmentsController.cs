using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ona.Commit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok();
        }
    }
}
