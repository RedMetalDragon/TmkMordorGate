using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmkMordorGate.Models;

namespace TmkMordorGate.DbContext;

public class AuthTableConfiguration : IEntityTypeConfiguration<Auth>
{
    public void Configure(EntityTypeBuilder<Auth> builder)
    {
        // Specify schema and table name
        builder.ToTable("Auth", "TimeKeeper");

        // Primary key
        builder.HasKey(a => a.AuthID);
        builder.Property(a => a.AuthID)
            .HasColumnName("AuthID")
            .IsRequired()
            .ValueGeneratedOnAdd(); // Auto-increment

        builder.Property(a => a.Email)
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.EmployeeID)
            .HasColumnName("EmployeeID")
            .IsRequired();

        builder.Property(a => a.Salt)
            .HasColumnName("Salt")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(a => a.PasswordResetToken)
            .HasColumnName("PasswordResetToken")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(a => a.PasswordResetTokenExpiration)
            .HasColumnName("PasswordResetTokenExpiration")
            .IsRequired(false);

        builder.Property(a => a.KeepLoggedIn)
            .HasColumnName("KeepLoggedIn")
            .HasColumnType("binary(1)")
            .IsRequired(false);

        builder.Property(a => a.Status)
            .HasColumnName("Status")
            .IsRequired()
            .HasMaxLength(50);

        // Unique Constraint
        builder.HasIndex(a => a.EmployeeID)
            .IsUnique()
            .HasDatabaseName("UNIQUE_EmployeeID");


        /*NOTE*******
             Relationship removed to keep it simple so far
             since we only need the Authentication table for now.
             We can add it back in later if needed or when we have
             Authorization implemented.
         */

        #region Relationships

        // // Foreign Key Relationship
        // builder.HasOne(a => a.Employee)
        //     .WithMany() // Assuming no navigation property on Employee
        //     .HasForeignKey(a => a.EmployeeID)
        //     .HasConstraintName("FK_Employee_Auth_EmployeeID");

        #endregion
    }
}