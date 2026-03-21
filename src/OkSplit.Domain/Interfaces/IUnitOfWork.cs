namespace OkSplit.Domain.Interfaces;

public interface IUnitOfWork
{
    IGroupRepository Groups { get; }
    Task<int> SaveChangesAsync();
}
