using BLL.DTO.IdentityDtos;
using DAL.DTO.EmployerDtos;

namespace BLL.Mappers;

public class IdentityMapper
{
    public static DalEmployerCreate MapToDal(BllEmployerRegister bll)
    {
        return new DalEmployerCreate
        {
            FirstName = bll.FirstName,
            LastName = bll.LastName,
            Email = bll.Email,
            Phone = bll.Phone,
            Password = bll.Password
        };
    }
}