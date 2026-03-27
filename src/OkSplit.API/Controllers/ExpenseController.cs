using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkSplit.Application.DTOs.Expense;
using OkSplit.Application.Interfaces;

namespace OkSplit.API.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ICloudinaryService _cloudinaryService;

    public ExpenseController(IExpenseService expenseService, ICloudinaryService cloudinaryService)
    {
        _expenseService = expenseService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var result = await _expenseService.CreateAsync(GetUserId(), dto);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] Guid groupId,
        [FromQuery] string? category,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? paidBy,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var (expenses, totalCount) = await _expenseService.GetFilteredAsync(
            GetUserId(), groupId, category, startDate, endDate, paidBy, page, limit);
        return Ok(new { expenses, totalCount });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _expenseService.GetDetailAsync(GetUserId(), id);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateExpenseDto dto)
    {
        var result = await _expenseService.UpdateAsync(GetUserId(), id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _expenseService.DeleteAsync(GetUserId(), id);
        return NoContent();
    }

    [HttpPost("upload-receipt")]
    public async Task<IActionResult> UploadReceipt(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        if (file.Length > 5 * 1024 * 1024) // 5MB limit
            return BadRequest(new { message = "File size must not exceed 5MB." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only JPG, PNG, and WebP images are allowed." });

        using var stream = file.OpenReadStream();
        var url = await _cloudinaryService.UploadImageAsync(stream, file.FileName);
        return Ok(new { url });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}
