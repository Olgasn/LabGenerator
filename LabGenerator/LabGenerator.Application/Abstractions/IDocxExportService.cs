namespace LabGenerator.Application.Abstractions;

public interface IDocxExportService
{
    Task<byte[]> ExportLabAsync(int labId, CancellationToken cancellationToken);
}