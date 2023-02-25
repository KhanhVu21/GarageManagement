using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using Mapster;
using Dapper;
using GarageManagement.Services.Dtos;
using System.Linq;
using System.ComponentModel;

namespace GarageManagement.Services.Repository
{
    public class CategoryBrandVehicleRepository : ICategoryBrandVehicleRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategoryBrandVehicleRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategoryBrandVehicleAvailable(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryBrandVehicle> CategoryBrandVehicles = await connection.QueryAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetAllCategoryBrandVehicleAvailable);
            int coutRecord = CategoryBrandVehicles.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryBrandVehicles = CategoryBrandVehicles.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryBrandVehicleDto> CategoryBrandVehicleDtos = CategoryBrandVehicles.Adapt<IEnumerable<CategoryBrandVehicleDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryBrandVehicleDtos.ToArray(),
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
        public async Task<TemplateApi> DeleteCategoryBrandVehicle(Guid IdCategoryBrandVehicle, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryBrandVehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);

                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query CategoryBrandVehicle by id
                CategoryBrandVehicle CategoryBrandVehicle = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle }, tran);

                //query Vehicle by IdCategoryBrandVehicle
                Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetVehicleByIdCategoryBrandVehicle, new { IdBrandVehicleCategory = IdCategoryBrandVehicle }, tran);

                //query CategoryModel by IdCategoryBrandVehicle
                Guid categoryModelId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetAllCategoryModelByIdCategoryBrandVehicle, new { IdCategoryBrandVehicle = IdCategoryBrandVehicle }, tran);

                if (CategoryBrandVehicle is null || vehicleId != Guid.Empty || categoryModelId != Guid.Empty)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = CategoryBrandVehicle is null ? "Đã có ID không tồn tại !" : vehicleId != Guid.Empty ? "Đã có phương tiện trong hãng xe này !" : "Đã có mẫu xe trong hãng này !"
                    };
                    return responseFalse;
                }

                //CategoryBrandVehicle
                await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryDeleteCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryBrandVehicle, "Delete", "CategoryBrandVehicle", ""));

                //save to CategoryBrandVehicle_Deleted
                await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertCategoryBrandVehicle_Deleted, CategoryBrandVehicle, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryBrandVehicle, "Create", "CategoryBrandVehicle_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategoryBrandVehicleByList(List<Guid> IdCategoryBrandVehicle, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryBrandVehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdCategoryBrandVehicle.Count; i++)
                {
                    //query CategoryBrandVehicle by id
                    CategoryBrandVehicle CategoryBrandVehicleId = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle[i] }, tran);

                    //query Vehicle by IdCategoryBrandVehicle
                    Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetVehicleByIdCategoryBrandVehicle, new { IdBrandVehicleCategory = IdCategoryBrandVehicle[i] }, tran);

                    //query CategoryModel by IdCategoryBrandVehicle
                    Guid categoryModelId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetAllCategoryModelByIdCategoryBrandVehicle, new { IdCategoryBrandVehicle = IdCategoryBrandVehicle[i] }, tran);

                    if (CategoryBrandVehicleId is null || vehicleId != Guid.Empty || categoryModelId != Guid.Empty)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryBrandVehicleId is null ? "Đã có ID không tồn tại !" : vehicleId != Guid.Empty ? "Đã có phương tiện trong hãng xe này !" : "Đã có mẫu xe trong hãng này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryDeleteCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryBrandVehicle[i], "Delete", "CategoryBrandVehicle", ""));

                    //save to CategoryBrandVehicle_Deleted
                    await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertCategoryBrandVehicle_Deleted, CategoryBrandVehicleId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryBrandVehicleId.Id, "Create", "CategoryBrandVehicle_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllCategoryBrandVehicle(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryBrandVehicle> CategoryBrandVehicles = await connection.QueryAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetAllCategoryBrandVehicle);
            int coutRecord = CategoryBrandVehicles.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryBrandVehicles = CategoryBrandVehicles.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryBrandVehicleDto> CategoryBrandVehicleDtos = CategoryBrandVehicles.Adapt<IEnumerable<CategoryBrandVehicleDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryBrandVehicleDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategoryBrandVehicleById(Guid IdCategoryBrandVehicle)
        {
            using var con = _DbContext.CreateConnection();
            CategoryBrandVehicle CategoryBrandVehicle = await con.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle });

            if (CategoryBrandVehicle != null)
            {
                CategoryBrandVehicleDto CategoryBrandVehicleDto = CategoryBrandVehicle.Adapt<CategoryBrandVehicleDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategoryBrandVehicleDto,
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
        public async Task<TemplateApi> InsertCategoryBrandVehicle(CategoryBrandVehicleDto CategoryBrandVehicleDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var CategoryBrandVehicle = CategoryBrandVehicleDto.Adapt<CategoryBrandVehicle>();

            //query user by iduserCurrentLogin
            User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(QueryCategoryBrandVehicle.queryGetUserById, new { Id = CategoryBrandVehicleDto.IdUserCurrent });

            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                IdWith = CategoryBrandVehicleDto.Id,
                Operation = "Create",
                Table = "CategoryBrandVehicle",
                Content = "",
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertCategoryBrandVehicle, CategoryBrandVehicle);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertDiary, diary);

            var response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Thêm mới thành công !"
            };
            return response;
        }
        public async Task<TemplateApi> UpdateCategoryBrandVehicle(CategoryBrandVehicleDto CategoryBrandVehicleDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var CategoryBrandVehicle = CategoryBrandVehicleDto.Adapt<CategoryBrandVehicle>();

            //get CategoryBrandVehicle by id
            CategoryBrandVehicle CategoryBrandVehicleById = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { CategoryBrandVehicleDto.Id });

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryBrandVehicle.queryGetUserById, new { Id = CategoryBrandVehicleDto.IdUserCurrent });
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (CategoryBrandVehicleById == null)
            {
                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return responseFalse;
            }
            //check id file exits
            if (CategoryBrandVehicleDto.idFile is not null)
            {
                CategoryBrandVehicle.logo = CategoryBrandVehicleById.logo;
            }

            await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryUpdateCategoryBrandVehicle, CategoryBrandVehicle);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryBrandVehicle.Id, "Update", "CategoryBrandVehicle", ""));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertDiary, diaries[i]);
            }

            var response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Cập nhật thành công !"
            };
            return response;
        }
        public async Task<TemplateApi> HideCategoryBrandVehicleByList(List<Guid> IdCategoryBrandVehicle, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Hủy kích hoạt thông tin loại thương hiệu xe" : "Kích hoạt thông tin loại phương tiện xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryBrandVehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdCategoryBrandVehicle.Count; i++)
                {
                    //query CategoryBrandVehicle by id
                    CategoryBrandVehicle CategoryBrandVehicle = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle[i] }, tran);

                    //query Vehicle by IdCategoryBrandVehicle
                    Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetVehicleByIdCategoryBrandVehicle, new { IdBrandVehicleCategory = IdCategoryBrandVehicle[i] }, tran);

                    //query CategoryModel by IdCategoryBrandVehicle
                    Guid categoryModelId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetAllCategoryModelByIdCategoryBrandVehicle, new { IdCategoryBrandVehicle = IdCategoryBrandVehicle[i] }, tran);

                    if (CategoryBrandVehicle is null || vehicleId != Guid.Empty && IsHide || categoryModelId != Guid.Empty && IsHide)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryBrandVehicle is null ? "Đã có ID không tồn tại !" : vehicleId != Guid.Empty ? "Đã có phương tiện trong hãng xe này !" : "Đã có mẫu xe trong hãng này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryHideCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle[i], IsHide = IsHide }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryBrandVehicle[i], "Update", "CategoryBrandVehicle", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategoryBrandVehicle(Guid IdCategoryBrandVehicle, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin loại thương hiệu xe" : "Hiển thị thông tin loại phương tiện xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryBrandVehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query CategoryBrandVehicle by id
            CategoryBrandVehicle CategoryBrandVehicle = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle }, tran);

            //query Vehicle by IdCategoryBrandVehicle
            Guid vehicleId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetVehicleByIdCategoryBrandVehicle, new { IdBrandVehicleCategory = IdCategoryBrandVehicle }, tran);

            //query CategoryModel by IdCategoryBrandVehicle
            Guid categoryModelId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCategoryBrandVehicle.queryGetAllCategoryModelByIdCategoryBrandVehicle, new { IdCategoryBrandVehicle = IdCategoryBrandVehicle }, tran);

            if (CategoryBrandVehicle is null || vehicleId != Guid.Empty && IsHide || categoryModelId != Guid.Empty && IsHide)
            {
                tran.Rollback();

                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = CategoryBrandVehicle is null ? "Đã có ID không tồn tại !" : vehicleId != Guid.Empty ? "Đã có phương tiện trong hãng xe này !" : "Đã có mẫu xe trong hãng này !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryHideCategoryBrandVehicle, new { Id = IdCategoryBrandVehicle, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertDiary, diary, tran);

            tran.Commit();

            var response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = $"{content} thành công !"
            };
            return response;
        }
        #endregion
    }
    internal static class QueryCategoryBrandVehicle
    {
        public const string queryInsertCategoryBrandVehicle = @"INSERT INTO [dbo].[CategoryBrandVehicle]
                                                                           ([Id]
                                                                           ,[Name]
                                                                           ,[Code]
                                                                           ,[Status]
                                                                           ,[CreatedDate]
                                                                           ,[IsHide]
                                                                           ,[logo])
                                                                 VALUES (@Id, @Name, @Code, @Status,
                                                                        @CreatedDate, @IsHide, @logo)";
        public const string queryUpdateCategoryBrandVehicle = @"UPDATE [dbo].[CategoryBrandVehicle] SET Name = @Name,
                                                                        logo = @logo
                                                                        WHERE Id = @Id";
        public const string queryDeleteCategoryBrandVehicle = "DELETE FROM [dbo].[CategoryBrandVehicle] WHERE Id = @Id";
        public const string queryGetByIdCategoryBrandVehicle = "select * from [dbo].[CategoryBrandVehicle] where Id = @Id";
        public const string queryGetAllCategoryBrandVehicle = "select *from [dbo].[CategoryBrandVehicle] order by CreatedDate desc";
        public const string queryGetAllCategoryBrandVehicleAvailable = "select *from [dbo].[CategoryBrandVehicle] where IsHide = 0 order by CreatedDate desc";
        public const string queryHideCategoryBrandVehicle = "UPDATE [dbo].[CategoryBrandVehicle] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCategoryBrandVehicle_Deleted = @"INSERT INTO [dbo].[CategoryBrandVehicle_Deleted]
                                                                               ([Id]
                                                                               ,[Name]
                                                                               ,[Code]
                                                                               ,[Status]
                                                                               ,[CreatedDate]
                                                                               ,[IsHide])
                                                                         VALUES (@Id, @Name, @Code, @Status,
                                                                                @CreatedDate, @IsHide)";
        public const string queryGetAllIdCategoryBrandVehicle = "select Id from [dbo].[CategoryBrandVehicle]";
        public const string queryGetVehicleByIdCategoryBrandVehicle = "select Id from [dbo].[Vehicle] where IdBrandVehicleCategory = @IdBrandVehicleCategory";
        public const string queryGetAllCategoryModelByIdCategoryBrandVehicle = "select Id from [dbo].[CategoryModel] where IdCategoryBrandVehicle = @IdCategoryBrandVehicle";

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
