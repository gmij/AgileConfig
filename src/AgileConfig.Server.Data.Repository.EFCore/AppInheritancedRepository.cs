using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class AppInheritancedRepository : EFCoreRepository<AppInheritanced, string>, IAppInheritancedRepository
{
    public AppInheritancedRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
