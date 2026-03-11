using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class RoleFunctionRepository : EFCoreRepository<RoleFunction, string>, IRoleFunctionRepository
{
    public RoleFunctionRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
