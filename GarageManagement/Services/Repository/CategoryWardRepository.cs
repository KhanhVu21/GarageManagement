using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using Mapster;
using Dapper;
using GarageManagement.Services.IRepository;

namespace GarageManagement.Services.Repository
{
    public class CategoryWardRepository: ICategoryWardRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategoryWardRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategoryWardByIdDistrict(int pageNumber, int pageSize, string DistrictCode)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryWard> CategoryWards = await connection.QueryAsync<CategoryWard>(QueryCategoryWard.queryGetAllCategoryWardByIdDistrict, new { DistrictCode = DistrictCode });
            int coutRecord = CategoryWards.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryWards = CategoryWards.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryWardDto> CategoryWardDtos = CategoryWards.Adapt<IEnumerable<CategoryWardDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryWardDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCategoryWard(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryWard> CategoryWards = await connection.QueryAsync<CategoryWard>(QueryCategoryWard.queryGetAllCategoryWard);
            int coutRecord = CategoryWards.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryWards = CategoryWards.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryWardDto> CategoryWardDtos = CategoryWards.Adapt<IEnumerable<CategoryWardDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryWardDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategoryWardById(Guid IdCategoryWard)
        {
            using var con = _DbContext.CreateConnection();
            CategoryWard CategoryWard = await con.QueryFirstOrDefaultAsync<CategoryWard>(QueryCategoryWard.queryGetByIdCategoryWard, new { Id = IdCategoryWard });

            if (CategoryWard != null)
            {
                CategoryWardDto CategoryWardDto = CategoryWard.Adapt<CategoryWardDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategoryWardDto,
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
        public async Task<TemplateApi> DeleteCategoryWard(Guid IdCategoryWard, Guid IdUserCurrent)
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
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryWard.queryGetUserById, new { Id = IdUserCurrent }, tran);
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table CategoryWard
                CategoryWard CategoryWard = await connection.QueryFirstOrDefaultAsync<CategoryWard>(QueryCategoryWard.queryGetByIdCategoryWard, new { Id = IdCategoryWard }, tran);

                if (CategoryWard is null)
                {
                    //save to diary if false    
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(QueryCategoryWard.queryDeleteCategoryWard, new { Id = CategoryWard.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryWard.Id, "Delete", "CategoryWard", ""));


                //save to CategoryWard_Deleted
                await connection.ExecuteAsync(QueryCategoryWard.queryInsertCategoryWard_Deleted, CategoryWard, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryWard.Id, "Create", "CategoryWard_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategoryWardByList(List<Guid> IdCategoryWard, Guid IdUserCurrent)
        {

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryWard.queryGetUserById, new { Id = IdUserCurrent }, tran);

            try
            {
                for (int i = 0; i < IdCategoryWard.Count; i++)
                {
                    //query CategoryWard by id
                    CategoryWard CategoryWardId = await connection.QueryFirstOrDefaultAsync<CategoryWard>(QueryCategoryWard.queryGetByIdCategoryWard, new { Id = IdCategoryWard[i] }, tran);

                    saveDiaryModel = new SaveDiaryModel()
                    {
                        IdUserCurrent = userByIDCurrentLogin.Id,
                        Fullname = userByIDCurrentLogin.Fullname ?? "",
                        IsSuccess = true,
                    };

                    if (CategoryWardId is null)
                    {
                        tran.Rollback();

                        response = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = "Đã có ID không tồn tại !"
                        };
                        return response;
                    }
                    await connection.ExecuteAsync(QueryCategoryWard.queryDeleteCategoryWard, new { Id = IdCategoryWard[i] }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryWard[i], "Delete", "CategoryWard", ""));

                    //save to CategoryWard_Deleted
                    await connection.ExecuteAsync(QueryCategoryWard.queryInsertCategoryWard_Deleted, CategoryWardId, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryWardId.Id, "Create", "CategoryWard_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diaries[i], tran);
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
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> InsertCategoryWard(CategoryWardDto CategoryWardDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryWard = CategoryWardDto.Adapt<CategoryWard>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryWard.queryGetUserById, new { Id = CategoryWardDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryWardDto.Id,
                    Operation = "Create",
                    Table = "CategoryWard",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCategoryWard.queryInsertCategoryWard, CategoryWard, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCategoryWard(CategoryWardDto CategoryWardDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryWard = CategoryWardDto.Adapt<CategoryWard>();

                //get CategoryWard by id
                CategoryWard CategoryWardById = await connection.QueryFirstOrDefaultAsync<CategoryWard>(QueryCategoryWard.queryGetByIdCategoryWard, new { CategoryWardDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryWard.queryGetUserById, new { Id = CategoryWardDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryWardDto.Id,
                    Operation = "Update",
                    Table = "CategoryWard",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CategoryWardById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCategoryWard.queryUpdateCategoryWard, CategoryWard, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> HideCategoryWardByList(List<Guid> IdCategoryWard, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin khu vực" : "Kích hoạt thông tin khu vực";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryWard.queryGetUserById, new { Id = IdUserCurrent }, tran);

            try
            {
                for (int i = 0; i < IdCategoryWard.Count; i++)
                {
                    //query CategoryWard by id
                    var CategoryWard = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryWard.queryGetByIdCategoryWard, new { Id = IdCategoryWard[i] }, tran);

                    saveDiaryModel = new SaveDiaryModel()
                    {
                        IdUserCurrent = userByIDCurrentLogin.Id,
                        Fullname = userByIDCurrentLogin.Fullname ?? "",
                        IsSuccess = true,
                    };

                    if (CategoryWard is null)
                    {
                        tran.Rollback();

                        response = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = "Đã có ID không tồn tại !"
                        };
                        return response;
                    }
                    await connection.ExecuteAsync(QueryCategoryWard.queryHideCategoryWard, new { Id = IdCategoryWard[i], IsHide = IsHide }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryWard[i], "Update", "CategoryWard", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategoryWard(Guid IdCategoryWard, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin khu vực" : "Hiển thị thông tin khu vực";


            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            //query CategoryWard by id
            var CategoryWard = await connection.QueryFirstOrDefaultAsync<CategoryWard>(QueryCategoryWard.queryGetByIdCategoryWard, new { Id = IdCategoryWard }, tran);

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryWard.queryGetUserById, new { Id = IdUserCurrent }, tran);

            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                IdWith = IdCategoryWard,
                Operation = "Update",
                Table = "CategoryWard",
                Content = content,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (CategoryWard is null)
            {
                //save to diary if false
                saveDiaryModel.IsSuccess = false;
                Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diaryFalse, tran);
                tran.Commit();

                response = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return response;
            }
            await connection.ExecuteAsync(QueryCategoryWard.queryHideCategoryWard, new { Id = IdCategoryWard, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCategoryWard.queryInsertDiary, diary, tran);

            tran.Commit();

            response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Ẩn thành công !"
            };
            return response;
        }
        #endregion
    }
    internal static class QueryCategoryWard
    {
        public const string queryInsertCategoryWard = @"INSERT INTO [dbo].[CategoryWard]
                               ([Id]
                               ,[WardName]
                               ,[WardCode]
                               ,[DistrictCode]
                               ,[Status]
                               ,[IsHide]
                               ,[CreatedDate])
                         VALUES (@Id, @WardName, @WardCode, @DistrictCode,
                                @Status, @IsHide,@CreatedDate)";
        public const string queryUpdateCategoryWard = @"UPDATE [dbo].[CategoryWard] SET WardName = @WardName,
                                        WardCode = @WardCode,
                                        DistrictCode = @DistrictCode
                                        WHERE Id = @Id";
        public const string queryDeleteCategoryWard = "DELETE FROM [dbo].[CategoryWard] WHERE Id = @Id";
        public const string queryGetByIdCategoryWard = "select * from [dbo].[CategoryWard] where Id = @Id";
        public const string queryGetAllCategoryWard = "select *from [dbo].[CategoryWard] order by CreatedDate desc";
        public const string queryHideCategoryWard = "UPDATE [dbo].[CategoryWard] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCategoryWard_Deleted = @"INSERT INTO [dbo].[CategoryWard_Deleted]
                               ([Id]
                               ,[WardName]
                               ,[WardCode]
                               ,[DistrictCode]
                               ,[Status]
                               ,[IsHide]
                               ,[CreatedDate])
                         VALUES (@Id, @WardName, @WardCode, @DistrictCode,
                                @Status, @IsHide,@CreatedDate)";
        public const string queryGetAllIdCategoryWard = "select Id from [dbo].[CategoryWard]";

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
        public const string queryGetAllCategoryWardByIdDistrict = "select *from [dbo].[CategoryWard] where DistrictCode = @DistrictCode order by CreatedDate desc";
    }
}
