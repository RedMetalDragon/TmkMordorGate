using Yarp.ReverseProxy.Model;

namespace TmkMordorGate.Config.Interfaces;

public interface IBlackListTokenService
{
    bool IsBlacklisted(string token);
    void RevokeToken(string token);
}