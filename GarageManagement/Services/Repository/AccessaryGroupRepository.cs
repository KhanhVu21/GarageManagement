using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using Dapper;
using System.Text.RegularExpressions;

namespace GarageManagement.Services.Repository
{
    public class AccessaryGroupRepository: IAccessaryGroupRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public AccessaryGroupRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteAccessaryGroup(Guid IdAccessaryGroup, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessaryGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);

                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query AccessaryGroup by id
                AccessaryGroup AccessaryGroup = await connection.QueryFirstOrDefaultAsync<AccessaryGroup>(QueryAccessaryGroup.queryGetByIdAccessaryGroup, new { Id = IdAccessaryGroup }, tran);
                
                // get Accssesary by id IdGroup
                var Accessarys = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessaryGroup.queryGetAllAccessaryByGroupID, new { GroupID = IdAccessaryGroup }, tran);

                if (AccessaryGroup is null || Accessarys is not null)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = AccessaryGroup is null ? "Đã có ID không tồn tại !" : "Đã có phụ kiện trong nhóm này !"
                    };
                    return responseFalse;
                }

                //AccessaryGroup
                await connection.ExecuteAsync(QueryAccessaryGroup.queryDeleteAccessaryGroup, new { Id = IdAccessaryGroup }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAccessaryGroup, "Delete", "AccessaryGroup", ""));

                //save to AccessaryGroup_Deleted
                await connection.ExecuteAsync(QueryAccessaryGroup.queryInsertAccessaryGroup_Deleted, AccessaryGroup, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAccessaryGroup, "Create", "AccessaryGroup_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAccessaryGroup.queryInsertDiary, diaries[i], tran);
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
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> DeleteAccessaryGroupByList(List<Guid> IdAccessaryGroup, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessaryGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdAccessaryGroup.Count; i++)
                {
                    //query AccessaryGroup by id
                    AccessaryGroup AccessaryGroupId = await connection.QueryFirstOrDefaultAsync<AccessaryGroup>(QueryAccessaryGroup.queryGetByIdAccessaryGroup, new { Id = IdAccessaryGroup[i] }, tran);

                    // get Accssesary by id IdGroup
                    var Accessarys = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessaryGroup.queryGetAllAccessaryByGroupID, new { GroupID = IdAccessaryGroup[i] }, tran);

                    if (AccessaryGroupId is null || Accessarys is not null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = AccessaryGroupId is null ? "Đã có ID không tồn tại !" : "Đã có phụ kiện trong nhóm này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryAccessaryGroup.queryDeleteAccessaryGroup, new { Id = IdAccessaryGroup[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAccessaryGroup[i], "Delete", "AccessaryGroup", ""));

                    //save to AccessaryGroup_Deleted
                    await connection.ExecuteAsync(QueryAccessaryGroup.queryInsertAccessaryGroup_Deleted, AccessaryGroupId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAccessaryGroup[i], "Create", "AccessaryGroup_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAccessaryGroup.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllAccessaryGroup(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<AccessaryGroup> AccessaryGroups = await connection.QueryAsync<AccessaryGroup>(QueryAccessaryGroup.queryGetAllAccessaryGroup);
            int coutRecord = AccessaryGroups.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                AccessaryGroups = AccessaryGroups.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<AccessaryGroupDto> AccessaryGroupDtos = AccessaryGroups.Adapt<IEnumerable<AccessaryGroupDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = AccessaryGroupDtos.ToArray(),
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
        public async Task<TemplateApi> GetAccessaryGroupById(Guid IdAccessaryGroup)
        {
            using var con = _DbContext.CreateConnection();
            AccessaryGroup AccessaryGroup = await con.QueryFirstOrDefaultAsync<AccessaryGroup>(QueryAccessaryGroup.queryGetByIdAccessaryGroup, new { Id = IdAccessaryGroup });

            if (AccessaryGroup != null)
            {
                AccessaryGroupDto AccessaryGroupDto = AccessaryGroup.Adapt<AccessaryGroupDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = AccessaryGroupDto,
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
        public async Task<TemplateApi> InsertAccessaryGroup(AccessaryGroupDto AccessaryGroupDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var AccessaryGroup = AccessaryGroupDto.Adapt<AccessaryGroup>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessaryGroup.queryGetUserById, new { Id = AccessaryGroupDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryAccessaryGroup.queryInsertAccessaryGroup, AccessaryGroup, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, AccessaryGroup.Id, "Create", "AccessaryGroup", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAccessaryGroup.queryInsertDiary, diaries[i], tran);
                }
                tran.Commit();

                var response = new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Thêm mới thành công !"
                };
                return response;
            }
            catch(Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateAccessaryGroup(AccessaryGroupDto AccessaryGroupDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var AccessaryGroup = AccessaryGroupDto.Adapt<AccessaryGroup>();

            try
            {
                //get AccessaryGroup by id
                AccessaryGroup AccessaryGroupById = await connection.QueryFirstOrDefaultAsync<AccessaryGroup>(QueryAccessaryGroup.queryGetByIdAccessaryGroup, new { AccessaryGroupDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessaryGroup.queryGetUserById, new { Id = AccessaryGroupDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (AccessaryGroupById == null)
                {
                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryAccessaryGroup.queryUpdateAccessaryGroup, AccessaryGroup, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, AccessaryGroup.Id, "Update", "AccessaryGroup", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAccessaryGroup.queryInsertDiary, diaries[i], tran);
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
            catch(Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        #endregion
    }
    internal static class QueryAccessaryGroup
    {
        public const string queryInsertAccessaryGroup = @"INSERT INTO [dbo].[AccessaryGroup]
                                                                       ([Id]
                                                                       ,[GroupName]
                                                                       ,[Code]
                                                                       ,[CreatedDate]
                                                                       ,[Status])
                                                                 VALUES (@Id, @GroupName, @Code, @CreatedDate,
                                                                        @Status)";
        public const string queryUpdateAccessaryGroup = @"UPDATE [dbo].[AccessaryGroup] SET GroupName = @GroupName
                                                                        WHERE Id = @Id";
        public const string queryDeleteAccessaryGroup = "DELETE FROM [dbo].[AccessaryGroup] WHERE Id = @Id";
        public const string queryGetByIdAccessaryGroup = "select * from [dbo].[AccessaryGroup] where Id = @Id";
        public const string queryGetAllAccessaryGroup = "select *from [dbo].[AccessaryGroup] order by CreatedDate desc";
        public const string queryInsertAccessaryGroup_Deleted = @"INSERT INTO [dbo].[Delete_AccessaryGroup]
                                                                       ([Id]
                                                                       ,[GroupName]
                                                                       ,[Code]
                                                                       ,[CreatedDate]
                                                                       ,[Status])
                                                                 VALUES (@Id, @GroupName, @Code, @CreatedDate,
                                                                        @Status)";
        public const string queryGetAllIdAccessaryGroup = "select Id from [dbo].[AccessaryGroup]";
        public const string queryGetAllAccessaryByGroupID = "select Id from [dbo].[Accessary] where GroupID = @GroupID";

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
