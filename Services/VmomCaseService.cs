using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using FreeSql;
using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public class VmomCaseService : IVmomCaseService
{
    private const int FixedUserId = 1;
    private static readonly string[] RequiredOutputFiles = ["RZ.txt", "eqpr_iota.txt", "vmom.out"];

    private readonly IFreeSql _fsql;
    private readonly DataStoragePath _storagePath;
    private readonly VmomNamelistBuilder _namelistBuilder;
    private readonly ILogger<VmomCaseService> _logger;

    public VmomCaseService(
        IFreeSql fsql,
        DataStoragePath storagePath,
        VmomNamelistBuilder namelistBuilder,
        ILogger<VmomCaseService> logger)
    {
        _fsql = fsql;
        _storagePath = storagePath;
        _namelistBuilder = namelistBuilder;
        _logger = logger;
    }

    public async Task<int> CreateCaseAsync(string title, string inputContent, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entity = new VmomCase
        {
            UserId = FixedUserId,
            Title = string.IsNullOrWhiteSpace(title) ? "vmom2-case" : title.Trim(),
            Status = "queued",
            InputContent = inputContent,
            WorkDirectory = string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };

        var caseId = Convert.ToInt32(await _fsql.Insert<VmomCase>().AppendData(entity).ExecuteIdentityAsync());
        var caseDirectory = BuildCaseDirectory(caseId);

        Directory.CreateDirectory(caseDirectory);
        await File.WriteAllTextAsync(Path.Combine(caseDirectory, "input.in"), inputContent, cancellationToken);

        await _fsql.Update<VmomCase>()
            .Set(c => c.WorkDirectory, caseDirectory)
            .Set(c => c.Status, "running")
            .Set(c => c.UpdatedAt, DateTime.UtcNow)
            .Where(c => c.Id == caseId && c.UserId == FixedUserId)
            .ExecuteAffrowsAsync(cancellationToken);

        _ = Task.Run(() => RunVmom2Async(caseId, caseDirectory));

        return caseId;
    }

    public Task<int> CreateCaseFromFormAsync(string title, IReadOnlyDictionary<string, string> fields, CancellationToken cancellationToken = default)
    {
        var inputContent = _namelistBuilder.BuildEqinptNamelist(fields);
        return CreateCaseAsync(title, inputContent, cancellationToken);
    }

    public async Task<IReadOnlyList<CaseListItem>> ListCasesAsync(CancellationToken cancellationToken = default)
    {
        return await ListCaseItemsAsync(cancellationToken);
    }

    public async Task<CaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var cases = await ListCaseItemsAsync(cancellationToken);
        return BuildOverview(cases);
    }

    public async Task<(CaseOverviewResponse Overview, IReadOnlyList<CaseListItem> Cases)> GetBroadcastPayloadAsync(CancellationToken cancellationToken = default)
    {
        var cases = await ListCaseItemsAsync(cancellationToken);
        var overview = BuildOverview(cases);
        return (overview, cases);
    }

    public async Task<VmomCaseDetail?> GetCaseDetailAsync(int caseId, CancellationToken cancellationToken = default)
    {
        var entity = await _fsql.Select<VmomCase>()
            .Where(c => c.Id == caseId && c.UserId == FixedUserId)
            .FirstAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var rzPath = Path.Combine(entity.WorkDirectory, "RZ.txt");
        var eqprIotaPath = Path.Combine(entity.WorkDirectory, "eqpr_iota.txt");
        var vmomOutPath = Path.Combine(entity.WorkDirectory, "vmom.out");

        var rzText = ReadTextOrEmpty(rzPath);
        var eqprIotaText = ReadTextOrEmpty(eqprIotaPath);
        var vmomOutText = ReadTextOrEmpty(vmomOutPath);

        return new VmomCaseDetail(
            entity.Id,
            entity.UserId,
            entity.Title,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ErrorMessage,
            entity.InputContent,
            rzText,
            eqprIotaText,
            vmomOutText);
    }

    public async Task<VmomCaseWorkspace?> GetCaseWorkspaceAsync(int caseId, CancellationToken cancellationToken = default)
    {
        var entity = await _fsql.Select<VmomCase>()
            .Where(c => c.Id == caseId && c.UserId == FixedUserId)
            .FirstAsync(cancellationToken);

        if (entity is null || string.IsNullOrWhiteSpace(entity.WorkDirectory))
        {
            return null;
        }

        var files = Directory.Exists(entity.WorkDirectory)
            ? Directory.GetFiles(entity.WorkDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList()
            : [];

        return new VmomCaseWorkspace(
            entity.Id,
            entity.Status,
            entity.WorkDirectory,
            files);
    }

    public async Task<(byte[] Content, string FileName)?> GetCaseZipAsync(int caseId, CancellationToken cancellationToken = default)
    {
        var entity = await _fsql.Select<VmomCase>()
            .Where(c => c.Id == caseId && c.UserId == FixedUserId)
            .FirstAsync(cancellationToken);

        if (entity is null || string.IsNullOrWhiteSpace(entity.WorkDirectory) || !Directory.Exists(entity.WorkDirectory))
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var files = Directory.GetFiles(entity.WorkDirectory, "*", SearchOption.AllDirectories);
            foreach (var filePath in files)
            {
                var relativePath = Path.GetRelativePath(entity.WorkDirectory, filePath);
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Fastest);
                await using var input = File.OpenRead(filePath);
                await using var output = entry.Open();
                await input.CopyToAsync(output, cancellationToken);
            }
        }

        return (memoryStream.ToArray(), $"case-{caseId}.zip");
    }

    private async Task RunVmom2Async(int caseId, string caseDirectory)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "vmom2",
                WorkingDirectory = caseDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            if (!process.Start())
            {
                await MarkFailedAsync(caseId, "vmom2 process failed to start.");
                return;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignore kill failures and mark case as timeout.
                }

                await MarkFailedAsync(caseId, "vmom2 execution timeout after 5 minutes.");
                return;
            }

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            var vmomOutPath = Path.Combine(caseDirectory, "vmom.out");
            if (!File.Exists(vmomOutPath) && !string.IsNullOrWhiteSpace(stdout))
            {
                await File.WriteAllTextAsync(vmomOutPath, stdout + Environment.NewLine + stderr);
            }

            if (process.ExitCode != 0)
            {
                var message = BuildErrorMessage(stderr, process.ExitCode);
                await MarkFailedAsync(caseId, message);
                return;
            }

            var missingFile = RequiredOutputFiles
                .Select(name => Path.Combine(caseDirectory, name))
                .FirstOrDefault(path => !File.Exists(path));

            if (missingFile is not null)
            {
                await MarkFailedAsync(caseId, $"vmom2 finished but output is missing: {Path.GetFileName(missingFile)}");
                return;
            }

            await _fsql.Update<VmomCase>()
                .Set(c => c.Status, "success")
                .Set(c => c.ErrorMessage, (string?)null)
                .Set(c => c.UpdatedAt, DateTime.UtcNow)
                .Where(c => c.Id == caseId && c.UserId == FixedUserId)
                .ExecuteAffrowsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "vmom2 case execution failed, caseId={CaseId}", caseId);
            await MarkFailedAsync(caseId, ex.Message);
        }
    }

    private async Task MarkFailedAsync(int caseId, string message)
    {
        await _fsql.Update<VmomCase>()
            .Set(c => c.Status, "failed")
            .Set(c => c.ErrorMessage, message)
            .Set(c => c.UpdatedAt, DateTime.UtcNow)
            .Where(c => c.Id == caseId && c.UserId == FixedUserId)
            .ExecuteAffrowsAsync();
    }

    private async Task<IReadOnlyList<CaseListItem>> ListCaseItemsAsync(CancellationToken cancellationToken)
    {
        var data = await _fsql.Select<VmomCase>()
            .Where(c => c.UserId == FixedUserId)
            .OrderByDescending(c => c.Id)
            .ToListAsync(cancellationToken);

        return data
            .Select(c => new CaseListItem(c.Id, c.Title, c.Status, c.CreatedAt, c.UpdatedAt, c.ErrorMessage))
            .ToList();
    }

    private static CaseOverviewResponse BuildOverview(IReadOnlyList<CaseListItem> cases)
    {
        var runningCount = cases.Count(c => c.Status is "running" or "queued");
        var successCount = cases.Count(c => c.Status == "success");
        var failedCount = cases.Count(c => c.Status == "failed");
        var recentCases = cases.Take(8).ToList();

        return new CaseOverviewResponse(
            cases.Count,
            runningCount,
            successCount,
            failedCount,
            recentCases);
    }

    private static string BuildErrorMessage(string stderr, int exitCode)
    {
        var head = $"vmom2 exited with code {exitCode}.";
        if (string.IsNullOrWhiteSpace(stderr))
        {
            return head;
        }

        var text = stderr.Trim();
        return text.Length <= 480 ? $"{head} {text}" : $"{head} {text[..480]}";
    }

    private string BuildCaseDirectory(int caseId)
    {
        var root = Path.Combine(_storagePath.Root, FixedUserId.ToString());
        Directory.CreateDirectory(root);
        return Path.Combine(root, caseId.ToString());
    }

    private static string ReadTextOrEmpty(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        return File.ReadAllText(filePath, Encoding.UTF8);
    }
}
