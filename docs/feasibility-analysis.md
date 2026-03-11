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
1) **契约梳理与验收基线**
   - 固化 WebSocket/REST 契约文档与示例（沿用 `Agile.Config.Protocol` 类型）；补充契约级自动化测试（心跳、reload/offline 推送、配置拉取响应头）。  
2) **EF Code First 引入**
   - 建立新的 `AgileConfig.Db` 项目：添加 `AgileConfigDbContext`，按 `Data.Entity` 实体配置 Fluent API，补充索引/并发字段；为多库配置独立的 provider（SQL Server/MySQL/PostgreSQL/SQLite）。  
   - 用第一次迁移生成数据库（迁移脚本 + 种子数据：初始管理员、默认环境配置）。  
3) **仓储替换与业务验证**
   - 为现有仓储接口编写 EF 实现并切换选择器；保持接口签名不变以保证 API/WebSocket 行为一致。  
   - 针对关键业务（发布配置、继承、注册中心心跳、节点下线广播）编写/运行集成测试，确保 `WebsocketAction` 数据保持兼容。  
4) **前端 Blazor 化**
   - 新建 AntDesign Blazor 前端项目（同域托管于 `wwwroot/ui`），对齐现有页面：登录、应用/配置管理、发布、节点/服务视图、操作日志。  
   - 实现与旧前端等价的 API 调用与 WebSocket 订阅（`/ws`），复用现有权限/角色模型。  
   - 渐进切换：并行保留旧 React 入口，内部灰度验证后再默认到 Blazor。  
5) **发布与运维**
   - 更新 CI/CD 脚本：`dotnet ef database update`、前端构建产物打包到镜像；补充健康检查与回滚脚本。  
   - 编写升级指引：从 FreeSql 数据库到 EF 迁移的备份/验证步骤，WebSocket/REST 兼容性回归清单。

## 5. 主要风险与缓解
- **数据差异风险**：FreeSql 与 EF 默认命名/类型映射不同，需通过迁移前后对比脚本和回归测试校验；必要时提供数据迁移脚本。  
- **协议回归风险**：WebSocket 心跳/推送格式需保持，使用契约测试和端到端冒烟（拉配置、推 offline/reload）验证。  
- **性能影响**：EF 可能带来查询差异，需对热路径（配置读取、发布）加缓存与跟踪查询计划，并启用连接池/批量操作。  
- **前端切换成本**：Blazor 生态与现有 React 插件差异，需提前选型替代（图表、代码高亮等），并通过分阶段灰度降低切换风险。

## 6. 结论
在保持现有通信协议的前提下，迁移到全栈 .NET（后端 .NET 10 + EF Code First，前端 AntDesign Blazor）可行。按上述阶段推进可控制风险并逐步替换底层实现，同时保证业务逻辑与客户端协议兼容。
