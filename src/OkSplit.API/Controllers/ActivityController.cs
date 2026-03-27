using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkSplit.Domain.Interfaces;

namespace OkSplit.API.Controllers;

[ApiController]
[Route("api/activity")]
[Authorize]
public class ActivityController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivityController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivity(
        [FromQuery] Guid? groupId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 30)
    {
        var userId = GetUserId();

        if (groupId.HasValue)
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId.Value);
            if (group == null) return NotFound();
            if (!group.Members.Any(m => m.UserId == userId))
                return Forbid();

            var (logs, totalCount) = await _unitOfWork.ActivityLogs.GetByGroupAsync(groupId.Value, page, limit);
            return Ok(new
            {
                activities = logs.Select(a => new
                {
                    a.Id, a.Action, a.EntityType, a.EntityId,
                    a.Description, a.Metadata, a.CreatedAt,
                    userName = a.User.FullName
                }),
                totalCount
            });
        }
        else
        {
            var (logs, totalCount) = await _unitOfWork.ActivityLogs.GetByUserGroupsAsync(userId, page, limit);
            return Ok(new
            {
                activities = logs.Select(a => new
                {
                    a.Id, a.Action, a.EntityType, a.EntityId,
                    a.Description, a.Metadata, a.CreatedAt,
                    userName = a.User.FullName,
                    groupName = a.Group.Name
                }),
                totalCount
            });
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}
