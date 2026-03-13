using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;
using Microsoft.EntityFrameworkCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class PublishTimelineRepository : EFCoreRepository<PublishTimeline, string>, IPublishTimelineRepository
{
    public PublishTimelineRepository(AgileConfigDbContext context) : base(context)
    {
    }

    public async Task<string> GetLastPublishTimelineNodeIdAsync(string appId, string env)
    {
        var timeline = await _context.PublishTimelines
            .Where(x => x.AppId == appId && x.Env == env)
            .OrderByDescending(x => x.PublishTime)
            .FirstOrDefaultAsync();
        return timeline?.Id ?? "";
    }
}
