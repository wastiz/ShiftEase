using Xunit;
using Moq;
using BLL.Services;
using DAL.Contracts;
using DAL.DTO.DepartmentDtos;
using Domain;
using DTOs.DepartmentDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepoMock;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _departmentRepoMock = new Mock<IDepartmentRepository>();
        _service = new DepartmentService(_departmentRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDepartment_WhenExists()
    {
        var dalDepartment = new DalDepartment { Id = 1, Name = "Test Department", Description = "Description", Color = "#fff" };

        _departmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dalDepartment);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Test Department", result.Name);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _departmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((DalDepartment)null);

        var result = await _service.GetByIdAsync(1000);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByOrganizationIdAsync_ReturnsDepartments()
    {
        var dalDepartments = new List<DalDepartment>
        {
            new DalDepartment { Id = 1, Name = "G1", Description = "D1", Color = "#123" },
            new DalDepartment { Id = 2, Name = "G2", Description = "D2", Color = "#456" }
        };

        _departmentRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(1)).ReturnsAsync(dalDepartments);

        var result = await _service.GetAllByOrganizationIdAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Name == "G1");
        Assert.Contains(result, g => g.Name == "G2");
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsDepartment()
    {
        var bllDto = new BllDepartmentCreate
        {
            Name = "New",
            Description = "Desc",
            Color = "#000"
        };

        var dalResponse = new DalDepartment
        {
            Id = 99,
            Name = bllDto.Name,
            Description = bllDto.Description,
            Color = bllDto.Color,
        };

        _departmentRepoMock.Setup(r => r.CreateAsync(It.IsAny<DalDepartmentCreate>())).ReturnsAsync(dalResponse);

        var result = await _service.CreateAsync(bllDto, 1);

        Assert.NotNull(result);
        Assert.Equal("New", result.Name);
        Assert.Equal("#000", result.Color);
        Assert.Equal(99, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsDepartment()
    {
        var bllDto = new BllDepartment
        {
            Id = 1,
            Name = "Updated",
            Description = "Updated Desc",
            Color = "#999"
        };

        var dalResponse = new DalDepartment
        {
            Id = 1,
            Name = "Updated",
            Description = "Updated Desc",
            Color = "#999"
        };

        _departmentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DalDepartment>())).ReturnsAsync(dalResponse);

        var result = await _service.UpdateAsync(bllDto);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("#999", result.Color);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        _departmentRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenDepartmentNotFound()
    {
        _departmentRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(1);

        Assert.False(result);
    }
}
