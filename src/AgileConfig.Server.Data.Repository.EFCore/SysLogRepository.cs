using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class SysLogRepository : EFCoreRepository<SysLog, string>, ISysLogRepository
{
    public SysLogRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
