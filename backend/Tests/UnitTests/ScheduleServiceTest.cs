using BLL.DTO.ScheduleDtos;
using BLL.Contracts;
using BLL.Services;
using DAL.DTO.ScheduleDtos;
using DAL.Contracts;
using DTOs;
using DTOs.DepartmentDtos;
using DTOs.OrganizationDtos;
using DTOs.ScheduleDtos;
using Moq;
using Xunit;

public class ScheduleServiceTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepoMock     = new();
    private readonly Mock<IEmployeeService> _employeeServiceMock     = new();
    private readonly Mock<IShiftTemplateService> _shiftTypeServiceMock = new();
    private readonly Mock<IDepartmentService> _departmentServiceMock  = new();
    private readonly Mock<IOrganizationService> _orgServiceMock       = new();
    private readonly ScheduleService _service;

    public ScheduleServiceTests()
    {
        _service = new ScheduleService(
            _scheduleRepoMock.Object,
            _employeeServiceMock.Object,
            _shiftTypeServiceMock.Object,
            _departmentServiceMock.Object,
            _orgServiceMock.Object
        );
    }

    // ── GetScheduleSummaryAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetScheduleSummaryAsync_ReturnsMappedSummary()
    {
        const int orgId = 1;

        _scheduleRepoMock.Setup(r => r.GetScheduleSummaryAsync(orgId))
            .ReturnsAsync(new DalScheduleSummary
            {
                ConfirmedSchedules   = new() { new DalScheduleItem { Id = 1, StartDate = new DateOnly(2025, 5, 1), EndDate = new DateOnly(2025, 5, 31) } },
                UnconfirmedSchedules = new() { new DalScheduleItem { Id = 2, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 6, 30) } }
            });

        // GetScheduleSummaryAsync calls these for every schedule item
        _scheduleRepoMock.Setup(r => r.GetScheduleShiftsByScheduleIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<DalShift>());
        _shiftTypeServiceMock.Setup(s => s.GetByOrganizationIdAsync(orgId))
            .ReturnsAsync(new List<BllShiftTemplate>());
        _departmentServiceMock.Setup(d => d.GetAllByOrganizationIdAsync(orgId))
            .ReturnsAsync(new List<BllDepartment>());
        _orgServiceMock.Setup(o => o.GetOrganizationHolidaysByIdAsync(orgId))
            .ReturnsAsync(new List<BllHoliday>());
        _orgServiceMock.Setup(o => o.GetOrganizationWorkDaysByIdAsync(orgId))
            .ReturnsAsync(new List<BllWorkDay>());

        var result = await _service.GetScheduleSummaryAsync(orgId);

        Assert.Single(result.ConfirmedSchedules);
        Assert.Single(result.UnconfirmedSchedules);
        // Month is derived from StartDate, not the raw string field
        Assert.Equal("May",  result.ConfirmedSchedules[0].Month);
        Assert.Equal("2025", result.ConfirmedSchedules[0].Year);
        Assert.Equal("June", result.UnconfirmedSchedules[0].Month);
    }

    // ── UnconfirmScheduleAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UnconfirmScheduleAsync_DelegatesToRepository()
    {
        _scheduleRepoMock.Setup(r => r.UnconfirmSchedule(1)).ReturnsAsync(true);

        var result = await _service.UnconfirmScheduleAsync(1);

        Assert.True(result);
        _scheduleRepoMock.Verify(r => r.UnconfirmSchedule(1), Times.Once);
    }

    // ── UpsertScheduleAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpsertScheduleAsync_ScheduleDoesNotExist_CreatesNewOne()
    {
        const int orgId = 1;
        var post = new BllSchedulePost
        {
            StartDate   = new DateOnly(2025, 6, 1),
            EndDate     = new DateOnly(2025, 6, 30),
            IsConfirmed = false,
            Shifts      = new List<BllSchedulePost.ShiftPost>()
        };

        _scheduleRepoMock
            .Setup(r => r.GetScheduleIdByDateRange(orgId, post.StartDate, post.EndDate, false))
            .ReturnsAsync((int?)null);
        _scheduleRepoMock
            .Setup(r => r.CreateScheduleAsync(It.IsAny<DalSchedulePost>(), orgId))
            .ReturnsAsync(true);

        var result = await _service.UpsertScheduleAsync(orgId, post);

        Assert.True(result.Success);
        Assert.Equal("Schedule created", result.Message);
    }

    [Fact]
    public async Task UpsertScheduleAsync_ScheduleExists_UpdatesExistingOne()
    {
        const int orgId = 1;
        var post = new BllSchedulePost
        {
            StartDate   = new DateOnly(2025, 6, 1),
            EndDate     = new DateOnly(2025, 6, 30),
            IsConfirmed = false,
            Shifts      = new List<BllSchedulePost.ShiftPost>()
        };

        _scheduleRepoMock
            .Setup(r => r.GetScheduleIdByDateRange(orgId, post.StartDate, post.EndDate, false))
            .ReturnsAsync(123);
        _scheduleRepoMock
            .Setup(r => r.UpdateScheduleAsync(orgId, 123, It.IsAny<DalSchedulePost>()))
            .ReturnsAsync(true);

        var result = await _service.UpsertScheduleAsync(orgId, post);

        Assert.True(result.Success);
        Assert.Equal("Schedule updated", result.Message);
    }

    [Fact]
    public async Task UpsertScheduleAsync_UpdateFails_ReturnsFailure()
    {
        const int orgId = 1;
        var post = new BllSchedulePost
        {
            StartDate   = new DateOnly(2025, 6, 1),
            EndDate     = new DateOnly(2025, 6, 30),
            IsConfirmed = false,
            Shifts      = new List<BllSchedulePost.ShiftPost>()
        };

        _scheduleRepoMock
            .Setup(r => r.GetScheduleIdByDateRange(orgId, post.StartDate, post.EndDate, false))
            .ReturnsAsync(123);
        _scheduleRepoMock
            .Setup(r => r.UpdateScheduleAsync(orgId, 123, It.IsAny<DalSchedulePost>()))
            .ReturnsAsync(false);

        var result = await _service.UpsertScheduleAsync(orgId, post);

        Assert.False(result.Success);
    }
}
