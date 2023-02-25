using Dapper;
using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;

namespace GarageManagement.Services.Repository
{
    public class EmployeeGroupRepository: IEmployeeGroupRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public EmployeeGroupRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllEmployeeGroupAndEmployees(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            var EmployeeGroups = await connection.QueryAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetAllEmployeeGroupAvailable);
            int coutRecord = EmployeeGroups.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                EmployeeGroups = EmployeeGroups.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var EmployeeGroupDtos = EmployeeGroups.Adapt<IEnumerable<EmployeeGroupDto>>().ToList();
            for (int i = 0; i < EmployeeGroupDtos.Count; i++)
            {
                var customers = await connection.QueryAsync<Employee>(QueryEmployeeGroup.queryGetEmployeeByIdGroup, new { IdGroup = EmployeeGroupDtos[i].Id });
                EmployeeGroupDtos[i].employees = customers.ToList();
            }
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = EmployeeGroupDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllEmployeeGroupAvailable(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<EmployeeGroup> EmployeeGroups = await connection.QueryAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetAllEmployeeGroupAvailable);
            int coutRecord = EmployeeGroups.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                EmployeeGroups = EmployeeGroups.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<EmployeeGroupDto> EmployeeGroupDtos = EmployeeGroups.Adapt<IEnumerable<EmployeeGroupDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = EmployeeGroupDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllEmployeeGroup(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<EmployeeGroup> EmployeeGroups = await connection.QueryAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetAllEmployeeGroup);
            int coutRecord = EmployeeGroups.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                EmployeeGroups = EmployeeGroups.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<EmployeeGroupDto> EmployeeGroupDtos = EmployeeGroups.Adapt<IEnumerable<EmployeeGroupDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = EmployeeGroupDtos.ToArray(),
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
        public async Task<TemplateApi> GetEmployeeGroupById(Guid IdEmployeeGroup)
        {
            using var con = _DbContext.CreateConnection();
            EmployeeGroup EmployeeGroup = await con.QueryFirstOrDefaultAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetByIdEmployeeGroup, new { Id = IdEmployeeGroup });

