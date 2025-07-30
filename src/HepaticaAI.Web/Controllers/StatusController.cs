using Microsoft.AspNetCore.Mvc;

namespace HepaticaAI.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new { status = "Running", timestamp = DateTime.UtcNow });
        }
    }
}
