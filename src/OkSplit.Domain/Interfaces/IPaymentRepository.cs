using OkSplit.Domain.Entities;

namespace OkSplit.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByRazorpayOrderIdAsync(string orderId);
    Task<(List<Payment> Payments, int TotalCount)> GetByUserAsync(Guid userId, int page, int limit);
    Task AddAsync(Payment payment);
    void Update(Payment payment);
}
