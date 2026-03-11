using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class PublishTimelineRepository : EFCoreRepository<PublishTimeline, string>, IPublishTimelineRepository
{
    public PublishTimelineRepository(AgileConfigDbContext context) : base(context)
    {
    }
}
