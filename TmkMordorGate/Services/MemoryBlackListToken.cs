using TmkMordorGate.Config.Interfaces;

namespace TmkMordorGate.Services;

public class MemoryBlackListToken : IBlackListTokenService
{
    //TODO: Make this class thread safe
    //private static readonly object _lock = new();
    //private readonly HashSet<string> _blackListedTokens = new();
    
    private readonly HashSet<string> _blackListedTokens = [];

    public bool IsBlacklisted(string token)
    {
        return _blackListedTokens.Contains(token);
    }

    public void RevokeToken(string token)
    {
        _blackListedTokens.Add(token);
    }
}