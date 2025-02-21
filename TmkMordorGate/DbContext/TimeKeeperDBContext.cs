using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using TmkMordorGate.Config;
using TmkMordorGate.Config.Interfaces;
using TmkMordorGate.Models;

namespace TmkMordorGate.DbContext;

public class TimeKeeperDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly IDatabaseSettings _settings;

    public TimeKeeperDbContext(DbContextOptions options, IDatabaseSettings settings) : base(options)
    {
        _settings = settings;
    }

    public DbSet<Auth> Auths { get; set; }
    
    public DbSet<Permission> Permissions { get; set; }
    
    public DbSet<Role> Roles { get; set; }
    
    public DbSet<Feature> Features { get; set; }
    
    public DbSet<Plan> Plans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuthTableConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionTableConfiguration());
        modelBuilder.ApplyConfiguration(new RoleTableConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureTableConfiguration());
        modelBuilder.ApplyConfiguration(new PlanTableConfiguration());
    }

    public override int SaveChanges()
    {
        throw new InvalidOperationException("Db changes not allowed from TmkMordorGate");
    }
}