using Microsoft.AspNetCore.Mvc;
using TheModernStoic.Application.DTOs;
using TheModernStoic.Domain.Interfaces;

namespace TheModernStoic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JournalController : ControllerBase
{
    private readonly IJournalService _journalService;

    public JournalController(IJournalService journalService)
    {
        _journalService = journalService;
    }

    [HttpPost] // POST api/journal
    public async Task<IActionResult> Create([FromBody] CreateJournalDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Text)) 
            return BadRequest("Entry cannot be empty");

        var response = await _journalService.ProcessJournalEntryAsync(request.Text);
        return Ok(response);
    }

    [HttpGet] // GET api/journal
    public async Task<IActionResult> GetHistory()
    {
        var history = await _journalService.GetHistoryAsync();
        return Ok(history);
    }
}