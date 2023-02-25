using GarageManagement.Data.Context;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using Dapper;
using Mapster;
using GarageManagement.Services.Common.Function;
using GarageManagement.Data.Entity;
using GarageManagement.Services.IRepository;

namespace GarageManagement.Services.Repository
{
    public class HolidayRepository: IHolidayRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public HolidayRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteHolidayByList(List<Guid> IdHoliday, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryHoliday.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdHoliday.Count; i++)
                {
                    //query Holiday by id
                    var HolidayId = await connection.QueryFirstOrDefaultAsync<Holiday>(QueryHoliday.queryGetByIdHoliday, new { Id = IdHoliday[i] }, tran);

                    if (HolidayId is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = "Đã có ID không tồn tại !"
                        };
                    }

                    await connection.ExecuteAsync(QueryHoliday.queryDeleteHoliday, new { Id = IdHoliday[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdHoliday[i], "Delete", "Holiday", ""));

                    //save to Holiday_Deleted
                    await connection.ExecuteAsync(QueryHoliday.queryInsertHoliday_Deleted, HolidayId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, HolidayId.Id, "Create", "Holiday_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryHoliday.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllHoliday(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Holiday> Holidays = await connection.QueryAsync<Holiday>(QueryHoliday.queryGetAllHoliday);
            int coutRecord = Holidays.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Holidays = Holidays.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<HolidayDto> HolidayDtos = Holidays.Adapt<IEnumerable<HolidayDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = HolidayDtos.ToArray(),
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
        public async Task<TemplateApi> GetHolidayById(Guid IdHoliday)
        {
            using var con = _DbContext.CreateConnection();
            Holiday Holiday = await con.QueryFirstOrDefaultAsync<Holiday>(QueryHoliday.queryGetByIdHoliday, new { Id = IdHoliday });

            if (Holiday != null)
            {
                HolidayDto HolidayDto = Holiday.Adapt<HolidayDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = HolidayDto,
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
        public async Task<TemplateApi> InsertHoliday(HolidayDto HolidayDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var Holiday = HolidayDto.Adapt<Holiday>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryHoliday.queryGetUserById, new { Id = HolidayDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryHoliday.queryInsertHoliday, Holiday, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Holiday.Id, "Create", "Holiday", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryHoliday.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateHoliday(HolidayDto HolidayDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var Holiday = HolidayDto.Adapt<Holiday>();

            //get Holiday by id
            Holiday HolidayById = await connection.QueryFirstOrDefaultAsync<Holiday>(QueryHoliday.queryGetByIdHoliday, new { HolidayDto.Id });

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryHoliday.queryGetUserById, new { Id = HolidayDto.IdUserCurrent });
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (HolidayById == null)
            {
                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryHoliday.queryUpdateHoliday, Holiday);
            //save diary if success
            diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Holiday.Id, "Update", "Holiday", ""));

            //save to table diary
            for (int i = 0; i < diaries.Count; i++)
            {
                await connection.ExecuteAsync(QueryHoliday.queryInsertDiary, diaries[i]);
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
    internal static class QueryHoliday
    {
        public const string queryInsertHoliday = @"INSERT INTO [dbo].[Holiday]
                                                       ([Id]
                                                       ,[NameHoliday]
                                                       ,[DateHoliday]
                                                       ,[CreatedDate]
                                                       ,[Status]
                                                       ,[PercentBonusForWork])
                                VALUES (@Id, @NameHoliday, @DateHoliday, @CreatedDate, @Status, @PercentBonusForWork)";
        public const string queryUpdateHoliday = @"UPDATE [dbo].[Holiday] SET NameHoliday = @NameHoliday,
                                                                        DateHoliday = @DateHoliday,
                                                                        PercentBonusForWork = @PercentBonusForWork
                                                                        WHERE Id = @Id";
        public const string queryDeleteHoliday = "DELETE FROM [dbo].[Holiday] WHERE Id = @Id";
        public const string queryGetByIdHoliday = "select * from [dbo].[Holiday] where Id = @Id";
        public const string queryGetAllHoliday = "select *from [dbo].[Holiday] order by CreatedDate desc";
        public const string queryInsertHoliday_Deleted = @"INSERT INTO [dbo].[Delete_Holiday]
                                                       ([Id]
                                                       ,[NameHoliday]
                                                       ,[DateHoliday]
                                                       ,[CreatedDate]
                                                       ,[Status]
                                                       ,[PercentBonusForWork])
                                VALUES (@Id, @NameHoliday, @DateHoliday, @CreatedDate, @Status, @PercentBonusForWork)";

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
