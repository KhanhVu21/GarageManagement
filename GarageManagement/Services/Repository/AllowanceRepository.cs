using GarageManagement.Data.Context;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using Dapper;
using Mapster;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.Common.Function;
using GarageManagement.Data.Entity;
using System.Drawing;
using Org.BouncyCastle.Security.Certificates;

namespace GarageManagement.Services.Repository
{
    public class AllowanceRepository: IAllowanceRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public AllowanceRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteAllowanceByList(List<Guid> IdAllowance, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAllowance.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdAllowance.Count; i++)
                {
                    //query Allowance by id
                    var AllowanceId = await connection.QueryFirstOrDefaultAsync<Allowance>(QueryAllowance.queryGetByIdAllowance, new { Id = IdAllowance[i] }, tran);

                    var allowanceEnployee = await connection.QueryFirstOrDefaultAsync<Allowance>(QueryAllowance.queryGetEmployee_AllowanceByIdAllowance, new { IdAllowance = IdAllowance[i] }, tran);

                    if (AllowanceId is null || allowanceEnployee is not null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = AllowanceId is null ? "Đã có ID không tồn tại !" : "Trợ cấp này đã thêm cho nhân viên không thể xóa !"
                        };
                    }

                    await connection.ExecuteAsync(QueryAllowance.queryDeleteAllowance, new { Id = IdAllowance[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdAllowance[i], "Delete", "Allowance", ""));

                    //save to Allowance_Deleted
                    await connection.ExecuteAsync(QueryAllowance.queryInsertAllowance_Deleted, AllowanceId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, AllowanceId.Id, "Create", "Allowance_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAllowance.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllAllowance(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Allowance> Allowances = await connection.QueryAsync<Allowance>(QueryAllowance.queryGetAllAllowance);
            int coutRecord = Allowances.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Allowances = Allowances.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<AllowanceDto> AllowanceDtos = Allowances.Adapt<IEnumerable<AllowanceDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = AllowanceDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllAllowanceByIdEmployee(int pageNumber, int pageSize, Guid IdEmployee)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Allowance> Allowances = await connection.QueryAsync<Allowance>(QueryAllowance.queryGetAllowanceInforOfEmployee, new {Id = IdEmployee });
            int coutRecord = Allowances.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Allowances = Allowances.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<AllowanceDto> AllowanceDtos = Allowances.Adapt<IEnumerable<AllowanceDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = AllowanceDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllowanceById(Guid IdAllowance)
        {
            using var con = _DbContext.CreateConnection();
            Allowance Allowance = await con.QueryFirstOrDefaultAsync<Allowance>(QueryAllowance.queryGetByIdAllowance, new { Id = IdAllowance });

            if (Allowance != null)
            {
                AllowanceDto AllowanceDto = Allowance.Adapt<AllowanceDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = AllowanceDto,
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
        public async Task<TemplateApi> InsertAllowance(AllowanceDto AllowanceDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var Allowance = AllowanceDto.Adapt<Allowance>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAllowance.queryGetUserById, new { Id = AllowanceDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryAllowance.queryInsertAllowance, Allowance, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Allowance.Id, "Create", "Allowance", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAllowance.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertAllowanceForEmployee(Guid IdEmployee, List<Guid> IdAllowances, Guid IdUserCurrent)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAllowance.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                var employeeAllowance = await connection.QueryAsync<Employee_Allowance>(QueryAllowance.queryGetEmployee_AllowanceByIdEmployeeAndIdAllowance, new { IdEmployee = IdEmployee }, tran);
                if (employeeAllowance is not null)
                {
                    var ids = employeeAllowance.Select(x => x.Id).ToList();
                    await connection.ExecuteAsync(QueryAllowance.queryDeleteAllowanceById, new { Id = ids }, tran);
                }

                for (int i = 0; i < IdAllowances.Count();i++)
                {
                    var Allowance = await connection.QueryFirstOrDefaultAsync<Allowance>(QueryAllowance.queryGetByIdAllowance, new { Id = IdAllowances[i] }, tran);
                    var Employee = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployee },tran);

                    if(Allowance is null || Employee is null)
                    {
                        return new TemplateApi()
                        {
                            Success = true,
                            Fail = false,
                            Message = "Thêm mới không thành công !"
                        };
                    }

                    var employee_Allowance = new Employee_Allowance()
                    {
                        Id = Guid.NewGuid(),
                        IdAllowance = IdAllowances[i],
                        IdEmployee = IdEmployee,
                        CreatedDate= DateTime.Now,
                    };

                    await connection.ExecuteAsync(QueryAllowance.queryInsertEmployee_Allowance, employee_Allowance, tran);
                    // save to table diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, employee_Allowance.Id, "Create", "Employee_Allowance", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryAllowance.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateAllowance(AllowanceDto AllowanceDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var Allowance = AllowanceDto.Adapt<Allowance>();

            //get Allowance by id
            Allowance AllowanceById = await connection.QueryFirstOrDefaultAsync<Allowance>(QueryAllowance.queryGetByIdAllowance, new { AllowanceDto.Id });

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryAllowance.queryGetUserById, new { Id = AllowanceDto.IdUserCurrent });
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (AllowanceById == null)
            {
                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryAllowance.queryUpdateAllowance, Allowance);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Allowance.Id, "Update", "Allowance", ""));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryAllowance.queryInsertDiary, diaries[i]);
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
    internal static class QueryAllowance
    {
        public const string queryInsertAllowance = @"INSERT INTO [dbo].[Allowance]
                                                               ([Id]
                                                               ,[Name]
                                                               ,[Amount]
                                                               ,[CreatedDate]
                                                               ,[Status])
                                                      VALUES (@Id, @Name, @Amount, @CreatedDate, @Status)";
        public const string queryInsertEmployee_Allowance = @"INSERT INTO [dbo].[Employee_Allowance]
                                       ([Id]
                                       ,[IdAllowance]
                                       ,[IdEmployee]
                                       ,[CreatedDate])
                       VALUES (@Id, @IdAllowance, @IdEmployee, @CreatedDate)";

        public const string queryUpdateAllowance = @"UPDATE [dbo].[Allowance] SET Name = @Name,
                                                                        Amount = @Amount
                                                                        WHERE Id = @Id";
        public const string queryDeleteAllowance = "DELETE FROM [dbo].[Allowance] WHERE Id = @Id";
        public const string queryDeleteAllowanceById = "DELETE FROM [dbo].[Employee_Allowance] WHERE Id in @Id";
        public const string queryGetByIdAllowance = "select * from [dbo].[Allowance] where Id = @Id";
        public const string queryGetEmployee_AllowanceByIdEmployeeAndIdAllowance = "select * from [dbo].[Employee_Allowance] where IdEmployee = @IdEmployee";
        public const string queryGetEmployee_AllowanceByIdAllowance = "select * from [dbo].[Employee_Allowance] where IdAllowance = @IdAllowance";
        public const string queryGetAllAllowance = "select *from [dbo].[Allowance] order by CreatedDate desc";
        public const string queryInsertAllowance_Deleted = @"INSERT INTO [dbo].[Delete_Allowance]
                                                               ([Id]
                                                               ,[Name]
                                                               ,[Amount]
                                                               ,[CreatedDate]
                                                               ,[Status])
                                                      VALUES (@Id, @Name, @Amount, @CreatedDate, @Status)";

        public const string queryGetAllowanceInforOfEmployee = @"
                                                                select a.* from Allowance a
                                                                left join Employee_Allowance ea on ea.IdAllowance = a.Id
                                                                left join Employee e on e.Id = ea.IdEmployee
                                                                where e.Id = @Id";

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
