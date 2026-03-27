# Fusim AI Assistant
[![CI](https://github.com/fusion-simulation/fusim-ai-assistant/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/fusion-simulation/fusim-ai-assistant/actions/workflows/ci.yml)

一个基于 **ASP.NET Core 8 + Blazor Server** 的 `vmom2` Web 管理界面，集成了算例提交、后台执行、结果查看、实时状态推送，以及基于 **Semantic Kernel** 的 AI 调参与结果分析能力。

[demo](https://vmom2.fusim.cn)

[视频](https://gal.eziosweet.cn/download/1/temp/vmom2解说视频.mp4)

## 功能概览

- 登录认证
  - 使用 Cookie 认证
  - 当前内置测试账号：`guest / guest`
- 首页总览
  - 查看算例统计信息与最近算例
  - 通过 SignalR 实时刷新状态
- 新建算例
  - 方式 1：直接提交 `input.in` / `eqinpt` 文本
  - 方式 2：通过表单填写字段，自动生成 `eqinpt`
  - 集成 **AI 调参助手**，可根据当前输入给出参数修改建议并直接回写
- 算例列表
  - 查看全部算例状态
  - 实时更新列表变化
- 算例详情
  - 在线查看 `RZ.txt`、`eqpr_iota.txt`、`vmom.out`
  - 下载当前算例 zip 包
  - 集成 **AI 算例助手**，可分析结果、解释异常、生成绘图请求
  - 支持图像预览
- 界面体验
  - 支持浅色 / 深色主题切换

## 技术栈

- .NET 8 / ASP.NET Core / Blazor Server
- ASP.NET Core Controllers
- SignalR
- FreeSql + SQLite
- Microsoft Semantic Kernel
- OpenAI-compatible Chat Completion API
- Markdig（Markdown 渲染）

## 项目结构

```text
.
├── Components/                  # 可复用组件（含 AI 聊天侧边栏）
├── Controllers/                 # HTTP API（认证、VMOM）
├── Hubs/                        # SignalR Hub
├── Layout/                      # 布局与导航
├── Models/                      # 领域模型与 DTO
├── Pages/                       # Blazor 页面
├── Services/                    # 算例执行、AI、状态广播、主题等服务
├── tests/FusimAiAssiant.Tests/  # xUnit 测试
├── wwwroot/                     # 静态资源
├── Program.cs                   # 应用启动与依赖注入
└── appsettings-sample.json      # 配置示例
```

## 运行前准备

1. 安装 **.NET 8 SDK**
2. 确保系统中可直接调用 `vmom2` 命令（已加入 `PATH`）
3. 确保系统已安装 `gnuplot`
4. 准备一个 **OpenAI 兼容接口**，供 Semantic Kernel 调用

说明：

- 如果 `vmom2` 不可用，算例会执行失败并记录错误信息。
- `gnuplot` 用于 AI 助手生成绘图结果时的图片输出。

## 配置说明

以 `appsettings-sample.json` 为模板创建本地配置：

```bash
cp appsettings-sample.json appsettings.json
```

示例配置：

```json
{
  "SemanticKernel": {
    "OpenAI": {
      "BaseUrl": "https://api.openai.com/v1",
      "ModelId": "gpt-5.3-instant",
      "ApiKey": "YOUR_OWN_API_KEY"
    }
  },
  "Storage": {
    "DataDirectory": "./data"
  }
}
```

关键配置项：

- `SemanticKernel:OpenAI:BaseUrl`：OpenAI 兼容接口地址
- `SemanticKernel:OpenAI:ModelId`：聊天模型名称
- `SemanticKernel:OpenAI:ApiKey`：API Key
- `Storage:DataDirectory`：数据目录

`Storage:DataDirectory` 下会保存：

- SQLite 数据库文件
- 每个算例的独立工作目录
- 结果文件与生成图片

应用启动时会校验 Semantic Kernel 配置是否完整。

也可以使用环境变量覆盖配置，例如：

- `SemanticKernel__OpenAI__BaseUrl`
- `SemanticKernel__OpenAI__ModelId`
- `SemanticKernel__OpenAI__ApiKey`
- `Storage__DataDirectory`

## 快速启动

在项目根目录执行：

```bash
dotnet restore
dotnet build
dotnet test
dotnet run
```

开发时可使用：

```bash
dotnet watch
```

默认开发地址见 `Properties/launchSettings.json`：

- HTTP: `http://localhost:5037`
- HTTPS: `https://localhost:7143`

## 常用命令

```bash
dotnet restore   # 安装依赖
dotnet build     # 编译项目
dotnet run       # 启动应用
dotnet watch     # 热重载开发
dotnet test      # 运行测试
```

## HTTP API 与实时通道

主要接口：

- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/vmom/catalog`
- `GET /api/vmom/overview`
- `GET /api/vmom/cases`
- `GET /api/vmom/cases/{caseId}`
- `POST /api/vmom/cases`
- `POST /api/vmom/cases/from-form`
- `GET /api/vmom/cases/{caseId}/download`
- `POST /api/vmom/cases/{caseId}/agent/chat`
- `POST /api/vmom/submit-agent/chat`
- `GET /api/vmom/cases/{caseId}/plots/{imageFileName}`

SignalR Hub：

- `/hubs/case-status`

## 测试

测试位于：`tests/FusimAiAssiant.Tests`

当前覆盖内容包括：

- Semantic Kernel 配置注册
- 登录认证控制器
- AI 调参与算例分析服务
- 输入草稿与页面逻辑
- 主题与客户端会话状态
- 聊天视图与图片预览相关逻辑

运行测试：

```bash
dotnet test
```

## 安全说明

- 当前 `guest / guest` 仅用于本地测试，不适合生产环境。
- 不要提交真实 API Key。
- 建议通过环境变量或本地机密管理注入 `SemanticKernel:OpenAI:ApiKey`。
- `Storage:DataDirectory` 需要可写，并建议放在系统保护目录之外。
