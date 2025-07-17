using DTOs.GroupDtos;

public interface IGroupService
{
    Task<BllGroup?> GetByIdAsync(int groupId);
    Task<List<BllGroup>> GetAllByOrganizationIdAsync(int organizationId);
    Task<BllGroup> CreateAsync(BllGroupCreate dto, int organizationId);
    Task<BllGroup?> UpdateAsync(BllGroup updateDto);
    Task<bool> UpdateAutorenewStatusAsync(int groupId, bool newStatus);
    Task<bool> DeleteAsync(int groupId);
}
