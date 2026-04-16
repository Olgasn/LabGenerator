using LabGenerator.Domain.Entities;

namespace LabGenerator.Application.Abstractions;

public interface IMasterAssignmentService
{
    Task<MasterAssignment> GenerateDraftAsync(int labId, CancellationToken cancellationToken);
    Task<MasterAssignment?> GetCurrentAsync(int labId, CancellationToken cancellationToken);
    Task<MasterAssignment> UpdateContentAsync(int masterAssignmentId, string content, CancellationToken cancellationToken);
    Task<MasterAssignment> ApproveAsync(int masterAssignmentId, CancellationToken cancellationToken);
}