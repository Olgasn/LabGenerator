using LabGenerator.Domain.Entities;

namespace LabGenerator.Application.Abstractions;

public interface ILabSupplementaryMaterialService
{
    Task<LabSupplementaryMaterial> GenerateAsync(int labId, bool force, CancellationToken cancellationToken);

    Task<LabSupplementaryMaterial?> GetCurrentAsync(int labId, CancellationToken cancellationToken);

    bool IsUpToDate(LabSupplementaryMaterial material, string sourceFingerprint);

    string BuildSourceFingerprint(string masterContent, IReadOnlyCollection<string> variantFingerprints);
}
