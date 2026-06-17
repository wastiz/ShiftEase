using BLL.DTO.ScheduleDtos;
using BLL.Contracts;
using BLL.Services;
using DAL;
using Domain;
using Domain.Enums;
using DTOs;
using DTOs.DepartmentDtos;
using DTOs.EmployeeDtos;
using DTOs.OrganizationDtos;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class GreedyScheduleGeneratorServiceTests : IDisposable
{
    private readonly Mock<IOrganizationService> _orgServiceMock      = new();
    private readonly Mock<IShiftTemplateService> _shiftServiceMock   = new();
    private readonly Mock<IEmployeeService> _empServiceMock          = new();
    private readonly Mock<IDepartmentService> _departmentServiceMock = new();
    private readonly AppDbContext _dbContext;
    private readonly GreedyScheduleGeneratorService _generatorService;

    public GreedyScheduleGeneratorServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ScheduleGen_{Guid.NewGuid()}")
            .Options;
        _dbContext = new AppDbContext(options);

        _generatorService = new GreedyScheduleGeneratorService(
            _orgServiceMock.Object,
            _shiftServiceMock.Object,
            _empServiceMock.Object,
            _departmentServiceMock.Object,
            _dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static BllOrganization Org(List<BllHoliday>? holidays = null) => new()
    {
        Id              = 1,
        Name            = "Test Org",
        OrganizationType = "Test",
        IsOpen24_7      = false,
        NightShiftBonus = 0,
        HolidayBonus    = 0,
        EmployeeCount   = 0,
        Holidays        = holidays ?? new List<BllHoliday>(),
        WorkDays        = new List<BllWorkDay>
        {
            new() { DayOfWeek = DayOfWeek.Monday,    StartTime = "08:00", EndTime = "20:00" },
            new() { DayOfWeek = DayOfWeek.Tuesday,   StartTime = "08:00", EndTime = "20:00" },
            new() { DayOfWeek = DayOfWeek.Wednesday, StartTime = "08:00", EndTime = "20:00" },
            new() { DayOfWeek = DayOfWeek.Thursday,  StartTime = "08:00", EndTime = "20:00" },
            new() { DayOfWeek = DayOfWeek.Friday,    StartTime = "08:00", EndTime = "20:00" }
        }
    };

    private static List<BllShiftTemplate> MorningShiftType() =>
        new()
        {
            new() { Id = 1, Name = "Morning", StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(16), MinEmployees = 1, MaxEmployees = 3,
                    Color = "#fff", DepartmentId = 1, OrganizationId = 1 }
        };

    private static List<BllDepartment> Dept(List<BllShiftTemplate> shiftTypes) =>
        new()
        {
            new BllDepartment
            {
                Id                      = 1,
                Name                    = "Dept1",
                Color                   = "#fff",
                WorkingDays             = new List<DayOfWeek>(), // empty = all org days
                DefaultSchedulePattern  = SchedulePattern.Flexible,
                ShiftTypes              = shiftTypes
            }
        };

    private static List<BllEmployee> Employees(int count) =>
        Enumerable.Range(1, count).Select(i => new BllEmployee
        {
            Id                  = i,
            FirstName           = $"Emp{i}",
            LastName            = $"Last{i}",
            Email               = $"emp{i}@test.com",
            Position            = "Worker",
            DepartmentIds       = new List<int> { 1 },
            PrimaryDepartmentId = 1,
            EmploymentRate      = 1.0f
        }).ToList();

    private void Setup(BllOrganization org, List<BllDepartment> departments, List<BllEmployee> employees)
    {
        _orgServiceMock.Setup(x => x.GetOrganizationByIdAsync(org.Id)).ReturnsAsync(org);
        _departmentServiceMock.Setup(x => x.GetAllByOrganizationIdAsync(org.Id)).ReturnsAsync(departments);
        _empServiceMock.Setup(x => x.GetFullDataByOrganizationIdAsync(org.Id)).ReturnsAsync(employees);
    }

    // ── Error cases ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateGreedyScheduleAsync_InvalidDateRange_ReturnsError()
    {
        var result = await _generatorService.GenerateGreedyScheduleAsync(1, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 2, 1),
            EndDate   = new DateOnly(2025, 1, 1)
        });

        Assert.Equal(GenerateStatus.Error, result.Status);
        Assert.Equal(GenerateErrorCode.InvalidDateRange, result.Error);
    }

    [Fact]
    public async Task GenerateGreedyScheduleAsync_OrganizationNotFound_ReturnsError()
    {
        _orgServiceMock.Setup(x => x.GetOrganizationByIdAsync(99))
            .ReturnsAsync((BllOrganization)null);

        var result = await _generatorService.GenerateGreedyScheduleAsync(99, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 1, 1),
            EndDate   = new DateOnly(2025, 1, 31)
        });

        Assert.Equal(GenerateStatus.Error, result.Status);
        Assert.Equal(GenerateErrorCode.OrganizationNotFound, result.Error);
    }

    [Fact]
    public async Task GenerateGreedyScheduleAsync_NoWorkDays_ReturnsError()
    {
        var emptyOrg = new BllOrganization
        {
            Id              = 1,
            Name            = "Test Org",
            OrganizationType = "Test",
            IsOpen24_7      = false,
            NightShiftBonus = 0,
            HolidayBonus    = 0,
            EmployeeCount   = 0,
            Holidays        = new List<BllHoliday>(),
            WorkDays        = new List<BllWorkDay>()  // intentionally empty
        };
        _orgServiceMock.Setup(x => x.GetOrganizationByIdAsync(1)).ReturnsAsync(emptyOrg);

        var result = await _generatorService.GenerateGreedyScheduleAsync(1, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 1, 1),
            EndDate   = new DateOnly(2025, 1, 31)
        });

        Assert.Equal(GenerateStatus.Error, result.Status);
        Assert.Equal(GenerateErrorCode.NoWorkDaysConfigured, result.Error);
    }

    [Fact]
    public async Task GenerateGreedyScheduleAsync_NoDepartmentsWithShiftTypes_ReturnsError()
    {
        _orgServiceMock.Setup(x => x.GetOrganizationByIdAsync(1)).ReturnsAsync(Org());
        _departmentServiceMock.Setup(x => x.GetAllByOrganizationIdAsync(1))
            .ReturnsAsync(new List<BllDepartment>
            {
                new() { Id = 1, Name = "Empty", Color = "#fff",
                        ShiftTypes = new List<BllShiftTemplate>() }
            });

        var result = await _generatorService.GenerateGreedyScheduleAsync(1, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 1, 1),
            EndDate   = new DateOnly(2025, 1, 31)
        });

        Assert.Equal(GenerateStatus.Error, result.Status);
        Assert.Equal(GenerateErrorCode.NoShiftTypes, result.Error);
    }

    // ── Holiday logic ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateGreedyScheduleAsync_FullHoliday_ShiftsNotGeneratedOnThatDay()
    {
        var org = Org(holidays: new List<BllHoliday>
        {
            new() { Month = 1, Day = 6, IsShortenedDay = false, HolidayName = "Holiday" }
            // Jan 6 2025 is a Monday
        });
        Setup(org, Dept(MorningShiftType()), Employees(2));

        var result = await _generatorService.GenerateGreedyScheduleAsync(1, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 1, 6),
            EndDate   = new DateOnly(2025, 1, 6)
        });

        // Could be error (ShiftTypesDontFitSchedule) or success with empty shifts – either way no shifts on that date
        Assert.DoesNotContain(result.Shifts, s => s.Date == new DateOnly(2025, 1, 6));
    }

    [Fact]
    public async Task GenerateGreedyScheduleAsync_ShortenedHoliday_OnlyFittingShiftGenerated()
    {
        // Jan 3 2025 = Friday, shortened to 10-14
        var org = Org(holidays: new List<BllHoliday>
        {
            new() { Month = 1, Day = 3, IsShortenedDay = true,
                    StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(14),
                    HolidayName = "Short Holiday" }
        });

        var shiftTypes = new List<BllShiftTemplate>
        {
            // Does NOT fit 10-14 window (starts at 8)
            new() { Id = 1, Name = "AllDay",  StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(16), MinEmployees = 1, MaxEmployees = 2,
                    Color = "#fff", DepartmentId = 1, OrganizationId = 1 },
            // Fits exactly
            new() { Id = 2, Name = "Short", StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(14), MinEmployees = 1, MaxEmployees = 2,
                    Color = "#aaa", DepartmentId = 1, OrganizationId = 1 }
        };
        Setup(org, Dept(shiftTypes), Employees(2));

        var result = await _generatorService.GenerateGreedyScheduleAsync(1, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 1, 3),
            EndDate   = new DateOnly(2025, 1, 3)
        });

        var jan3Shifts = result.Shifts.Where(s => s.Date == new DateOnly(2025, 1, 3)).ToList();
        Assert.Single(jan3Shifts);
        Assert.Equal(2, jan3Shifts[0].ShiftTypeId);
    }

    // ── Warning cases ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateGreedyScheduleAsync_AllEmployeesOnTimeOff_AddsWarning()
    {
        var employees = Employees(1);
        Setup(Org(), Dept(MorningShiftType()), employees);

        // Vacation covers the whole range
        _dbContext.Vacations.Add(new Vacation
        {
            EmployeeId = 1,
            StartDate  = new DateOnly(2025, 1, 1),
            EndDate    = new DateOnly(2025, 1, 31)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _generatorService.GenerateGreedyScheduleAsync(1, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 1, 1),
            EndDate   = new DateOnly(2025, 1, 31)
        });

        Assert.Contains(GenerateWarningCode.AllEmployeesOnTimeOff, result.Warnings);
    }

    [Fact]
    public async Task GenerateGreedyScheduleAsync_EmployeeOnTimeOff_NotAssignedToShiftsOnThoseDays()
    {
        var employees = Employees(2);
        Setup(Org(), Dept(MorningShiftType()), employees);

        // Employee 1 is on sick leave the entire range
        _dbContext.SickLeaves.Add(new SickLeave
        {
            EmployeeId = 1,
            StartDate  = new DateOnly(2025, 1, 6),
            EndDate    = new DateOnly(2025, 1, 10)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _generatorService.GenerateGreedyScheduleAsync(1, new BllScheduleGenerateRequest
        {
            StartDate = new DateOnly(2025, 1, 6),
            EndDate   = new DateOnly(2025, 1, 10)
        });

        // Employee 1 must not appear in any shift during the sick leave window
        var shiftsWithEmployee1 = result.Shifts
            .Where(s => s.Employees.Any(e => e.Id == 1))
            .ToList();

        Assert.Empty(shiftsWithEmployee1);
    }

}
