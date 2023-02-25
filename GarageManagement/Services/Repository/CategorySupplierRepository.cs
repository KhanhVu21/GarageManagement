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
    public class CategorySupplierRepository: ICategorySupplierRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CategorySupplierRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCategorySupplierAvailable(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategorySupplier> CategorySuppliers = await connection.QueryAsync<CategorySupplier>(QueryCategorySupplier.queryGetAllCategorySupplierAvailable);
            int coutRecord = CategorySuppliers.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategorySuppliers = CategorySuppliers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategorySupplierDto> CategorySupplierDtos = CategorySuppliers.Adapt<IEnumerable<CategorySupplierDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategorySupplierDtos.ToArray(),
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
        public async Task<TemplateApi> DeleteCategorySupplier(Guid IdCategorySupplier, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategorySupplier.queryGetUserById, new { Id = IdUserCurrent }, tran);

                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query CategorySupplier by id
                CategorySupplier CategorySupplier = await connection.QueryFirstOrDefaultAsync<CategorySupplier>(QueryCategorySupplier.queryGetByIdCategorySupplier, new { Id = IdCategorySupplier }, tran);

                if (CategorySupplier is null)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = CategorySupplier is null ? "Đã có ID không tồn tại !" : ""
                    };
                    return responseFalse;
                }

                //CategorySupplier
                await connection.ExecuteAsync(QueryCategorySupplier.queryDeleteCategorySupplier, new { Id = IdCategorySupplier }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategorySupplier, "Delete", "CategorySupplier", ""));

                //save to CategorySupplier_Deleted
                await connection.ExecuteAsync(QueryCategorySupplier.queryInsertCategorySupplier_Deleted, CategorySupplier, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategorySupplier, "Create", "CategorySupplier_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategorySupplier.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCategorySupplierByList(List<Guid> IdCategorySupplier, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategorySupplier.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdCategorySupplier.Count; i++)
                {
                    //query CategorySupplier by id
                    CategorySupplier CategorySupplierId = await connection.QueryFirstOrDefaultAsync<CategorySupplier>(QueryCategorySupplier.queryGetByIdCategorySupplier, new { Id = IdCategorySupplier[i] }, tran);

                    if (CategorySupplierId is null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategorySupplierId is null ? "Đã có ID không tồn tại !" : ""
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategorySupplier.queryDeleteCategorySupplier, new { Id = IdCategorySupplier[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategorySupplier[i], "Delete", "CategorySupplier", ""));

                    //save to CategorySupplier_Deleted
                    await connection.ExecuteAsync(QueryCategorySupplier.queryInsertCategorySupplier_Deleted, CategorySupplierId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategorySupplierId.Id, "Create", "CategorySupplier_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategorySupplier.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllCategorySupplier(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CategorySupplier> CategorySuppliers = await connection.QueryAsync<CategorySupplier>(QueryCategorySupplier.queryGetAllCategorySupplier);
            int coutRecord = CategorySuppliers.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CategorySuppliers = CategorySuppliers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CategorySupplierDto> CategorySupplierDtos = CategorySuppliers.Adapt<IEnumerable<CategorySupplierDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CategorySupplierDtos.ToArray(),
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
        public async Task<TemplateApi> GetCategorySupplierById(Guid IdCategorySupplier)
        {
            using var con = _DbContext.CreateConnection();
            CategorySupplier CategorySupplier = await con.QueryFirstOrDefaultAsync<CategorySupplier>(QueryCategorySupplier.queryGetByIdCategorySupplier, new { Id = IdCategorySupplier });

            if (CategorySupplier != null)
            {
                CategorySupplierDto CategorySupplierDto = CategorySupplier.Adapt<CategorySupplierDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CategorySupplierDto,
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
        public async Task<TemplateApi> InsertCategorySupplier(CategorySupplierDto CategorySupplierDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var CategorySupplier = CategorySupplierDto.Adapt<CategorySupplier>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategorySupplier.queryGetUserById, new { Id = CategorySupplierDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCategorySupplier.queryInsertCategorySupplier, CategorySupplier, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategorySupplier.Id, "Create", "CategorySupplier", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategorySupplier.queryInsertDiary, diaries[i], tran);
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
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateCategorySupplier(CategorySupplierDto CategorySupplierDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var CategorySupplier = CategorySupplierDto.Adapt<CategorySupplier>();

            try
            {
                //get CategorySupplier by id
                CategorySupplier CategorySupplierById = await connection.QueryFirstOrDefaultAsync<CategorySupplier>(QueryCategorySupplier.queryGetByIdCategorySupplier, new { CategorySupplierDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategorySupplier.queryGetUserById, new { Id = CategorySupplierDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CategorySupplierById == null)
                {
                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryCategorySupplier.queryUpdateCategorySupplier, CategorySupplier, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CategorySupplier.Id, "Update", "CategorySupplier", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategorySupplier.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategorySupplierByList(List<Guid> IdCategorySupplier, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa nhà cung cấp" : "Kích hoạt nhà cung cấp";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategorySupplier.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdCategorySupplier.Count; i++)
                {
                    //query CategorySupplier by id
                    CategorySupplier CategorySupplier = await connection.QueryFirstOrDefaultAsync<CategorySupplier>(QueryCategorySupplier.queryGetByIdCategorySupplier, new { Id = IdCategorySupplier[i] }, tran);

                    if (CategorySupplier is null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CategorySupplier is null ? "Đã có ID không tồn tại !" : ""
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCategorySupplier.queryHideCategorySupplier, new { Id = IdCategorySupplier[i], IsHide = IsHide }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCategorySupplier[i], "Update", "CategorySupplier", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategorySupplier.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCategorySupplier(Guid IdCategorySupplier, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Hiển thị nhà cung cấp" : "Hủy hiển thị nhà cung cấp";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCategorySupplier.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query CategorySupplier by id
            CategorySupplier CategorySupplier = await connection.QueryFirstOrDefaultAsync<CategorySupplier>(QueryCategorySupplier.queryGetByIdCategorySupplier, new { Id = IdCategorySupplier }, tran);

            if (CategorySupplier is null)
            {
                tran.Rollback();

                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = CategorySupplier is null ? "Đã có ID không tồn tại !" : ""
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryCategorySupplier.queryHideCategorySupplier, new { Id = IdCategorySupplier, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCategorySupplier.queryInsertDiary, diary, tran);

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
    internal static class QueryCategorySupplier
    {
        public const string queryInsertCategorySupplier = @"INSERT INTO [dbo].[CategorySupplier]
                                                       ([Id]
                                                       ,[SupplierName]
                                                       ,[SupplierCode]
                                                       ,[TaxCode]
                                                       ,[Address]
                                                       ,[Note]
                                                       ,[IdCategory]
                                                       ,[Status]
                                                       ,[CreatedDate]
                                                       ,[CreatedBy]
                                                       ,[IsHide]
                                                       ,[IdCity]
                                                       ,[IdDistrict]
                                                       ,[IdWard])
                                  VALUES (@Id, @SupplierName, @SupplierCode, @TaxCode, @Address, @Note,
                                  @IdCategory, @Status, @CreatedDate, @CreatedBy, @IsHide, @IdCity, @IdDistrict, @IdWard)";
        public const string queryUpdateCategorySupplier = @"UPDATE [dbo].[CategorySupplier] SET SupplierName = @SupplierName,
                                                                        TaxCode = @TaxCode,
                                                                        Address = @Address,
                                                                        Note = @Note,
                                                                        IdCategory = @IdCategory,
                                                                        IdCity = @IdCity,
                                                                        IdDistrict = @IdDistrict,
                                                                        IdWard = @IdWard
                                                                        WHERE Id = @Id";
        public const string queryDeleteCategorySupplier = "DELETE FROM [dbo].[CategorySupplier] WHERE Id = @Id";
        public const string queryGetByIdCategorySupplier = "select * from [dbo].[CategorySupplier] where Id = @Id";
        public const string queryGetAllCategorySupplier = "select *from [dbo].[CategorySupplier] order by CreatedDate desc";
        public const string queryGetAllCategorySupplierAvailable = "select *from [dbo].[CategorySupplier] where IsHide = 0 order by CreatedDate desc";
        public const string queryInsertCategorySupplier_Deleted = @"INSERT INTO [dbo].[Delete_CategorySupplier]
                                                                       ([Id]
                                                                       ,[SupplierName]
                                                                       ,[SupplierCode]
                                                                       ,[TaxCode]
                                                                       ,[Address]
                                                                       ,[Note]
                                                                       ,[IdCategory]
                                                                       ,[Status]
                                                                       ,[CreatedDate]
                                                                       ,[CreatedBy]
                                                                       ,[IsHide]
                                                                       ,[IdCity]
                                                                       ,[IdDistrict]
                                                                       ,[IdWard])
                                                  VALUES (@Id, @SupplierName, @SupplierCode, @TaxCode, @Address, @Note,
                                                  @IdCategory, @Status, @CreatedDate, @CreatedBy, @IsHide, @IdCity, @IdDistrict, @IdWard)";
        public const string queryGetAllIdCategorySupplier = "select Id from [dbo].[CategorySupplier]";
        public const string queryHideCategorySupplier = "UPDATE [dbo].[CategorySupplier] SET IsHide = @IsHide WHERE Id = @Id";

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
