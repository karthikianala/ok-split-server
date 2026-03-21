using OkSplit.Domain.Interfaces;
using OkSplit.Infrastructure.Data;

namespace OkSplit.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IGroupRepository? _groups;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGroupRepository Groups => _groups ??= new GroupRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
