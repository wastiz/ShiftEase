using BLL.DTO.IdentityDtos;
using DAL.DTO.EmployerDtos;

namespace BLL.Mappers;

public class IdentityMapper
{
    public static DalEmployerCreate MapToDal(BllRegister bll)
    {
        return new DalEmployerCreate
        {
            FirstName = bll.FirstName,
            LastName = bll.LastName,
            Email = bll.Email,
            Password = bll.Password
        };
    }
}