using Azure;
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
    public class EmployeeRepository : IEmployeeRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public EmployeeRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllEmployeeByIdGroup(int pageNumber, int pageSize, Guid IdGroup)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Employee> Employees = await connection.QueryAsync<Employee>(QueryEmployee.queryGetAllEmployeeByIdGroup, new { IdGroup = IdGroup });
            int coutRecord = Employees.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Employees = Employees.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<EmployeeDto> EmployeeDtos = Employees.Adapt<IEnumerable<EmployeeDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = EmployeeDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllEmployee(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Employee> Employees = await connection.QueryAsync<Employee>(QueryEmployee.queryGetAllEmployee);
            int coutRecord = Employees.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Employees = Employees.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<EmployeeDto> EmployeeDtos = Employees.Adapt<IEnumerable<EmployeeDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = EmployeeDtos.ToArray(),
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
        public async Task<TemplateApi> GetEmployeeById(Guid IdEmployee)
        {
            using var con = _DbContext.CreateConnection();
            Employee Employee = await con.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployee });

            if (Employee != null)
            {
                EmployeeDto EmployeeDto = Employee.Adapt<EmployeeDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = EmployeeDto,
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
        public async Task<Employee> GetEmployeeExcel(Guid IdEmployee)
        {
            using var con = _DbContext.CreateConnection();
            var Employee = await con.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployee });
            return Employee;
        }
        public async Task<TemplateApi> DeleteEmployee(Guid IdEmployee, Guid IdUserCurrent)
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
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table Employee
                Employee Employee = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployee }, tran);

                var repairOrders_Employee = await connection.QueryFirstOrDefaultAsync<RepairOrders_Employee>(QueryEmployee.queryRepairOrders_EmployeeByIDEmployee, new { IdEmployee = IdEmployee }, tran);

                if (Employee is null || repairOrders_Employee is not null)
                {
                    tran.Rollback();

                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = Employee is null ? "Đã có ID không tồn tại !" : "Đã có nhân viên trong đang sửa chữa không thể xóa !"
                    };
                }

                await connection.ExecuteAsync(QueryEmployee.queryDeleteEmployee, new { Id = Employee.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Employee.Id, "Delete", "Employee", ""));

                //save to Employee_Deleted
                await connection.ExecuteAsync(QueryEmployee.queryInsertEmployee_Deleted, Employee, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Employee.Id, "Create", "Employee_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployee.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteEmployeeByList(List<Guid> IdEmployee, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdEmployee.Count; i++)
                {
                    //query Employee by id
                    var EmployeeId = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployee[i] }, tran);

                    var repairOrders_Employee = await connection.QueryFirstOrDefaultAsync<RepairOrders_Employee>(QueryEmployee.queryRepairOrders_EmployeeByIDEmployee, new { IdEmployee = IdEmployee[i] }, tran);

                    if (EmployeeId is null || repairOrders_Employee is not null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = EmployeeId is null ? "Đã có ID không tồn tại !" : "Đã có nhân viên trong đang sửa chữa không thể xóa !"
                        };
                    }
                    await connection.ExecuteAsync(QueryEmployee.queryDeleteEmployee, new { Id = IdEmployee[i] }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdEmployee[i], "Delete", "Employee", ""));

                    //save to Employee_Deleted
                    await connection.ExecuteAsync(QueryEmployee.queryInsertEmployee_Deleted, EmployeeId, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, EmployeeId.Id, "Create", "Employee_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployee.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertEmployee(EmployeeDto EmployeeDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var checkExistEmail = await connection.QueryFirstOrDefaultAsync<Customer>(QueryEmployee.queryGetByEmail, new { Email = EmployeeDto.Email }, tran);
                if (checkExistEmail != null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Email này đã tồn tại !"
                    };
                }

                var checkExistPhone = await connection.QueryFirstOrDefaultAsync<Customer>(QueryEmployee.queryGetByPhone, new { Phone = EmployeeDto.Phone }, tran);
                if (checkExistPhone != null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Số điện thoại này đã tồn tại !"
                    };
                }

                var Employee = EmployeeDto.Adapt<Employee>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee.queryGetUserById, new { Id = EmployeeDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = EmployeeDto.Id,
                    Operation = "Create",
                    Table = "Employee",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryEmployee.queryInsertEmployee, Employee, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryEmployee.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateEmployee(EmployeeDto EmployeeDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                var checkExistEmail = await connection.QueryFirstOrDefaultAsync<Customer>(QueryEmployee.queryGetByEmail, new { Email = EmployeeDto.Email }, tran);
                if (checkExistEmail != null && checkExistEmail.Id != EmployeeDto.Id)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Email này đã tồn tại !"
                    };
                }

                var checkExistPhone = await connection.QueryFirstOrDefaultAsync<Customer>(QueryEmployee.queryGetByPhone, new { Phone = EmployeeDto.Phone }, tran);
                if (checkExistPhone != null && checkExistPhone.Id != EmployeeDto.Id)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Số điện thoại này đã tồn tại !"
                    };
                }

                var Employee = EmployeeDto.Adapt<Employee>();

                //get Employee by id
                Employee EmployeeById = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { EmployeeDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee.queryGetUserById, new { Id = EmployeeDto.IdUserCurrent }, tran);

                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (EmployeeById == null)
                {
                    var responseNotFound = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseNotFound;
                }
                //check id file exits
                if (EmployeeDto.idFile is not null)
                {
                    Employee.Avatar = EmployeeById.Avatar;
                }

                await connection.ExecuteAsync(QueryEmployee.queryUpdateEmployee, Employee, tran);

                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Employee.Id, "Update", "Employee", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployee.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideEmployeeByList(List<Guid> IdEmployee, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin loại thương hiệu xe" : "Kích hoạt thông tin loại phương tiện xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdEmployee.Count; i++)
                {
                    //query Employee by id
                    var Employee = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployee[i] }, tran);

                    if (Employee is null)
                    {
                        var responseNotFound = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = "Đã có ID không tồn tại !"
                        };
                        return responseNotFound;
                    }
                    await connection.ExecuteAsync(QueryEmployee.queryHideEmployee, new { Id = IdEmployee[i], IsHide = IsHide }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdEmployee[i], "Update", "Employee", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployee.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideEmployee(Guid IdEmployee, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin loại thương hiệu xe" : "Hiển thị thông tin loại phương tiện xe";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            //query Employee by id
            var Employee = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployee }, tran);

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployee.queryGetUserById, new { Id = IdUserCurrent }, tran);

            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                IdWith = IdEmployee,
                Operation = "Update",
                Table = "Employee",
                Content = content,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (Employee is null)
            {
                //save to diary if false
                saveDiaryModel.IsSuccess = false;
                Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryEmployee.queryInsertDiary, diaryFalse, tran);
                tran.Commit();

                response = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return response;
            }
            await connection.ExecuteAsync(QueryEmployee.queryHideEmployee, new { Id = IdEmployee, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryEmployee.queryInsertDiary, diary, tran);

            tran.Commit();

            response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Ẩn thành công !"
            };
            return response;
        }
        public async Task<TemplateApi> SalaryCalculateByMonth(Guid IdEmployees, DateTime date)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var weekendDatesArray = GetNumberSatAndSunOfMonth(date.Day, date.Month, date.Year);
            var MonToFriDatesArray = GetNumberMondayToFridayOfMonth(date.Day, date.Month, date.Year);
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            int weekendDaysInMonth = GetAllWeekendDayInMonth(date.Month, date.Year);
            float numberDayOff = 0;
            float numberDayOverTime = 0;
            float holidaysInMonToFri = 0;
            float totalMoneyDayOffAndOverTime = 0;
            float dayWork = date.Day - weekendDatesArray.Length;

            var Holidays = await connection.QueryAsync<Holiday>(QueryHoliday.queryGetAllHoliday);
            var EmployeeDayOff = await connection.QueryAsync<EmployeeDayOff>(QueryEmployee.queryGetEmployeeDayOffByIdEmployee, new { IdEmployee = IdEmployees, month = date.Month });
            var Employee = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = IdEmployees });
            var holidaysInMonth = Holidays.Where(e => e.DateHoliday is not null && e.DateHoliday.Value.Month == date.Month);
            var allowance = await connection.QueryAsync<AlowanceOfEmployee>(QueryEmployee.queryGetAllowanceInforOfEmployee, new { Id = IdEmployees });
            float totalMoneyInDay = Employee.SalaryBase / (daysInMonth - weekendDaysInMonth);
            float totalAllowanceAmount = await connection.QueryFirstOrDefaultAsync<float>(QueryEmployee.queryGetAllowanceAmountOfEmployee, new { Id = IdEmployees });

            if (Employee is null || EmployeeDayOff is null)
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
            }

            if (EmployeeDayOff is not null)
            {
                numberDayOff += EmployeeDayOff.Where(e => e.Onleave == 1 && e.TypeOfDayOff == 3).Count();
                numberDayOff += (EmployeeDayOff.Where(e => e.Onleave == 1 && e.TypeOfDayOff == 1).Count()
                    + EmployeeDayOff.Where(e => e.Onleave == 1 && e.TypeOfDayOff == 2).Count()) / 2.0f;

                numberDayOverTime += EmployeeDayOff.Where(e => e.Onleave == 2 && e.TypeOfDayOff == 6).Count();
                numberDayOverTime += (EmployeeDayOff.Where(e => e.Onleave == 2 && e.TypeOfDayOff == 4).Count()
                    + EmployeeDayOff.Where(e => e.Onleave == 2 && e.TypeOfDayOff == 5).Count()) / 2.0f;

                var temp = CalculateTotalMoney(EmployeeDayOff.ToList(), holidaysInMonth.ToList(), totalMoneyInDay);
                totalMoneyDayOffAndOverTime += temp[0];

                holidaysInMonToFri += holidaysInMonth.Where(e => MonToFriDatesArray.Contains(e.DateHoliday.Value.Day)).Count();
                var countHolidayInWeekDays = temp[1];

                if (countHolidayInWeekDays != 0)
                {
                    holidaysInMonToFri -= countHolidayInWeekDays;
                }
                else holidaysInMonToFri = 0;

                dayWork = (dayWork - numberDayOff) - countHolidayInWeekDays;
            }

            float totalSocialInsurance = Employee.SalaryBase * (Employee.SocialInsurancePercent / 100);
            float totalMoneyBeforeTax = (((dayWork * totalMoneyInDay) - totalSocialInsurance) + totalAllowanceAmount + totalMoneyDayOffAndOverTime);
            float totalTaxOfEmployee = totalMoneyBeforeTax * (Employee.TaxPercent / 100);
            float totalSalaryReality = totalMoneyBeforeTax - totalTaxOfEmployee;

            var salaryInforByEmployee = new SalaryInforByEmployee()
            {
                totalDayWork = dayWork,
                SalaryBase = Employee.SalaryBase,
                totalSalaryReality = totalSalaryReality,
                totalDayOff = numberDayOff,
                totalDayOverTime = numberDayOverTime,
                totalSocialInsurance = totalSocialInsurance,
                IdEmployee = IdEmployees,
                totalTaxOfEmployee = totalTaxOfEmployee,
                AllowanceOfEmployees = allowance.ToList()
            };

            return new TemplateApi()
            {
                Payload = salaryInforByEmployee,
                Message = "Lấy thông tin thành công",
                Success = true,
                Fail = false,
                TotalElement = 1,
            };
        }
        public async Task<TemplateApi> SalaryCalculateAllEmployeeByMonth(DateTime date, int pageNumber, int pageSize)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var weekendDatesArray = GetNumberSatAndSunOfMonth(date.Day, date.Month, date.Year);
            var MonToFriDatesArray = GetNumberMondayToFridayOfMonth(date.Day, date.Month, date.Year);
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            int weekendDaysInMonth = GetAllWeekendDayInMonth(date.Month, date.Year);
            float holidaysInMonToFri = 0;

            var allEmployees = await connection.QueryAsync<Employee>(QueryEmployee.queryGetAllEmployee);
            int coutRecord = allEmployees.Count();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var salaryInforByEmployees = new List<SalaryInforByEmployee>();
            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                allEmployees = allEmployees.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            foreach (var data in allEmployees)
            {
                float numberDayOff = 0;
                float numberDayOverTime = 0;
                float totalMoneyDayOffAndOverTime = 0;
                float dayWork = date.Day - weekendDatesArray.Length;

                var Holidays = await connection.QueryAsync<Holiday>(QueryHoliday.queryGetAllHoliday);
                var EmployeeDayOff = await connection.QueryAsync<EmployeeDayOff>(QueryEmployee.queryGetEmployeeDayOffByIdEmployee, new { IdEmployee = data.Id, month = date.Month });
                var Employee = await connection.QueryFirstOrDefaultAsync<Employee>(QueryEmployee.queryGetByIdEmployee, new { Id = data.Id });
                var holidaysInMonth = Holidays.Where(e => e.DateHoliday is not null && e.DateHoliday.Value.Month == date.Month);
                var allowance = await connection.QueryAsync<AlowanceOfEmployee>(QueryEmployee.queryGetAllowanceInforOfEmployee, new { Id = data.Id });
                float totalMoneyInDay = Employee.SalaryBase / (daysInMonth - weekendDaysInMonth);
                float totalAllowanceAmount = await connection.QueryFirstOrDefaultAsync<float>(QueryEmployee.queryGetAllowanceAmountOfEmployee, new { Id = data.Id });

                if (Employee is null || EmployeeDayOff is null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                }

                if (EmployeeDayOff is not null)
                {
                    numberDayOff += EmployeeDayOff.Where(e => e.Onleave == 1 && e.TypeOfDayOff == 3).Count();
                    numberDayOff += (EmployeeDayOff.Where(e => e.Onleave == 1 && e.TypeOfDayOff == 1).Count()
                        + EmployeeDayOff.Where(e => e.Onleave == 1 && e.TypeOfDayOff == 2).Count()) / 2.0f;

                    numberDayOverTime += EmployeeDayOff.Where(e => e.Onleave == 2 && e.TypeOfDayOff == 6).Count();
                    numberDayOverTime += (EmployeeDayOff.Where(e => e.Onleave == 2 && e.TypeOfDayOff == 4).Count()
                        + EmployeeDayOff.Where(e => e.Onleave == 2 && e.TypeOfDayOff == 5).Count()) / 2.0f;

                    var temp = CalculateTotalMoney(EmployeeDayOff.ToList(), holidaysInMonth.ToList(), totalMoneyInDay);
                    totalMoneyDayOffAndOverTime += temp[0];

                    holidaysInMonToFri += holidaysInMonth.Where(e => MonToFriDatesArray.Contains(e.DateHoliday.Value.Day)).Count();
                    var countHolidayInWeekDays = temp[1];

                    if (countHolidayInWeekDays != 0)
                    {
                        holidaysInMonToFri -= countHolidayInWeekDays;
                    }
                    else holidaysInMonToFri = 0;

                    dayWork = (dayWork - numberDayOff) - countHolidayInWeekDays;
                }

                float totalSocialInsurance = Employee.SalaryBase * (Employee.SocialInsurancePercent / 100);
                float totalMoneyBeforeTax = (((dayWork * totalMoneyInDay) - totalSocialInsurance) + totalAllowanceAmount + totalMoneyDayOffAndOverTime);
                float totalTaxOfEmployee = totalMoneyBeforeTax * (Employee.TaxPercent / 100);
                float totalSalaryReality = totalMoneyBeforeTax - totalTaxOfEmployee;

                salaryInforByEmployees.Add(new SalaryInforByEmployee()
                {
                    totalDayWork = dayWork,
                    SalaryBase = Employee.SalaryBase,
                    totalSalaryReality = totalSalaryReality,
                    totalDayOff = numberDayOff,
                    totalDayOverTime = numberDayOverTime,
                    totalSocialInsurance = totalSocialInsurance,
                    IdEmployee = data.Id,
                    totalTaxOfEmployee = totalTaxOfEmployee,
                    AllowanceOfEmployees = allowance.ToList()
                });
            }

            return new TemplateApi()
            {
                ListPayload = salaryInforByEmployees.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        #endregion

        #region METHOD
        public int GetAllWeekendDayInMonth(int month, int year)
        {
            DateTime startDate = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);

            int weekendCount = 0;

            for (int i = 0; i < daysInMonth; i++)
            {
                DateTime date = startDate.AddDays(i);

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekendCount++;
                }
            }

            return weekendCount;
        }
        public int[] GetNumberSatAndSunOfMonth(int day, int month, int year)
        {
            // Create a list to store the dates
            List<int> weekendDates = new List<int>();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = new DateTime(year, month, day);

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekendDates.Add(date.Day);
                }
            }

            return weekendDates.ToArray();
        }
        public int[] GetNumberMondayToFridayOfMonth(int day,int month, int year)
        {
            // Create a list to store the dates
            List<int> weekDates = new List<int>();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = new DateTime(year, month, day);

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Monday
                || date.DayOfWeek == DayOfWeek.Tuesday
                || date.DayOfWeek == DayOfWeek.Wednesday
                || date.DayOfWeek == DayOfWeek.Thursday
                || date.DayOfWeek == DayOfWeek.Friday)
                {
                    weekDates.Add(date.Day);
                }
            }

            return weekDates.ToArray(); 
        }
        public float[] CalculateTotalMoney(List<EmployeeDayOff> employeeDayOffs, List<Holiday> holidays, float totalMoneyInDay)
        {
            // Create a list to store the datas
            var result = new List<float>();
            float totalMoney = 0;
            float countHoliday = 0.0f;

            // Create a dictionary of holidays for quick lookup
            var holidayDict = holidays.ToDictionary(h => h.DateHoliday.Value.Day, h => h.PercentBonusForWork);

            // Loop through each employeeDayOff and calculate the overtime pay
            foreach (var dayOff in employeeDayOffs)
            {
                if (dayOff.Onleave == 2)
                {
                    if (holidayDict.TryGetValue(dayOff.Dayoff.Value.Day, out var bonusPercentage))
                    {
                        switch (dayOff.TypeOfDayOff)
                        {
                            case 4:
                            case 5:
                                totalMoney += totalMoneyInDay * ((bonusPercentage / 2) / 100);
                                countHoliday += 0.5f;
                                break;
                            case 6:
                                totalMoney += totalMoneyInDay * (bonusPercentage / 100);
                                countHoliday++;
                                break;
                        }
                    }
                    else
                    {
                        totalMoney += totalMoneyInDay;
                        countHoliday++;
                    }
                }
            }
            result.Add(totalMoney);
            result.Add(countHoliday);

            return result.ToArray();
        }
        #endregion
    }
    internal static class QueryEmployee
    {
        public const string queryInsertEmployee = @"INSERT INTO [dbo].[Employee]
                               ([Id]
                               ,[Code]
                               ,[Name]
                               ,[Sex]
                               ,[Birthday]
                               ,[Phone]
                               ,[Email]
                               ,[Address]
                               ,[IdCity]
                               ,[IdDistrict]
                               ,[IdWard]
                               ,[TaxNumber]
                               ,[AccountNumber]
                               ,[Note]
                               ,[Avatar]
                               ,[TypeOfEmployee]
                               ,[IdGroup]
                               ,[Status]
                               ,[CreatedDate]
                               ,[SalaryBase]
                               ,[SocialInsurancePercent]
                               ,[TaxPercent])
                         VALUES (@Id, @Code, @Name, @Sex,
                                @Birthday, @Phone, @Email, @Address,
                                @IdCity, @IdDistrict, @IdWard, @TaxNumber,
                                @AccountNumber, @Note, @Avatar, @TypeOfEmployee, @IdGroup, @Status, @CreatedDate,@SalaryBase,@SocialInsurancePercent,@TaxPercent)";
        public const string queryUpdateEmployee = @"UPDATE [dbo].[Employee] SET
                                        Name = @Name,
                                        Sex = @Sex,
                                        Birthday = @Birthday,
                                        Phone = @Phone,
                                        Email = @Email,
                                        Address = @Address,
                                        IdCity = @IdCity,
                                        IdDistrict = @IdDistrict,
                                        IdWard = @IdWard,
                                        TaxNumber = @TaxNumber,
                                        AccountNumber = @AccountNumber,
                                        Note = @Note,
                                        Avatar = @Avatar,
                                        TypeOfEmployee = @TypeOfEmployee,
                                        SalaryBase = @SalaryBase,
                                        SocialInsurancePercent = @SocialInsurancePercent,
                                        TaxPercent = @TaxPercent,
                                        IdGroup = @IdGroup
                                        WHERE Id = @Id";
        public const string queryDeleteEmployee = "DELETE FROM [dbo].[Employee] WHERE Id = @Id";
        public const string queryGetByIdEmployee = "select * from [dbo].[Employee] where Id = @Id";
        public const string queryGetByPhone = "select * from [dbo].[Employee] where Phone = @Phone";
        public const string queryGetByEmail = "select * from [dbo].[Employee] where Email = @Email";
        public const string queryGetEmployeeDayOffByIdEmployee = "select * from [dbo].[EmployeeDayOff] t where IdEmployee = @IdEmployee and MONTH(t.Dayoff) = @month";
        public const string queryGetAllEmployee = "select *from [dbo].[Employee] order by CreatedDate desc";
        public const string queryHideEmployee = "UPDATE [dbo].[Employee] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertEmployee_Deleted = @"INSERT INTO [dbo].[Employee_Deleted]
                               ([Id]
                               ,[Code]
                               ,[Name]
                               ,[Sex]
                               ,[Birthday]
                               ,[Phone]
                               ,[Email]
                               ,[Address]
                               ,[IdCity]
                               ,[IdDistrict]
                               ,[IdWard]
                               ,[TaxNumber]
                               ,[AccountNumber]
                               ,[Note]
                               ,[Avatar]
                               ,[TypeOfEmployee]
                               ,[IdGroup]
                               ,[Status]
                               ,[CreatedDate]
                               ,[SalaryBase]
                               ,[SocialInsurancePercent]
                               ,[TaxPercent])
                         VALUES (@Id, @Code, @Name, @Sex,
                                @Birthday, @Phone, @Email, @Address,
                                @IdCity, @IdDistrict, @IdWard, @TaxNumber,
                                @AccountNumber, @Note, @Avatar, @TypeOfEmployee, @IdGroup, @Status, @CreatedDate,@SalaryBase,@SocialInsurancePercent,@TaxPercent)";
        public const string queryGetAllIdEmployee = "select Id from [dbo].[Employee]";
        public const string queryGetAllEmployeeByIdGroup = "select *from [dbo].[Employee] where IdGroup = @IdGroup order by CreatedDate desc";
        public const string queryRepairOrders_EmployeeByIDEmployee = "select Id from [dbo].[RepairOrders_Employee] where IdEmployee = @IdEmployee";
        public const string queryGetAllowanceAmountOfEmployee = @"
                                                            select COALESCE(SUM(a.Amount), 0) from Allowance a 
                                                            left join Employee_Allowance ea on ea.IdAllowance = a.Id
                                                            left join Employee e on e.Id = ea.IdEmployee
                                                            where e.Id = @Id";
        public const string queryGetAllowanceInforOfEmployee = @"
                                                            select a.Name, a.Amount from Allowance a
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
