using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmkMordorGate.Models;

namespace TmkMordorGate.DbContext;

public class PermissionTableConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Specify schema and table name
        builder.ToTable("Permission", "TimeKeeper");

        // Primary key
        builder.HasKey(p => p.PermissionID);
        builder.Property(p => p.PermissionID)
            .HasColumnName("PermissionID")
            .IsRequired()
            .ValueGeneratedOnAdd(); // Auto-increment

        // Properties
        builder.Property(p => p.RoleID)
            .HasColumnName("RoleID")
            .IsRequired();

        builder.Property(p => p.FeatureID)
            .HasColumnName("FeatureID")
            .IsRequired();

        // Foreign keys relationship
        builder.HasOne(p => p.Role)
            .WithMany()
            .HasForeignKey(p => p.RoleID)
            .HasConstraintName("FK_Role_Permission_RoleID")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Feature)
            .WithMany()
            .HasForeignKey(p => p.FeatureID)
            .HasConstraintName("FK_Feature_Permission_FeatureID");
    }
}