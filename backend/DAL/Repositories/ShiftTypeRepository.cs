using DAL.DTO.ShiftTypeDtos;
using DAL.RepositoryInterfaces;
using Domain;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ShiftTypeRepository : IShiftTypeRepository
    {
        private readonly AppDbContext _context;

        public ShiftTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DalShiftType?> GetByIdAsync(int id)
        {
            var shiftType = await _context.ShiftTypes.FirstOrDefaultAsync(st => st.Id == id);
            return new DalShiftType()
            {
                Id = shiftType.Id,
                Name = shiftType.Name,
                StartTime = shiftType.StartTime,
                EndTime = shiftType.EndTime,
                EmployeeNeeded = shiftType.EmployeeNeeded,
                Color = shiftType.Color
            };
        }

        public async Task<List<DalShiftType>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _context.ShiftTypes
                .Where(st => st.OrganizationId == organizationId)
                .Select(st => new DalShiftType
                {
                    Id = st.Id,
                    Name = st.Name,
                    StartTime = st.StartTime,
                    EndTime = st.EndTime,
                    EmployeeNeeded = st.EmployeeNeeded,
                    Color = st.Color,
                })
                .ToListAsync();
        }

        public async Task<DalShiftType> CreateAsync(DalShiftType shiftTypeDto)
        {
            var entity = new ShiftType
            {
                Name = shiftTypeDto.Name,
                StartTime = shiftTypeDto.StartTime,
                EndTime = shiftTypeDto.EndTime,
                EmployeeNeeded = shiftTypeDto.EmployeeNeeded,
                Color = shiftTypeDto.Color,
                OrganizationId = shiftTypeDto.OrganizationId
            };

            _context.ShiftTypes.Add(entity);
            await _context.SaveChangesAsync();
            
            shiftTypeDto.Id = entity.Id;
            return shiftTypeDto;
        }


        public async Task<DalShiftType?> UpdateAsync(DalShiftType updateDto)
        {
            var existing = await _context.ShiftTypes
                .FirstOrDefaultAsync(s => s.Id == updateDto.Id);

            if (existing == null)
            {
                return null;
            }
                
            existing.Name = updateDto.Name;
            existing.Color = updateDto.Color;
            existing.StartTime = updateDto.StartTime;
            existing.EndTime = updateDto.EndTime;
            existing.EmployeeNeeded = updateDto.EmployeeNeeded;
            existing.OrganizationId = updateDto.OrganizationId;

            await _context.SaveChangesAsync();
            return updateDto;
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
