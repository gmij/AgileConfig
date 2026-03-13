using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class PublishDetailRepository : EFCoreRepository<PublishDetail, string>, IPublishDetailRepository
{
    public PublishDetailRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
