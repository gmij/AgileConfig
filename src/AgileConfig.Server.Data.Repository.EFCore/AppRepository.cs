using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class AppRepository : EFCoreRepository<App, string>, IAppRepository
{
    public AppRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
