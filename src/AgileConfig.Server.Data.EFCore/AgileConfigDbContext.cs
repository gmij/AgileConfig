using Microsoft.EntityFrameworkCore;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.EFCore;

public class AgileConfigDbContext : DbContext
{
    public AgileConfigDbContext(DbContextOptions<AgileConfigDbContext> options) : base(options)
    {
    }

    public DbSet<App> Apps { get; set; } = null!;
    public DbSet<AppInheritanced> AppInheritanceds { get; set; } = null!;
    public DbSet<Config> Configs { get; set; } = null!;
    public DbSet<ConfigPublished> ConfigPublisheds { get; set; } = null!;
    public DbSet<Function> Functions { get; set; } = null!;
    public DbSet<PublishDetail> PublishDetails { get; set; } = null!;
    public DbSet<PublishTimeline> PublishTimelines { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<RoleFunction> RoleFunctions { get; set; } = null!;
    public DbSet<ServerNode> ServerNodes { get; set; } = null!;
    public DbSet<ServiceInfo> ServiceInfos { get; set; } = null!;
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<SysLog> SysLogs { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserAppAuth> UserAppAuths { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // App
        modelBuilder.Entity<App>(entity =>
        {
            entity.ToTable("agc_app");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Secret).HasMaxLength(36);
            entity.Property(e => e.Group).HasMaxLength(50);
            entity.Property(e => e.Creator).HasMaxLength(36);
            entity.HasIndex(e => e.Name);
        });

        // AppInheritanced
        modelBuilder.Entity<AppInheritanced>(entity =>
        {
            entity.ToTable("agc_appInheritanced");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.AppId).HasMaxLength(36).HasColumnName("appid");
            entity.Property(e => e.InheritancedAppId).HasMaxLength(36).HasColumnName("inheritanced_appid");
            entity.HasIndex(e => e.AppId);
            entity.HasIndex(e => e.InheritancedAppId);
        });

        // Config
        modelBuilder.Entity<Config>(entity =>
        {
            entity.ToTable("agc_config");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.AppId).HasMaxLength(36);
            entity.Property(e => e.Group).HasMaxLength(100).HasColumnName("g");
            entity.Property(e => e.Key).HasMaxLength(100).HasColumnName("k");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Env).HasMaxLength(50);
            entity.HasIndex(e => new { e.AppId, e.Env });
            entity.HasIndex(e => new { e.AppId, e.Key, e.Env });
        });

        // ConfigPublished
        modelBuilder.Entity<ConfigPublished>(entity =>
        {
            entity.ToTable("agc_config_published");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.AppId).HasMaxLength(36);
            entity.Property(e => e.Group).HasMaxLength(100).HasColumnName("g");
            entity.Property(e => e.Key).HasMaxLength(100).HasColumnName("k");
            entity.Property(e => e.Env).HasMaxLength(50);
            entity.Property(e => e.ConfigId).HasMaxLength(36);
            entity.Property(e => e.PublishTimelineId).HasMaxLength(36);
            entity.HasIndex(e => new { e.AppId, e.Env });
            entity.HasIndex(e => e.PublishTimelineId);
        });

        // Function
        modelBuilder.Entity<Function>(entity =>
        {
            entity.ToTable("agc_function");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
        });

        // PublishDetail
        modelBuilder.Entity<PublishDetail>(entity =>
        {
            entity.ToTable("agc_publish_detail");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.PublishTimelineId).HasMaxLength(36);
            entity.Property(e => e.ConfigId).HasMaxLength(36);
            entity.Property(e => e.Env).HasMaxLength(50);
            entity.HasIndex(e => e.PublishTimelineId);
        });

        // PublishTimeline
        modelBuilder.Entity<PublishTimeline>(entity =>
        {
            entity.ToTable("agc_publish_timeline");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.AppId).HasMaxLength(36);
            entity.Property(e => e.PublishUserId).HasMaxLength(36);
            entity.Property(e => e.PublishUserName).HasMaxLength(50);
            entity.Property(e => e.Log).HasMaxLength(100);
            entity.Property(e => e.Env).HasMaxLength(50);
            entity.HasIndex(e => new { e.AppId, e.Env });
            entity.HasIndex(e => e.PublishTime);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("agc_role");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
        });

        // RoleFunction
        modelBuilder.Entity<RoleFunction>(entity =>
        {
            entity.ToTable("agc_role_function");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.RoleId).HasMaxLength(64);
            entity.Property(e => e.FunctionId).HasMaxLength(64);
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.FunctionId);
        });

        // ServerNode
        modelBuilder.Entity<ServerNode>(entity =>
        {
            entity.ToTable("agc_server_node");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Remark).HasMaxLength(100);
            entity.HasIndex(e => e.Status);
        });

        // ServiceInfo
        modelBuilder.Entity<ServiceInfo>(entity =>
        {
            entity.ToTable("agc_service_info");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.ServiceId).HasMaxLength(100);
            entity.Property(e => e.ServiceName).HasMaxLength(100);
            entity.Property(e => e.Ip).HasMaxLength(100);
            entity.Property(e => e.MetaData).HasMaxLength(2000);
            entity.Property(e => e.HeartBeatMode).HasMaxLength(10);
            entity.Property(e => e.CheckUrl).HasMaxLength(2000);
            entity.Property(e => e.AlarmUrl).HasMaxLength(2000);
            entity.HasIndex(e => new { e.ServiceId, e.ServiceName });
        });

        // Setting
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("agc_setting");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
        });

        // SysLog
        modelBuilder.Entity<SysLog>(entity =>
        {
            entity.ToTable("agc_sys_log");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.AppId).HasMaxLength(36);
            entity.Property(e => e.LogType).HasMaxLength(256);
            entity.HasIndex(e => e.LogTime);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("agc_user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Salt).HasMaxLength(36);
            entity.Property(e => e.Team).HasMaxLength(50);
            entity.HasIndex(e => e.UserName).IsUnique();
        });

        // UserAppAuth
        modelBuilder.Entity<UserAppAuth>(entity =>
        {
            entity.ToTable("agc_user_app_auth");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.AppId).HasMaxLength(36);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.HasIndex(e => new { e.AppId, e.UserId });
        });

        // UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("agc_user_role");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.RoleId).HasMaxLength(64);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RoleId);
        });

        // Seed initial data
        AgileConfigDbSeedData.SeedData(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Configure string default max length
        configurationBuilder.Properties<string>().HaveMaxLength(500);
    }
}
