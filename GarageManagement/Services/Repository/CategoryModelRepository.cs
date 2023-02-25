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
    public class CategoryModelRepository: ICategoryModelRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategoryModelRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategoryModelAvailable(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryModel> CategoryModels = await connection.QueryAsync<CategoryModel>(QueryCategoryModel.queryGetAllCategoryModelAvailable);
            int coutRecord = CategoryModels.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryModels = CategoryModels.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryModelDto> CategoryModelDtos = CategoryModels.Adapt<IEnumerable<CategoryModelDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryModelDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCategoryModelByIdIdCategoryBrandVehicle(int pageNumber, int pageSize, Guid IdCategoryBrandVehicle)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryModel> CategoryModels = await connection.QueryAsync<CategoryModel>(QueryCategoryModel.queryGetAllCategoryModelByIdIdCategoryBrandVehicle, new { IdCategoryBrandVehicle = IdCategoryBrandVehicle });
            int coutRecord = CategoryModels.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryModels = CategoryModels.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryModelDto> CategoryModelDtos = CategoryModels.Adapt<IEnumerable<CategoryModelDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryModelDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCategoryModel(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryModel> CategoryModels = await connection.QueryAsync<CategoryModel>(QueryCategoryModel.queryGetAllCategoryModel);
            int coutRecord = CategoryModels.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryModels = CategoryModels.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryModelDto> CategoryModelDtos = CategoryModels.Adapt<IEnumerable<CategoryModelDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryModelDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategoryModelById(Guid IdCategoryModel)
        {
            using var con = _DbContext.CreateConnection();
            CategoryModel CategoryModel = await con.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { Id = IdCategoryModel });

            if (CategoryModel != null)
            {
                CategoryModelDto CategoryModelDto = CategoryModel.Adapt<CategoryModelDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategoryModelDto,
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
        public async Task<TemplateApi> DeleteCategoryModel(Guid IdCategoryModel, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryModel.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table CategoryModel
                CategoryModel CategoryModel = await connection.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { Id = IdCategoryModel }, tran);

                //query Vehicle by CategoryModel
                Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryModel.queryGetVehicleByIdCategoryModel, new { IdModel = IdCategoryModel }, tran);

                if (CategoryModel is null || vehicleId != Guid.Empty)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = CategoryModel is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong mẫu xe này !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryCategoryModel.queryDeleteCategoryModel, new { Id = CategoryModel.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryModel.Id, "Delete", "CategoryModel", ""));

                //save to CategoryModel_Deleted
                await connection.ExecuteAsync(QueryCategoryModel.queryInsertCategoryModel_Deleted, CategoryModel, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryModel.Id, "Create", "CategoryModel_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryModel.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategoryModelByList(List<Guid> IdCategoryModel, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryModel.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdCategoryModel.Count; i++)
                {
                    //query CategoryModel by id
                    CategoryModel CategoryModelId = await connection.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { Id = IdCategoryModel[i] }, tran);

                    //query Vehicle by CategoryModel
                    Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryModel.queryGetVehicleByIdCategoryModel, new { IdModel = IdCategoryModel[i] }, tran);

                    if (CategoryModelId is null || vehicleId != Guid.Empty)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryModelId is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong mẫu xe này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategoryModel.queryDeleteCategoryModel, new { Id = IdCategoryModel[i] }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryModel[i], "Delete", "CategoryModel", ""));

                    //save to CategoryModel_Deleted
                    await connection.ExecuteAsync(QueryCategoryModel.queryInsertCategoryModel_Deleted, CategoryModelId, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryModelId.Id, "Create", "CategoryModel_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryModel.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertCategoryModel(CategoryModelDto CategoryModelDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryBrandVehicle = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = CategoryModelDto.IdCategoryBrandVehicle }, tran);
                if (CategoryBrandVehicle.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Thương hiệu {CategoryBrandVehicle.Name} không có sẵn !"
                    };
                }

                var CategoryModel = CategoryModelDto.Adapt<CategoryModel>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryModel.queryGetUserById, new { Id = CategoryModelDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryModelDto.Id,
                    Operation = "Create",
                    Table = "CategoryModel",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCategoryModel.queryInsertCategoryModel, CategoryModel, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryModel.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCategoryModel(CategoryModelDto CategoryModelDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryBrandVehicle = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = CategoryModelDto.IdCategoryBrandVehicle }, tran);
                if (CategoryBrandVehicle.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Thương hiệu {CategoryBrandVehicle.Name} không có sẵn !"
                    };
                }

                var CategoryModel = CategoryModelDto.Adapt<CategoryModel>();

                //get CategoryModel by id
                CategoryModel CategoryModelById = await connection.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { CategoryModelDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryModel.queryGetUserById, new { Id = CategoryModelDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryModelDto.Id,
                    Operation = "Update",
                    Table = "CategoryModel",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CategoryModelById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryModel.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCategoryModel.queryUpdateCategoryModel, CategoryModel, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryModel.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> HideCategoryModelByList(List<Guid> IdCategoryModel, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin model xe" : "Kích hoạt thông tin model xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryModel.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCategoryModel.Count; i++)
                {
                    //query CategoryModel by id
                    CategoryModel CategoryModel = await connection.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { Id = IdCategoryModel[i] }, tran);

                    //query Vehicle by CategoryModel
                    Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryModel.queryGetVehicleByIdCategoryModel, new { IdModel = IdCategoryModel[i] }, tran);

                    if (CategoryModel is null || vehicleId != Guid.Empty && IsHide)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryModel is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong mẫu xe này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategoryModel.queryHideCategoryModel, new { Id = IdCategoryModel[i], IsHide = IsHide }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryModel[i], "Update", "CategoryModel", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryModel.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategoryModel(Guid IdCategoryModel, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin model xe" : "Hiển thị thông tin model xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryModel.queryGetUserById, new { Id = IdUserCurrent }, tran);

            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query CategoryModel by id
            CategoryModel CategoryModel = await connection.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { Id = IdCategoryModel }, tran);

            //query Vehicle by CategoryModel
            Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryModel.queryGetVehicleByIdCategoryModel, new { IdModel = IdCategoryModel }, tran);

            if (CategoryModel is null || vehicleId != Guid.Empty && IsHide)
            {
                tran.Rollback();

                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = CategoryModel is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong mẫu xe này !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryCategoryModel.queryHideCategoryModel, new { Id = IdCategoryModel, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCategoryModel.queryInsertDiary, diary, tran);

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
    internal static class QueryCategoryModel
    {
        public const string queryInsertCategoryModel = @"INSERT INTO [dbo].[CategoryModel]
                                                           ([Id]
                                                           ,[ModelName]
                                                           ,[Status]
                                                           ,[CreatedDate]
                                                           ,[IsHide]
                                                           ,[IdCategoryBrandVehicle])
                                                     VALUES (@Id, @ModelName, @Status,
                                                            @CreatedDate, @IsHide, @IdCategoryBrandVehicle)";
        public const string queryUpdateCategoryModel = @"UPDATE [dbo].[CategoryModel] SET ModelName = @ModelName,
                                                        IdCategoryBrandVehicle = @IdCategoryBrandVehicle
                                                        WHERE Id = @Id";
        public const string queryDeleteCategoryModel = "DELETE FROM [dbo].[CategoryModel] WHERE Id = @Id";
        public const string queryGetByIdCategoryModel = "select * from [dbo].[CategoryModel] where Id = @Id";
        public const string queryGetAllCategoryModel = "select *from [dbo].[CategoryModel] order by CreatedDate desc";
        public const string queryGetAllCategoryModelAvailable = "select *from [dbo].[CategoryModel] where IsHide = 0 order by CreatedDate desc";
        public const string queryHideCategoryModel = "UPDATE [dbo].[CategoryModel] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCategoryModel_Deleted = @"INSERT INTO [dbo].[CategoryModel_Deleted]
                                                           ([Id]
                                                           ,[ModelName]
                                                           ,[Status]
                                                           ,[CreatedDate]
                                                           ,[IsHide]
                                                           ,[IdCategoryBrandVehicle])
                                                     VALUES (@Id, @ModelName, @Status,
                                                            @CreatedDate, @IsHide, @IdCategoryBrandVehicle)";
        public const string queryGetAllIdCategoryModel = "select Id from [dbo].[CategoryModel]";
        public const string queryGetAllCategoryModelByIdIdCategoryBrandVehicle = "select *from [dbo].[CategoryModel] where IdCategoryBrandVehicle = @IdCategoryBrandVehicle and IsHide = 0 order by CreatedDate desc";
        public const string queryGetVehicleByIdCategoryModel = "select Id from [dbo].[Vehicle] where IdModel = @IdModel";


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
