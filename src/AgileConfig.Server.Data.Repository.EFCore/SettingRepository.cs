using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class SettingRepository : EFCoreRepository<Setting, string>, ISettingRepository
{
    public SettingRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
