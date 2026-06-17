using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Contracts;
using BLL.Services;
using DAL.DTO.OrganizationDtos;
using DAL.Contracts;
using Domain;
using DTOs.OrganizationDtos;
using Moq;
using Xunit;

public class OrganizationServiceTests
{
    private readonly Mock<IOrganizationRepository> _organizationRepoMock;
    private readonly Mock<IEmployerRepository> _employerRepoMock;
    private readonly OrganizationService _organizationService;

    public OrganizationServiceTests()
    {
        _organizationRepoMock = new Mock<IOrganizationRepository>();
        _employerRepoMock     = new Mock<IEmployerRepository>();
        _organizationService  = new OrganizationService(
            _organizationRepoMock.Object,
            _employerRepoMock.Object);
    }

    [Fact]
    public async Task GetAllByEmployerIdAsync_ReturnsList()
    {
        int employerId = 1;
        _organizationRepoMock.Setup(repo => repo.GetAllByEmployerIdAsync(employerId))
            .ReturnsAsync(new List<DalOrganization> { new DalOrganization { Id = 1, Name = "TestOrg" } });

        var result = await _organizationService.GetAllByEmployerIdAsync(employerId);

        Assert.Single(result);
        Assert.Equal("TestOrg", result[0].Name);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsOrganization()
    {
        var bllOrg = new BllOrganizationCreate
        {
            Name = "OrgName",
            EmployerId = 1,
            Description = "Some description",
            WorkDays = new List<BllWorkDay>
            {
                new BllWorkDay { DayOfWeek = DayOfWeek.Monday, StartTime = "09:00", EndTime = "17:00" },
                new BllWorkDay { DayOfWeek = DayOfWeek.Tuesday, StartTime = "10:00", EndTime = "18:00" }
            }
        };
        _organizationRepoMock.Setup(r => r.CreateAsync(It.IsAny<DalOrganizationCreate>()))
            .ReturnsAsync(new DalOrganization { Id = 1, Name = "OrgName" });

        var result = await _organizationService.CreateAsync(bllOrg);

        Assert.NotNull(result);
        Assert.Equal("OrgName", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ValidOwnershipAndData_ReturnsOrganization()
    {
        var dto = new BllOrganizationUpdate { Id = 1, Name = "UpdatedName" };
        _organizationRepoMock.Setup(r => r.IsOrganizationBelongsToEmployerAsync(1, 1)).ReturnsAsync(true);
        _organizationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DalOrganizationUpdate>()))
            .ReturnsAsync(new DalOrganization { Id = 1, Name = "UpdatedName" });

        var result = await _organizationService.UpdateAsync(dto, 1);

        Assert.NotNull(result);
        Assert.Equal("UpdatedName", result.Name);
    }

    [Fact]
    public async Task CheckOrganizationEntities_ReturnsCorrectStructure()
    {
        _organizationRepoMock.Setup(r => r.CheckOrganizationEntities(1))
            .ReturnsAsync(new DalOrganizationEntitiesCheckResult
            {
                Departments = true,
                Employees = false,
                ShiftTypes = true,
                Schedules = false
            });

        var result = await _organizationService.CheckOrganizationEntities(1);

        Assert.True(result.Departments);
        Assert.False(result.Employees);
        Assert.True(result.ShiftTypes);
        Assert.False(result.Schedules);
    }
}
