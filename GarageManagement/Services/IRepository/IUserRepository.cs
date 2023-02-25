using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.IRepository
{
    public interface IUserRepository
    {
        #region CRUD TABLE USER_TYPE
        Task<IEnumerable<UserTypeDto>> getAllUserType();
        UserTypeDto getTypeUser(string TypeCode);
        UserTypeDto getUserType(Guid id);
        #endregion

        #region CURD TABLE USER_ROLE
        User_RoleDto getRoleOfUser(Guid IdUSer);
        List<User_RoleDto> getListRoleOfUser(Guid IdUser);
        Task<TemplateApi> InsertUser_Role(Guid IdRole, Guid IdUser, Guid IdUserCurrent);
        Task<TemplateApi> DeleteUser_Role(Guid IdUser, Guid IdRole, Guid IdUserCurrent);
        #endregion

        #region CRUD TABLE ROLE
        Task<TemplateApi> getRoleByIdUser(Guid IdUser);
        RoleDto getUserRole(string roleType);
        RoleDto getUserRolebyId(Guid Id);
        List<RoleDto> getAllRole();
        #endregion

        #region CRUD TABLE USER
        UserDto getUserByID(Guid Id);
        Task<TemplateApi> getAllUser(int pageNumber, int pageSize);
        Task<TemplateApi> getUserById(Guid IdUser);
        Task<TemplateApi> UpdateUser(UserDto userDto);
        Task<TemplateApi> InsertUser(UserDto newUser);
        Task<TemplateApi> InsertUserByAdmin(UserDto newUser);
        Task<TemplateApi> RemoveUser(Guid Id, Guid IdUserCurrent);
        Task<TemplateApi> LockAccountUser(Guid Id, bool isLock, Guid IdUserCurrent);
        Task<TemplateApi> RemoveUserByList(List<Guid> IdUser, Guid IdUserCurrent);
        Task<TemplateApi> LockAccountUserByList(List<Guid> IdUser, bool isLock, Guid IdUserCurrent);
        Task<TemplateApi> getAllUserAvailable(int pageNumber, int pageSize);

        #endregion

        #region Login And Regist Account
        UserDto getUserByEmail(string email);
        Task<TemplateApi> ActiveUserByCode(string email, string code);
        Task<TemplateApi> UpdateActiveCode(string code, string email);
        Task<TemplateApi> UpdatePassword(string email, string newPassword);
        #endregion
    }
}
