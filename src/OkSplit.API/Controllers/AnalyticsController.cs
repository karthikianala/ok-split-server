using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkSplit.Application.Interfaces;

namespace OkSplit.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly(
        [FromQuery] Guid? groupId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-6);
        var end = endDate ?? DateTime.UtcNow;

        var data = await _analyticsService.GetMonthlyBreakdownAsync(GetUserId(), groupId, start, end);
        return Ok(new { data });
    }

    [HttpGet("category")]
    public async Task<IActionResult> GetCategory(
        [FromQuery] Guid? groupId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var data = await _analyticsService.GetCategorySpendAsync(GetUserId(), groupId, startDate, endDate);
        return Ok(new { data });
    }

    [HttpGet("group-summary/{groupId:guid}")]
    public async Task<IActionResult> GetGroupSummary(Guid groupId)
    {
        var data = await _analyticsService.GetGroupSummaryAsync(GetUserId(), groupId);
        return Ok(data);
    }

    [HttpGet("personal-summary")]
    public async Task<IActionResult> GetPersonalSummary()
    {
        var data = await _analyticsService.GetPersonalSummaryAsync(GetUserId());
        return Ok(data);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}
