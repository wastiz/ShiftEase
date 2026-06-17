using DAL.Contracts;
using DAL.DTO.DepartmentDtos;
using DAL.Mappers;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;

        public DepartmentRepository(AppDbContext context)
        {
            _context = context;
        }
        
        //Get department by its Id
        public async Task<DalDepartment> GetByIdAsync(int departmentId)
        {
            var department = await _context.Departments
                .Include(g => g.ShiftTemplates)
                .FirstOrDefaultAsync(g => g.Id == departmentId);
            return DepartmentMapper.ToDal(department);
        }

        //Get all departments by org Id
        public async Task<List<DalDepartment>> GetAllByOrganizationIdAsync(int organizationId)
        {
            var departments = await _context.Departments
                .Include(g => g.ShiftTemplates)
                .Where(g => g.OrganizationId == organizationId)
                .ToListAsync();

            return departments.Select(DepartmentMapper.ToDal).ToList();
        }
        
        //Create department
        public async Task<DalDepartment> CreateAsync(DalDepartmentCreate dto)
        {
            var newDepartment = DepartmentMapper.ToDomain(dto);

            _context.Departments.Add(newDepartment);
            await _context.SaveChangesAsync();

            return DepartmentMapper.ToDal(newDepartment);
        }

        //Update Department by its Id
        public async Task<DalDepartment> UpdateAsync(DalDepartment updatedDepartment)
        {
            var department = await _context.Departments
                .Include(g => g.ShiftTemplates)
                .FirstOrDefaultAsync(g => g.Id == updatedDepartment.Id);
            if (department == null) return null;

            department.Name = updatedDepartment.Name;
            department.Description = updatedDepartment.Description;
            department.Color = updatedDepartment.Color;
            department.StartTime = updatedDepartment.StartTime;
            department.EndTime = updatedDepartment.EndTime;
            _context.Entry(department).Property(d => d.WorkingDays).CurrentValue =
                updatedDepartment.WorkingDays.Distinct().ToList();
            department.DefaultSchedulePattern = updatedDepartment.DefaultSchedulePattern;

            await _context.SaveChangesAsync();

            return DepartmentMapper.ToDal(department);
        }
        
        //Update autorenewal status
        public async Task<bool> UpdateAutorenewalAsync(int departmentId, bool autorenewal)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(g => g.Id == departmentId);

            if (department == null) return false;

            department.AutorenewSchedules  = autorenewal;
            await _context.SaveChangesAsync();
            return true;
        }
        
        //Delete Department by its Id
        public async Task<bool> DeleteAsync(int departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                return false;
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BelongsToOrganizationAsync(int departmentId, int orgId)
            => await _context.Departments.AnyAsync(d => d.Id == departmentId && d.OrganizationId == orgId);
    }
}
