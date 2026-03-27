using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Enums;

namespace OkSplit.Infrastructure.Data.Configurations;

public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        builder.ToTable("settlements");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(SettlementStatus.Pending);
        builder.Property(s => s.PaymentMethod).HasMaxLength(30).IsRequired();
        builder.Property(s => s.RazorpayPaymentId).HasMaxLength(100);
        builder.Property(s => s.RazorpayOrderId).HasMaxLength(100);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(s => s.Group)
            .WithMany()
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PaidByUser)
            .WithMany()
            .HasForeignKey(s => s.PaidBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PaidToUser)
            .WithMany()
            .HasForeignKey(s => s.PaidTo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.GroupId);
        builder.HasIndex(s => s.PaidBy);
        builder.HasIndex(s => s.PaidTo);
    }
}
