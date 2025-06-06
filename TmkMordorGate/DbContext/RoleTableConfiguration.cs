using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmkMordorGate.Models;

namespace TmkMordorGate.DbContext;

public class RoleTableConfiguration: IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Specify schema and table name
        builder.ToTable("Role", "TimeKeeper");

        // Primary key
        builder.HasKey(r => r.RoleID);
        builder.Property(r => r.RoleID)
            .HasColumnName("RoleID")
            .IsRequired()
            .ValueGeneratedOnAdd(); // Auto-increment

        // Properties
        builder.Property(r => r.RoleName)
            .HasColumnName("RoleName")
            .IsRequired()
            .HasMaxLength(225);

        builder.Property(r => r.RoleDescription)
            .HasColumnName("RoleDescription")
            .HasMaxLength(255);
    }
}