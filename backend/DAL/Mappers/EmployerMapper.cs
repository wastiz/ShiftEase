using DAL.DTO.EmployerDtos;

namespace DAL.Mappers;

public static class EmployerMapper
{
    public static DalEmployerFullData ToDal(Employer e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        Password = e.Password ?? string.Empty,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    public static Employer ToDomain(DalEmployerCreate dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Password = dto.Password,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
