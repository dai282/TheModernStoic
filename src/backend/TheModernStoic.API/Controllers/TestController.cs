using Microsoft.AspNetCore.Mvc;
using TheModernStoic.Domain.Interfaces;

namespace TheModernStoic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IVectorSearchService _searchService;

    // Constructor Injection handles the dependencies
    public TestController(IVectorSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var results = await _searchService.SearchAsync(q);
        return Ok(results);
    }
}