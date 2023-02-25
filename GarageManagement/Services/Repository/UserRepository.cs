using Dapper;
using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using System.Data;

namespace GarageManagement.Services.Repository
{
    public class UserRepository : IUserRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public UserRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region CRUD TABLE USER_TYPE
        public async Task<IEnumerable<UserTypeDto>> getAllUserType()
        {
            string query = "select * from [dbo].[UserType]";
            using var connection = _DbContext.CreateConnection();
            IEnumerable<UserType> userTypes = await connection.QueryAsync<UserType>(query);

            IEnumerable<UserTypeDto> userTypeDto = new List<UserTypeDto>();
            if (userTypes != null)
            {
                userTypeDto = userTypes.Adapt<IEnumerable<UserTypeDto>>();
                return userTypeDto;
            }
            return userTypeDto;
        }
        public UserTypeDto getTypeUser(string TypeCode)
        {
            var query = "select * from [dbo].[UserType] where TypeCode = @TypeCode";

            using var con = _DbContext.CreateConnection();

            UserType userType = con.QueryFirstOrDefault<UserType>(query, new { TypeCode });
            var userTypeDto = new UserTypeDto();
            if (userType != null)
            {
                userTypeDto = userType.Adapt<UserTypeDto>();
                return userTypeDto;
            }
            return userTypeDto;
        }
        public UserTypeDto getUserType(Guid IdUserType)
        {
            var query = "select * from [dbo].[UserType] where Id = @Id";

            using var con = _DbContext.CreateConnection();
            UserType userType = con.QueryFirstOrDefault<UserType>(query, new { Id = IdUserType });

            var userTypeDto = new UserTypeDto();
            if (userType != null)
            {
                userTypeDto = userType.Adapt<UserTypeDto>();
                return userTypeDto;
            }
            return userTypeDto;
        }
        #endregion

        #region CRUD TABLE USER
        public async Task<TemplateApi> getAllUserAvailable(int pageNumber, int pageSize)
        {
            string query = "select *from [dbo].[User] where IsDeleted = @IsDeleted and IsLocked = @IsLocked order by CreatedDate desc";
            using var connection = _DbContext.CreateConnection();

            IEnumerable<User> users = await connection.QueryAsync<User>(query, new { IsDeleted = false, IsLocked = false });
            int coutRecord = users.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                users = users.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<UserDto> userDtos = users.Adapt<IEnumerable<UserDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = userDtos.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
            return templateApi;
        }
        public async Task<TemplateApi> RemoveUserByList(List<Guid> IdUser, Guid IdUserCurrent)
        {
            var query = "update [dbo].[User] set IsDeleted = @IsDeleted, Email = @Email where Id = @Id";
            var queryById = "select *from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            try
            {
                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = IdUserCurrent }, tran);

                if (userByIDCurrentLogin != null)
                {
                    for (int i = 0; i < IdUser.Count; i++)
                    {
                        saveDiaryModel = new SaveDiaryModel()
                        {
                            IdUserCurrent = userByIDCurrentLogin.Id,
                            IdWith = IdUser[i],
                            Operation = "Delete",
                            Table = "User",
                            Content = "",
                            Fullname = userByIDCurrentLogin.Fullname ?? "",
                            IsSuccess = true,
                        };
                        User user = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = IdUser[i] }, tran);
                        if (user == null)
                        {
                            //save to diary if false
                            saveDiaryModel.IsSuccess = false;
                            Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                            await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                            continue;
                        }

                        await connection.ExecuteAsync(query, new { Id = IdUser[i], IsDeleted = true, Email = $"{user.Email}/{user.Id}" }, tran);

