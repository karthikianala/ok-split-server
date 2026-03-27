namespace OkSplit.Domain.Interfaces;

public interface IUnitOfWork
{
    IGroupRepository Groups { get; }
    IExpenseRepository Expenses { get; }
    ISettlementRepository Settlements { get; }
    IPaymentRepository Payments { get; }
    IActivityLogRepository ActivityLogs { get; }
    Task<int> SaveChangesAsync();
}
