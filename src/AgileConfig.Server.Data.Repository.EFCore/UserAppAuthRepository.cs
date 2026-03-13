using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class UserAppAuthRepository : EFCoreRepository<UserAppAuth, string>, IUserAppAuthRepository
{
    public UserAppAuthRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
