using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OkSplit.Domain.Entities;

namespace OkSplit.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
