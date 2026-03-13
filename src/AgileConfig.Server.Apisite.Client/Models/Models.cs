namespace AgileConfig.Server.Apisite.Client.Models;

// ── Common response wrappers ─────────────────────────────────────────────────

public class AgileResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class AgileResponse<T> : AgileResponse
{
    public T? Data { get; set; }
}

public class PagedResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int Current { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<T> Data { get; set; } = new();
}

// ── App models ───────────────────────────────────────────────────────────────

public class AppInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Secret { get; set; }
    public string? Group { get; set; }
    public bool Inheritanced { get; set; }
    public List<string> InheritancedApps { get; set; } = new();
    public List<string> InheritancedAppNames { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public string? Creator { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public List<AppInfo>? Children { get; set; }
}

/// <summary>简化的可继承应用列表项（InheritancedApps 接口返回）</summary>
public class InheritancedAppItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class AppAuthInfo
{
    public string AppId { get; set; } = "";
    /// <summary>有权限的用户 ID 列表</summary>
    public List<string> AuthorizedUsers { get; set; } = new();
}

public class AddEditAppRequest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Secret { get; set; }
    public string? Group { get; set; }
    public bool Inheritanced { get; set; }
    public List<string> InheritancedApps { get; set; } = new();
    public bool Enabled { get; set; } = true;
}

// ── User models ───────────────────────────────────────────────────────────────

public class UserInfo
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? Team { get; set; }
    public List<string> UserRoleIds { get; set; } = new();
    public List<string> UserRoleNames { get; set; } = new();
    /// <summary>0 = Normal, 1 = Deleted</summary>
    public int Status { get; set; }
}

public class AddUserRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Team { get; set; }
    public List<string> UserRoleIds { get; set; } = new();
}

public class EditUserRequest
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? Team { get; set; }
    public List<string> UserRoleIds { get; set; } = new();
}

// ── ServerNode models ─────────────────────────────────────────────────────────

public class ServerNodeInfo
{
    /// <summary>节点地址，同时也是 ID</summary>
    public string Address { get; set; } = "";
    public string? Remark { get; set; }
    /// <summary>0 = Offline, 1 = Online</summary>
    public int Status { get; set; }
    public DateTime? LastEchoTime { get; set; }
}

public class AddNodeRequest
{
    public string Address { get; set; } = "";
    public string? Remark { get; set; }
}

// ── Config models ─────────────────────────────────────────────────────────────

public class ConfigInfo
{
    public string Id { get; set; } = "";
    public string AppId { get; set; } = "";
    public string? Group { get; set; }
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string Env { get; set; } = "DEV";
    /// <summary>0 = Add, 1 = Edit, 2 = Deleted, 10 = Published (Commit)</summary>
    public int EditStatus { get; set; }
    /// <summary>0 = WaitPublish, 1 = Online</summary>
    public int OnlineStatus { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class AddEditConfigRequest
{
    public string Id { get; set; } = "";
    public string AppId { get; set; } = "";
    public string? Group { get; set; }
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? Description { get; set; }
}

public class WaitPublishStatus
{
    public int AddCount { get; set; }
    public int EditCount { get; set; }
    public int DeleteCount { get; set; }
}

public class PublishRequest
{
    public string AppId { get; set; } = "";
    public string[]? Ids { get; set; }
    public string? Log { get; set; }
}

public class PublishTimelineNode
{
    public string Id { get; set; } = "";
    public DateTime? PublishTime { get; set; }
    public string? PublishUserName { get; set; }
    public string? Log { get; set; }
}

public class PublishHistoryEntry
{
    public int Key { get; set; }
    public PublishTimelineNode? TimelineNode { get; set; }
    public List<ConfigInfo> List { get; set; } = new();
}

// ── Service models ────────────────────────────────────────────────────────────

public class ServiceInfo
{
    public string Id { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string? Ip { get; set; }
    public int? Port { get; set; }
    public string? MetaData { get; set; }
    /// <summary>0 = Healthy, 1 = Unhealthy</summary>
    public int Status { get; set; }
    public DateTime? RegisterTime { get; set; }
    public DateTime? LastHeartBeat { get; set; }
    public string? HeartBeatMode { get; set; }
    public string? CheckUrl { get; set; }
    public string? AlarmUrl { get; set; }
}

// ── SysLog models ─────────────────────────────────────────────────────────────

public class SysLogInfo
{
    public string Id { get; set; } = "";
    public string? AppId { get; set; }
    public int LogType { get; set; }
    public string? LogText { get; set; }
    public DateTime? LogTime { get; set; }
}

// ── Client (connected app clients) models ────────────────────────────────────

public class ClientInfo
{
    public string Id { get; set; } = "";
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? AppId { get; set; }
    public string? Ip { get; set; }
    public string? Tag { get; set; }
    public DateTime? LastHeartbeatTime { get; set; }
    public string Status { get; set; } = "Online";
    public List<string>? Env { get; set; }
}

// ── Dashboard / Report models ─────────────────────────────────────────────────

public class DashboardStats
{
    public int AppCount { get; set; }
    public int ConfigCount { get; set; }
    public int NodeCount { get; set; }
    public int ServiceCount { get; set; }
    public int ServiceOnlineCount { get; set; }
}

// ── Login ─────────────────────────────────────────────────────────────────────

public class LoginRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResult
{
    public string? Status { get; set; }
    public string? Token { get; set; }
    public string? Type { get; set; }
}
