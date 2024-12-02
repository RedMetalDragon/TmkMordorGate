using Microsoft.EntityFrameworkCore;
using TmkMordorGate.DbContext;
using TmkMordorGate.Models;
using TmkMordorGate.Repositories.Interfaces;

namespace TmkMordorGate.Repositories;

public class TmkAuthenticationRepository : IAuthenticationRepository
{
    private readonly TimeKeeperDbContext _dbContext;

    public TmkAuthenticationRepository(TimeKeeperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Auth?> GetUser(string emailAddress)
    {
        try
        {
            var usr = await _dbContext.Auths.FirstOrDefaultAsync(x => x != null && x.Email == emailAddress);
            return usr;
        }
        finally
        {
            await _dbContext.DisposeAsync();
        }
    }
}