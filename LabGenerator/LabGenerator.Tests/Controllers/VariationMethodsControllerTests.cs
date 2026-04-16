using LabGenerator.Domain.Entities;
using LabGenerator.Infrastructure.Data;
using LabGenerator.Tests.Helpers;
using LabGenerator.WebAPI.Controllers;
using LabGenerator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LabGenerator.Tests.Controllers;

public sealed class VariationMethodsControllerTests
{
    [Fact]
    public async Task Create_ReturnsConflict_WhenCodeAlreadyExists()
    {
        await using var db = TestDbContextFactory.Create();
        db.VariationMethods.Add(new VariationMethod
        {
            Code = "existing_code",
            Name = "Existing method",
            IsSystem = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new VariationMethodsController(db);

        var result = await controller.Create(
            new CreateVariationMethodRequest
            {
                Code = "existing_code",
                Name = "Duplicate method",
                Description = "duplicate"
            },
            CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsEntity_WhenRequestIsValid()
    {
        await using var db = TestDbContextFactory.Create();
        var controller = new VariationMethodsController(db);

        var actionResult = await controller.Create(
            new CreateVariationMethodRequest
            {
                Code = "new_code",
                Name = "New method",
                Description = "Custom variation method"
            },
            CancellationToken.None);

        var created = Assert.IsType<VariationMethod>(actionResult.Value);
        Assert.True(created.Id > 0);
        Assert.Equal("new_code", created.Code);
        Assert.Equal("New method", created.Name);
        Assert.False(created.IsSystem);
        Assert.Single(db.VariationMethods.Where(x => x.Code == "new_code"));
    }
}