                        //save diary if success
                        Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                        await connection.ExecuteAsync(querySaveDiary, diary, tran);
                    }
                }

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Xóa thành công !"
                };
                return response;

            }
            catch (Exception)
            {
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> LockAccountUserByList(List<Guid> IdUser, bool isLock, Guid IdUserCurrent)
        {
            var queryById = "select * from [dbo].[User] where Id = @Id";
            var query = "update [dbo].[User] set IsLocked = @IsLocked where Id = @Id";
            var content = isLock ? "Khóa tài khoản" : "Mở khóa tài khoản";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            try
            {
                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = IdUserCurrent }, tran);

                if (userByIDCurrentLogin != null)
                {
                    for (int i = 0; i < IdUser.Count; i++)
                    {
                        saveDiaryModel = new SaveDiaryModel()
                        {
                            IdUserCurrent = userByIDCurrentLogin.Id,
                            IdWith = IdUser[i],
                            Operation = "Update",
                            Table = "User",
                            Content = content,
                            Fullname = userByIDCurrentLogin.Fullname ?? "",
                            IsSuccess = true,
                        };

                        User userById = connection.QueryFirstOrDefault<User>(queryById, new { Id = IdUser[i] }, tran);
                        if (userById == null)
                        {
                            //save to diary if false
                            saveDiaryModel.IsSuccess = false;
                            Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                            await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                            continue;
                        }

                        await connection.ExecuteAsync(query, new { IsLocked = isLock, Id = IdUser[i] }, tran);

                        //save diary if success
                        Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                        await connection.ExecuteAsync(querySaveDiary, diary, tran);
                    }
                }
                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = $"{content} thành công !"
                };
                return response;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public UserDto getUserByID(Guid IdUser)
        {
            var query = "select * from [dbo].[User] where Id = @Id";

            using var con = _DbContext.CreateConnection();
            User user = con.QueryFirstOrDefault<User>(query, new { Id = IdUser });
            var userDto = new UserDto();
            if (user != null)
            {
                userDto = user.Adapt<UserDto>();
                return userDto;
            }
            return userDto;
        }
        public async Task<TemplateApi> getAllUser(int pageNumber, int pageSize)
        {
            string query = "select *from [dbo].[User] where IsDeleted = @IsDeleted order by CreatedDate desc";
            using var connection = _DbContext.CreateConnection();
            IEnumerable<User> users = await connection.QueryAsync<User>(query, new { IsDeleted = false });
            int coutRecord = users.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                users = users.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<UserDto> userDtos = users.Adapt<IEnumerable<UserDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = userDtos.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
            return templateApi;
        }
        public async Task<TemplateApi> getUserById(Guid IdUser)
        {
            var query = "select *from [dbo].[User] where Id = @Id";

            using var con = _DbContext.CreateConnection();
            User user = await con.QueryFirstOrDefaultAsync<User>(query, new { Id = IdUser });

            if (user != null)
            {
                UserDto userDto = user.Adapt<UserDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = userDto,
                    Message = "Lấy user thành công",
                    Success = true,
                    Fail = false,
                    TotalElement = 1,
                };
                return templateApi;
            }
            else
            {
                var templateApi = new TemplateApi()
                {
                    Message = "Không tìm thấy user",
                    Success = false,
                    Fail = true,
                };
                return templateApi;
            }
        }
        public async Task<TemplateApi> InsertUser(UserDto newUser)
        {
            var query = @"INSERT INTO [dbo].[User]
                           ([Id]
                           ,[Fullname]
                           ,[Description]
                           ,[Password]
                           ,[Email]
                           ,[Phone]
                           ,[UserTypeId]
                           ,[Address]
                           ,[Status]
                           ,[CreatedDate]
                           ,[UserCode]
                           ,[IsLocked]
                           ,[IsDeleted]
                           ,[UnitId]
                           ,[IsActive]
                           ,[CreatedBy]
                           ,[ActiveCode]
                           ,[Avatar])
                     VALUES
                           (@Id
                           ,@Fullname
                           ,@Description
                           ,@Password
                           ,@Email
                           ,@Phone
                           ,@UserTypeId
                           ,@Address
                           ,@Status
                           ,@CreatedDate
                           ,@UserCode
                           ,@IsLocked
                           ,@IsDeleted
                           ,@UnitId
                           ,@IsActive
                           ,@CreatedBy
                           ,@ActiveCode
                           ,@Avatar)";
            var queryRole = "select * from [dbo].[Role] where RoleName = @RoleName";
            var queryInsertUserRole = @"INSERT INTO [dbo].[User_Role]
                           ([Id]
                           ,[IdRole]
                           ,[IdUser])
                     VALUES
                           (@Id, @IdRole, @IdUser)";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var IdUser_Role = new Guid();

            try
            {
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = newUser.Id,
                    IdWith = newUser.Id,
                    Operation = "Create",
                    Table = "User",
                    Content = "đăng kí tài khoản",
                    Fullname = newUser.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table role
                var parameters = new DynamicParameters();
                parameters.Add("RoleName", "User", DbType.String);
                Role role = connection.QueryFirstOrDefault<Role>(queryRole, parameters, tran);
                if (role != null)
                {
                    var user_Role = new User_Role()
                    {
                        Id = Guid.NewGuid(),
                        IdUser = newUser.Id,
                        IdRole = role.Id,
                    };

                    IdUser_Role = user_Role.Id;

                    var user = new User();
                    user = newUser.Adapt<User>();

                    await connection.ExecuteAsync(query, user, tran);
                    await connection.ExecuteAsync(queryInsertUserRole, user_Role, tran);

                    //save to diary if success
                    //save by table user
                    Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diary, tran);
                    //save by table user role
                    diary.Table = "User_Role";
                    diary.WithId = IdUser_Role;
                    diary.Id = Guid.NewGuid();
                    await connection.ExecuteAsync(querySaveDiary, diary, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = true,
                        Fail = false,
                        Message = "Thêm mới thành công !"
                    };
                    return response;
                }
                else
                {
                    //save to diary if false
                    //save by table user
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);
                    //save by table user role
                    diaryFalse.Table = "User_Role";
                    diaryFalse.WithId = IdUser_Role;
                    diaryFalse.Id = Guid.NewGuid();
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy role cho user "
                    };
                    return response;
                }

            }
            catch (Exception)
            {
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> InsertUserByAdmin(UserDto newUser)
        {
            var query = @"INSERT INTO [dbo].[User]
                           ([Id]
                           ,[Fullname]
                           ,[Description]
                           ,[Password]
                           ,[Email]
                           ,[Phone]
                           ,[UserTypeId]
                           ,[Address]
                           ,[Status]
                           ,[CreatedDate]
                           ,[UserCode]
                           ,[IsLocked]
                           ,[IsDeleted]
                           ,[UnitId]
                           ,[IsActive]
                           ,[CreatedBy]
                           ,[ActiveCode]
                           ,[Avatar])
                     VALUES
                           (@Id
                           ,@Fullname
                           ,@Description
                           ,@Password
                           ,@Email
                           ,@Phone
                           ,@UserTypeId
                           ,@Address
                           ,@Status
                           ,@CreatedDate
                           ,@UserCode
                           ,@IsLocked
                           ,@IsDeleted
                           ,@UnitId
                           ,@IsActive
                           ,@CreatedBy
                           ,@ActiveCode
                           ,@Avatar)";
            var queryRole = "select * from [dbo].[Role] where RoleName = @RoleName";
            var queryInsertUserRole = @"INSERT INTO [dbo].[User_Role]
                           ([Id]
                           ,[IdRole]
                           ,[IdUser])
                     VALUES
                           (@Id, @IdRole, @IdUser)";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";
            var queryById = "select *from [dbo].[User] where Id = @Id";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var IdUser_Role = new Guid();

            try
            {
                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = newUser.IdUserCurrent }, tran);

                if (userByIDCurrentLogin != null)
                {
                    saveDiaryModel = new SaveDiaryModel()
                    {
                        IdUserCurrent = userByIDCurrentLogin.Id,
                        IdWith = newUser.Id,
                        Operation = "Create",
                        Table = "User",
                        Content = newUser.Fullname ?? "",
                        Fullname = userByIDCurrentLogin.Fullname ?? "",
                        IsSuccess = true,
                    };
                }

                //query table role
                var parameters = new DynamicParameters();
                parameters.Add("RoleName", "User", DbType.String);
                Role role = connection.QueryFirstOrDefault<Role>(queryRole, parameters, tran);
                if (role != null)
                {
                    var user_Role = new User_Role()
                    {
                        Id = Guid.NewGuid(),
                        IdUser = newUser.Id,
                        IdRole = role.Id,
                    };
                    IdUser_Role = user_Role.Id;

                    var user = new User();
                    user = newUser.Adapt<User>();

                    await connection.ExecuteAsync(query, user, tran);
                    await connection.ExecuteAsync(queryInsertUserRole, user_Role, tran);

                    //save to diary if success
                    //save by table user
                    Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diary, tran);
                    //save by table user role
                    diary.Table = "User_Role";
                    diary.WithId = IdUser_Role;
                    diary.Id = Guid.NewGuid();
                    await connection.ExecuteAsync(querySaveDiary, diary, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = true,
                        Fail = false,
                        Message = "Thêm mới thành công !"
                    };
                    return response;
                }
                else
                {
                    //save to diary if false
                    //save by table user
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);
                    //save by table user role
                    diaryFalse.Table = "User_Role";
                    diaryFalse.WithId = IdUser_Role;
                    diaryFalse.Id = Guid.NewGuid();
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);
                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy role cho user "
                    };
                    return response;
                }

            }
            catch (Exception)
            {
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateUser(UserDto userDto)
        {
            var query = @"UPDATE [dbo].[User] SET Fullname = @Fullname,
                                                Description = @Description,
                                                Phone = @Phone,
                                                UserTypeId = @UserTypeId,
                                                Address = @Address,
                                                UnitId = @UnitId,
                                                Avatar = @Avatar
                                                WHERE Id = @Id";
            var queryById = "select *from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            try
            {
                var user = new User();
                user = userDto.Adapt<User>();

                //query user by iduser
                User userByID = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { userDto.Id }, tran);

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = userDto.IdUserCurrent }, tran);

                if (userByIDCurrentLogin != null)
                {
                    saveDiaryModel = new SaveDiaryModel()
                    {
                        IdUserCurrent = userByIDCurrentLogin.Id,
                        IdWith = userDto.Id,
                        Operation = "Update",
                        Table = "User",
                        Content = userDto.Fullname ?? "",
                        Fullname = userByIDCurrentLogin.Fullname ?? "",
                        IsSuccess = true,
                    };
                }

                if (userByID == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(query, user, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Cập nhật thành công !"
                };
                return response;
            }
            catch (Exception)
            {

                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> RemoveUser(Guid IdUser, Guid IdUserCurrent)
        {
            var query = "update [dbo].[User] set IsDeleted = @IsDeleted, Email = @Email where Id = @Id";
            var queryById = "select *from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            try
            {
                //query user by iduser
                User user = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = IdUser }, tran);

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = IdUser,
                    Operation = "Delete",
                    Table = "User",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (user == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);
                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(query, new { Id = IdUser, IsDeleted = true, Email = $"{user.Email}/{user.Id}" }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Xóa thành công !"
                };
                return response;

            }
            catch (Exception)
            {
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> LockAccountUser(Guid IdUser, bool isLock, Guid IdUserCurrent)
        {
            var queryById = "select * from [dbo].[User] where Id = @Id";
            var query = "update [dbo].[User] set IsLocked = @IsLocked where Id = @Id";
            var content = isLock ? "khóa tài khoản" : "Mở khóa tài khoản";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            try
            {
                //query user by id
                User userById = connection.QueryFirstOrDefault<User>(queryById, new { Id = IdUser }, tran);

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = IdUserCurrent }, tran);

                if (userByIDCurrentLogin != null)
                {
                    saveDiaryModel = new SaveDiaryModel()
                    {
                        IdUserCurrent = userByIDCurrentLogin.Id,
                        IdWith = IdUser,
                        Operation = "Update",
                        Table = "User",
                        Content = content,
                        Fullname = userByIDCurrentLogin.Fullname ?? "",
                        IsSuccess = true,
                    };
                }

                if (userById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(query, new { IsLocked = isLock, Id = IdUser }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = $"{content} thành công !"
                };
                return response;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        #endregion

        #region Login And Regist Account
        public UserDto getUserByEmail(string email)
        {
            var query = "select * from [dbo].[User] where Email = @Email";

            using var con = _DbContext.CreateConnection();
            User user = con.QueryFirstOrDefault<User>(query, new { email });

            var userDto = new UserDto();
            if (user != null)
            {
                userDto = user.Adapt<UserDto>();
                return userDto;
            }
            return userDto;
        }
        public async Task<TemplateApi> ActiveUserByCode(string email, string code)
        {
            var query = "update [dbo].[User] set IsActive = @IsActive where Id = @Id";
            var queryByEmail = "select * from [dbo].[User] where Email = @Email";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            try
            {
                //query user by email
                User userByEmail = connection.QueryFirstOrDefault<User>(queryByEmail, new { email }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByEmail.Id,
                    IdWith = userByEmail.Id,
                    Operation = "Update",
                    Table = "User",
                    Content = "kích hoạt tài khoản bởi mã code",
                    Fullname = userByEmail.Fullname ?? "",
                    IsSuccess = true,
                };

                if (userByEmail == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(query, new { IsActive = true, userByEmail.Id }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Cập nhật thành công !"
                };
                return response;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateActiveCode(string code, string email)
        {
            var query = "update [dbo].[User] set ActiveCode = @ActiveCode where Id = @Id";
            var queryByEmail = "select * from [dbo].[User] where Email = @Email";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                //query user by email
                User userByEmail = connection.QueryFirstOrDefault<User>(queryByEmail, new { email }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByEmail.Id,
                    IdWith = userByEmail.Id,
                    Operation = "Update",
                    Table = "User",
                    Content = "cập nhật mã kích hoạt của tài khoản",
                    Fullname = userByEmail.Fullname ?? "",
                    IsSuccess = true,
                };

                if (userByEmail == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(query, new { ActiveCode = code, userByEmail.Id }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);
                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Cập nhật thành công !"
                };
                return response;
            }
            catch (Exception)
            {

                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdatePassword(string email, string newPassword)
        {
            var query = "update [dbo].[User] set Password = @Password where Id = @Id";
            var queryByEmail = "select * from [dbo].[User] where Email = @Email";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                // query user by email
                User userByEmail = connection.QueryFirstOrDefault<User>(queryByEmail, new { email }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByEmail.Id,
                    IdWith = userByEmail.Id,
                    Operation = "Update",
                    Table = "User",
                    Content = "cập nhật mật khẩu của tài khoản",
                    Fullname = userByEmail.Fullname ?? "",
                    IsSuccess = true,
                };

                if (userByEmail == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(query, new { Password = newPassword, userByEmail.Id }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Cập nhật thành công !"
                };
                return response;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        #endregion

        #region CURD TABLE USER_ROLE
        public User_RoleDto getRoleOfUser(Guid IdUSer)
        {
            var query = "select * from [dbo].[User_Role] where IdUser = @IdUser";

            using var con = _DbContext.CreateConnection();

            User_Role user_Role = con.QueryFirstOrDefault<User_Role>(query, new { IdUSer });
            var user_RoleDto = new User_RoleDto();
            if (user_Role != null)
            {
                user_RoleDto = user_Role.Adapt<User_RoleDto>();
                return user_RoleDto;
            }
            return user_RoleDto;
        }
        public List<User_RoleDto> getListRoleOfUser(Guid IdUSer)
        {
            var query = "select * from [dbo].[User_Role] where IdUser = @IdUser";

            using var con = _DbContext.CreateConnection();
            var user_Roles = con.Query(query, new { IdUSer });
            var user_RoleDtos = new List<User_RoleDto>();

            if (user_Roles != null)
            {
                user_RoleDtos = user_Roles.Adapt<List<User_RoleDto>>();
                return user_RoleDtos;
            }
            return user_RoleDtos;
        }
        public async Task<TemplateApi> InsertUser_Role(Guid IdRole, Guid IdUser, Guid IdUserCurrent)
        {
            var query = @"INSERT INTO [dbo].[User_Role]
                           ([Id]
                           ,[IdRole]
                           ,[IdUser])
                     VALUES
                           (@Id, @IdRole, @IdUser)";
            var queryTableRole = @"select * from Role where Id = @IdRole";
            var queryById = "select * from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                // query table role
                Role roleById = await connection.QueryFirstOrDefaultAsync<Role>(queryTableRole, new { IdRole }, tran);

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = IdUser,
                    Operation = "Create",
                    Table = "User_Role",
                    Content = $"thêm role {roleById.RoleName} cho user",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (roleById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                var user_Role = new User_Role()
                {
                    Id = Guid.NewGuid(),
                    IdUser = IdUser,
                    IdRole = roleById.Id,
                };

                await connection.ExecuteAsync(query, user_Role, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Thêm mới thành công !"
                };
                return response;
            }
            catch (Exception)
            {
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> DeleteUser_Role(Guid IdUser, Guid IdRole, Guid IdUserCurrent)
        {
            var query = "delete from User_Role where IdUser = @IdUser and IdRole = @IdRole";
            var queryById = "select * from User_Role where IdUser = @IdUser and IdRole = @IdRole";
            var queryUserById = "select * from [dbo].[User] where Id = @Id";
            var queryTableRole = @"select * from Role where Id = @IdRole";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
                           ([Id]
                           ,[Content]
                           ,[UserId]
                           ,[DateCreate]
                           ,[Title]
                           ,[Operation]
                           ,[Table]
                           ,[IsSuccess]
                           ,[UserName]
                           ,[WithId])
                     VALUES
                           (@Id ,@Content, @UserId, @DateCreate, @Title, @Operation, @Table, @IsSuccess, @UserName, @WithId)";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                // query table role
                Role roleById = await connection.QueryFirstOrDefaultAsync<Role>(queryTableRole, new { IdRole }, tran);

                //query table user_role
                User_Role user_Role = await connection.QueryFirstOrDefaultAsync<User_Role>(queryById, new { IdUser, IdRole }, tran);

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryUserById, new { Id = IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = IdUser,
                    Operation = "Delete",
                    Table = "User_Role",
                    Content = $"xóa role {roleById.RoleName} cho user",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (user_Role == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(query, new { IdRole, IdUser }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Xóa thành công !"
                };
                return response;

            }
            catch (Exception)
            {
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        #endregion

        #region CRUD TABLE ROLE
        public RoleDto getUserRole(string roleType)
        {
            var query = "select * from [dbo].[Role] where RoleName = @RoleName";

            using var con = _DbContext.CreateConnection();
            Role role = con.QueryFirstOrDefault<Role>(query, new { roleType });

            var roleDto = new RoleDto();
            if (role != null)
            {
                roleDto = role.Adapt<RoleDto>();
                return roleDto;
            }
            return roleDto;
        }
        public RoleDto getUserRolebyId(Guid IdRole)
        {
            var query = "select * from [dbo].[Role] where Id = @IdRole";

            using var con = _DbContext.CreateConnection();
            Role role = con.QueryFirstOrDefault<Role>(query, new { IdRole });

            var roleDto = new RoleDto();
            if (role != null)
            {
                roleDto = role.Adapt<RoleDto>();
                return roleDto;
            }
            return roleDto;
        }
        public List<RoleDto> getAllRole()
        {
            var query = "select * from [dbo].[Role] where IsDeleted = @IsDeleted";

            using var con = _DbContext.CreateConnection();
            var roles = con.Query(query, new { IsDeleted = false });

            var roleDto = new List<RoleDto>();
            if (roles != null)
            {
                roleDto = roles.Adapt<List<RoleDto>>();
            }
            return roleDto;
        }
        public async Task<TemplateApi> getRoleByIdUser(Guid IdUser)
        {
            var query = "select r.* from [dbo].[Role] r, User_Role ur, [dbo].[User] u where ur.IdUser = u.Id and ur.IdRole = r.Id and u.Id = @IdUser";

            using var con = _DbContext.CreateConnection();
            Role role = await con.QueryFirstOrDefaultAsync<Role>(query, new { IdUser });

            if (role != null)
            {
                RoleDto roleDto = role.Adapt<RoleDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = roleDto,
                    Message = "Lấy thành công",
                    Success = true,
                    Fail = false,
                    TotalElement = 1,
                };
                return templateApi;
            }
            else
            {
                var templateApi = new TemplateApi()
                {
                    Message = "Không tìm thấy user",
                    Success = false,
                    Fail = true,
                };
                return templateApi;
            }
        }
        #endregion
    }
}
