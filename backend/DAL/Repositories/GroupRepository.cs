using Microsoft.EntityFrameworkCore;
using DAL;
using DAL.DTO.GroupDtos;
using Domain;

namespace DAL.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;

        public GroupRepository(AppDbContext context)
        {
            _context = context;
        }
        
        //Get group by its Id
        public async Task<DalGroup> GetByIdAsync(int groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            return new DalGroup()
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Color = group.Color,
                AutorenewSchedules = group.AutorenewSchedules
            };
        }
        
        //Get all groups by org Id
        public async Task<List<DalGroup>> GetAllByOrganizationIdAsync(int organizationId)
        {
            return await _context.Groups
                .Where(g => g.OrganizationId == organizationId)
                .Select(g => new DalGroup()
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Color = g.Color,
                    AutorenewSchedules = g.AutorenewSchedules
                })
                .ToListAsync();
        }
        
        //Create group
        public async Task<DalGroup> CreateAsync(DalGroupCreate dto)
        {
            var newGroup = new Group
            {
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color,
                AutorenewSchedules = dto.AutorenewSchedules,
                OrganizationId = dto.OrganizationId
            };

            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();

            return new DalGroup
            {
                Id = newGroup.Id,
                Name = newGroup.Name,
                Description = newGroup.Description,
                Color = newGroup.Color,
                AutorenewSchedules = newGroup.AutorenewSchedules
            };
        }
        
        //Update Group by its Id
        public async Task<DalGroup> UpdateAsync(DalGroup updatedGroup)
        {
            var group = await _context.Groups.FindAsync(updatedGroup.Id);
            if (group == null)
            {
                return null;
            }

            group.Name = updatedGroup.Name;
            group.Description = updatedGroup.Description;
            group.Color = updatedGroup.Color;

            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            return new DalGroup()
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Color = group.Color
            };
        }
        
        //Update autorenewal status
        public async Task<bool> UpdateAutorenewalAsync(int groupId, bool autorenewal)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) return false;

            group.AutorenewSchedules  = autorenewal;
            await _context.SaveChangesAsync();
            return true;
        }
        
        //Delete Group by its Id
        public async Task<bool> DeleteAsync(int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return false;
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
