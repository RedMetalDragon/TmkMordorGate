using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmkMordorGate.Models;

namespace TmkMordorGate.DbContext;

public class PlanTableConfiguration: IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        // Specify schema and table name
        builder.ToTable("Plan", "TimeKeeper");

        // Primary key
        builder.HasKey(p => p.PlanID);
        builder.Property(p => p.PlanID)
            .HasColumnName("PlanID")
            .IsRequired()
            .ValueGeneratedOnAdd(); // Auto-increment

        // Properties
        builder.Property(p => p.PlanName)
            .HasColumnName("PlanName")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Description)
            .HasColumnName("Description")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("binary(1)");

        builder.Property(p => p.Price)
            .HasColumnName("Price")
            .HasColumnType("decimal(10, 2)");

        builder.Property(p => p.CreationDate)
            .HasColumnName("CreationDate")
            .HasColumnType("datetime");
    }
}

