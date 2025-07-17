using Microsoft.EntityFrameworkCore;
using Domain;
using Microsoft.Extensions.Logging;
using DAL.DTO.OrganizationDtos;
using DAL.RepositoryInterfaces;
using DTOs.OrganizationDtos;

namespace DAL
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrganizationRepository> _logger;

        public OrganizationRepository(AppDbContext context, ILogger<OrganizationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        //Get all organizations
        public async Task<List<DalOrganizationInfo>> GetAllAsync()
        {
            return await _context.Organizations
                .Select(o => new DalOrganizationInfo
                {
                    Id = o.Id,
                    Name = o.Name,
                    Description = o.Description,
                    Address = o.Address,
                    Phone = o.Phone,
                    Website = o.Website,
                    OrganizationType = o.OrganizationType,
                    IsOpen24_7 = o.IsOpen24_7,
                    NightShiftBonus = o.NightShiftBonus,
                    HolidayBonus = o.HolidayBonus,
                    PhotoUrl = o.PhotoUrl,
                    EmployeeCount = o.EmployeeCount
                })
                .ToListAsync();
        }
        
        //Get organization info by id
        public async Task<DalOrganizationInfo> GetByIdAsync(int id)
        {
            var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null) return null;
            
            var dto = new DalOrganizationInfo
            {
                Id = organization.Id,
                Name = organization.Name,
                Description = organization.Description,
                Address = organization.Address,
                Phone = organization.Phone,
                Website = organization.Website,
                OrganizationType = organization.OrganizationType,
                IsOpen24_7 = organization.IsOpen24_7,
                NightShiftBonus = organization.NightShiftBonus,
                HolidayBonus = organization.HolidayBonus,
                PhotoUrl = organization.PhotoUrl,
                EmployeeCount = organization.EmployeeCount
            };

            return dto;
        }
        
        //Get Organization by organization name
        public async Task<Organization> GetByNameAsync(string name)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.Name == name);
        }
        
        //Get Employer Id by OrganizationId
        public async Task<int> GetEmployerIdByOrganizationIdAsync(int organizationId)
        {
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
                throw new Exception($"Organization with ID {organizationId} not found.");

            return organization.EmployerId;
        }

        
        //Get all employer's organizations by employer's id
        public async Task<List<DalOrganizationInfo>> GetAllByEmployerIdAsync(int employerId)
        {
            return await _context.Organizations
                .Where(o => o.EmployerId == employerId)
                .Select(o => new DalOrganizationInfo
                {
                    Id = o.Id,
                    Name = o.Name,
                    Description = o.Description,
                    Address = o.Address,
                    Phone = o.Phone,
                    Website = o.Website,
                    OrganizationType = o.OrganizationType,
                    IsOpen24_7 = o.IsOpen24_7,
                    NightShiftBonus = o.NightShiftBonus,
                    HolidayBonus = o.HolidayBonus,
                    PhotoUrl = o.PhotoUrl,
                    EmployeeCount = o.EmployeeCount
                    
                })
                .ToListAsync();
        }
        
        //Get Organization holidays (without names, just dates)
        public async Task<List<DalHoliday>> GetHolidaysByOrganizationIdAsync(int organizationId)
        {
            return await _context.OrganizationHolidays
                .Where(h => h.OrganizationId == organizationId)
                .Select(h => new DalHoliday { HolidayName = h.HolidayName, Month = h.Month, Day = h.Day })
                .ToListAsync();
        }
        
        //Get Organization Work Days (week days with work time)
        public async Task<List<DalWorkDay>> GetWorkScheduleByOrganizationIdAsync(int organizationId)
        {
            return await _context.OrganizationWorkDays
                .Where(wd => wd.OrganizationId == organizationId)
                .Select(wd => new DalWorkDay { WeekDayName = wd.WeekDay.Name, From = wd.StartTime.ToString(), To = wd.EndTime.ToString() })
                .ToListAsync();
        }
        
        //Get count of new organization (for last month)
        public async Task<int> GetNewCountLastMonthAsync()
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            return await _context.Organizations.CountAsync(o => o.CreatedAt >= oneMonthAgo);
        }
        
        //Get count of organization without Employees
        public async Task<int> GetCountWithoutEmployeesAsync()
        {
            return await _context.Organizations
                .CountAsync(o => !o.Employees.Any());
        }
        
        //Check if user created all entities (group, employees, shiftTypes, schedules)
        public async Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId)
        {
            return await _context.Organizations
                .Where(o => o.Id == orgId)
                .Select(o => new DalOrganizationEntitiesCheckResult
                {
                    Groups = o.Groups.Any(),
                    Employees = o.Employees.Any(),
                    ShiftTypes = o.ShiftTypes.Any(),
                    Schedules = o.Schedules.Any()
                })
                .FirstOrDefaultAsync() ?? new DalOrganizationEntitiesCheckResult();
        }
        
        //Check if organization belongs to employer
        public async Task<bool> IsOrganizationOwnedByEmployerAsync(int orgId, int employerId)
        {
            var organization = await _context.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orgId);

            if (organization == null)
                return false;

            return organization.EmployerId == employerId;
        }
        
        //Create Organizations
        public async Task<bool> CreateAsync(DalOrganizationCreate createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employerExists = await _context.Employers.AnyAsync(e => e.Id == createDto.EmployerId);
                if (!employerExists)
                {
                    _logger.LogWarning($"Employer with id {createDto.EmployerId} not found");
                    return false;
                }

                // Creating Organization
                var organization = new Organization
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Address = createDto.Address,
                    Phone = createDto.Phone,
                    Website = createDto.Website,
                    OrganizationType = createDto.OrganizationType,
                    IsOpen24_7 = createDto.IsOpen24_7,
                    NightShiftBonus = createDto.NightShiftBonus,
                    HolidayBonus = createDto.HolidayBonus,
                    PhotoUrl = createDto.PhotoUrl,
                    EmployeeCount = createDto.EmployeeCount,
                    EmployerId = createDto.EmployerId
                };

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync(); //To get org Id

                // Adding org work days
                if (!organization.IsOpen24_7 && createDto.OrganizationWorkDays != null)
                {
                    foreach (var wk in createDto.OrganizationWorkDays)
                    {
                        var weekDay = await _context.WeekDays.FirstOrDefaultAsync(w => w.Name.ToLower() == wk.WeekDayName.ToLower());
                        if (weekDay == null)
                        {
                            _logger.LogWarning($"WeekDay '{wk.WeekDayName}' not found");
                            continue;
                        }

                        var workDay = new OrganizationWorkDay
                        {
                            WeekDayId = weekDay.Id,
                            StartTime = TimeSpan.Parse(wk.From),
                            EndTime = TimeSpan.Parse(wk.To),
                            OrganizationId = organization.Id
                        };

                        _context.OrganizationWorkDays.Add(workDay);
                    }

                    await _context.SaveChangesAsync();
                }
                
                // Adding Holidays
                var holidays = createDto.OrganizationHolidays?
                    .Select(h => new OrganizationHoliday
                    {
                        HolidayName = h.HolidayName,
                        Month = h.Month,
                        Day = h.Day
                    }).ToList() ?? new List<OrganizationHoliday>();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                await transaction.RollbackAsync();
                return false;
            }
        }
        
        
        // Update Organization by its id
        public async Task<bool> UpdateAsync(DalOrganizationUpdate updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if organization exists
                var organization = await _context.Organizations
                    .Include(o => o.OrganizationHolidays)
                    .Include(o => o.OrganizationWorkDays)
                    .FirstOrDefaultAsync(o => o.Id == updateDto.OrganizationId);

                if (organization == null)
                {
                    _logger.LogWarning($"Organization with id {updateDto.OrganizationId} not found");
                    return false;
                }

                // Update organization details
                organization.Name = updateDto.Name;
                organization.Description = updateDto.Description;
                organization.Address = updateDto.Address;
                organization.Phone = updateDto.Phone;
                organization.Website = updateDto.Website;
                organization.OrganizationType = updateDto.OrganizationType;
                organization.IsOpen24_7 = updateDto.IsOpen24_7;
                organization.NightShiftBonus = updateDto.NightShiftBonus;
                organization.HolidayBonus = updateDto.HolidayBonus;
                organization.PhotoUrl = updateDto.PhotoUrl;
                organization.EmployeeCount = updateDto.EmployeeCount;

                // Updating Holidays
                if (updateDto.OrganizationHolidays != null)
                {
                    // Remove old holidays
                    _context.OrganizationHolidays.RemoveRange(organization.OrganizationHolidays);

                    // Add new holidays
                    organization.OrganizationHolidays = updateDto.OrganizationHolidays
                        .Select(h => new OrganizationHoliday
                        {
                            HolidayName = h.HolidayName,
                            Month = h.Month,
                            Day = h.Day
                        })
                        .ToList();
                }

                // Updating WorkDays
                if (!organization.IsOpen24_7 && updateDto.OrganizationWorkDays != null)
                {
                    // Remove old workdays
                    _context.OrganizationWorkDays.RemoveRange(organization.OrganizationWorkDays);

                    foreach (var wk in updateDto.OrganizationWorkDays)
                    {
                        var weekDay = await _context.WeekDays.FirstOrDefaultAsync(w => w.Name.ToLower() == wk.WeekDayName.ToLower());
                        if (weekDay == null)
                        {
                            _logger.LogWarning($"WeekDay '{wk.WeekDayName}' not found");
                            continue;
                        }

                        var workDay = new OrganizationWorkDay
                        {
                            WeekDayId = weekDay.Id,
                            StartTime = TimeSpan.Parse(wk.From),
                            EndTime = TimeSpan.Parse(wk.To),
                            OrganizationId = organization.Id
                        };

                        _context.OrganizationWorkDays.Add(workDay);
                    }
                }

                _context.Organizations.Update(organization);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization");
                await transaction.RollbackAsync();
                return false;
            }
        }
        
        //Delete Organization by its id
        public async Task<bool> DeleteAsync(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return false;
            }

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}