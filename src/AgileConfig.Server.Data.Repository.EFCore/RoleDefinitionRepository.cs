using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class RoleDefinitionRepository : EFCoreRepository<Role, string>, IRoleDefinitionRepository
{
    public RoleDefinitionRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
