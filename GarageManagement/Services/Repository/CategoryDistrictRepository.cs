using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using Dapper;

namespace GarageManagement.Services.Repository
{
    public class CategoryDistrictRepository: ICategoryDistrictRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategoryDistrictRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategoryDistrictByIdCity(int pageNumber, int pageSize, string CityCode)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryDistrict> CategoryDistricts = await connection.QueryAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetAllCategoryDistrictByIdCity, new { CityCode = CityCode });
            int coutRecord = CategoryDistricts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryDistricts = CategoryDistricts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryDistrictDto> CategoryDistrictDtos = CategoryDistricts.Adapt<IEnumerable<CategoryDistrictDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryDistrictDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCategoryDistrict(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategoryDistrict> CategoryDistricts = await connection.QueryAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetAllCategoryDistrict);
            int coutRecord = CategoryDistricts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategoryDistricts = CategoryDistricts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategoryDistrictDto> CategoryDistrictDtos = CategoryDistricts.Adapt<IEnumerable<CategoryDistrictDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategoryDistrictDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategoryDistrictById(Guid IdCategoryDistrict)
        {
            using var con = _DbContext.CreateConnection();
            CategoryDistrict CategoryDistrict = await con.QueryFirstOrDefaultAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetByIdCategoryDistrict, new { Id = IdCategoryDistrict });

            if (CategoryDistrict != null)
            {
                CategoryDistrictDto CategoryDistrictDto = CategoryDistrict.Adapt<CategoryDistrictDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategoryDistrictDto,
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
        public async Task<TemplateApi> DeleteCategoryDistrict(Guid IdCategoryDistrict, Guid IdUserCurrent)
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
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryDistrict.queryGetUserById, new { Id = IdUserCurrent }, tran);
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table CategoryDistrict
                CategoryDistrict CategoryDistrict = await connection.QueryFirstOrDefaultAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetByIdCategoryDistrict, new { Id = IdCategoryDistrict }, tran);

                if (CategoryDistrict is null)
                {
                    //save to diary if false    
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(QueryCategoryDistrict.queryDeleteCategoryDistrict, new { Id = CategoryDistrict.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryDistrict.Id, "Delete", "CategoryDistrict", ""));


                //save to CategoryDistrict_Deleted
                await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertCategoryDistrict_Deleted, CategoryDistrict, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryDistrict.Id, "Create", "CategoryDistrict_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategoryDistrictByList(List<Guid> IdCategoryDistrict, Guid IdUserCurrent)
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
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryDistrict.queryGetUserById, new { Id = IdUserCurrent }, tran);
            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCategoryDistrict.Count; i++)
                {
                    //query CategoryDistrict by id
                    CategoryDistrict CategoryDistrictId = await connection.QueryFirstOrDefaultAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetByIdCategoryDistrict, new { Id = IdCategoryDistrict[i] }, tran);

                    if (CategoryDistrictId is null)
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
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryDeleteCategoryDistrict, new { Id = IdCategoryDistrict[i] }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryDistrict[i], "Delete", "CategoryDistrict", ""));

                    //save to CategoryDistrict_Deleted
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertCategoryDistrict_Deleted, CategoryDistrictId, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategoryDistrictId.Id, "Create", "CategoryDistrict_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertCategoryDistrict(CategoryDistrictDto CategoryDistrictDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryDistrict = CategoryDistrictDto.Adapt<CategoryDistrict>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryDistrict.queryGetUserById, new { Id = CategoryDistrictDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryDistrictDto.Id,
                    Operation = "Create",
                    Table = "CategoryDistrict",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertCategoryDistrict, CategoryDistrict, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCategoryDistrict(CategoryDistrictDto CategoryDistrictDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CategoryDistrict = CategoryDistrictDto.Adapt<CategoryDistrict>();

                //get CategoryDistrict by id
                CategoryDistrict CategoryDistrictById = await connection.QueryFirstOrDefaultAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetByIdCategoryDistrict, new { CategoryDistrictDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryDistrict.queryGetUserById, new { Id = CategoryDistrictDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CategoryDistrictDto.Id,
                    Operation = "Update",
                    Table = "CategoryDistrict",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CategoryDistrictById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCategoryDistrict.queryUpdateCategoryDistrict, CategoryDistrict, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> HideCategoryDistrictByList(List<Guid> IdCategoryDistrict, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin quận huyện" : "Kích hoạt thông tin quận huyện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryDistrict.queryGetUserById, new { Id = IdUserCurrent }, tran);

            try
            {
                for (int i = 0; i < IdCategoryDistrict.Count; i++)
                {
                    //query CategoryDistrict by id
                    var CategoryDistrict = await connection.QueryFirstOrDefaultAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetByIdCategoryDistrict, new { Id = IdCategoryDistrict[i] }, tran);

                    saveDiaryModel = new SaveDiaryModel()
                    {
                        IdUserCurrent = userByIDCurrentLogin.Id,
                        Fullname = userByIDCurrentLogin.Fullname ?? "",
                        IsSuccess = true,
                    };

                    if (CategoryDistrict is null)
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
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryHideCategoryDistrict, new { Id = IdCategoryDistrict[i], IsHide = IsHide }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategoryDistrict[i], "Update", "CategoryDistrict", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategoryDistrict(Guid IdCategoryDistrict, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin quận huyện" : "Hiển thị thông tin quận huyện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            //query CategoryDistrict by id
            var CategoryDistrict = await connection.QueryFirstOrDefaultAsync<CategoryDistrict>(QueryCategoryDistrict.queryGetByIdCategoryDistrict, new { Id = IdCategoryDistrict }, tran);

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategoryDistrict.queryGetUserById, new { Id = IdUserCurrent }, tran);

            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                IdWith = IdCategoryDistrict,
                Operation = "Update",
                Table = "CategoryDistrict",
                Content = content,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (CategoryDistrict is null)
            {
                //save to diary if false
                saveDiaryModel.IsSuccess = false;
                Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diaryFalse, tran);
                tran.Commit();

                response = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return response;
            }
            await connection.ExecuteAsync(QueryCategoryDistrict.queryHideCategoryDistrict, new { Id = IdCategoryDistrict, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCategoryDistrict.queryInsertDiary, diary, tran);

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
    internal static class QueryCategoryDistrict
    {
        public const string queryInsertCategoryDistrict = @"INSERT INTO [dbo].[CategoryDistrict]
                                                                   ([Id]
                                                                   ,[DistrictName]
                                                                   ,[DistrictCode]
                                                                   ,[CityCode]
                                                                   ,[Status]
                                                                   ,[IsHide]
                                                                   ,[CreatedDate])
                                                             VALUES (@Id, @DistrictName, @DistrictCode, @CityCode,
                                                                    @Status, @IsHide, @CreatedDate)";
        public const string queryUpdateCategoryDistrict = @"UPDATE [dbo].[CategoryDistrict] SET DistrictName = @DistrictName,
                                        DistrictCode = @DistrictCode,
                                        CityCode = @CityCode
                                        WHERE Id = @Id";
        public const string queryDeleteCategoryDistrict = "DELETE FROM [dbo].[CategoryDistrict] WHERE Id = @Id";
        public const string queryGetByIdCategoryDistrict = "select * from [dbo].[CategoryDistrict] where Id = @Id";
        public const string queryGetAllCategoryDistrict = "select *from [dbo].[CategoryDistrict] order by CreatedDate desc";
        public const string queryHideCategoryDistrict = "UPDATE [dbo].[CategoryDistrict] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCategoryDistrict_Deleted = @"INSERT INTO [dbo].[CategoryDistrict_Deleted]
                                                                   ([Id]
                                                                   ,[DistrictName]
                                                                   ,[DistrictCode]
                                                                   ,[CityCode]
                                                                   ,[Status]
                                                                   ,[IsHide]
                                                                   ,[CreatedDate])
                                                             VALUES (@Id, @DistrictName, @DistrictCode, @CityCode,
                                                                    @Status, @IsHide, @CreatedDate)";
        public const string queryGetAllIdCategoryDistrict = "select Id from [dbo].[CategoryDistrict]";
        public const string queryGetAllCategoryDistrictByIdCity = "select *from [dbo].[CategoryDistrict] where CityCode = @CityCode order by CreatedDate desc";
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
