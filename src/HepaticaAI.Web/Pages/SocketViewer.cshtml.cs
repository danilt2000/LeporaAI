using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HepaticaAI.Web.Pages;

public class SocketViewer : PageModel
{
    private readonly ILogger<SocketViewer> _logger;

    public SocketViewer(ILogger<SocketViewer> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}

