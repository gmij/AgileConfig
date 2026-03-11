# AgileConfig 全栈 .NET 架构可行性分析与行动计划

## 1. 现状与通信协议
- **核心协议**：客户端通过 `GET /api/config/app/{appId}?env=xxx` 以 Basic Auth（`appid`+`secret`）拉取配置，返回配置列表并在响应头附带 `publish-time-line-id`（`src/AgileConfig.Server.Apisite/Controllers/api/ConfigController.cs`）。  
- **WebSocket 通道**：`/ws` 路径使用 Basic Auth 校验，必带 `appid`、可选 `env`（默认 `DEV`）、`client_name`、`client_tag`，服务器记录 clientId、心跳时间等（`WebsocketHandlerMiddleware`）。  
- **消息格式**：统一 JSON `{"Module":"c|r","Action":"ping|reload|offline","Data":""}`，枚举定义见 `src/Agile.Config.Protocol/VMS.cs`。  
  - 配置中心客户端心跳 `c:ping` 或 `ping`，服务端回复 `Ping` 动作，其中 Data 为配置 MD5（<=1.7.6）或发布时间线虚拟 ID（>=1.7.7），实现于 `MessageHandler`。  
  - 注册中心客户端心跳 `s:ping:{id}`，回复包含服务列表版本 MD5（`MessageHandler`）。  
  - 服务端主动广播：通过 `RemoteOpController`、`RemoteServerProxyController` 将 `reload/offline` 等动作推送到全部、指定应用或单个客户端（`WebsocketCollection`）。  
- **前端**：React + Ant Design（`src/AgileConfig.Server.UI/react-ui-antd`），调用上述 REST & WebSocket 协议。  
- **后端/数据层**：ASP.NET Core 10，数据访问基于 FreeSql 与仓储选择器（`src/AgileConfig.Server.Data.Freesql`, `...Repository.*`），实体集中于 `src/AgileConfig.Server.Data.Entity`，支持多数据库。

## 2. 迁移目标
- 后端保持 `.NET 10`，数据访问切换为 EF Core Code First，保留现有 REST & WebSocket 协议与业务逻辑。
- 前端替换为 Ant Design Blazor，仍消费当前 API/WebSocket 协议。
- 利用 EF Migrations 获取自动迁移能力，方便多数据库（SQL Server/MySQL/PostgreSQL/SQLite）部署。

## 3. 可行性评估
- **协议兼容性**：通信契约已在独立协议项目中定义，WebSocket 与 REST 路径/头部/消息格式清晰，可在 EF/Blazor 重构中保持不变并通过契约测试保障。  
- **数据模型对齐**：现有实体已具备字段/关系定义，直接可映射到 EF `DbContext`，Code First 迁移可生成等效表结构；需补充并验证多环境数据隔离（env 字段）和发布时间线关联。  
- **业务层**：Service/Repository 接口分层良好（`AgileConfig.Server.IService`），可用 EF 实现替换 FreeSql 仓储，保持接口不变，从而最小化上层变更。  
- **前端迁移**：API 面向资源的 REST + WebSocket 推送，Blazor 前端可直接调用；AntDesign Blazor 组件库覆盖表单/表格/通知等能力，复刻现有 React 交互可行。  
- **运维与部署**：现有 Docker/compose 流程可继续，需在镜像中添加 `dotnet ef database update` 钩子；EF 运行时依赖与 .NET 10 一致，额外风险可控。

## 4. 行动计划（保持协议不变）
1) **契约梳理与验收基线（现做）**
   - 固化 WebSocket/REST 契约文档与示例（沿用 `Agile.Config.Protocol` 类型）；补充契约级自动化测试（心跳、reload/offline 推送、配置拉取响应头）。  
   - 生成当前接口的 OpenAPI/Swashbuckle 快照作为回归基准。  
2) **EF Code First 引入（迭代 1）**
   - 新建 `AgileConfig.Db` 项目：添加 `AgileConfigDbContext`，按 `src/AgileConfig.Server.Data.Entity` 实体配置 Fluent API，补充索引/并发字段；为 SQL Server/MySQL/PostgreSQL/SQLite 分别配置 provider。  
   - 创建首个迁移（Init），种子初始管理员、默认环境、演示应用；提供 SQL 脚本导出用于运维。  
3) **仓储替换与业务验证（迭代 2）**
   - 为现有仓储接口（`AgileConfig.Server.IService`/`...Data.Repository.*`）提供 EF 实现并切换选择器注入；接口签名保持不变。  
   - 回归关键链路：发布配置、继承、注册中心心跳、节点下线广播，确保 `WebsocketAction` 数据兼容。  
