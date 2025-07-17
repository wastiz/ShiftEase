using BLL.Services;
using BLL.Interfaces;
using DAL.DTO.EmployeeDtos;
using Domain.Models;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Repositories;
using DTOs.EmployeeDtos;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _repoMock = new Mock<IEmployeeRepository>();
        _service = new EmployeeService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ReturnsError_IfEmailExists()
    {
        _repoMock.Setup(r => r.EmployeeEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await _service.CreateAsync(new BllEmployeeCreate { Email = "test@test.com" }, 1);

        Assert.False(result.Success);
        Assert.Equal("Employee with this email already exists.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_ReturnsSuccess_IfValid()
    {
        _repoMock.Setup(r => r.EmployeeEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.EmployeePhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<DalEmployeeCreate>())).ReturnsAsync(true);

        var result = await _service.CreateAsync(new BllEmployeeCreate
        {
            Email = "new@employee.com",
            Phone = "123456"
        }, 1);

        Assert.True(result.Success);
        Assert.Equal("Employee created successfully", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_IfDeleteFails()
    {
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);
        var result = await _service.DeleteAsync(1);
        Assert.False(result.Success);
        Assert.Equal("Failed to delete employee", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsSuccess_IfDeleteSucceeds()
    {
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
        var result = await _service.DeleteAsync(1);
        Assert.True(result.Success);
    }
}
