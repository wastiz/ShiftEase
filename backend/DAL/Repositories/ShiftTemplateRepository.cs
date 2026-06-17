using DAL.Contracts;
using DAL.DTO.ShiftTypeDtos;
using DAL.Mappers;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ShiftTemplateRepository : IShiftTemplateRepository
    {
        private readonly AppDbContext _context;

        public ShiftTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DalShiftTemplate?> GetByIdAsync(int id)
        {
            var st = await _context.ShiftTypes.FirstOrDefaultAsync(s => s.Id == id);
            if (st == null) return null;

            return ShiftTemplateMapper.ToDal(st);
        }

        public async Task<List<DalShiftTemplate>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _context.ShiftTypes
                .Where(st => st.DepartmentId == departmentId)
                .Select(st => ShiftTemplateMapper.ToDal(st))
                .ToListAsync();
        }

        public async Task<List<DalShiftTemplate>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _context.ShiftTypes
                .Where(st => st.OrganizationId == organizationId || st.Department.OrganizationId == organizationId)
                .Select(st => ShiftTemplateMapper.ToDal(st))
                .ToListAsync();
        }

        public async Task<DalShiftTemplate> CreateAsync(DalShiftTemplateCreate dto)
        {
            var entity = ShiftTemplateMapper.ToDomain(dto);

            _context.ShiftTypes.Add(entity);
            await _context.SaveChangesAsync();

            return ShiftTemplateMapper.ToDal(entity);
        }

        public async Task<DalShiftTemplate?> UpdateAsync(DalShiftTemplateUpdate dto)
        {
            var existing = await _context.ShiftTypes.FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (existing == null) return null;

            existing.Name = dto.Name;
            existing.Color = dto.Color;
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.MinEmployees = dto.MinEmployees;
            existing.MaxEmployees = dto.MaxEmployees;
            existing.BreakDuration = dto.BreakDuration;

            await _context.SaveChangesAsync();

            return ShiftTemplateMapper.ToDal(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var shiftType = await _context.ShiftTypes.FindAsync(id);
            if (shiftType == null) return false;

            _context.ShiftTypes.Remove(shiftType);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
