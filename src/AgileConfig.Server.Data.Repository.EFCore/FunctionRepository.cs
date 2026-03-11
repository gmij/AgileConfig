using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class FunctionRepository : EFCoreRepository<Function, string>, IFunctionRepository
{
    public FunctionRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
