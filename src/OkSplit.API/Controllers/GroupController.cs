using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkSplit.Application.DTOs.Group;
using OkSplit.Application.Interfaces;

namespace OkSplit.API.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IExpenseService _expenseService;

    public GroupController(IGroupService groupService, IExpenseService expenseService)
    {
        _groupService = groupService;
        _expenseService = expenseService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
    {
        var result = await _groupService.CreateAsync(GetUserId(), dto);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserGroups([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var (groups, totalCount) = await _groupService.GetUserGroupsAsync(GetUserId(), page, limit);
        return Ok(new { groups, totalCount, page, limit });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _groupService.GetDetailAsync(GetUserId(), id);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupDto dto)
    {
        var result = await _groupService.UpdateAsync(GetUserId(), id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _groupService.DeleteAsync(GetUserId(), id);
        return NoContent();
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto dto)
    {
        var result = await _groupService.AddMemberAsync(GetUserId(), id, dto);
        return StatusCode(201, result);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        await _groupService.RemoveMemberAsync(GetUserId(), id, userId);
        return NoContent();
    }

    [HttpPut("{id:guid}/members/{userId:guid}/role")]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid userId, [FromBody] UpdateMemberRoleDto dto)
    {
        var result = await _groupService.UpdateMemberRoleAsync(GetUserId(), id, userId, dto);
        return Ok(result);
    }

    [HttpGet("{id:guid}/balances")]
    public async Task<IActionResult> GetBalances(Guid id)
    {
        var balances = await _expenseService.GetGroupBalancesAsync(GetUserId(), id);
        return Ok(new { balances });
    }

    [HttpGet("{id:guid}/simplified-debts")]
    public async Task<IActionResult> GetSimplifiedDebts(Guid id)
    {
        var debts = await _expenseService.GetSimplifiedDebtsAsync(GetUserId(), id);
        return Ok(new { debts });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}
