using FreeSql;
using FusimAiAssiant.Hubs;
using FusimAiAssiant.Services;

var builder = WebApplication.CreateBuilder(args);
var dataDirectory = ResolveDataDirectory(
    builder.Environment.ContentRootPath,
    builder.Configuration["Storage:DataDirectory"]);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSemanticKernelFoundation(builder.Configuration);
builder.Services.AddSingleton(new DataStoragePath(dataDirectory));

builder.Services.AddSingleton<IFreeSql>(sp =>
{
    var storagePath = sp.GetRequiredService<DataStoragePath>();
    var dbPath = Path.Combine(storagePath.Root, "vmom2.db");
    Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

    return new FreeSqlBuilder()
        .UseConnectionString(DataType.Sqlite, $"Data Source={dbPath}")
        .UseAutoSyncStructure(true)
        .Build();
});

builder.Services.AddSingleton<VmomInputCatalogService>();
builder.Services.AddSingleton<VmomNamelistBuilder>();
builder.Services.AddSingleton<IVmomCaseService, VmomCaseService>();
builder.Services.AddHostedService<CaseStatusBroadcastService>();
builder.Services.AddScoped<ClientSessionService>();

builder.Services.AddScoped(sp =>
{
    var navigation = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapHub<CaseStatusHub>("/hubs/case-status");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

static string ResolveDataDirectory(string contentRootPath, string? configuredPath)
{
    var path = string.IsNullOrWhiteSpace(configuredPath) ? "data" : configuredPath.Trim();
    if (Path.IsPathRooted(path))
    {
        return path;
    }

    return Path.GetFullPath(Path.Combine(contentRootPath, path));
}
