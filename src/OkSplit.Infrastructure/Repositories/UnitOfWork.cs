using OkSplit.Domain.Interfaces;
using OkSplit.Infrastructure.Data;

namespace OkSplit.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IGroupRepository? _groups;
    private IExpenseRepository? _expenses;
    private ISettlementRepository? _settlements;
    private IPaymentRepository? _payments;
    private IActivityLogRepository? _activityLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGroupRepository Groups => _groups ??= new GroupRepository(_context);
    public IExpenseRepository Expenses => _expenses ??= new ExpenseRepository(_context);
    public ISettlementRepository Settlements => _settlements ??= new SettlementRepository(_context);
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
    public IActivityLogRepository ActivityLogs => _activityLogs ??= new ActivityLogRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
