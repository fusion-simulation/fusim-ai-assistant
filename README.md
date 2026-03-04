# Fusim AI Assistant
[![Build Status](https://app.travis-ci.com/fusion-simulation/fusim-ai-assistant.svg?branch=main)](https://app.travis-ci.com/fusion-simulation/fusim-ai-assistant)

一个基于 **ASP.NET Core 8 + Blazor Server** 的 `vmom2` Web 管理界面。

项目提供从网页提交 `eqinpt` 输入、后台执行 `vmom2`、实时查看算例状态、查看结果文件以及打包下载结果的完整流程。

## 功能概览

- 登录（当前为测试账号）
  - 固定账号：`guest`
  - 固定密码：`guest`
- 新建算例
  - 方式 1：直接提交 `input.in` 文本
  - 方式 2：通过表单填写字段，自动生成 `eqinpt` namelist
- 后台执行
  - 为每个算例创建独立工作目录
  - 异步执行 `vmom2`
  - 超时控制（5 分钟）与失败状态回写
- 实时状态推送
  - 使用 SignalR 每秒广播概况和算例列表变化
- 结果查看与下载
  - 在线查看 `RZ.txt`、`eqpr_iota.txt`、`vmom.out`
  - 下载算例目录 zip 包

## 技术栈

- .NET 8 (`net8.0`)
- ASP.NET Core + Blazor Server
- FreeSql + SQLite
- SignalR

## 运行前准备

1. 安装 .NET 8 SDK
2. 确保系统中可直接调用 `vmom2` 命令（在 `PATH` 中）
3. 确保系统已经安装了 `gnuplot`
如果 `vmom2` 不可用，算例会进入 `failed` 状态并记录错误信息。

## 快速启动

在项目根目录执行：

```bash
dotnet restore
dotnet run
```


## 配置说明

配置文件：`appsettings-sample.json`

需要复制一份并命名为 `appsettings.json`，然后根据需要修改配置项：
