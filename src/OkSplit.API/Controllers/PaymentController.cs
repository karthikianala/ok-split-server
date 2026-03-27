using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkSplit.Application.Interfaces;
using OkSplit.Domain.Enums;
using OkSplit.Domain.Interfaces;

namespace OkSplit.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentController(IPaymentGatewayService paymentGateway, IUnitOfWork unitOfWork)
    {
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var settlement = await _unitOfWork.Settlements.GetByIdAsync(request.SettlementId);
        if (settlement == null)
            throw new KeyNotFoundException("Settlement not found.");

        if (settlement.PaidBy != GetUserId())
            throw new UnauthorizedAccessException("Only the debtor can create a payment order.");

        if (settlement.Status != SettlementStatus.Pending)
            throw new ArgumentException("Settlement is not in pending state.");

        var (orderId, amount, currency, key) = await _paymentGateway.CreateOrderAsync(request.Amount);

        var payment = new Domain.Entities.Payment
        {
            SettlementId = request.SettlementId,
            RazorpayOrderId = orderId,
            Amount = amount,
            Currency = currency
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { razorpayOrderId = orderId, amount, currency, key });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest request)
    {
        var isValid = _paymentGateway.VerifyPayment(
            request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

        if (!isValid)
            return BadRequest(new { success = false, message = "Payment verification failed." });

        var payment = await _unitOfWork.Payments.GetByRazorpayOrderIdAsync(request.RazorpayOrderId);
        if (payment == null)
            throw new KeyNotFoundException("Payment not found.");

        payment.RazorpayPaymentId = request.RazorpayPaymentId;
        payment.RazorpaySignature = request.RazorpaySignature;
        payment.Status = "Captured";
        payment.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Payments.Update(payment);

        // Mark settlement as completed
        var settlement = payment.Settlement;
        settlement.Status = SettlementStatus.Completed;
        settlement.RazorpayPaymentId = request.RazorpayPaymentId;
        settlement.RazorpayOrderId = request.RazorpayOrderId;
        settlement.SettledAt = DateTime.UtcNow;
        _unitOfWork.Settlements.Update(settlement);

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var (payments, totalCount) = await _unitOfWork.Payments.GetByUserAsync(GetUserId(), page, limit);
        var result = payments.Select(p => new
        {
            p.Id,
            p.RazorpayOrderId,
            p.RazorpayPaymentId,
            p.Amount,
            p.Currency,
            p.Status,
            paidTo = p.Settlement.PaidToUser.FullName,
            p.CreatedAt
        });
        return Ok(new { payments = result, totalCount });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}

public class CreateOrderRequest
{
    public Guid SettlementId { get; set; }
    public decimal Amount { get; set; }
}

public class VerifyPaymentRequest
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}
