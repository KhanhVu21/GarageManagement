using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GarageManagement.Attribute
{
    public class AuthorizeActionFilter : IAuthorizationFilter
    {
        private readonly string _permission;
        private readonly IUserRepository _userRepository;

        public AuthorizeActionFilter(string permission, IUserRepository userRepository)
        {
            _permission = permission;
            _userRepository = userRepository;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // authorization
            var idUserCurrent = context.HttpContext.Items["User"] != null ? (Guid)context.HttpContext.Items["User"] : Guid.Empty;

            if (_permission == "ADMIN")
            {
                bool isAuthorized = CheckAdminPermission(idUserCurrent);

                if (!isAuthorized)
                {
                    context.Result = new JsonResult(new
                    {
                        Success = false,
                        Fail = true,
                        Message = "Bạn cần đăng nhập tài khoản Admin"
                    })
                    { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
            if (_permission == "USER")
            {
                bool isAuthorized = CheckUserPermission(idUserCurrent);

                if (!isAuthorized)
                {
                    context.Result = new JsonResult(new
                    {
                        Success = false,
                        Fail = true,
                        Message = "Bạn cần đăng nhập tài khoản User hoặc Admin"
                    })
                    { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
        }
        private bool CheckAdminPermission(Guid idUserCurrent)
        {
            var user = _userRepository.getUserByID(idUserCurrent);
            // get list role of user
            List<User_RoleDto> user_Role = _userRepository.getListRoleOfUser(user.Id);
            // loop throught list role and check exits role admin
            for (int i = 0; i < user_Role.Count; i++)
            {
                var role = _userRepository.getUserRolebyId(user_Role[i].IdRole);
                if (role != null)
                {
                    if (role.IsAdmin)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CheckUserPermission(Guid idUserCurrent)
        {
            var user = _userRepository.getUserByID(idUserCurrent);
            // get list role of user
            List<User_RoleDto> user_Role = _userRepository.getListRoleOfUser(user.Id);
            // loop throught list role and check exits role admin
            for (int i = 0; i < user_Role.Count; i++)
            {
                var role = _userRepository.getUserRolebyId(user_Role[i].IdRole);
                if (role != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
