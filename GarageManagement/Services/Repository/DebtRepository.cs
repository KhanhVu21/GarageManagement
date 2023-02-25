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
    public class DebtRepository: IDebtRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public DebtRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteDebt(Guid IdDebt, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryDebt.queryGetUserById, new { Id = IdUserCurrent }, tran);

                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query Debt by id
                Debt Debt = await connection.QueryFirstOrDefaultAsync<Debt>(QueryDebt.queryGetByIdDebt, new { Id = IdDebt }, tran);

                if (Debt is null)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = Debt is null ? "Đã có ID không tồn tại !" : ""
                    };
                    return responseFalse;
                }

                //Debt
                await connection.ExecuteAsync(QueryDebt.queryDeleteDebt, new { Id = IdDebt }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdDebt, "Delete", "Debt", ""));

                //save to Debt_Deleted
                await connection.ExecuteAsync(QueryDebt.queryInsertDebt_Deleted, Debt, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdDebt, "Create", "Debt_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryDebt.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteDebtByList(List<Guid> IdDebt, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryDebt.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdDebt.Count; i++)
                {
                    //query Debt by id
                    Debt DebtId = await connection.QueryFirstOrDefaultAsync<Debt>(QueryDebt.queryGetByIdDebt, new { Id = IdDebt[i] }, tran);

                    if (DebtId is null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = DebtId is null ? "Đã có ID không tồn tại !" : ""
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryDebt.queryDeleteDebt, new { Id = IdDebt[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdDebt[i], "Delete", "Debt", ""));

                    //save to Debt_Deleted
                    await connection.ExecuteAsync(QueryDebt.queryInsertDebt_Deleted, DebtId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdDebt[i], "Create", "Debt_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryDebt.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllDebt(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Debt> Debts = await connection.QueryAsync<Debt>(QueryDebt.queryGetAllDebt);
            int coutRecord = Debts.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Debts = Debts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<DebtDto> DebtDtos = Debts.Adapt<IEnumerable<DebtDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = DebtDtos.ToArray(),
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
        public async Task<TemplateApi> GetDebtById(Guid IdDebt)
        {
            using var con = _DbContext.CreateConnection();
            Debt Debt = await con.QueryFirstOrDefaultAsync<Debt>(QueryDebt.queryGetByIdDebt, new { Id = IdDebt });

            if (Debt != null)
            {
                DebtDto DebtDto = Debt.Adapt<DebtDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = DebtDto,
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
        public async Task<TemplateApi> InsertDebt(DebtDto DebtDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var Debt = DebtDto.Adapt<Debt>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryDebt.queryGetUserById, new { Id = DebtDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryDebt.queryInsertDebt, Debt, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Debt.Id, "Create", "Debt", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryDebt.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateDebt(DebtDto DebtDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var Debt = DebtDto.Adapt<Debt>();

            try
            {
                //get Debt by id
                Debt DebtById = await connection.QueryFirstOrDefaultAsync<Debt>(QueryDebt.queryGetByIdDebt, new { DebtDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryDebt.queryGetUserById, new { Id = DebtDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (DebtById == null)
                {
                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryDebt.queryUpdateDebt, Debt, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Debt.Id, "Update", "Debt", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryDebt.queryInsertDiary, diaries[i], tran);
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
        #endregion
    }
    internal static class QueryDebt
    {
        public const string queryInsertDebt = @"INSERT INTO [dbo].[Debt]
           ([Id]
           ,[DebtContent]
           ,[Deposit]
           ,[TotalPay]
           ,[DebtNumber]
           ,[LastPay]
           ,[IdRepairOrders]
           ,[CreatedDate]
           ,[Status])
            VALUES (@Id, @DebtContent, @Deposit, @TotalPay, @DebtNumber, @LastPay, @IdRepairOrders, @CreatedDate, @Status)";
        public const string queryUpdateDebt = @"UPDATE [dbo].[Debt] SET DebtContent = @DebtContent,
                                                                    Deposit = @Deposit,
                                                                    TotalPay = @TotalPay,
                                                                    DebtNumber = @DebtNumber,
                                                                    LastPay = @LastPay
                                                                    WHERE Id = @Id";
        public const string queryDeleteDebt = "DELETE FROM [dbo].[Debt] WHERE Id = @Id";
        public const string queryGetByIdDebt = "select * from [dbo].[Debt] where Id = @Id";
        public const string queryGetAllDebt = @"SELECT d.* FROM
                                                Debt d left join RO_RepairOders ro on d.IdRepairOrders = ro.Id
                                                where d.LastPay = 0 and d.Deposit < ro.TotalMoney";
        public const string queryInsertDebt_Deleted = @"INSERT INTO [dbo].[Delete_Debt]
           ([Id]
           ,[DebtContent]
           ,[Deposit]
           ,[TotalPay]
           ,[DebtNumber]
           ,[LastPay]
           ,[IdRepairOrders]
           ,[CreatedDate]
           ,[Status])
            VALUES (@Id, @DebtContent, @Deposit, @TotalPay, @DebtNumber, @LastPay, @IdRepairOrders, @CreatedDate, @Status)";
        public const string queryGetAllIdDebt = "select Id from [dbo].[Debt]";

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