            if (EmployeeGroup != null)
            {
                EmployeeGroupDto EmployeeGroupDto = EmployeeGroup.Adapt<EmployeeGroupDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = EmployeeGroupDto,
                    Message = "Lấy thông tin thành công",
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
                    Message = "Không tìm thấy kết quả",
                    Success = false,
                    Fail = true,
                };
                return templateApi;
            }
        }
        public async Task<TemplateApi> DeleteEmployeeGroup(Guid IdEmployeeGroup, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table EmployeeGroup
                EmployeeGroup EmployeeGroup = await connection.QueryFirstOrDefaultAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetByIdEmployeeGroup, new { Id = IdEmployeeGroup }, tran);

                //query Engineer by idGroup
                Guid EngineerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryEmployeeGroup.queryGetAllEmployeeByIdGroup, new { IdGroup = IdEmployeeGroup }, tran);

                if (EmployeeGroup is null || EngineerId != Guid.Empty)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = EmployeeGroup is null ? "Đã có ID không tồn tại !" : "Đã có thợ sửa trong nhóm này !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryEmployeeGroup.queryDeleteEmployeeGroup, new { Id = EmployeeGroup.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, EmployeeGroup.Id, "Delete", "EmployeeGroup", ""));


                //save to EmployeeGroup_Deleted
                await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertEmployeeGroup_Deleted, EmployeeGroup, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, EmployeeGroup.Id, "Create", "EmployeeGroup_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteEmployeeGroupByList(List<Guid> IdEmployeeGroup, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdEmployeeGroup.Count; i++)
                {
                    //query EmployeeGroup by id
                    EmployeeGroup EmployeeGroupId = await connection.QueryFirstOrDefaultAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetByIdEmployeeGroup, new { Id = IdEmployeeGroup[i] }, tran);

                    //query Engineer by idGroup
                    Guid EngineerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryEmployeeGroup.queryGetAllEmployeeByIdGroup, new { IdGroup = IdEmployeeGroup[i] }, tran);

                    if (EmployeeGroupId is null || EngineerId != Guid.Empty)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = EmployeeGroupId is null ? "Đã có ID không tồn tại !" : "Đã có thợ sửa trong nhóm này !"
                        };
                        return responseFalse;
                    }
                    await connection.ExecuteAsync(QueryEmployeeGroup.queryDeleteEmployeeGroup, new { Id = IdEmployeeGroup[i] }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdEmployeeGroup[i], "Delete", "EmployeeGroup", ""));

                    //save to EmployeeGroup_Deleted
                    await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertEmployeeGroup_Deleted, EmployeeGroupId, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, EmployeeGroupId.Id, "Create", "EmployeeGroup_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertDiary, diaries[i], tran);
                }

                tran.Commit();

                var response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Xóa thành công !"
                };
                return response;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> InsertEmployeeGroup(EmployeeGroupDto EmployeeGroupDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var EmployeeGroup = EmployeeGroupDto.Adapt<EmployeeGroup>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeGroup.queryGetUserById, new { Id = EmployeeGroupDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = EmployeeGroupDto.Id,
                    Operation = "Create",
                    Table = "EmployeeGroup",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertEmployeeGroup, EmployeeGroup, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertDiary, diary, tran);

                tran.Commit();

                var response = new TemplateApi()
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
        public async Task<TemplateApi> UpdateEmployeeGroup(EmployeeGroupDto EmployeeGroupDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                var EmployeeGroup = EmployeeGroupDto.Adapt<EmployeeGroup>();

                //get EmployeeGroup by id
                EmployeeGroup EmployeeGroupById = await connection.QueryFirstOrDefaultAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetByIdEmployeeGroup, new { EmployeeGroupDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer.queryGetUserById, new { Id = EmployeeGroupDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (EmployeeGroupById == null)
                {
                    tran.Commit();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryEmployeeGroup.queryUpdateEmployeeGroup, EmployeeGroup, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, EmployeeGroupDto.Id, "Update", "EmployeeGroup", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertDiary, diaries[i], tran);
                }

                tran.Commit();

                var response = new TemplateApi()
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
        public async Task<TemplateApi> HideEmployeeGroupByList(List<Guid> IdEmployeeGroup, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin loại thương hiệu xe" : "Kích hoạt thông tin loại phương tiện xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdEmployeeGroup.Count; i++)
                {
                    //query EmployeeGroup by id
                    var EmployeeGroup = await connection.QueryFirstOrDefaultAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetByIdEmployeeGroup, new { Id = IdEmployeeGroup[i] }, tran);

                    //query Engineer by idGroup
                    Guid EngineerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryEmployeeGroup.queryGetAllEmployeeByIdGroup, new { IdGroup = IdEmployeeGroup[i] }, tran);

                    if (EmployeeGroup is null || EngineerId != Guid.Empty && IsHide)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = EmployeeGroup is null ? "Đã có ID không tồn tại !" : "Đã có thợ sửa trong nhóm này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryEmployeeGroup.queryHideEmployeeGroup, new { Id = IdEmployeeGroup[i], IsHide = IsHide }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdEmployeeGroup[i], "Update", "EmployeeGroup", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertDiary, diaries[i], tran);
                }

                tran.Commit();

                var response = new TemplateApi()
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
        public async Task<TemplateApi> HideEmployeeGroup(Guid IdEmployeeGroup, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin loại thương hiệu xe" : "Hiển thị thông tin loại phương tiện xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query EmployeeGroup by id
            var EmployeeGroup = await connection.QueryFirstOrDefaultAsync<EmployeeGroup>(QueryEmployeeGroup.queryGetByIdEmployeeGroup, new { Id = IdEmployeeGroup }, tran);

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query Engineer by idGroup
            Guid EngineerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryEmployeeGroup.queryGetAllEmployeeByIdGroup, new { IdGroup = IdEmployeeGroup}, tran);

            if (EmployeeGroup is null || EngineerId != Guid.Empty && IsHide)
            {
                tran.Rollback();

                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = EmployeeGroup is null ? "Đã có ID không tồn tại !" : "Đã có thợ sửa trong nhóm này !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryEmployeeGroup.queryHideEmployeeGroup, new { Id = IdEmployeeGroup, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertDiary, diary, tran);

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryEmployeeGroup.queryInsertDiary, diaries[i], tran);
            }

            tran.Commit();

            var response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Ẩn thành công !"
            };
            return response;
        }
        #endregion
    }
    internal static class QueryEmployeeGroup
    {
        public const string queryInsertEmployeeGroup = @"INSERT INTO [dbo].[EmployeeGroup]
                               ([Id]
                               ,[GroupName]
                               ,[GroupCode]
                               ,[Status]
                               ,[CreateDate]
                               ,[IsHide])
                         VALUES (@Id, @GroupName, @GroupCode, @Status,
                                @CreateDate, @IsHide)";
        public const string queryUpdateEmployeeGroup = @"UPDATE [dbo].[EmployeeGroup] SET GroupName = @GroupName,
                                        GroupCode = @GroupCode
                                        WHERE Id = @Id";
        public const string queryDeleteEmployeeGroup = "DELETE FROM [dbo].[EmployeeGroup] WHERE Id = @Id";
        public const string queryGetByIdEmployeeGroup = "select * from [dbo].[EmployeeGroup] where Id = @Id";
        public const string queryGetAllEmployeeGroup = "select *from [dbo].[EmployeeGroup] order by CreateDate desc";
        public const string queryGetAllEmployeeGroupAvailable = "select *from [dbo].[EmployeeGroup] order by CreateDate desc";
        public const string queryHideEmployeeGroup = "UPDATE [dbo].[EmployeeGroup] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertEmployeeGroup_Deleted = @"INSERT INTO [dbo].[EmployeeGroup_Deleted]
                               ([Id]
                               ,[GroupName]
                               ,[GroupCode]
                               ,[Status]
                               ,[CreateDate]
                               ,[IsHide])
                         VALUES (@Id, @GroupName, @GroupCode, @Status,
                                @CreateDate, @IsHide)";
        public const string queryGetAllIdEmployeeGroup = "select Id from [dbo].[EmployeeGroup]";
        public const string queryGetAllEmployeeByIdGroup = "select Id from [dbo].[Employee] where IdGroup = @IdGroup";
        public const string queryGetEmployeeByIdGroup = "select * from [dbo].[Employee] where IdGroup = @IdGroup";


        //query table user and diary
        public const string queryGetUserById = "select Id, Fullname from [dbo].[User] where Id = @Id";
        public const string queryInsertDiary = @"INSERT INTO [dbo].[Diary]
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
    }
}
