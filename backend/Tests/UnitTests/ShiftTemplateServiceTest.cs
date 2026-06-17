using BLL.Services;
using DAL.Contracts;
using DAL.DTO.ShiftTypeDtos;
using DTOs;
using Moq;
using Xunit;

public class ShiftTemplateServiceTest
{
    private readonly Mock<IShiftTemplateRepository> _repoMock;
    private readonly ShiftTemplateService _service;

    public ShiftTemplateServiceTest()
    {
        _repoMock = new Mock<IShiftTemplateRepository>();
        _service  = new ShiftTemplateService(_repoMock.Object);
    }

    // ── Get ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DalShiftTemplate?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedTemplate_WhenFound()
    {
        var dal = new DalShiftTemplate
        {
            Id           = 1,
            Name         = "Morning",
            StartTime    = TimeSpan.FromHours(8),
            EndTime      = TimeSpan.FromHours(16),
            MinEmployees = 2,
            MaxEmployees = 5,
            Color        = "#fff",
            DepartmentId = 1,
            OrganizationId = 1
        };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dal);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Morning", result!.Name);
        Assert.Equal(2, result.MinEmployees);
        Assert.Equal(5, result.MaxEmployees);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsCreatedTemplate()
    {
        var input = new BllShiftTemplateCreate
        {
            Name           = "Evening",
            StartTime      = TimeSpan.FromHours(16),
            EndTime        = TimeSpan.FromHours(22),
            MinEmployees   = 1,
            MaxEmployees   = 3,
            Color          = "#FFAA00",
            DepartmentId   = 1,
            OrganizationId = 1
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<DalShiftTemplateCreate>()))
            .ReturnsAsync(new DalShiftTemplate
            {
                Id             = 10,
                Name           = input.Name,
                StartTime      = input.StartTime,
                EndTime        = input.EndTime,
                MinEmployees   = input.MinEmployees,
                MaxEmployees   = input.MaxEmployees,
                Color          = input.Color,
                DepartmentId   = input.DepartmentId,
                OrganizationId = input.OrganizationId
            });

        var result = await _service.CreateAsync(input);

        Assert.Equal("Evening", result.Name);
        Assert.Equal(10, result.Id);
        Assert.Equal(1, result.MinEmployees);
        Assert.Equal(3, result.MaxEmployees);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DalShiftTemplateUpdate>()))
            .ReturnsAsync((DalShiftTemplate?)null);

        var result = await _service.UpdateAsync(new BllShiftTemplateUpdate { Id = 99, Name = "X", Color = "#000" });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsMappedTemplate_WhenSucceeds()
    {
        var updated = new DalShiftTemplate
        {
            Id           = 5,
            Name         = "Night",
            StartTime    = TimeSpan.FromHours(22),
            EndTime      = TimeSpan.FromHours(6),
            MinEmployees = 1,
            MaxEmployees = 2,
            Color        = "#000"
        };
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DalShiftTemplateUpdate>()))
            .ReturnsAsync(updated);

        var result = await _service.UpdateAsync(new BllShiftTemplateUpdate
        {
            Id   = 5,
            Name = "Night",
            Color = "#000"
        });

        Assert.NotNull(result);
        Assert.Equal("Night", result!.Name);
        Assert.Equal(5, result.Id);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        _repoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(99);

        Assert.False(result);
    }
}
