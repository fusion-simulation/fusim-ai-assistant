using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Microsoft.AspNetCore.Mvc;

namespace FusimAiAssiant.Controllers;

[ApiController]
[Route("api/vmom")]
public class VmomController : ControllerBase
{
    private readonly IVmomCaseService _caseService;
    private readonly VmomInputCatalogService _inputCatalogService;
    private readonly ICaseDetailChatAgentService _chatAgentService;

    public VmomController(
        IVmomCaseService caseService,
        VmomInputCatalogService inputCatalogService,
        ICaseDetailChatAgentService chatAgentService)
    {
        _caseService = caseService;
        _inputCatalogService = inputCatalogService;
        _chatAgentService = chatAgentService;
    }

    [HttpGet("catalog")]
    public ActionResult<VmomInputCatalogResponse> GetCatalog()
    {
        var (fields, templateInput) = _inputCatalogService.GetCatalog();
        return Ok(new VmomInputCatalogResponse(fields, templateInput));
    }

    [HttpGet("cases")]
    public async Task<ActionResult<IReadOnlyList<CaseListItem>>> GetCases(CancellationToken cancellationToken)
    {
        var data = await _caseService.ListCasesAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("overview")]
    public async Task<ActionResult<CaseOverviewResponse>> GetOverview(CancellationToken cancellationToken)
    {
        var data = await _caseService.GetOverviewAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("cases/{caseId:int}")]
    public async Task<ActionResult<VmomCaseDetail>> GetCase(int caseId, CancellationToken cancellationToken)
    {
        var data = await _caseService.GetCaseDetailAsync(caseId, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(data);
    }

    [HttpPost("cases")]
    public async Task<ActionResult<object>> CreateCase([FromBody] CreateCaseRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.InputContent))
        {
            return BadRequest(new { message = "InputContent 不能为空" });
        }

        var id = await _caseService.CreateCaseAsync(request.Title, request.InputContent, cancellationToken);
        return Ok(new { id });
    }

    [HttpPost("cases/from-form")]
    public async Task<ActionResult<object>> CreateCaseFromForm([FromBody] CreateCaseFromFormRequest request, CancellationToken cancellationToken)
    {
        if (request.Fields is null || request.Fields.Count == 0)
        {
            return BadRequest(new { message = "Fields 不能为空" });
        }

        var id = await _caseService.CreateCaseFromFormAsync(request.Title, request.Fields, cancellationToken);
        return Ok(new { id });
    }

    [HttpGet("cases/{caseId:int}/download")]
    public async Task<IActionResult> DownloadCaseZip(int caseId, CancellationToken cancellationToken)
    {
        var zip = await _caseService.GetCaseZipAsync(caseId, cancellationToken);
        if (zip is null)
        {
            return NotFound();
        }

        return File(zip.Value.Content, "application/zip", zip.Value.FileName);
    }

    [HttpPost("cases/{caseId:int}/agent/chat")]
    public async Task<ActionResult<CaseAgentChatResponse>> ChatCaseAgent(
        int caseId,
        [FromBody] CaseAgentChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message 不能为空" });
        }

        var response = await _chatAgentService.ChatAsync(caseId, request.Message, cancellationToken);
        return Ok(response);
    }

    [HttpGet("cases/{caseId:int}/plots/{imageFileName}")]
    public async Task<IActionResult> GetCasePlotImage(
        int caseId,
        string imageFileName,
        CancellationToken cancellationToken)
    {
        var workspace = await _caseService.GetCaseWorkspaceAsync(caseId, cancellationToken);
        if (workspace is null || string.IsNullOrWhiteSpace(workspace.WorkDirectory))
        {
            return NotFound();
        }

        var plotsDirectory = Path.Combine(workspace.WorkDirectory, "plots");
        var rootPath = Path.GetFullPath(plotsDirectory);
        var imagePath = Path.GetFullPath(Path.Combine(plotsDirectory, imageFileName));
        if (!imagePath.StartsWith(rootPath, StringComparison.Ordinal) || !System.IO.File.Exists(imagePath))
        {
            return NotFound();
        }

        return PhysicalFile(imagePath, "image/png");
    }
}
