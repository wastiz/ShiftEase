using Xunit;
using Moq;
using BLL.Services;
using DAL.Repositories;
using DAL.DTO.GroupDtos;
using Domain;
using DTOs.GroupDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GroupServiceTests
{
    private readonly Mock<IGroupRepository> _groupRepoMock;
    private readonly GroupService _service;

    public GroupServiceTests()
    {
        _groupRepoMock = new Mock<IGroupRepository>();
        _service = new GroupService(_groupRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsGroup_WhenExists()
    {
        var dalGroup = new DalGroup { Id = 1, Name = "Test Group", Description = "Description", Color = "#fff" };

        _groupRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dalGroup);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Test Group", result.Name);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _groupRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((DalGroup)null);

        var result = await _service.GetByIdAsync(1000);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByOrganizationIdAsync_ReturnsGroups()
    {
        var dalGroups = new List<DalGroup>
        {
            new DalGroup { Id = 1, Name = "G1", Description = "D1", Color = "#123" },
            new DalGroup { Id = 2, Name = "G2", Description = "D2", Color = "#456" }
        };

        _groupRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(1)).ReturnsAsync(dalGroups);

        var result = await _service.GetAllByOrganizationIdAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Name == "G1");
        Assert.Contains(result, g => g.Name == "G2");
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsGroup()
    {
        var bllDto = new BllGroupCreate
        {
            Name = "New",
            Description = "Desc",
            Color = "#000"
        };

        var dalResponse = new DalGroup
        {
            Id = 99,
            Name = bllDto.Name,
            Description = bllDto.Description,
            Color = bllDto.Color,
        };

        _groupRepoMock.Setup(r => r.CreateAsync(It.IsAny<DalGroupCreate>())).ReturnsAsync(dalResponse);

        var result = await _service.CreateAsync(bllDto, 1);

        Assert.NotNull(result);
        Assert.Equal("New", result.Name);
        Assert.Equal("#000", result.Color);
        Assert.Equal(99, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsGroup()
    {
        var bllDto = new BllGroup
        {
            Id = 1,
            Name = "Updated",
            Description = "Updated Desc",
            Color = "#999"
        };

        var dalResponse = new DalGroup
        {
            Id = 1,
            Name = "Updated",
            Description = "Updated Desc",
            Color = "#999"
        };

        _groupRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DalGroup>())).ReturnsAsync(dalResponse);

        var result = await _service.UpdateAsync(bllDto);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("#999", result.Color);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        _groupRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenGroupNotFound()
    {
        _groupRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(1);

        Assert.False(result);
    }
}
