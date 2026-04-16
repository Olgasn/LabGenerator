using LabGenerator.Domain.Entities;

namespace LabGenerator.Application.Abstractions;

public interface IVariantGenerationService
{
    Task<IReadOnlyList<AssignmentVariant>> GenerateVariantsAsync(
        int labId,
        int count,
        int? variationProfileId,
        CancellationToken cancellationToken);
}