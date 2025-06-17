using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmkMordorGate.Models;

namespace TmkMordorGate.DbContext;

public class FeatureTableConfiguration: IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        // Specify schema and table name
        builder.ToTable("Feature", "TimeKeeper");

        // Primary key
        builder.HasKey(f => f.FeatureID);
        builder.Property(f => f.FeatureID)
            .HasColumnName("FeatureID")
            .IsRequired()
            .ValueGeneratedOnAdd(); // Auto-increment

        // Properties
        builder.Property(f => f.FeatureName)
            .HasColumnName("FeatureName")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.Description)
            .HasColumnName("Description")
            .HasMaxLength(255);

        builder.Property(f => f.PlanID)
            .HasColumnName("PlanID")
            .IsRequired();

        builder.Property(f => f.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("binary(1)")
            .IsRequired();

        // Foreign key relationship
        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(f => f.PlanID)
            .HasConstraintName("FK_Plan_Feature_PlanID");
    }
}