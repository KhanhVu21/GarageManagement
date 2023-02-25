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
    public class ImportExportAccessaryReceiptRepository : IImportExportAccessaryReceiptRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public ImportExportAccessaryReceiptRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteImportExportAccessaryReceipt(Guid IdImportExportAccessaryReceipt, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryImportExportAccessaryReceipt.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query ImportExportAccessaryReceipt by id
                ImportExportAccessaryReceipt ImportExportAccessaryReceipt = await connection.QueryFirstOrDefaultAsync<ImportExportAccessaryReceipt>(QueryImportExportAccessaryReceipt.queryGetByIdImportExportAccessaryReceipt, new { Id = IdImportExportAccessaryReceipt }, tran);

                if (ImportExportAccessaryReceipt is null)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = ImportExportAccessaryReceipt is null ? "Đã có ID không tồn tại !" : ""
                    };
                    return responseFalse;
                }

                //ImportExportAccessaryReceipt
                await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryDeleteImportExportAccessaryReceipt, new { Id = IdImportExportAccessaryReceipt }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdImportExportAccessaryReceipt, "Delete", "ImportExportAccessaryReceipt", ""));

                //save to ImportExportAccessaryReceipt_Deleted
                await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryInsertImportExportAccessaryReceipt_Deleted, ImportExportAccessaryReceipt, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdImportExportAccessaryReceipt, "Create", "ImportExportAccessaryReceipt_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteImportExportAccessaryReceiptByList(List<Guid> IdImportExportAccessaryReceipt, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryImportExportAccessaryReceipt.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdImportExportAccessaryReceipt.Count; i++)
                {
                    //query ImportExportAccessaryReceipt by id
                    ImportExportAccessaryReceipt ImportExportAccessaryReceiptId = await connection.QueryFirstOrDefaultAsync<ImportExportAccessaryReceipt>(QueryImportExportAccessaryReceipt.queryGetByIdImportExportAccessaryReceipt, new { Id = IdImportExportAccessaryReceipt[i] }, tran);

                    if (ImportExportAccessaryReceiptId is null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = ImportExportAccessaryReceiptId is null ? "Đã có ID không tồn tại !" : ""
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryDeleteImportExportAccessaryReceipt, new { Id = IdImportExportAccessaryReceipt[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdImportExportAccessaryReceipt[i], "Delete", "ImportExportAccessaryReceipt", ""));

                    //save to ImportExportAccessaryReceipt_Deleted
                    await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryInsertImportExportAccessaryReceipt_Deleted, ImportExportAccessaryReceiptId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, ImportExportAccessaryReceiptId.Id, "Create", "ImportExportAccessaryReceipt_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllImportExportAccessaryReceipt(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<ImportExportAccessaryReceipt> ImportExportAccessaryReceipts = await connection.QueryAsync<ImportExportAccessaryReceipt>(QueryImportExportAccessaryReceipt.queryGetAllImportExportAccessaryReceipt);
            int coutRecord = ImportExportAccessaryReceipts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                ImportExportAccessaryReceipts = ImportExportAccessaryReceipts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<ImportExportAccessaryReceiptDto> ImportExportAccessaryReceiptDtos = ImportExportAccessaryReceipts.Adapt<IEnumerable<ImportExportAccessaryReceiptDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = ImportExportAccessaryReceiptDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllImportExportAccessaryReceiptByIdAccessary(int pageNumber, int pageSize, Guid IdAccessary)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<ImportExportAccessaryReceipt> ImportExportAccessaryReceipts = await connection.QueryAsync<ImportExportAccessaryReceipt>(QueryImportExportAccessaryReceipt.queryGetAllImportExportAccessaryReceiptByIdAccessary, new { IdAccessary = IdAccessary });
            int coutRecord = ImportExportAccessaryReceipts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                ImportExportAccessaryReceipts = ImportExportAccessaryReceipts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<ImportExportAccessaryReceiptDto> ImportExportAccessaryReceiptDtos = ImportExportAccessaryReceipts.Adapt<IEnumerable<ImportExportAccessaryReceiptDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = ImportExportAccessaryReceiptDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllImportExportAccessaryReceiptByIdEmployee(int pageNumber, int pageSize, Guid IdEmployee)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<ImportExportAccessaryReceipt> ImportExportAccessaryReceipts = await connection.QueryAsync<ImportExportAccessaryReceipt>(QueryImportExportAccessaryReceipt.queryGetAllImportExportAccessaryReceiptByIdEmployee, new { IdEmployee = IdEmployee });
            int coutRecord = ImportExportAccessaryReceipts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                ImportExportAccessaryReceipts = ImportExportAccessaryReceipts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<ImportExportAccessaryReceiptDto> ImportExportAccessaryReceiptDtos = ImportExportAccessaryReceipts.Adapt<IEnumerable<ImportExportAccessaryReceiptDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = ImportExportAccessaryReceiptDtos.ToArray(),
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
        public async Task<TemplateApi> GetImportExportAccessaryReceiptById(Guid IdImportExportAccessaryReceipt)
        {
            using var con = _DbContext.CreateConnection();
            ImportExportAccessaryReceipt ImportExportAccessaryReceipt = await con.QueryFirstOrDefaultAsync<ImportExportAccessaryReceipt>(QueryImportExportAccessaryReceipt.queryGetByIdImportExportAccessaryReceipt, new { Id = IdImportExportAccessaryReceipt });

            if (ImportExportAccessaryReceipt != null)
            {
                ImportExportAccessaryReceiptDto ImportExportAccessaryReceiptDto = ImportExportAccessaryReceipt.Adapt<ImportExportAccessaryReceiptDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = ImportExportAccessaryReceiptDto,
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
        public async Task<TemplateApi> InsertImportExportAccessaryReceipt(ImportExportAccessaryReceiptDto ImportExportAccessaryReceiptDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var ImportExportAccessaryReceipt = ImportExportAccessaryReceiptDto.Adapt<ImportExportAccessaryReceipt>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                Accessary Accessary = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryGetByIdAccessary, new { Id = ImportExportAccessaryReceiptDto.IdAccessary },tran);
                Accessary.Inventory += ImportExportAccessaryReceiptDto.ImportExport;
                await connection.ExecuteAsync(QueryAccessary.queryUpdateAccessary, Accessary, tran);

                ImportExportAccessaryReceipt.TotalMoney = ImportExportAccessaryReceipt.ImportExport * Accessary.PriceImport;

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryImportExportAccessaryReceipt.queryGetUserById, new { Id = ImportExportAccessaryReceiptDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryInsertImportExportAccessaryReceipt, ImportExportAccessaryReceipt, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, ImportExportAccessaryReceipt.Id, "Create", "ImportExportAccessaryReceipt", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateImportExportAccessaryReceipt(ImportExportAccessaryReceiptDto ImportExportAccessaryReceiptDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var ImportExportAccessaryReceipt = ImportExportAccessaryReceiptDto.Adapt<ImportExportAccessaryReceipt>();

            //get ImportExportAccessaryReceipt by id
            ImportExportAccessaryReceipt ImportExportAccessaryReceiptById = await connection.QueryFirstOrDefaultAsync<ImportExportAccessaryReceipt>(QueryImportExportAccessaryReceipt.queryGetByIdImportExportAccessaryReceipt, new { ImportExportAccessaryReceiptDto.Id });

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryImportExportAccessaryReceipt.queryGetUserById, new { Id = ImportExportAccessaryReceiptDto.IdUserCurrent });
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (ImportExportAccessaryReceiptById == null)
            {
                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryUpdateImportExportAccessaryReceipt, ImportExportAccessaryReceipt);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, ImportExportAccessaryReceipt.Id, "Update", "ImportExportAccessaryReceipt", ""));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryImportExportAccessaryReceipt.queryInsertDiary, diaries[i]);
            }

            var response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Cập nhật thành công !"
            };
            return response;
        }
        #endregion
    }
    internal static class QueryImportExportAccessaryReceipt
    {
        public const string queryInsertImportExportAccessaryReceipt = @"INSERT INTO [dbo].[ImportExportAccessaryReceipt]
                                                                           ([Id]
                                                                           ,[Code]
                                                                           ,[IdAccessary]
                                                                           ,[IdEmployee]
                                                                           ,[DescriptionIEX]
                                                                           ,[TotalMoney]
                                                                           ,[ImportExport]
                                                                           ,[CreatedDate]
                                                                           ,[Status])
                                                                 VALUES (@Id, @Code, @IdAccessary, @IdEmployee, @DescriptionIEX, @TotalMoney, @ImportExport, @CreatedDate, @Status)";
        public const string queryUpdateImportExportAccessaryReceipt = @"UPDATE [dbo].[ImportExportAccessaryReceipt] SET IdAccessary = @IdAccessary,
                                                                        IdEmployee = @IdEmployee,
                                                                        DescriptionIEX = @DescriptionIEX,
                                                                        TotalMoney = @TotalMoney,
                                                                        ImportExport = @ImportExport
                                                                        WHERE Id = @Id";
        public const string queryDeleteImportExportAccessaryReceipt = "DELETE FROM [dbo].[ImportExportAccessaryReceipt] WHERE Id = @Id";
        public const string queryGetByIdImportExportAccessaryReceipt = "select * from [dbo].[ImportExportAccessaryReceipt] where Id = @Id";
        public const string queryGetAllImportExportAccessaryReceipt = "select *from [dbo].[ImportExportAccessaryReceipt] order by CreatedDate desc";
        public const string queryInsertImportExportAccessaryReceipt_Deleted = @"INSERT INTO [dbo].[Delete_ImportExportAccessaryReceipt]
                                                                           ([Id]
                                                                           ,[Code]
                                                                           ,[IdAccessary]
                                                                           ,[IdEmployee]
                                                                           ,[DescriptionIEX]
                                                                           ,[TotalMoney]
                                                                           ,[ImportExport]
                                                                           ,[CreatedDate]
                                                                           ,[Status])
                                                                 VALUES (@Id, @Code, @IdAccessary, @IdEmployee, @DescriptionIEX, @TotalMoney, @ImportExport, @CreatedDate, @Status)";

        public const string queryGetAllIdImportExportAccessaryReceipt = "select Id from [dbo].[ImportExportAccessaryReceipt]";
        public const string queryGetAllImportExportAccessaryReceiptByIdAccessary = "select * from [dbo].[ImportExportAccessaryReceipt] where IdAccessary = @IdAccessary";
        public const string queryGetAllImportExportAccessaryReceiptByIdEmployee = "select * from [dbo].[ImportExportAccessaryReceipt] where IdEmployee = @IdEmployee";

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
