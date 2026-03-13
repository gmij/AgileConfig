namespace AgileConfig.Server.UI.Blazor.Models;

public class AppModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Secret { get; set; } = "";
    public string? Group { get; set; }
    public bool Inheritanced { get; set; }
    public List<string> InheritancedApps { get; set; } = new();
    public List<string> InheritancedAppNames { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public DateTime? CreateTime { get; set; }
    public string? Creator { get; set; }
}

public class ConfigModel
{
    public string Id { get; set; } = "";
    public string AppId { get; set; } = "";
    public string? Group { get; set; }
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? Description { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public int EditStatus { get; set; } // 0=New, 1=Modified, 2=Deleted, 10=Published
    public int OnlineStatus { get; set; } // 0=WaitPublish, 1=Online
    public string Env { get; set; } = "DEV";
}

public class UserModel
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? Password { get; set; }
    public string? Email { get; set; }
    public bool Enabled { get; set; } = true;
    public List<string> Roles { get; set; } = new();
    public DateTime? CreateTime { get; set; }
}

public class UserAuthModel
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Permission { get; set; } = "R"; // R=Read, RW=ReadWrite
}

public class ServiceInfoModel
{
    public string Id { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Ip { get; set; } = "";
    public int Port { get; set; }
    public List<string>? MetaData { get; set; }
    public DateTime? RegisterTime { get; set; }
    public DateTime? HeartbeatTime { get; set; }
    public string Status { get; set; } = ""; // Healthy, Unhealthy
}

public class ServerNodeModel
{
    public string Id { get; set; } = "";
    public string Address { get; set; } = "";
    public string? Remark { get; set; }
    public int Status { get; set; } // 0=Disabled, 1=Enabled
    public DateTime? CreateTime { get; set; }
    public DateTime? LastHeartbeatTime { get; set; }
}

public class SysLogModel
{
    public string Id { get; set; } = "";
    public string AppId { get; set; } = "";
    public string? LogType { get; set; }
    public string? LogTime { get; set; }
    public string? LogText { get; set; }
}

public class RoleModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<string>? Functions { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class PagedResult<T>
{
    public List<T>? Data { get; set; }
    public int Current { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

public class WaitPublishStatus
{
    public int AddCount { get; set; }
    public int EditCount { get; set; }
    public int DeleteCount { get; set; }
}

public class DashboardStatistics
{
    public int AppCount { get; set; }
    public int ConfigCount { get; set; }
    public int NodeCount { get; set; }
    public int NodeOnlineCount { get; set; }
    public int ClientCount { get; set; }
}

public class ClientInfoModel
{
    public string Id { get; set; } = "";
    public string AppId { get; set; } = "";
    public string? Name { get; set; }
    public string? Ip { get; set; }
    public string? Tag { get; set; }
    public DateTime? LastHeartbeatTime { get; set; }
    public string Status { get; set; } = "Online";
}

public class PublishTimelineNode
{
    public string Id { get; set; } = "";
    public DateTime? PublishTime { get; set; }
    public string? PublishUserName { get; set; }
    public string? Log { get; set; }
}

public class PublishDetailNode
{
    public PublishTimelineNode TimelineNode { get; set; } = new();
    public List<ConfigModel> Configs { get; set; } = new();
}

public class QueryModel<T>
{
    public SortModel[]? SortModel { get; set; }
}

public class SortModel
{
    public string FieldName { get; set; } = "";
    public string Sort { get; set; } = ""; // ascend, descend
}
