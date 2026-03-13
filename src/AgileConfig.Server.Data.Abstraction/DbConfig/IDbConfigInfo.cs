namespace AgileConfig.Server.Data.Abstraction.DbProvider;

public interface IDbConfigInfo
{
    string ConnectionString { get; }
    string Env { get; }

    string Provider { get; }

    /// <summary>
    /// ORM provider: "freesql" (default), "efcore", or "mongodb"
    /// </summary>
    string ORMProvider { get; }
}