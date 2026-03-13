using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Data.Abstraction.DbProvider;

public class DbConfigInfoFactory : IDbConfigInfoFactory
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, IDbConfigInfo> _envProviders = new();

    public DbConfigInfoFactory(IConfiguration configuration)
    {
        var providerPath = "db:provider";
        var connPath = "db:conn";
        var ormProviderPath = "db:ormProvider";

        var providerValue = configuration[providerPath];
        var connValue = configuration[connPath];
        var ormProviderValue = configuration[ormProviderPath];

        if (string.IsNullOrEmpty(providerValue)) throw new ArgumentNullException(providerPath);
        if (string.IsNullOrEmpty(connValue)) throw new ArgumentNullException(connPath);

        var configInfo = new DbConfigInfo("", providerValue, connValue, ormProviderValue);
        _default = configInfo;
        _envProviders.TryAdd(providerPath, configInfo);
        _configuration = configuration;
    }

    private IDbConfigInfo _default { get; }

    public IDbConfigInfo GetConfigInfo(string env = "")
    {
        if (string.IsNullOrEmpty(env)) return _default;

        var providerPath = "";
        var connPath = "";
        var ormProviderPath = "";
        providerPath = $"db:env:{env}:provider";
        connPath = $"db:env:{env}:conn";
        ormProviderPath = $"db:env:{env}:ormProvider";

        _envProviders.TryGetValue(providerPath, out var configInfo);

        if (configInfo != null) return configInfo;

        var providerValue = _configuration[providerPath];
        var connValue = _configuration[connPath];
        var ormProviderValue = _configuration[ormProviderPath];

        if (string.IsNullOrEmpty(providerValue)) return _default;
        if (string.IsNullOrEmpty(connValue)) return _default;

        configInfo = new DbConfigInfo(env, providerValue, connValue, ormProviderValue);
        _envProviders.TryAdd(providerPath, configInfo);

        return configInfo;
    }
}