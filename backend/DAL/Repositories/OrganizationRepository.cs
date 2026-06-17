using DAL.Contracts;
using DAL.DTO.OrganizationDtos;
using DAL.Mappers;
using Domain;
using DTOs.OrganizationDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrganizationRepository> _logger;
        private readonly IScheduleRepository _scheduleRepository;

        public OrganizationRepository(AppDbContext context, ILogger<OrganizationRepository> logger, IScheduleRepository scheduleRepository)
        {
            _context = context;
            _logger = logger;
            _scheduleRepository = scheduleRepository;
        }
        
        //Get all organizations
        public async Task<List<DalOrganization>> GetAllAsync()
        {
            var orgs = await _context.Organizations
                .Include(o => o.OrganizationHolidays)
                .Include(o => o.OrganizationWorkDays)
                .ToListAsync();
            return orgs.Select(OrganizationMapper.ToDal).ToList();
        }

        //Get organization info by id
        public async Task<DalOrganization?> GetByIdAsync(int id)
        {
            var org = await _context.Organizations
                .Include(o => o.OrganizationHolidays)
                .Include(o => o.OrganizationWorkDays)
                .FirstOrDefaultAsync(o => o.Id == id);
            return org == null ? null : OrganizationMapper.ToDal(org);
        }

        //Get all employer's organizations by employer's id
        public async Task<List<DalOrganization>> GetAllByEmployerIdAsync(int employerId)
        {
            var orgs = await _context.Organizations
                .Where(o => o.EmployerId == employerId)
                .Include(o => o.OrganizationHolidays)
                .Include(o => o.OrganizationWorkDays)
                .ToListAsync();
            return orgs.Select(OrganizationMapper.ToDal).ToList();
        }

        //Get Organization by organization name
        public async Task<DalOrganization?> GetByNameAsync(string name)
        {
            var org = await _context.Organizations
                .Include(o => o.OrganizationHolidays)
                .Include(o => o.OrganizationWorkDays)
                .FirstOrDefaultAsync(o => o.Name == name);
            return org == null ? null : OrganizationMapper.ToDal(org);
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
        
        //Get Organization holidays
        public async Task<List<DalHoliday>> GetHolidaysByOrganizationIdAsync(int organizationId)
        {
            return await _context.OrganizationHolidays
                .Where(h => h.OrganizationId == organizationId)
                .Select(h => new DalHoliday { Name = h.Name, Month = h.Month, Day = h.Day })
                .ToListAsync();
        }
        
        //Get Organization Work Days
        public async Task<List<DalWorkDay>> GetWorkScheduleByOrganizationIdAsync(int organizationId)
        {
            return await _context.OrganizationWorkDays
                .Where(wd => wd.OrganizationId == organizationId)
                .Select(wd => new DalWorkDay 
                { 
                    DayOfWeek = wd.DayOfWeek, 
                    StartTime = string.Format("{0:hh\\:mm}", wd.StartTime), 
                    EndTime = string.Format("{0:hh\\:mm}", wd.EndTime) 
                })
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
        
        //Check if user created all entities (department, employees, shiftTypes, schedules)
        public async Task<DalOrganizationEntitiesCheckResult> CheckOrganizationEntities(int orgId)
        {
            return await _context.Organizations
                .Where(o => o.Id == orgId)
                .Select(o => new DalOrganizationEntitiesCheckResult
                {
                    Departments = o.Departments.Any(),
                    Employees = o.Employees.Any(),
                    ShiftTypes = o.ShiftTypes.Any(),
                    Schedules = o.Schedules.Any()
                })
                .FirstOrDefaultAsync() ?? new DalOrganizationEntitiesCheckResult();
        }
        
        //Check if organization belongs to employer
        public async Task<bool> IsOrganizationBelongsToEmployerAsync(int orgId, int employerId)
        {
            return await _context.Organizations.AnyAsync(o => o.Id == orgId && o.EmployerId == employerId);
        }
        
        //Get Organization Data
        public async Task<DalOrganizationData> GetOrganizationDataByIdAsync(int organizationId)
        {
            var organization = await _context.Organizations
                .Where(o => o.Id == organizationId)
                .Select(o => new DalOrganizationData
                {
                    Id = o.Id,
                    Name = o.Name
                })
                .FirstOrDefaultAsync();

            if (organization == null)
                return null;

            organization.DepartmentCount = await _context.Departments
                .Where(g => g.OrganizationId == organizationId)
                .CountAsync();

            organization.ShiftTypeCount = await _context.ShiftTypes
                .Where(st => st.OrganizationId == organizationId)
                .CountAsync();

            organization.EmployeeCount = await _context.Employees
                .Where(e => e.OrganizationId == organizationId)
                .CountAsync();
            
            var scheduleSummary = await _scheduleRepository.GetScheduleSummaryAsync(organizationId);
            organization.ScheduleCount = scheduleSummary.ConfirmedSchedules.Count + scheduleSummary.UnconfirmedSchedules.Count;
            organization.ScheduleSummary = scheduleSummary;
            
            return organization;
        }

        
        //Create Organizations
        public async Task<DalOrganization> CreateAsync(DalOrganizationCreate createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employerExists = await _context.Employers.AnyAsync(e => e.Id == createDto.EmployerId);
                if (!employerExists)
                    throw new InvalidOperationException($"Employer with id {createDto.EmployerId} not found.");

                var organization = OrganizationMapper.ToDomain(createDto);

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();

                if (!organization.IsOpen24_7 && createDto.OrganizationWorkDays != null)
                {
                    var workDays = createDto.OrganizationWorkDays.Select(wk => new OrganizationWorkDay
                    {
                        DayOfWeek = wk.DayOfWeek,
                        StartTime = TimeSpan.Parse(wk.StartTime),
                        EndTime = TimeSpan.Parse(wk.EndTime),
                        OrganizationId = organization.Id
                    }).ToList();

                    _context.OrganizationWorkDays.AddRange(workDays);
                    organization.OrganizationWorkDays = workDays;
                }

                if (createDto.OrganizationHolidays != null && createDto.OrganizationHolidays.Any())
                {
                    var holidays = createDto.OrganizationHolidays.Select(h => new OrganizationHoliday
                    {
                        Name = h.Name,
                        Month = h.Month,
                        Day = h.Day,
                        OrganizationId = organization.Id
                    }).ToList();

                    _context.OrganizationHolidays.AddRange(holidays);
                    organization.OrganizationHolidays = holidays;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return OrganizationMapper.ToDal(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        
        // Update Organization by its id
        public async Task<DalOrganization> UpdateAsync(DalOrganizationUpdate updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var organization = await _context.Organizations
                    .Include(o => o.OrganizationHolidays)
                    .Include(o => o.OrganizationWorkDays)
                    .FirstOrDefaultAsync(o => o.Id == updateDto.OrganizationId);

                if (organization == null)
                    throw new InvalidOperationException($"Organization with id {updateDto.OrganizationId} not found.");

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

                _context.OrganizationHolidays.RemoveRange(organization.OrganizationHolidays);
                _context.OrganizationWorkDays.RemoveRange(organization.OrganizationWorkDays);

                var newHolidays = updateDto.OrganizationHolidays?
                    .Select(h => new OrganizationHoliday
                    {
                        Name = h.Name,
                        Month = h.Month,
                        Day = h.Day,
                        OrganizationId = organization.Id
                    }).ToList() ?? new();

                var newWorkDays = (!organization.IsOpen24_7 ? updateDto.OrganizationWorkDays : null)?
                    .Select(wk => new OrganizationWorkDay
                    {
                        DayOfWeek = wk.DayOfWeek,
                        StartTime = TimeSpan.Parse(wk.StartTime),
                        EndTime = TimeSpan.Parse(wk.EndTime),
                        OrganizationId = organization.Id
                    }).ToList() ?? new();

                _context.OrganizationHolidays.AddRange(newHolidays);
                _context.OrganizationWorkDays.AddRange(newWorkDays);

                organization.OrganizationHolidays = newHolidays;
                organization.OrganizationWorkDays = newWorkDays;

                _context.Organizations.Update(organization);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return OrganizationMapper.ToDal(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization");
                await transaction.RollbackAsync();
                throw;
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
