# Repository Guidelines

## Project Structure & Module Organization
This repository is an ASP.NET Core Blazor Server app (`net8.0`). Keep feature logic close to existing folders:
- `Pages/`: routeable UI pages (e.g., `Home.razor`, `Cases.razor`, `CaseDetail.razor`)
- `Layout/`: shared layout and navigation components
- `Controllers/`: HTTP APIs (auth and VMOM endpoints)
- `Hubs/`: SignalR hubs for realtime updates (`/hubs/case-status`)
- `Services/`: business logic, data access wiring, Semantic Kernel setup
- `Models/`: domain entities and DTOs
- `wwwroot/`: static assets
- `tests/FusimAiAssiant.Tests/`: xUnit tests

## Build, Test, and Development Commands
Run from repository root:
- `dotnet restore`: install NuGet dependencies
- `dotnet build`: compile app and tests
- `dotnet run`: start the Blazor server app (uses `Properties/launchSettings.json` profiles)
- `dotnet watch`: local dev loop with hot reload
- `dotnet test`: run xUnit tests in `tests/FusimAiAssiant.Tests`

## Coding Style & Naming Conventions
Follow C# defaults with 4-space indentation and nullable reference types enabled.
- Use `PascalCase` for public types/methods/properties
- Use `camelCase` for local variables/parameters
- Keep interfaces prefixed with `I` (e.g., `IVmomCaseService`)
- Name Razor components by page intent (`Submit.razor`, `Login.razor`)
- Keep CSS as component-scoped `*.razor.css` where possible

## Testing Guidelines
Testing uses `xUnit` + `Microsoft.NET.Test.Sdk`.
- Place tests under `tests/FusimAiAssiant.Tests`
- Name test files by subject (`SemanticKernelRegistrationTests.cs`)
- Use method names in `MethodName_ExpectedBehavior` style
- Add tests for DI registration, options validation, and service behavior changes
- Run `dotnet test` before opening a PR

## Commit & Pull Request Guidelines
Git history follows Conventional Commit style with scopes, for example:
- `feat(sk): add Semantic Kernel OpenAI foundation`
- `fix(signalr,ui): re-subscribe SignalR groups on reconnect`

For pull requests:
- Keep commits focused and messages scoped
- Describe user-visible impact and technical approach
- Link related issue(s)
- Include test evidence (`dotnet test` output)
- Add screenshots/GIFs for UI changes in `Pages/` or `Layout/`

## Security & Configuration Tips
Do not commit real API keys. Configure `SemanticKernel:OpenAI:ApiKey` via environment secrets for local/dev environments. Keep `Storage:DataDirectory` writable and outside system-protected paths when deploying.

## Write new code
If you are adding new features or making changes, please ensure that you:
- Follow the existing code style and structure.
- Add appropriate tests for your changes.
- Use context7 to search for documentation.