using LabGenerator.Domain.Entities;

namespace LabGenerator.Application.Abstractions;

public interface IVerificationService
{
    Task<VerificationReport> VerifyVariantAsync(int variantId, CancellationToken cancellationToken);
}