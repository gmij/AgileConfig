namespace AgileConfig.Server.Data.Abstraction.DbProvider;

public class DbConfigInfo : IDbConfigInfo
{
    public DbConfigInfo(string env, string provider, string conn, string ormProvider = "")
    {
        Env = env;
        Provider = provider;
        ConnectionString = conn;
        ORMProvider = string.IsNullOrWhiteSpace(ormProvider) ? "freesql" : ormProvider.ToLower();
    }

    public string Env { get; }

    public string Provider { get; }

    public string ConnectionString { get; }

    public string ORMProvider { get; }
}