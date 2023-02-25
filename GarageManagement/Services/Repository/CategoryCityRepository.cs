using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using Mapster;
using Dapper;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;

namespace GarageManagement.Services.Repository
{
    public class CategoryCityRepository: ICategoryCityRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategoryCityRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategoryCity(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryCity> CategoryCitys = await connection.QueryAsync<CategoryCity>(QueryCategoryCity.queryGetAllCategoryCity);
            int coutRecord = CategoryCitys.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryCitys = CategoryCitys.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryCityDto> CategoryCityDtos = CategoryCitys.Adapt<IEnumerable<CategoryCityDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryCityDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategoryCityById(Guid IdCategoryCity)
        {
            using var con = _DbContext.CreateConnection();
            CategoryCity CategoryCity = await con.QueryFirstOrDefaultAsync<CategoryCity>(QueryCategoryCity.queryGetByIdCategoryCity, new { Id = IdCategoryCity });

            if (CategoryCity != null)
            {
                CategoryCityDto CategoryCityDto = CategoryCity.Adapt<CategoryCityDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategoryCityDto,
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
        public async Task<TemplateApi> DeleteCategoryCity(Guid IdCategoryCity, Guid IdUserCurrent)
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
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryCity.queryGetUserById, new { Id = IdUserCurrent }, tran);
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table CategoryCity
                CategoryCity CategoryCity = await connection.QueryFirstOrDefaultAsync<CategoryCity>(QueryCategoryCity.queryGetByIdCategoryCity, new { Id = IdCategoryCity }, tran);

                if (CategoryCity is null)
                {
                    //save to diary if false    
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(QueryCategoryCity.queryDeleteCategoryCity, new { Id = CategoryCity.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryCity.Id, "Delete", "CategoryCity", ""));


                //save to CategoryCity_Deleted
                await connection.ExecuteAsync(QueryCategoryCity.queryInsertCategoryCity_Deleted, CategoryCity, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryCity.Id, "Create", "CategoryCity_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategoryCityByList(List<Guid> IdCategoryCity, Guid IdUserCurrent)
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
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryCity.queryGetUserById, new { Id = IdUserCurrent }, tran);
            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCategoryCity.Count; i++)
                {
                    //query CategoryCity by id
                    CategoryCity CategoryCityId = await connection.QueryFirstOrDefaultAsync<CategoryCity>(QueryCategoryCity.queryGetByIdCategoryCity, new { Id = IdCategoryCity[i] }, tran);

                    if (CategoryCityId is null)
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
                    await connection.ExecuteAsync(QueryCategoryCity.queryDeleteCategoryCity, new { Id = IdCategoryCity[i] }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryCity[i], "Delete", "CategoryCity", ""));

                    //save to CategoryCity_Deleted
                    await connection.ExecuteAsync(QueryCategoryCity.queryInsertCategoryCity_Deleted, CategoryCityId, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryCityId.Id, "Create", "CategoryCity_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertCategoryCity(CategoryCityDto CategoryCityDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryCity = CategoryCityDto.Adapt<CategoryCity>();

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(QueryCategoryCity.queryGetUserById, new { Id = CategoryCityDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryCityDto.Id,
                    Operation = "Create",
                    Table = "CategoryCity",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCategoryCity.queryInsertCategoryCity, CategoryCity, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCategoryCity(CategoryCityDto CategoryCityDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryCity = CategoryCityDto.Adapt<CategoryCity>();

                //get CategoryCity by id
                CategoryCity CategoryCityById = await connection.QueryFirstOrDefaultAsync<CategoryCity>(QueryCategoryCity.queryGetByIdCategoryCity, new { CategoryCityDto.Id }, tran);

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(QueryCategoryCity.queryGetUserById, new { Id = CategoryCityDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryCityDto.Id,
                    Operation = "Update",
                    Table = "CategoryCity",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CategoryCityById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCategoryCity.queryUpdateCategoryCity, CategoryCity, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> HideCategoryCityByList(List<Guid> IdCategoryCity, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin thành phố" : "Kích hoạt thông tin thành phố";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryCity.queryGetUserById, new { Id = IdUserCurrent }, tran);
            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCategoryCity.Count; i++)
                {
                    //query CategoryCity by id
                    CategoryCity CategoryCity = await connection.QueryFirstOrDefaultAsync<CategoryCity>(QueryCategoryCity.queryGetByIdCategoryCity, new { Id = IdCategoryCity[i] }, tran);

                    if (CategoryCity is null)
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
                    await connection.ExecuteAsync(QueryCategoryCity.queryHideCategoryCity, new { Id = IdCategoryCity[i], IsHide = IsHide }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryCity[i], "Update", "CategoryCity", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategoryCity(Guid IdCategoryCity, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin thành phố" : "Hiển thị thông tin thành phố";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            //query CategoryCity by id
            CategoryCity CategoryCity = await connection.QueryFirstOrDefaultAsync<CategoryCity>(QueryCategoryCity.queryGetByIdCategoryCity, new { Id = IdCategoryCity }, tran);

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryCity.queryGetUserById, new { Id = IdUserCurrent }, tran);

            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                IdWith = IdCategoryCity,
                Operation = "Update",
                Table = "CategoryCity",
                Content = content,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (CategoryCity is null)
            {
                //save to diary if false
                saveDiaryModel.IsSuccess = false;
                Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diaryFalse, tran);
                tran.Commit();

                response = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return response;
            }
            await connection.ExecuteAsync(QueryCategoryCity.queryHideCategoryCity, new { Id = IdCategoryCity, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCategoryCity.queryInsertDiary, diary, tran);

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
    internal static class QueryCategoryCity
    {
        public const string queryInsertCategoryCity = @"INSERT INTO [dbo].[CategoryCity]
                               ([Id]
                               ,[CityName]
                               ,[CityCode]
                               ,[Status]
                               ,[IsHide]
                               ,[CreateDate])
                         VALUES (@Id, @CityName, @CityCode, @Status,
                                @IsHide, @CreateDate)";
        public const string queryUpdateCategoryCity = @"UPDATE [dbo].[CategoryCity] SET CityName = @CityName,
                                        CityCode = @CityCode
                                        WHERE Id = @Id";
        public const string queryDeleteCategoryCity = "DELETE FROM [dbo].[CategoryCity] WHERE Id = @Id";
        public const string queryGetByIdCategoryCity = "select * from [dbo].[CategoryCity] where Id = @Id";
        public const string queryGetAllCategoryCity = "select *from [dbo].[CategoryCity] order by CreateDate desc";
        public const string queryHideCategoryCity = "UPDATE [dbo].[CategoryCity] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCategoryCity_Deleted = @"INSERT INTO [dbo].[CategoryCity_Deleted]
                               ([Id]
                               ,[CityName]
                               ,[CityCode]
                               ,[Status]
                               ,[IsHide]
                               ,[CreateDate])
                         VALUES (@Id, @CityName, @CityCode, @Status,
                                @IsHide, @CreateDate)";
        public const string queryGetAllIdCategoryCity = "select Id from [dbo].[CategoryCity]";


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
