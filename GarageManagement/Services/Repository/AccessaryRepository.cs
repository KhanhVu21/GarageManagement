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
    public class AccessaryRepository: IAccessaryRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public AccessaryRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteAccessary(Guid IdAccessary, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessary.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query Accessary by id
                Accessary Accessary = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryGetByIdAccessary, new { Id = IdAccessary }, tran);

                if (Accessary is null)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = Accessary is null ? "Đã có ID không tồn tại !" : ""
                    };
                    return responseFalse;
                }

                //Accessary
                await connection.ExecuteAsync(QueryAccessary.queryDeleteAccessary, new { Id = IdAccessary }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAccessary, "Delete", "Accessary", ""));

                //save to Accessary_Deleted
                await connection.ExecuteAsync(QueryAccessary.queryInsertAccessary_Deleted, Accessary, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAccessary, "Create", "Accessary_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAccessary.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteAccessaryByList(List<Guid> IdAccessary, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessary.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdAccessary.Count; i++)
                {
                    //query Accessary by id
                    var AccessaryId = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryGetByIdAccessary, new { Id = IdAccessary[i] }, tran);

                    var RepairOrders_Accessary = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryRepairOrders_Accessary, new { IdCategoryAccessary = IdAccessary[i] }, tran);

                    if (AccessaryId is null || RepairOrders_Accessary is not null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = AccessaryId is null ? "Đã có ID không tồn tại !" : "Phụ tùng đã tồn tại trong hóa đơn không thể xóa !"
                        };
                    }

                    await connection.ExecuteAsync(QueryAccessary.queryDeleteAccessary, new { Id = IdAccessary[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAccessary[i], "Delete", "Accessary", ""));

                    //save to Accessary_Deleted
                    await connection.ExecuteAsync(QueryAccessary.queryInsertAccessary_Deleted, AccessaryId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, AccessaryId.Id, "Create", "Accessary_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAccessary.queryInsertDiary, diaries[i], tran);
                }

                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Xóa thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> GetAllAccessary(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Accessary> Accessarys = await connection.QueryAsync<Accessary>(QueryAccessary.queryGetAllAccessary);
            int coutRecord = Accessarys.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Accessarys = Accessarys.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<AccessaryDto> AccessaryDtos = Accessarys.Adapt<IEnumerable<AccessaryDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = AccessaryDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllAccessaryByGroupID(int pageNumber, int pageSize, Guid GroupID)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Accessary> Accessarys = await connection.QueryAsync<Accessary>(QueryAccessary.queryGetAllAccessaryByGroupID, new { GroupID = GroupID });
            int coutRecord = Accessarys.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Accessarys = Accessarys.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<AccessaryDto> AccessaryDtos = Accessarys.Adapt<IEnumerable<AccessaryDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = AccessaryDtos.ToArray(),
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
        public async Task<TemplateApi> GetAccessaryById(Guid IdAccessary)
        {
            using var con = _DbContext.CreateConnection();
            Accessary Accessary = await con.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryGetByIdAccessary, new { Id = IdAccessary });

            if (Accessary != null)
            {
                AccessaryDto AccessaryDto = Accessary.Adapt<AccessaryDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = AccessaryDto,
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
        public async Task<TemplateApi> InsertAccessary(AccessaryDto AccessaryDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var Accessary = AccessaryDto.Adapt<Accessary>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessary.queryGetUserById, new { Id = AccessaryDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryAccessary.queryInsertAccessary, Accessary, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Accessary.Id, "Create", "Accessary", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAccessary.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateAccessary(AccessaryDto AccessaryDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var Accessary = AccessaryDto.Adapt<Accessary>();

            //get Accessary by id
            Accessary AccessaryById = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryGetByIdAccessary, new { AccessaryDto.Id });

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAccessary.queryGetUserById, new { Id = AccessaryDto.IdUserCurrent });
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (AccessaryById == null)
            {
                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryAccessary.queryUpdateAccessary, Accessary);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Accessary.Id, "Update", "Accessary", ""));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryAccessary.queryInsertDiary, diaries[i]);
            }
            
            var response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Cập nhật thành công !"
            };
            return response;
        }

        public async Task<List<Tuple<Guid, String, String>>> InventoryAlert()
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Accessary> accessarys = await connection.QueryAsync<Accessary>(QueryAccessary.queryGetAllAccessary);
            if(!accessarys.Any())
            {
                return new List<Tuple<Guid, string, string>>();
            }

            var result = accessarys.Where(x => x.Inventory <= x.InventoryAlert)
                .ToList();

            return result.Select(x => Tuple.Create(x.Id, x.Name, x.Code)).ToList();
        }
        #endregion
    }
    internal static class QueryAccessary
    {
        public const string queryInsertAccessary = @"INSERT INTO [dbo].[Accessary]
                                                                   ([Id]
                                                                   ,[Code]
                                                                   ,[Name]
                                                                   ,[Description]
                                                                   ,[AccessaryGroup]
                                                                   ,[UnitName]
                                                                   ,[PriceImport]
                                                                   ,[PriceExport]
                                                                   ,[CreatedDate]
                                                                   ,[Status]
                                                                   ,[Inventory]
                                                                   ,[InventoryAlert]
                                                                   ,[GroupID])
                                                                 VALUES (@Id, @Code, @Name, @Description, @AccessaryGroup, @UnitName, @PriceImport, @PriceExport,
                                                                        @CreatedDate, @Status, @Inventory, @InventoryAlert, @GroupID)";
        public const string queryUpdateAccessary = @"UPDATE [dbo].[Accessary] SET Name = @Name,
                                                                        Description = @Description,
                                                                        AccessaryGroup = @AccessaryGroup,
                                                                        UnitName = @UnitName,
                                                                        PriceImport = @PriceImport,
                                                                        PriceExport = @PriceExport,
                                                                        Inventory = @Inventory,
                                                                        InventoryAlert = @InventoryAlert,
                                                                        GroupID = @GroupID
                                                                        WHERE Id = @Id";
        public const string queryGetByIdAccessary = "select * from [dbo].[Accessary] where Id = @Id";
        public const string queryDeleteAccessary = "DELETE FROM [dbo].[Accessary] WHERE Id = @Id";
        public const string queryGetAllAccessary = "select *from [dbo].[Accessary] order by CreatedDate desc";
        public const string queryInsertAccessary_Deleted = @"INSERT INTO [dbo].[Delete_Accessary]
                                                                   ([Id]
                                                                   ,[Code]
                                                                   ,[Name]
                                                                   ,[Description]
                                                                   ,[AccessaryGroup]
                                                                   ,[UnitName]
                                                                   ,[PriceImport]
                                                                   ,[PriceExport]
                                                                   ,[CreatedDate]
                                                                   ,[Status]
                                                                   ,[Inventory]
                                                                   ,[InventoryAlert]
                                                                   ,[GroupID])
                                                                 VALUES (@Id, @Code, @Name, @Description, @AccessaryGroup, @UnitName, @PriceImport, @PriceExport,
                                                                        @CreatedDate, @Status, @Inventory, @InventoryAlert, @GroupID)";
        public const string queryRepairOrders_Accessary = "select * from RepairOrders_Accessary where IdCategoryAccessary = @IdCategoryAccessary";

        public const string queryGetAllIdAccessary = "select Id from [dbo].[Accessary]";
        public const string queryGetAllAccessaryByGroupID = "select * from [dbo].[Accessary] where GroupID = @GroupID";

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
