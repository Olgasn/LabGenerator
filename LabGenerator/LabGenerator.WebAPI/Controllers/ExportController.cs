using LabGenerator.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace LabGenerator.WebAPI.Controllers;

[ApiController]
[Route("api/labs/{labId:int}/export")]
public class ExportController(IDocxExportService exportService) : ControllerBase
{
    [HttpGet("docx")]
    public async Task<IActionResult> ExportDocx(int labId, CancellationToken cancellationToken)
    {
        var bytes = await exportService.ExportLabAsync(labId, cancellationToken);
        var fileName = $"lab_{labId}.docx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }
}