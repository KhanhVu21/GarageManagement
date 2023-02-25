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
    public class OrtherCostRepository: IOrtherCostRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public OrtherCostRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteOrtherCost(Guid IdOrtherCost, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryOrtherCost.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query OrtherCost by id
                OrtherCost OrtherCost = await connection.QueryFirstOrDefaultAsync<OrtherCost>(QueryOrtherCost.queryGetByIdOrtherCost, new { Id = IdOrtherCost }, tran);

                if (OrtherCost is null)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = OrtherCost is null ? "Đã có ID không tồn tại !" : ""
                    };
                    return responseFalse;
                }

                //OrtherCost
                await connection.ExecuteAsync(QueryOrtherCost.queryDeleteOrtherCost, new { Id = IdOrtherCost }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdOrtherCost, "Delete", "OrtherCost", ""));

                //save to OrtherCost_Deleted
                await connection.ExecuteAsync(QueryOrtherCost.queryInsertOrtherCost_Deleted, OrtherCost, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdOrtherCost, "Create", "OrtherCost_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryOrtherCost.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteOrtherCostByList(List<Guid> IdOrtherCost, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryOrtherCost.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdOrtherCost.Count; i++)
                {
                    //query OrtherCost by id
                    OrtherCost OrtherCostId = await connection.QueryFirstOrDefaultAsync<OrtherCost>(QueryOrtherCost.queryGetByIdOrtherCost, new { Id = IdOrtherCost[i] }, tran);

                    if (OrtherCostId is null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = OrtherCostId is null ? "Đã có ID không tồn tại !" : ""
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryOrtherCost.queryDeleteOrtherCost, new { Id = IdOrtherCost[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdOrtherCost[i], "Delete", "OrtherCost", ""));

                    //save to OrtherCost_Deleted
                    await connection.ExecuteAsync(QueryOrtherCost.queryInsertOrtherCost_Deleted, OrtherCostId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, OrtherCostId.Id, "Create", "OrtherCost_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryOrtherCost.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllOrtherCost(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<OrtherCost> OrtherCosts = await connection.QueryAsync<OrtherCost>(QueryOrtherCost.queryGetAllOrtherCost);
            int coutRecord = OrtherCosts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                OrtherCosts = OrtherCosts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<OrtherCostDto> OrtherCostDtos = OrtherCosts.Adapt<IEnumerable<OrtherCostDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = OrtherCostDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllOrtherCostByIdRepairOders(int pageNumber, int pageSize, Guid IdRepairOders)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<OrtherCost> OrtherCosts = await connection.QueryAsync<OrtherCost>(QueryOrtherCost.queryGetAllOrtherCostIdRepairOders, new { IdRepairOders = IdRepairOders });
            int coutRecord = OrtherCosts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                OrtherCosts = OrtherCosts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<OrtherCostDto> OrtherCostDtos = OrtherCosts.Adapt<IEnumerable<OrtherCostDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = OrtherCostDtos.ToArray(),
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
        public async Task<TemplateApi> GetOrtherCostById(Guid IdOrtherCost)
        {
            using var con = _DbContext.CreateConnection();
            OrtherCost OrtherCost = await con.QueryFirstOrDefaultAsync<OrtherCost>(QueryOrtherCost.queryGetByIdOrtherCost, new { Id = IdOrtherCost });

            if (OrtherCost != null)
            {
                OrtherCostDto OrtherCostDto = OrtherCost.Adapt<OrtherCostDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = OrtherCostDto,
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
        public async Task<TemplateApi> InsertOrtherCost(OrtherCostDto OrtherCostDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var OrtherCost = OrtherCostDto.Adapt<OrtherCost>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryOrtherCost.queryGetUserById, new { Id = OrtherCostDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryOrtherCost.queryInsertOrtherCost, OrtherCost, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, OrtherCost.Id, "Create", "OrtherCost", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryOrtherCost.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateOrtherCost(OrtherCostDto OrtherCostDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var OrtherCost = OrtherCostDto.Adapt<OrtherCost>();

            //get OrtherCost by id
            OrtherCost OrtherCostById = await connection.QueryFirstOrDefaultAsync<OrtherCost>(QueryOrtherCost.queryGetByIdOrtherCost, new { OrtherCostDto.Id });

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryOrtherCost.queryGetUserById, new { Id = OrtherCostDto.IdUserCurrent });
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (OrtherCostById == null)
            {
                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryOrtherCost.queryUpdateOrtherCost, OrtherCost);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, OrtherCost.Id, "Update", "OrtherCost", ""));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryOrtherCost.queryInsertDiary, diaries[i]);
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

    internal static class QueryOrtherCost
    {
        public const string queryInsertOrtherCost = @"INSERT INTO [dbo].[OrtherCost]
                   ([Id]
                   ,[CostContent]
                   ,[Quantity]
                   ,[Price]
                   ,[TotalMoney]
                   ,[CreatedDate]
                   ,[Status]
                   ,[IdRepairOders])
                 VALUES (@Id, @CostContent, @Quantity, @Price, @TotalMoney, @CreatedDate, @Status, @IdRepairOders)";
        public const string queryUpdateOrtherCost = @"UPDATE [dbo].[OrtherCost] SET CostContent = @CostContent,
                                                                        Quantity = @Quantity,
                                                                        Price = @Price,
                                                                        TotalMoney = @TotalMoney
                                                                        WHERE Id = @Id";
        public const string queryDeleteOrtherCost = "DELETE FROM [dbo].[OrtherCost] WHERE Id = @Id";
        public const string queryGetByIdOrtherCost = "select * from [dbo].[OrtherCost] where Id = @Id";
        public const string queryGetAllOrtherCost = "select *from [dbo].[OrtherCost] order by CreatedDate desc";
        public const string queryInsertOrtherCost_Deleted = @"INSERT INTO [dbo].[Delete_OrtherCost]
                   ([Id]
                   ,[CostContent]
                   ,[Quantity]
                   ,[Price]
                   ,[TotalMoney]
                   ,[CreatedDate]
                   ,[Status]
                   ,[IdRepairOders])
                 VALUES (@Id, @CostContent, @Quantity, @Price, @TotalMoney, @CreatedDate, @Status, @IdRepairOders)";

        public const string queryGetAllIdOrtherCost = "select Id from [dbo].[OrtherCost]";
        public const string queryGetAllOrtherCostIdRepairOders = "select * from [dbo].[OrtherCost] where IdRepairOders = @IdRepairOders";

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

