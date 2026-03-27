using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkSplit.Domain.Entities;

namespace OkSplit.Infrastructure.Data.Configurations;

public class ExpenseSplitConfiguration : IEntityTypeConfiguration<ExpenseSplit>
{
    public void Configure(EntityTypeBuilder<ExpenseSplit> builder)
    {
        builder.ToTable("expense_splits");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.OwedAmount).HasPrecision(12, 2).IsRequired();
        builder.Property(s => s.IsSettled).HasDefaultValue(false);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.ExpenseId, s.UserId }).IsUnique();
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ExpenseId);
    }
}
