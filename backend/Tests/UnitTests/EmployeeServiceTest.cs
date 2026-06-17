using BLL.Services;
using BLL.Contracts;
using DAL.DTO.EmployeeDtos;
using Moq;
using Xunit;
using DAL.Contracts;
using DTOs.EmployeeDtos;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly Mock<IVacationService> _vacationServiceMock;
    private readonly Mock<ISickLeaveService> _sickLeaveServiceMock;
    private readonly Mock<IPersonalDayService> _personalDayServiceMock;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _repoMock               = new Mock<IEmployeeRepository>();
        _vacationServiceMock    = new Mock<IVacationService>();
        _sickLeaveServiceMock   = new Mock<ISickLeaveService>();
        _personalDayServiceMock = new Mock<IPersonalDayService>();

        _service = new EmployeeService(
            _repoMock.Object,
            _vacationServiceMock.Object,
            _sickLeaveServiceMock.Object,
            _personalDayServiceMock.Object
        );
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_EmailAlreadyExists_ReturnsFailure()
    {
        _repoMock.Setup(r => r.EmployeeEmailExistsAsync("taken@test.com")).ReturnsAsync(true);

        var result = await _service.CreateAsync(new BllEmployeeCreate
        {
            Email          = "taken@test.com",
            FirstName      = "A",
            LastName       = "B",
            Position       = "Dev",
            OrganizationId = 1
        });

        Assert.False(result.Success);
        Assert.Equal("This email is already assigned to other employee", result.Message);
    }

    [Fact]
    public async Task CreateAsync_PhoneAlreadyExists_ReturnsFailure()
    {
        _repoMock.Setup(r => r.EmployeeEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.EmployeePhoneExistsAsync("111")).ReturnsAsync(true);

        var result = await _service.CreateAsync(new BllEmployeeCreate
        {
            Email          = "new@test.com",
            Phone          = "111",
            FirstName      = "A",
            LastName       = "B",
            Position       = "Dev",
            OrganizationId = 1
        });

        Assert.False(result.Success);
        Assert.Equal("This phone is already assigned to other employee", result.Message);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsSuccess()
    {
        _repoMock.Setup(r => r.EmployeeEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.EmployeePhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<DalEmployeeCreate>())).ReturnsAsync(true);

        var result = await _service.CreateAsync(new BllEmployeeCreate
        {
            Email          = "new@employee.com",
            Phone          = "123456",
            FirstName      = "Jane",
            LastName       = "Doe",
            Position       = "Dev",
            OrganizationId = 1
        });

        Assert.True(result.Success);
        Assert.Equal("Employee created.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_RepositoryReturnsFalse_ReturnsFailure()
    {
        _repoMock.Setup(r => r.EmployeeEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.EmployeePhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<DalEmployeeCreate>())).ReturnsAsync(false);

        var result = await _service.CreateAsync(new BllEmployeeCreate
        {
            Email          = "fail@employee.com",
            Phone          = "999",
            FirstName      = "X",
            LastName       = "Y",
            Position       = "Dev",
            OrganizationId = 1
        });

        Assert.False(result.Success);
        Assert.Equal("Failed to create employee.", result.Message);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RepositoryReturnsFalse_ReturnsFailure()
    {
        _repoMock.Setup(r => r.GetOrgIdByEmployeeIdAsync(1)).ReturnsAsync((int?)null);
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(1);

        Assert.False(result.Success);
        Assert.Equal("Failed to delete employee", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryReturnsTrue_ReturnsSuccess()
    {
        _repoMock.Setup(r => r.GetOrgIdByEmployeeIdAsync(1)).ReturnsAsync(42);
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.Success);
        Assert.Equal("Employee deleted successfully", result.Message);
    }
}
