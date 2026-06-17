using DAL.DTO.ShiftTypeDtos;
using Domain;

namespace DAL.Mappers;

public static class ShiftTemplateMapper
{
    public static DalShiftTemplate ToDal(ShiftTemplate st) => new()
    {
        Id = st.Id,
        DepartmentId = st.DepartmentId,
        OrganizationId = st.OrganizationId,
        Name = st.Name,
        StartTime = st.StartTime,
        EndTime = st.EndTime,
        MinEmployees = st.MinEmployees,
        MaxEmployees = st.MaxEmployees,
        Color = st.Color,
        BreakDuration = st.BreakDuration
    };

    public static ShiftTemplate ToDomain(DalShiftTemplateCreate dto) => new()
    {
        Name = dto.Name,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        MinEmployees = dto.MinEmployees,
        MaxEmployees = dto.MaxEmployees,
        Color = dto.Color,
        BreakDuration = dto.BreakDuration,
        DepartmentId = dto.DepartmentId,
        OrganizationId = dto.OrganizationId
    };
}
