using Dapper;
using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using System.Drawing;

namespace GarageManagement.Services.Repository
{
    public class Employee_Salary_HistoryRepository: IEmployee_Salary_HistoryRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public Employee_Salary_HistoryRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }

        #endregion

        #region METHOD
        public async Task<IEnumerable<Employee_Salary_History>> Employee_Salary_HistoryByIdEmployeeAndMonth(int month)
        {
            using var con = _DbContext.CreateConnection();
            var employee_Salary_Histories = await con.QueryAsync<Employee_Salary_History>(QueryEmployee_Salary_History.queryEmployee_Salary_HistoryByMonth, new { month = month });
            return employee_Salary_Histories;
        }
        public async Task<Employee_Salary_History> Employee_Salary_HistoryByIdEmployeeAndMonth(Guid idEmployee, int month)
        {
            using var con = _DbContext.CreateConnection();
            var Employee_Salary_History = await con.QueryFirstOrDefaultAsync<Employee_Salary_History>(QueryEmployee_Salary_History.queryEmployee_Salary_HistoryByIdEmployeeAndMonth, new { IdEmployee = idEmployee, month = month });
            return Employee_Salary_History;
        }
        public async Task<TemplateApi> GetAllEmployee_Salary_History(int pageNumber, int pageSize, Guid IdEmployee)
        {
            using var connection = _DbContext.CreateConnection();
            var employee_Salary_Histories = await connection.QueryAsync<Employee_Salary_History>(QueryEmployee_Salary_History.queryEmployee_Salary_HistoryByIdEmployee, new { IdEmployee = IdEmployee });
            int coutRecord = employee_Salary_Histories.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                employee_Salary_Histories = employee_Salary_Histories.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = employee_Salary_Histories.ToArray(),
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
        public async Task<TemplateApi> GetEmployee_Salary_HistoryById(Guid IdEmployee_Salary_History)
        {
            using var con = _DbContext.CreateConnection();
            var Employee_Salary_History = await con.QueryFirstOrDefaultAsync<Employee_Salary_History>(QueryEmployee_Salary_History.queryGetByIdEmployee_Salary_History, new { Id = IdEmployee_Salary_History });

            if (Employee_Salary_History != null)
            {
                return new TemplateApi()
                {
                    Payload = Employee_Salary_History,
                    Message = "Lấy thông tin thành công",
                    Success = true,
                    Fail = false,
                    TotalElement = 1,
                };
            }
            return new TemplateApi()
            {
                Message = "Không tìm thấy kết quả",
                Success = false,
                Fail = true,
            };
        }
        public async Task<TemplateApi> InsertEmployee_Salary_History(Employee_Salary_HistoryDto Employee_Salary_HistoryDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var Employee_Salary_History = Employee_Salary_HistoryDto.Adapt<Employee_Salary_History>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee_Salary_History.queryGetUserById, new { Id = Employee_Salary_HistoryDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                int month = Employee_Salary_HistoryDto.DateSalary.Value.Month;
                int year = Employee_Salary_HistoryDto.DateSalary.Value.Year;

                var Employee_Salary_HistoryById = await connection.QueryFirstOrDefaultAsync<Employee_Salary_History>(QueryEmployee_Salary_History.queryEmployee_Salary_HistoryByIdEmployeeAndDateSalary, new { month = month, year = year, IdEmployee = Employee_Salary_HistoryDto.IdEmployee } ,tran);
                if(Employee_Salary_HistoryById is null)
                {
                    await connection.ExecuteAsync(QueryEmployee_Salary_History.queryInsertEmployee_Salary_History, Employee_Salary_History, tran);
                    // save to table diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Employee_Salary_History.Id, "Create", "Employee_Salary_History", ""));
                }
                else
                {
                    Employee_Salary_History.Id = Employee_Salary_HistoryById.Id;
                    Employee_Salary_History.IdEmployee = Employee_Salary_HistoryById.IdEmployee;
                    Employee_Salary_History.CreatedDate = Employee_Salary_HistoryById.CreatedDate;
                    Employee_Salary_History.Status = Employee_Salary_HistoryById.Status;
                    await connection.ExecuteAsync(QueryEmployee_Salary_History.queryUpdateEmployee_Salary_History, Employee_Salary_History, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Employee_Salary_History.Id, "Update", "Employee_Salary_History", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployee_Salary_History.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateEmployee_Salary_History(Employee_Salary_HistoryDto Employee_Salary_HistoryDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var Employee_Salary_History = Employee_Salary_HistoryDto.Adapt<Employee_Salary_History>();

            //get Employee_Salary_History by id
            Employee_Salary_History Employee_Salary_HistoryById = await connection.QueryFirstOrDefaultAsync<Employee_Salary_History>(QueryEmployee_Salary_History.queryGetByIdEmployee_Salary_History, new { Employee_Salary_HistoryDto.Id });

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee_Salary_History.queryGetUserById, new { Id = Employee_Salary_HistoryDto.IdUserCurrent });
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (Employee_Salary_HistoryById == null)
            {
                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryEmployee_Salary_History.queryUpdateEmployee_Salary_History, Employee_Salary_History);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Employee_Salary_History.Id, "Update", "Employee_Salary_History", ""));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryEmployee_Salary_History.queryInsertDiary, diaries[i]);
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
    internal static class QueryEmployee_Salary_History
    {
        public const string queryEmployee_Salary_HistoryByIdEmployee = "select * from [dbo].[Employee_Salary_History] where IdEmployee = @IdEmployee";
        public const string queryEmployee_Salary_HistoryByIdEmployeeAndMonth = "select * from [dbo].[Employee_Salary_History] where IdEmployee = @IdEmployee and MONTH(DateSalary) = @month";
        public const string queryEmployee_Salary_HistoryByMonth = "select * from [dbo].[Employee_Salary_History] where MONTH(DateSalary) = @month";
        public const string queryInsertEmployee_Salary_History = @"INSERT INTO [dbo].[Employee_Salary_History]
                               ([Id]
                               ,[IdEmployee]
                               ,[Allowance]
                               ,[SocialInsurance]
                               ,[SalaryBase]
                               ,[TotalSalaryReality]
                               ,[DateSalary]
                               ,[CreatedDate]
                               ,[Status]
                               ,[TaxPay])
           VALUES (@Id, @IdEmployee, @Allowance, @SocialInsurance, @SalaryBase,
                    @TotalSalaryReality, @DateSalary, @CreatedDate,@Status, @TaxPay)";
        public const string queryUpdateEmployee_Salary_History = @"UPDATE [dbo].[Employee_Salary_History] SET Allowance = @Allowance,
                                                                        SocialInsurance = @SocialInsurance,
                                                                        SalaryBase = @SalaryBase,
                                                                        TotalSalaryReality = @TotalSalaryReality,
                                                                        DateSalary = @DateSalary,
                                                                        TaxPay = @TaxPay
                                                                        WHERE Id = @Id";
        public const string queryGetByIdEmployee_Salary_History = "select * from [dbo].[Employee_Salary_History] where Id = @Id";
        public const string queryEmployee_Salary_HistoryByIdEmployeeAndDateSalary = @"select * from [dbo].[Employee_Salary_History] where MONTH(DateSalary) = @month
                                                                                    and YEAR(DateSalary) = @year
                                                                                    and IdEmployee = @IdEmployee";

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
