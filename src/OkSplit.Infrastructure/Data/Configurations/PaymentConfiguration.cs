using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkSplit.Domain.Entities;

namespace OkSplit.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.RazorpayOrderId).HasMaxLength(100).IsRequired();
        builder.Property(p => p.RazorpayPaymentId).HasMaxLength(100);
        builder.Property(p => p.RazorpaySignature).HasMaxLength(255);
        builder.Property(p => p.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("INR");
        builder.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("Created");
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(p => p.Settlement)
            .WithOne(s => s.Payment)
            .HasForeignKey<Payment>(p => p.SettlementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.SettlementId);
    }
}
