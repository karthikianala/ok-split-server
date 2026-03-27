using Microsoft.EntityFrameworkCore;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Interfaces;
using OkSplit.Infrastructure.Data;

namespace OkSplit.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByRazorpayOrderIdAsync(string orderId)
    {
        return await _context.Payments
            .Include(p => p.Settlement)
            .FirstOrDefaultAsync(p => p.RazorpayOrderId == orderId);
    }

    public async Task<(List<Payment> Payments, int TotalCount)> GetByUserAsync(Guid userId, int page, int limit)
    {
        var query = _context.Payments
            .Where(p => p.Settlement.PaidBy == userId)
            .Include(p => p.Settlement)
                .ThenInclude(s => s.PaidToUser)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var payments = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return (payments, totalCount);
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public void Update(Payment payment)
    {
        _context.Payments.Update(payment);
    }
}