4) **前端 Blazor 化（迭代 3）**
   - 新建 AntDesign Blazor 应用（同域部署到 `wwwroot/ui`），覆盖页面：登录、应用/配置管理、发布、节点/服务、操作日志、用户与角色。  
   - 实现等价的 REST 调用与 `/ws` 订阅，处理 `reload/offline/ping`，与现有权限模型对齐。  
   - 渐进切换：保留 React 入口，提供 Blazor Beta 开关，逐步默认至 Blazor。  
5) **发布与运维（迭代 4）**
   - CI/CD 增加 `dotnet ef database update`，前端构建产物打包镜像；健康检查与回滚脚本。  
   - 升级指南：FreeSql -> EF 数据迁移备份/校验步骤，WebSocket/REST 兼容性回归清单。

## 5. 主要风险与缓解
- **数据差异风险**：FreeSql 与 EF 默认命名/类型映射不同，需通过迁移前后对比脚本和回归测试校验；必要时提供数据迁移脚本。  
- **协议回归风险**：WebSocket 心跳/推送格式需保持，使用契约测试和端到端冒烟（拉配置、推 offline/reload）验证。  
- **性能影响**：EF 可能带来查询差异，需对热路径（配置读取、发布）加缓存与跟踪查询计划，并启用连接池/批量操作。  
- **前端切换成本**：Blazor 生态与现有 React 插件差异，需提前选型替代（图表、代码高亮等），并通过分阶段灰度降低切换风险。

## 7. 执行蓝图（可直接落地）
### 数据与 EF
- 引入包：`Microsoft.EntityFrameworkCore`、`...SqlServer`、`...Npgsql`、`...MySql`、`...Sqlite`、`Microsoft.EntityFrameworkCore.Design`。  
- `AgileConfigDbContext`：为每个实体显式配置表名、主键、索引（AppId+Env、PublishTimelineId 等）、并发/时间戳；配置关系（App-Config, Config-PublishTimeline, User-Role, Role-Function）。  
- 迁移与种子：`dotnet ef migrations add Init`，`dotnet ef database update`；种子管理员/admin、默认 env=DEV。  
- 数据迁移校验：生成 FreeSql 现库 vs EF 迁移库的结构 diff；按环境导出/导入脚本。

### 仓储与服务
- 新增 `Repository.EF` 实现，重用 `IUnitOfWork` 契约或替换为 EF `DbContext` 事务；在 `Data.Repository.Selector` 中切换/增加 EF 选项。  
- 关键功能校验：发布与回滚、继承合并、心跳与服务注册、配置推送（WebSocket `reload/offline`）、权限与操作日志。

### 前端 Blazor（AntDesign Blazor）
- 页面映射：登录/主页、应用/配置列表与编辑、发布视图（含时间线）、节点/服务、用户/角色、操作日志、系统设置。  
- API 封装：HttpClient + Typed Clients；WebSocket 封装处理 `ping/reload/offline`；消息提示与表格/表单用 AntDesign Blazor 组件。  
- 构建/部署：`dotnet publish` 产物内嵌到 `wwwroot/ui`，保留 React 入口做灰度开关。

### CI/CD 与环境
- Pipeline 步骤：还原 -> 构建 -> `dotnet ef database update` -> 前端构建 -> 打包镜像 -> 套件测试。  
- 环境变量：保留现有 `db__provider/db__conn`，新增 EF 连接串配置段；WebSocket/REST 端点保持 `/ws` 与 `/api/*`。  
- 监控：保留 OpenTelemetry 配置，增加 EF 查询跟踪开关。

### 测试计划（必须跑的用例）
- **契约回归**：REST（获取配置、发布、回滚、用户/角色 CRUD、注册中心接口）、WebSocket（`ping`/`c:ping`、`s:ping:{id}`、`reload/offline` 推送）。  
- **数据一致性**：EF CRUD 与继承链，发布时间线、MD5/虚拟 ID 计算与头部返回。  
- **权限/鉴权**：App Basic、Admin Basic、角色权限拦截。  
- **前端 E2E（Blazor）**：登录、增删改配置、发布、收到 reload 推送后前端刷新。  
- **性能与并发冒烟**：批量配置发布、并发心跳、节点上下线。

## 6. 结论
在保持现有通信协议的前提下，迁移到全栈 .NET（后端 .NET 10 + EF Code First，前端 AntDesign Blazor）可行。按上述阶段推进可控制风险并逐步替换底层实现，同时保证业务逻辑与客户端协议兼容。
