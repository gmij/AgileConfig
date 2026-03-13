using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class ServerNodeRepository : EFCoreRepository<ServerNode, string>, IServerNodeRepository
{
    public ServerNodeRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
