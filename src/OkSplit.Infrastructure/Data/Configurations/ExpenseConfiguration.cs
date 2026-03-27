using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkSplit.Domain.Entities;

namespace OkSplit.Infrastructure.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Description).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("INR");
        builder.Property(e => e.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.SplitType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.ReceiptUrl).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.Group)
            .WithMany()
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PaidByUser)
            .WithMany()
            .HasForeignKey(e => e.PaidBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Splits)
            .WithOne(s => s.Expense)
            .HasForeignKey(s => s.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.GroupId);
        builder.HasIndex(e => e.PaidBy);
    }
}
