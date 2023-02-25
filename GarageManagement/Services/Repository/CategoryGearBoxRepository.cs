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
    public class CategoryGearBoxRepository: ICategoryGearBoxRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategoryGearBoxRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategoryAvailable(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryGearBox> CategoryGearBoxs = await connection.QueryAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetAllCategoryGearBoxAvailable);
            int coutRecord = CategoryGearBoxs.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryGearBoxs = CategoryGearBoxs.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryGearBoxDto> CategoryGearBoxDtos = CategoryGearBoxs.Adapt<IEnumerable<CategoryGearBoxDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryGearBoxDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCategoryGearBox(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryGearBox> CategoryGearBoxs = await connection.QueryAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetAllCategoryGearBox);
            int coutRecord = CategoryGearBoxs.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryGearBoxs = CategoryGearBoxs.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryGearBoxDto> CategoryGearBoxDtos = CategoryGearBoxs.Adapt<IEnumerable<CategoryGearBoxDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryGearBoxDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategoryGearBoxById(Guid IdCategoryGearBox)
        {
            using var con = _DbContext.CreateConnection();
            CategoryGearBox CategoryGearBox = await con.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { Id = IdCategoryGearBox });

            if (CategoryGearBox != null)
            {
                CategoryGearBoxDto CategoryGearBoxDto = CategoryGearBox.Adapt<CategoryGearBoxDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategoryGearBoxDto,
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
        public async Task<TemplateApi> DeleteCategoryGearBox(Guid IdCategoryGearBox, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryGearBox.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query CategoryGearBox by id
                CategoryGearBox categoryGearBox = await connection.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { Id = IdCategoryGearBox }, tran);

                //query Vehicle by IdGearBox
                Vehicle vehicle = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryCategoryGearBox.queryGetVehicleByIdGearBox, new { IdGearBox = IdCategoryGearBox }, tran);

                if (categoryGearBox is null || vehicle is not null)
                {
                    tran.Rollback();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = categoryGearBox is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong hộp số này !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCategoryGearBox.queryDeleteCategoryGearBox, new { Id = categoryGearBox.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, categoryGearBox.Id, "Delete", "CategoryGearBox", ""));

                //save to CategoryGearBox_Deleted
                await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertCategoryGearBox_Deleted, categoryGearBox, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, categoryGearBox.Id, "Create", "CategoryGearBox_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategoryGearBoxByList(List<Guid> IdCategoryGearBox, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryGearBox.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdCategoryGearBox.Count; i++)
                {
                    //query CategoryGearBox by id
                    CategoryGearBox CategoryGearBoxId = await connection.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { Id = IdCategoryGearBox[i] }, tran);

                    //query Vehicle by IdGearBox
                    Vehicle vehicle = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryCategoryGearBox.queryGetVehicleByIdGearBox, new { IdGearBox = IdCategoryGearBox[i] }, tran);

                    if (CategoryGearBoxId is null || vehicle is not null)
                    {
                        tran.Rollback();

                        response = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryGearBoxId is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong hộp số này !"
                        };
                        return response;
                    }

                    await connection.ExecuteAsync(QueryCategoryGearBox.queryDeleteCategoryGearBox, new { Id = IdCategoryGearBox[i] }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryGearBox[i], "Delete", "CategoryGearBox", ""));

                    //save to CategoryGearBox_Deleted
                    await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertCategoryGearBox_Deleted, CategoryGearBoxId, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryGearBoxId.Id, "Create", "CategoryGearBox_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertCategoryGearBox(CategoryGearBoxDto CategoryGearBoxDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryGearBox = CategoryGearBoxDto.Adapt<CategoryGearBox>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryGearBox.queryGetUserById, new { Id = CategoryGearBoxDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryGearBoxDto.Id,
                    Operation = "Create",
                    Table = "CategoryGearBox",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertCategoryGearBox, CategoryGearBox, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCategoryGearBox(CategoryGearBoxDto CategoryGearBoxDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryGearBox = CategoryGearBoxDto.Adapt<CategoryGearBox>();

                //get CategoryGearBox by id
                CategoryGearBox CategoryGearBoxById = await connection.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { CategoryGearBoxDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryGearBox.queryGetUserById, new { Id = CategoryGearBoxDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryGearBoxDto.Id,
                    Operation = "Update",
                    Table = "CategoryGearBox",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };


                if (CategoryGearBoxById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCategoryGearBox.queryUpdateCategoryGearBox, CategoryGearBox, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> HideCategoryGearBoxByList(List<Guid> IdCategoryGearBox, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin hộp số xe" : "Kích hoạt thông tin loại hộp số xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryGearBox.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdCategoryGearBox.Count; i++)
                {
                    //query CategoryGearBox by id
                    var CategoryGearBox = await connection.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { Id = IdCategoryGearBox[i] }, tran);

                    //query Vehicle by IdGearBox
                    Vehicle vehicle = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryCategoryGearBox.queryGetVehicleByIdGearBox, new { IdGearBox = IdCategoryGearBox[i] }, tran);

                    if (CategoryGearBox is null || vehicle is not null && IsHide == true)
                    {
                        tran.Rollback();

                        response = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategoryGearBox is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong hộp số này !"
                        };
                        return response;
                    }

                    await connection.ExecuteAsync(QueryCategoryGearBox.queryHideCategoryGearBox, new { Id = IdCategoryGearBox[i], IsHide = IsHide }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryGearBox[i], "Update", "CategoryGearBox", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategoryGearBox(Guid IdCategoryGearBox, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin hộp số xe" : "Hiển thị thông tin loại hộp số xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();


            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryGearBox.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query CategoryGearBox by id
            CategoryGearBox CategoryGearBox = await connection.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { Id = IdCategoryGearBox }, tran);

            //query Vehicle by IdGearBox
            Vehicle vehicle = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryCategoryGearBox.queryGetVehicleByIdGearBox, new { IdGearBox = IdCategoryGearBox }, tran);

            if (CategoryGearBox is null || vehicle is not null && IsHide == true)
            {
                tran.Rollback();

                response = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = CategoryGearBox is null ? "Đã có ID không tồn tại !" : "Đã có phương tiện trong hộp số này !"
                };
                return response;
            }

            await connection.ExecuteAsync(QueryCategoryGearBox.queryHideCategoryGearBox, new { Id = IdCategoryGearBox, IsHide = IsHide }, tran);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryGearBox, "Update", "CategoryGearBox", content));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryCategoryGearBox.queryInsertDiary, diaries[i], tran);
            }

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
    internal static class QueryCategoryGearBox
    {
        public const string queryInsertCategoryGearBox = @"INSERT INTO [dbo].[CategoryGearBox]
                                                               ([Id]
                                                               ,[Name]
                                                               ,[Status]
                                                               ,[IsHide]
                                                               ,[CreatedDate])
                                                         VALUES (@Id, @Name, @Status,
                                                                @IsHide, @CreatedDate)";
        public const string queryUpdateCategoryGearBox = @"UPDATE [dbo].[CategoryGearBox] SET Name = @Name
                                                         WHERE Id = @Id";
        public const string queryDeleteCategoryGearBox = "DELETE FROM [dbo].[CategoryGearBox] WHERE Id = @Id";
        public const string queryGetByIdCategoryGearBox = "select * from [dbo].[CategoryGearBox] where Id = @Id";
        public const string queryGetAllCategoryGearBox = "select *from [dbo].[CategoryGearBox] order by CreatedDate desc";
        public const string queryGetAllCategoryGearBoxAvailable = "select *from [dbo].[CategoryGearBox] where IsHide = 0 order by CreatedDate desc";
        public const string queryHideCategoryGearBox = "UPDATE [dbo].[CategoryGearBox] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCategoryGearBox_Deleted = @"INSERT INTO [dbo].[CategoryGearBox_Deleted]
                                                               ([Id]
                                                               ,[Name]
                                                               ,[Status]
                                                               ,[IsHide]
                                                               ,[CreatedDate])
                                                         VALUES (@Id, @Name, @Status,
                                                                @IsHide, @CreatedDate)";
        public const string queryGetAllIdCategoryGearBox = "select Id from [dbo].[CategoryGearBox]";
        public const string queryGetVehicleByIdGearBox = "select * from [dbo].[Vehicle] where IdGearBox = @IdGearBox";

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
