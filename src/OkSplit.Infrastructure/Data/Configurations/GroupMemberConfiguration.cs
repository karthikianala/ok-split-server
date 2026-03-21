using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkSplit.Domain.Entities;

namespace OkSplit.Infrastructure.Data.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("group_members");

        builder.HasKey(gm => gm.Id);
        builder.Property(gm => gm.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(gm => gm.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Enums.GroupRole.Member);

        builder.Property(gm => gm.JoinedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();

        builder.HasOne(gm => gm.User)
            .WithMany()
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
