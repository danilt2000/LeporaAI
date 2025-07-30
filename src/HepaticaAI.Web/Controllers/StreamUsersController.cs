using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HepaticaAI.Web.Controllers
{
    [ApiController]
    [Route("api/")]
    public class StreamUsersController : ControllerBase
    {
        public record ToolCall(JsonElement? parameters);

        // POST api/GetStreamUsers
        [HttpPost("GetStreamUsers")]
        public IActionResult GetStreamUsers()
        {
            var users = new List<string>
            {
                "finn_gal",
                "MUSHDOG987",
                "nozhaluctacei4ac",
            };

            return Ok(new
            {
                tool_name = "GetStreamUsers",
                result = new { users }
            });
        }

        // POST api/BanUser
        [HttpPost("BanUser")]
        public IActionResult BanUser([FromBody] ToolCall? body)
        {
            return Ok(new
            {
                tool_name = "BanUser",

                result = string.Empty
            });
        }
    }
}
