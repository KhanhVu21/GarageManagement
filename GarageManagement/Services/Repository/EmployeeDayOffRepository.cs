using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using Dapper;
using Mapster;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.Repository
{
    public class EmployeeDayOffRepository: IEmployeeDayOffRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public EmployeeDayOffRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteEmployeeDayOffByList(List<Guid> IdEmployeeDayOff, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeDayOff.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdEmployeeDayOff.Count; i++)
                {
                    //query EmployeeDayOff by id
                    EmployeeDayOff EmployeeDayOffId = await connection.QueryFirstOrDefaultAsync<EmployeeDayOff>(QueryEmployeeDayOff.queryGetByIdEmployeeDayOff, new { Id = IdEmployeeDayOff[i] }, tran);

                    // get Accssesary by id IdGroup
                    var Accessarys = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryEmployeeDayOff.queryGetAllAccessaryByGroupID, new { GroupID = IdEmployeeDayOff[i] }, tran);

                    if (EmployeeDayOffId is null || Accessarys is not null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = EmployeeDayOffId is null ? "Đã có ID không tồn tại !" : "Đã có phụ kiện trong nhóm này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryEmployeeDayOff.queryDeleteEmployeeDayOff, new { Id = IdEmployeeDayOff[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdEmployeeDayOff[i], "Delete", "EmployeeDayOff", ""));

                    //save to EmployeeDayOff_Deleted
                    await connection.ExecuteAsync(QueryEmployeeDayOff.queryInsertEmployeeDayOff_Deleted, EmployeeDayOffId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdEmployeeDayOff[i], "Create", "EmployeeDayOff_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployeeDayOff.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllEmployeeDayOff(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<EmployeeDayOff> EmployeeDayOffs = await connection.QueryAsync<EmployeeDayOff>(QueryEmployeeDayOff.queryGetAllEmployeeDayOff);
            int coutRecord = EmployeeDayOffs.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                EmployeeDayOffs = EmployeeDayOffs.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<EmployeeDayOffDto> EmployeeDayOffDtos = EmployeeDayOffs.Adapt<IEnumerable<EmployeeDayOffDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = EmployeeDayOffDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllEmployeeOverTime(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<EmployeeDayOff> EmployeeDayOffs = await connection.QueryAsync<EmployeeDayOff>(QueryEmployeeDayOff.queryGetAllEmployeeOverTime);
            int coutRecord = EmployeeDayOffs.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                EmployeeDayOffs = EmployeeDayOffs.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<EmployeeDayOffDto> EmployeeDayOffDtos = EmployeeDayOffs.Adapt<IEnumerable<EmployeeDayOffDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = EmployeeDayOffDtos.ToArray(),
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
        public async Task<TemplateApi> GetEmployeeDayOffById(Guid IdEmployeeDayOff)
        {
            using var con = _DbContext.CreateConnection();
            EmployeeDayOff EmployeeDayOff = await con.QueryFirstOrDefaultAsync<EmployeeDayOff>(QueryEmployeeDayOff.queryGetByIdEmployeeDayOff, new { Id = IdEmployeeDayOff });

            if (EmployeeDayOff != null)
            {
                EmployeeDayOffDto EmployeeDayOffDto = EmployeeDayOff.Adapt<EmployeeDayOffDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = EmployeeDayOffDto,
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
        public async Task<TemplateApi> InsertEmployeeDayOff(EmployeeDayOffDto EmployeeDayOffDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var EmployeeDayOff = EmployeeDayOffDto.Adapt<EmployeeDayOff>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                
                var holiday = await connection.QueryFirstOrDefaultAsync<Holiday>(QueryEmployeeDayOff.queryGetHolidayByDateHoliday, new { day = EmployeeDayOffDto.Dayoff.Value.Day, month = EmployeeDayOffDto.Dayoff.Value.Month, year = EmployeeDayOffDto.Dayoff.Value.Year }, tran);
                if (holiday != null && EmployeeDayOffDto.Onleave == 0 || holiday != null && EmployeeDayOffDto.Onleave == 1)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không thể thêm ngày vắng của nhân viên vào ngày lễ !"
                    };
                }
                var EmployeeDayOffByEmployee = await connection.QueryFirstOrDefaultAsync<EmployeeDayOff>(QueryEmployeeDayOff.queryGetByIdEmployeeDayOffByIdEmployeeAndDate, new { IdEmployee = EmployeeDayOffDto.IdEmployee, day = EmployeeDayOffDto.Dayoff.Value.Day, month = EmployeeDayOffDto.Dayoff.Value.Month, year = EmployeeDayOffDto.Dayoff.Value.Year },tran);
                if (EmployeeDayOffByEmployee != null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Nhân viên đã {(EmployeeDayOffByEmployee.Onleave == 0 || EmployeeDayOffByEmployee.Onleave == 1 ? "nghỉ" : "tăng ca")} ngày này!"
                    };
                }

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeDayOff.queryGetUserById, new { Id = EmployeeDayOffDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryEmployeeDayOff.queryInsertEmployeeDayOff, EmployeeDayOff, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, EmployeeDayOff.Id, "Create", "EmployeeDayOff", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployeeDayOff.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateEmployeeDayOff(EmployeeDayOffDto EmployeeDayOffDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var EmployeeDayOff = EmployeeDayOffDto.Adapt<EmployeeDayOff>();

            try
            {
                var holiday = await connection.QueryFirstOrDefaultAsync<Holiday>(QueryEmployeeDayOff.queryGetHolidayByDateHoliday, new { day = EmployeeDayOffDto.Dayoff.Value.Day, month = EmployeeDayOffDto.Dayoff.Value.Month, year = EmployeeDayOffDto.Dayoff.Value.Year }, tran);
                if (holiday != null && EmployeeDayOffDto.Onleave == 0 || holiday != null && EmployeeDayOffDto.Onleave == 1)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không thể thêm ngày vắng của nhân viên vào ngày lễ !"
                    };
                }

                var EmployeeDayOffByEmployee = await connection.QueryFirstOrDefaultAsync<EmployeeDayOff>(QueryEmployeeDayOff.queryGetByIdEmployeeDayOffByIdEmployeeAndDate, new { IdEmployee = EmployeeDayOffDto.IdEmployee, day = EmployeeDayOffDto.Dayoff.Value.Day, month = EmployeeDayOffDto.Dayoff.Value.Month, year = EmployeeDayOffDto.Dayoff.Value.Year }, tran);
                if (EmployeeDayOffByEmployee != null && EmployeeDayOffByEmployee.Id != EmployeeDayOffDto.Id)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Nhân viên đã {(EmployeeDayOffByEmployee.Onleave == 0 || EmployeeDayOffByEmployee.Onleave == 1 ? "nghỉ" : "tăng ca")} ngày này!"
                    };
                }

                //get EmployeeDayOff by id
                EmployeeDayOff EmployeeDayOffById = await connection.QueryFirstOrDefaultAsync<EmployeeDayOff>(QueryEmployeeDayOff.queryGetByIdEmployeeDayOff, new { EmployeeDayOffDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryEmployeeDayOff.queryGetUserById, new { Id = EmployeeDayOffDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (EmployeeDayOffById == null)
                {
                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryEmployeeDayOff.queryUpdateEmployeeDayOff, EmployeeDayOff, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, EmployeeDayOff.Id, "Update", "EmployeeDayOff", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryEmployeeDayOff.queryInsertDiary, diaries[i], tran);
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
    internal static class QueryEmployeeDayOff
    {
        public const string queryInsertEmployeeDayOff = @"INSERT INTO [dbo].[EmployeeDayOff]
                                       ([Id]
                                       ,[IdEmployee]
                                       ,[Dayoff]
                                       ,[TypeOfDayOff]
                                       ,[CreatedDate]
                                       ,[Status]
                                       ,[Onleave])
                       VALUES (@Id, @IdEmployee, @Dayoff, @TypeOfDayOff, @CreatedDate, @Status, @Onleave)";
        public const string queryUpdateEmployeeDayOff = @"UPDATE [dbo].[EmployeeDayOff] SET IdEmployee = @IdEmployee,
                                                                        Dayoff = @Dayoff,
                                                                        TypeOfDayOff = @TypeOfDayOff,
                                                                        Onleave = @Onleave
                                                                        WHERE Id = @Id";
        public const string queryDeleteEmployeeDayOff = "DELETE FROM [dbo].[EmployeeDayOff] WHERE Id = @Id";
        public const string queryGetByIdEmployeeDayOff = "select * from [dbo].[EmployeeDayOff] where Id = @Id";
        public const string queryGetByIdEmployeeDayOffByIdEmployeeAndDate = @"select * from [dbo].[EmployeeDayOff] where IdEmployee = @IdEmployee 
                                                                                and day(Dayoff) = @day
                                                                                and month(Dayoff) = @month
                                                                                and year(Dayoff) = @year";
        public const string queryGetAllEmployeeDayOff = "select *from [dbo].[EmployeeDayOff] where Onleave = 0 or Onleave = 1 order by CreatedDate desc";
        public const string queryGetAllEmployeeOverTime = "select *from [dbo].[EmployeeDayOff] where Onleave = 2 order by CreatedDate desc";
        public const string queryInsertEmployeeDayOff_Deleted = @"INSERT INTO [dbo].[Delete_EmployeeDayOff]
                                       ([Id]
                                       ,[IdEmployee]
                                       ,[Dayoff]
                                       ,[TypeOfDayOff]
                                       ,[CreatedDate]
                                       ,[Status]
                                       ,[Onleave])
                       VALUES (@Id, @IdEmployee, @Dayoff, @TypeOfDayOff, @CreatedDate, @Status, @Onleave)";
        public const string queryGetAllIdEmployeeDayOff = "select Id from [dbo].[EmployeeDayOff]";
        public const string queryGetAllAccessaryByGroupID = "select Id from [dbo].[Accessary] where GroupID = @GroupID";
        public const string queryGetHolidayByDateHoliday = @"select * from [dbo].[Holiday] where DAY(DateHoliday) = @day
                                                            and MONTH(DateHoliday) = @month
                                                            and YEAR(DateHoliday) = @year";

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
