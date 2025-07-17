using BLL.DTO.ScheduleDtos;
using BLL.Interfaces;
using BLL.Mappers;
using BLL.Services;
using DAL.DTO.GroupDtos;
using DAL.DTO.ScheduleDtos;
using DAL.Repositories;
using DAL.RepositoryInterfaces;
using Domain;
using DTOs.ScheduleDtos;
using DTOs.Shifts;
using Moq;
using Xunit;

public class ScheduleServiceTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IShiftTypeRepository> _shiftTypeRepoMock = new();
    private readonly Mock<IGroupRepository> _groupRepoMock = new();
    private readonly Mock<IOrganizationRepository> _orgRepoMock = new();

    private readonly ScheduleService _service;

    public ScheduleServiceTests()
    {
        _service = new ScheduleService(
            _scheduleRepoMock.Object,
            _employeeRepoMock.Object,
            _shiftTypeRepoMock.Object,
            _groupRepoMock.Object,
            _orgRepoMock.Object
        );
    }

    [Fact]
    public async Task GetGroupScheduleSummariesAsync_ReturnsSummaries()
    {
        var orgId = 1;
        var groups = new List<DalGroup>
        {
            new DalGroup { Id = 1, Name = "Team A", AutorenewSchedules = true }
        };

        _groupRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId))
            .ReturnsAsync(groups);
        _scheduleRepoMock.Setup(r => r.GetConfirmedMonthsByGroupIdAsync(1))
            .ReturnsAsync(new List<string> { "May 2025" });
        _scheduleRepoMock.Setup(r => r.GetUnconfirmedMonthsByGroupIdAsync(1))
            .ReturnsAsync(new List<string> { "June 2025" });

        var result = await _service.GetGroupScheduleSummariesAsync(orgId);

        Assert.Single(result);
        Assert.Equal("Team A", result[0].GroupName);
    }

    [Fact]
    public async Task UnconfirmScheduleAsync_CallsRepository()
    {
        _scheduleRepoMock.Setup(r => r.UnconfirmSchedule(1)).ReturnsAsync(true);

        var result = await _service.UnconfirmScheduleAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task UpsertScheduleAsync_CreatesNewSchedule_IfNotExists()
    {
        var orgId = 1;
        var post = new BllSchedulePost
        {
            GroupId = 1,
            DateFrom = new DateOnly(2025, 6, 1),
            Autorenewal = true,
            Shifts = new List<BllSchedulePost.ShiftCreateDto>()
        };

        _groupRepoMock.Setup(r => r.UpdateAutorenewalAsync(post.GroupId, post.Autorenewal)).ReturnsAsync(true);
        _scheduleRepoMock
            .Setup(r => r.GetScheduleIdByGroupAndDateRange(
                orgId,
                post.GroupId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                false))
            .ReturnsAsync((int?)null);
        _scheduleRepoMock.Setup(r => r.CreateScheduleAsync(It.IsAny<DalSchedulePost>(), orgId))
            .ReturnsAsync(true);

        var result = await _service.UpsertScheduleAsync(orgId, post);

        Assert.True(result.Success);
        Assert.Equal("Schedule created", result.Message);
    }

    [Fact]
    public async Task UpsertScheduleAsync_UpdatesExistingSchedule_IfExists()
    {
        var orgId = 1;
        var post = new BllSchedulePost
        {
            GroupId = 1,
            DateFrom = new DateOnly(2025, 6, 1),
            Autorenewal = true,
            Shifts = new List<BllSchedulePost.ShiftCreateDto>()
        };

        _groupRepoMock.Setup(r => r.UpdateAutorenewalAsync(post.GroupId, post.Autorenewal)).ReturnsAsync(true);
        _scheduleRepoMock
            .Setup(r => r.GetScheduleIdByGroupAndDateRange(
                orgId,
                post.GroupId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                false))
            .ReturnsAsync(123);
        _scheduleRepoMock.Setup(r => r.UpdateScheduleAsync(orgId, 123, It.IsAny<DalSchedulePost>()))
            .ReturnsAsync(true);

        var result = await _service.UpsertScheduleAsync(orgId, post);

        Assert.True(result.Success);
        Assert.Equal("Schedule updated", result.Message);
    }
}
