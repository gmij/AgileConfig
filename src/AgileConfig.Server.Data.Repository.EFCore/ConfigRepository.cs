using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class ConfigRepository : EFCoreRepository<Config, string>, IConfigRepository
{
    public ConfigRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
