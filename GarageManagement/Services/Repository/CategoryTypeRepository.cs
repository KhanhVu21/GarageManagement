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
    public class CategoryTypeRepository: ICategoryTypeRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategoryTypeRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategoryTypeAvailable(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryType> CategoryTypes = await connection.QueryAsync<CategoryType>(QueryCategoryType.queryGetAllCategoryTypeAvailable);
            int coutRecord = CategoryTypes.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryTypes = CategoryTypes.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryTypeDto> CategoryTypeDtos = CategoryTypes.Adapt<IEnumerable<CategoryTypeDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryTypeDtos.ToArray(),
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

        public async Task<TemplateApi> GetAllCategoryType(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryType> CategoryTypes = await connection.QueryAsync<CategoryType>(QueryCategoryType.queryGetAllCategoryType);
            int coutRecord = CategoryTypes.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryTypes = CategoryTypes.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryTypeDto> CategoryTypeDtos = CategoryTypes.Adapt<IEnumerable<CategoryTypeDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryTypeDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategoryTypeById(Guid IdCategoryType)
        {
            using var con = _DbContext.CreateConnection();
            CategoryType CategoryType = await con.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { Id = IdCategoryType });

            if (CategoryType != null)
            {
                CategoryTypeDto CategoryTypeDto = CategoryType.Adapt<CategoryTypeDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategoryTypeDto,
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
        public async Task<TemplateApi> DeleteCategoryType(Guid IdCategoryType, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryType.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table CategoryType
                CategoryType CategoryType = await connection.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { Id = IdCategoryType }, tran);

                //query Vehicle by CategoryModel
                Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryType.queryGetVehicleByIdCategoryType, new { IdType = IdCategoryType }, tran);

                if (CategoryType is null || vehicleId != Guid.Empty)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = CategoryType is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong loại xe này !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryCategoryType.queryDeleteCategoryType, new { Id = CategoryType.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryType.Id, "Delete", "CategoryType", ""));


                //save to CategoryType_Deleted
                await connection.ExecuteAsync(QueryCategoryType.queryInsertCategoryType_Deleted, CategoryType, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryType.Id, "Create", "CategoryType_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryType.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategoryTypeByList(List<Guid> IdCategoryType, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryType.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCategoryType.Count; i++)
                {
                    //query CategoryType by id
                    CategoryType CategoryTypeId = await connection.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { Id = IdCategoryType[i] }, tran);

                    //query Vehicle by CategoryModel
                    Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryType.queryGetVehicleByIdCategoryType, new { IdType = IdCategoryType[i] }, tran);

                    if (CategoryTypeId is null || vehicleId != Guid.Empty)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryTypeId is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong loại xe này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategoryType.queryDeleteCategoryType, new { Id = IdCategoryType[i] }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryType[i], "Delete", "CategoryType", ""));

                    //save to CategoryType_Deleted
                    await connection.ExecuteAsync(QueryCategoryType.queryInsertCategoryType_Deleted, CategoryTypeId, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryTypeId.Id, "Create", "CategoryType_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryType.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertCategoryType(CategoryTypeDto CategoryTypeDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryType = CategoryTypeDto.Adapt<CategoryType>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryType.queryGetUserById, new { Id = CategoryTypeDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryTypeDto.Id,
                    Operation = "Create",
                    Table = "CategoryType",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCategoryType.queryInsertCategoryType, CategoryType, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryType.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCategoryType(CategoryTypeDto CategoryTypeDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryType = CategoryTypeDto.Adapt<CategoryType>();

                //get CategoryType by id
                CategoryType CategoryTypeById = await connection.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { CategoryTypeDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryType.queryGetUserById, new { Id = CategoryTypeDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryTypeDto.Id,
                    Operation = "Update",
                    Table = "CategoryType",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CategoryTypeById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryType.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCategoryType.queryUpdateCategoryType, CategoryType, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryType.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> HideCategoryTypeByList(List<Guid> IdCategoryType, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin loại xe" : "Kích hoạt thông tin loại xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryType.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCategoryType.Count; i++)
                {
                    //query CategoryType by id
                    CategoryType CategoryType = await connection.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { Id = IdCategoryType[i] }, tran);

                    //query Vehicle by CategoryModel
                    Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryType.queryGetVehicleByIdCategoryType, new { IdType = IdCategoryType[i] }, tran);

                    if (CategoryType is null || vehicleId != Guid.Empty && IsHide)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryType is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong loại xe này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategoryType.queryHideCategoryType, new { Id = IdCategoryType[i], IsHide = IsHide }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryType[i], "Update", "CategoryType", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryType.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategoryType(Guid IdCategoryType, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin loại xe" : "Hiển thị thông tin loại xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryType.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query CategoryType by id
            CategoryType CategoryType = await connection.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { Id = IdCategoryType }, tran);

            //query Vehicle by CategoryModel
            Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryType.queryGetVehicleByIdCategoryType, new { IdType = IdCategoryType }, tran);

            if (CategoryType is null || vehicleId != Guid.Empty && IsHide)
            {
                tran.Rollback();

                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = CategoryType is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong loại xe này !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryCategoryType.queryHideCategoryType, new { Id = IdCategoryType, IsHide = IsHide }, tran);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryType, "Update", "CategoryType", content));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryCategoryType.queryInsertDiary, diaries[i], tran);
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
    internal static class QueryCategoryType
    {
        public const string queryInsertCategoryType = @"INSERT INTO [dbo].[CategoryType]
                               ([Id]
                               ,[Name]
                               ,[Status]
                               ,[CreatedDate]
                               ,[IsHide])
                         VALUES (@Id, @Name, @Status,
                                @CreatedDate, @IsHide)";
        public const string queryUpdateCategoryType = @"UPDATE [dbo].[CategoryType] SET Name = @Name
                                        WHERE Id = @Id";
        public const string queryDeleteCategoryType = "DELETE FROM [dbo].[CategoryType] WHERE Id = @Id";
        public const string queryGetByIdCategoryType = "select * from [dbo].[CategoryType] where Id = @Id";
        public const string queryGetAllCategoryType = "select *from [dbo].[CategoryType] order by CreatedDate desc";
        public const string queryGetAllCategoryTypeAvailable = "select *from [dbo].[CategoryType] where IsHide = 0 order by CreatedDate desc";
        public const string queryHideCategoryType = "UPDATE [dbo].[CategoryType] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCategoryType_Deleted = @"INSERT INTO [dbo].[CategoryType_Deleted]
                               ([Id]
                               ,[Name]
                               ,[Status]
                               ,[CreatedDate]
                               ,[IsHide])
                         VALUES (@Id, @Name, @Status,
                                @CreatedDate, @IsHide)";
        public const string queryGetAllIdCategoryType = "select Id from [dbo].[CategoryType]";
        public const string queryGetVehicleByIdCategoryType = "select Id from [dbo].[Vehicle] where IdType = @IdType";


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
