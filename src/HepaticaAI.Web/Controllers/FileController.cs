using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

namespace HepaticaAI.Web.Controllers
{
    [ApiController]
    [Route("api/")]
    public class ToolsController : ControllerBase
    {
        public record ToolCall(JsonElement? parameters);

        // POST api/searchfiles
        [HttpPost("Searchfiles")]
        public IActionResult SearchFiles()
        {
            var files = new List<string>
            {
                "CV.pdf",
                "Report.pdf",
                "ProjectOverview.pdf"
            };
            return Ok(files);
        }

        // POST api/tools/getfileinfo
        [HttpPost("Getfileinfo")]
        public IActionResult GetFileInfo([FromBody] ToolCall request)
        {
            if (request.parameters is null
                || !request.parameters.Value.TryGetProperty("FileName", out var fn)
                || fn.ValueKind != JsonValueKind.String)
            {
                return BadRequest(new { error = "Missing or invalid FileName parameter" });
            }

            var fileName = fn.GetString()!;
            var text = $"[Тест] Содержимое файла «{fileName}»";
            return Ok(new { fileText = text });
        }
    }
}