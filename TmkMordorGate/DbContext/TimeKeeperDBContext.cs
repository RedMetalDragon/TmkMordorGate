using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using TmkMordorGate.Config;
using TmkMordorGate.Models;

namespace TmkMordorGate.DbContext;

public class TimeKeeperDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly DatabaseSettings _settings;

    public TimeKeeperDbContext(DbContextOptions options, DatabaseSettings settings) : base(options)
    {
        _settings = settings;
    }

    public DbSet<Auth> Auths { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuthTableConfiguration());
    }

    public override int SaveChanges()
    {
        throw new InvalidOperationException("Db changes not allowed from TmkMordorGate");
    }
}