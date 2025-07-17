using BLL.Services;
using DAL.RepositoryInterfaces;
using DAL.DTO.ShiftTypeDtos;
using DTOs;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class ShiftTypeServiceTest
{
    private readonly Mock<IShiftTypeRepository> _repoMock;
    private readonly ShiftTypeService _service;

    public ShiftTypeServiceTest()
    {
        _repoMock = new Mock<IShiftTypeRepository>();
        _service = new ShiftTypeService(_repoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedBllShiftType()
    {
        var dal = new DalShiftType { Id = 1, Name = "Morning" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dal);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Morning", result!.Name);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedShiftType()
    {
        var input = new BllShiftType
        {
            Name = "Evening",
            StartTime = TimeSpan.FromHours(16),
            EndTime = TimeSpan.FromHours(22),
            EmployeeNeeded = 2,
            Color = "#FFAA00"
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<DalShiftType>()))
            .ReturnsAsync(new DalShiftType
            {
                Id = 10,
                Name = input.Name,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                EmployeeNeeded = input.EmployeeNeeded,
                Color = input.Color,
                OrganizationId = 1
            });

        var result = await _service.CreateAsync(1, input);

        Assert.Equal("Evening", result.Name);
        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_IfNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DalShiftType?)null);

        var result = await _service.UpdateAsync(99, new BllShiftType());

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue()
    {
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
    }
}
