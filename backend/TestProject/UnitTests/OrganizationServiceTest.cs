using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.ServiceInterfaces;
using BLL.Services;
using DAL.DTO.OrganizationDtos;
using DAL.RepositoryInterfaces;
using Domain;
using DTOs.OrganizationDtos;
using Moq;
using Xunit;

public class OrganizationServiceTests
{
    private readonly Mock<IOrganizationRepository> _organizationRepoMock;
    private readonly OrganizationService _organizationService;

    public OrganizationServiceTests()
    {
        _organizationRepoMock = new Mock<IOrganizationRepository>();
        _organizationService = new OrganizationService(_organizationRepoMock.Object);
    }

    [Fact]
    public async Task GetAllByEmployerIdAsync_ReturnsList()
    {
        // Arrange
        int employerId = 1;
        _organizationRepoMock.Setup(repo => repo.GetAllByEmployerIdAsync(employerId))
            .ReturnsAsync(new List<DalOrganizationInfo> { new DalOrganizationInfo { Id = 1, Name = "TestOrg" } });

        // Act
        var result = await _organizationService.GetAllByEmployerIdAsync(employerId);

        // Assert
        Assert.Single(result);
        Assert.Equal("TestOrg", result[0].Name);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var bllOrg = new BllOrganizationCreate
        {
            Name = "OrgName",
            EmployerId = 1,
            Description = "Some description",
            OrganizationWorkDays = new List<BllWorkDay>
            {
                new BllWorkDay { WeekDayName = "Monday", From = "09:00", To = "17:00" },
                new BllWorkDay { WeekDayName = "Tuesday", From = "10:00", To = "18:00" }
            }
        };
        _organizationRepoMock.Setup(r => r.CreateAsync(It.IsAny<DalOrganizationCreate>())).ReturnsAsync(true);

        // Act
        var result = await _organizationService.CreateAsync(bllOrg);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_ValidOwnershipAndData_ReturnsTrue()
    {
        // Arrange
        var dto = new BllOrganizationUpdate { OrganizationId = 1, Name = "UpdatedName" };
        _organizationRepoMock.Setup(r => r.IsOrganizationOwnedByEmployerAsync(1, 1)).ReturnsAsync(true);
        _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DalOrganizationUpdate>())).ReturnsAsync(true);

        // Act
        var result = await _organizationService.UpdateAsync(dto, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NotOwned_ReturnsFalse()
    {
        var dto = new BllOrganizationUpdate { OrganizationId = 1 };
        _organizationRepoMock.Setup(r => r.IsOrganizationOwnedByEmployerAsync(1, 2)).ReturnsAsync(false);

        var result = await _organizationService.UpdateAsync(dto, 2);

        Assert.False(result);
    }

    [Fact]
    public async Task CheckOrganizationEntities_ReturnsCorrectStructure()
    {
        _organizationRepoMock.Setup(r => r.CheckOrganizationEntities(1))
            .ReturnsAsync(new DalOrganizationEntitiesCheckResult
            {
                Groups = true,
                Employees = false,
                ShiftTypes = true,
                Schedules = false
            });

        var result = await _organizationService.CheckOrganizationEntities(1);

        Assert.True(result.Groups);
        Assert.False(result.Employees);
        Assert.True(result.ShiftTypes);
        Assert.False(result.Schedules);
    }
}
