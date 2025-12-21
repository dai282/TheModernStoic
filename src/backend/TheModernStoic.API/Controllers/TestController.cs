using Microsoft.AspNetCore.Mvc;
using TheModernStoic.Application.DTOs;
using TheModernStoic.Domain.Interfaces;
using TheModernStoic.Infrastructure.Services;

namespace TheModernStoic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IVectorSearchService _searchService;
    private readonly IJournalService _journalService;

    // Constructor Injection handles the dependencies
    public TestController(IVectorSearchService searchService, IJournalService journalService)
    {
        _searchService = searchService;
        _journalService = journalService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var results = await _searchService.SearchAsync(q);
        return Ok(results);
    }

    [HttpPost("advise")]
    public async Task<IActionResult> GetResponse([FromBody] CreateJournalDto entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Text))
            return BadRequest("Journal entry cannot be empty.");

        var response = await _journalService.ProcessJournalEntryAsync(entry.Text);
        
        return Ok(response);
    }

}