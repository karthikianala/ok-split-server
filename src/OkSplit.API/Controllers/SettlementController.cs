using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkSplit.Application.DTOs.Settlement;
using OkSplit.Application.Interfaces;

namespace OkSplit.API.Controllers;

[ApiController]
[Route("api/settlements")]
[Authorize]
public class SettlementController : ControllerBase
{
    private readonly ISettlementService _settlementService;

    public SettlementController(ISettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSettlementDto dto)
    {
        var result = await _settlementService.CreateAsync(GetUserId(), dto);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetByGroup(
        [FromQuery] Guid groupId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var (settlements, totalCount) = await _settlementService.GetByGroupAsync(
            GetUserId(), groupId, status, page, limit);
        return Ok(new { settlements, totalCount });
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _settlementService.ConfirmAsync(GetUserId(), id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _settlementService.RejectAsync(GetUserId(), id);
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingActions()
    {
        var pending = await _settlementService.GetPendingActionsAsync(GetUserId());
        return Ok(new { pending, count = pending.Count });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}
