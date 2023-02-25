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
    public class RepairScheduleRepository : IRepairScheduleRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public RepairScheduleRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllRepairScheduleByIdCustomer(string cusCode, int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            var customer = await connection.QueryFirstOrDefaultAsync<Customer>(QueryRepairSchedule.queryGetCustomerByCode, new { Code = cusCode });
            if (customer == null)
            {
                return new TemplateApi()
                {
                    Message = "Mã khách hàng không tồn tại !",
                    Success = false,
                    Fail = true
                };
            }

            IEnumerable<RepairSchedule> RepairSchedules = await connection.QueryAsync<RepairSchedule>(QueryRepairSchedule.queryGetAllRepairScheduleByIdCustomer, new { IdCustomer = customer.Id });
            int coutRecord = RepairSchedules.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                RepairSchedules = RepairSchedules.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<RepairScheduleDto> RepairScheduleDtos = RepairSchedules.Adapt<IEnumerable<RepairScheduleDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            return new TemplateApi()
            {
                ListPayload = RepairScheduleDtos.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        public async Task<TemplateApi> GetAllRepairSchedule(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<RepairScheduleDto> repairScheduleDtos = await connection.QueryAsync<RepairScheduleDto>(QueryRepairSchedule.queryGetAllRepairSchedule);
            int coutRecord = repairScheduleDtos.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                repairScheduleDtos = repairScheduleDtos.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            return new TemplateApi()
            {
                ListPayload = repairScheduleDtos.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        public async Task<TemplateApi> GetRepairScheduleById(Guid IdRepairSchedule)
        {
            using var con = _DbContext.CreateConnection();
            var RepairSchedule = await con.QueryFirstOrDefaultAsync<RepairSchedule>(QueryRepairSchedule.queryGetRepairScheduleById, new { Id = IdRepairSchedule });

            if (RepairSchedule == null)
            {
                return new TemplateApi()
                {
                    Message = "Không tìm thấy kết quả",
                    Success = false,
                    Fail = true,
                };
            }

            var RepairScheduleDto = RepairSchedule.Adapt<RepairScheduleDto>();

            return new TemplateApi()
            {
                Payload = RepairScheduleDto,
                Message = "Lấy thông tin thành công",
                Success = true,
                Fail = false,
                TotalElement = 1,
            };
        }
        public async Task<TemplateApi> InsertRepairSchedule(RepairScheduleDto RepairScheduleDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var RepairSchedule = RepairScheduleDto.Adapt<RepairSchedule>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRepairSchedule.queryGetUserById, new { Id = RepairScheduleDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryRepairSchedule.queryInsertRepairSchedule, RepairSchedule, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RepairSchedule.Id, "Create", "RepairSchedule", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRepairSchedule.queryInsertDiary, diaries[i], tran);
                }
                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Thêm mới thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateRepairSchedule(RepairScheduleDto RepairScheduleDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var RepairSchedule = RepairScheduleDto.Adapt<RepairSchedule>();

            try
            {
                //get RepairSchedule by id
                RepairSchedule RepairScheduleById = await connection.QueryFirstOrDefaultAsync<RepairSchedule>(QueryRepairSchedule.queryGetRepairScheduleById, new { RepairScheduleDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRepairSchedule.queryGetUserById, new { Id = RepairScheduleDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (RepairScheduleById == null || RepairScheduleById.IsWaiting == false)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = RepairScheduleById == null ? "Không tìm thấy kết quả !" : "Lịch hẹn này đã hoàn thành hoặc bị hủy không thể cập nhật !"
                    };
                }

                await connection.ExecuteAsync(QueryRepairSchedule.queryUpdateRepairScheduleAD, RepairSchedule, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RepairSchedule.Id, "Update", "RepairSchedule", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRepairSchedule.queryInsertDiary, diaries[i], tran);
                }
                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Cập nhật thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> AcceptRepairScheduleByList(List<Guid> IdRepairSchedule, Guid IdUserCurrent, bool IsAccepted)
        {
            var content = IsAccepted ? "Chấp nhận lịch đăng kí" : "Từ chối lịch đăng kí";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRepairSchedule.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRepairSchedule.Count; i++)
                {
                    //query RepairSchedule by id
                    var RepairSchedule = await connection.QueryFirstOrDefaultAsync<RepairSchedule>(QueryRepairSchedule.queryGetRepairScheduleById, new { Id = IdRepairSchedule[i] }, tran);

                    if (RepairSchedule is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = RepairSchedule is null ? "Đã có ID không tồn tại !" : ""
                        };
                    }

                    await connection.ExecuteAsync(QueryRepairSchedule.queryAdjustAcceptRepairSchedule, new { Id = IdRepairSchedule[i], IsAccepted }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRepairSchedule[i], "Update", "RepairSchedule", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRepairSchedule.queryInsertDiary, diaries[i], tran);
                }

                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = $"{content} thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> CancelRepairScheduleByList(List<Guid> IdRepairSchedule, Guid IdUserCurrent, bool IsCancel, string reason)
        {
            var content = IsCancel ? "Hủy lịch đăng kí" : "Tiếp tục lịch đăng kí";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRepairSchedule.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRepairSchedule.Count; i++)
                {
                    //query RepairSchedule by id
                    RepairSchedule RepairSchedule = await connection.QueryFirstOrDefaultAsync<RepairSchedule>(QueryRepairSchedule.queryGetRepairScheduleById, new { Id = IdRepairSchedule[i] }, tran);

                    if (RepairSchedule is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = RepairSchedule is null ? "Đã có ID không tồn tại !" : ""
                        };
                    }

                    await connection.ExecuteAsync(QueryRepairSchedule.queryAdjustCancelRepairSchedule, new { Id = IdRepairSchedule[i], IsCancel, NoteGarage = reason }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRepairSchedule[i], "Update", "RepairSchedule", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRepairSchedule.queryInsertDiary, diaries[i], tran);
                }

                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = $"{content} thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> WaitingRepairScheduleByList(List<Guid> IdRepairSchedule, Guid IdUserCurrent, bool IsWaiting)
        {
            var content = IsWaiting ? "Tạm hoãn lịch đăng kí" : "Tiếp tục lịch đăng kí";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRepairSchedule.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRepairSchedule.Count; i++)
                {
                    //query RepairSchedule by id
                    RepairSchedule RepairSchedule = await connection.QueryFirstOrDefaultAsync<RepairSchedule>(QueryRepairSchedule.queryGetRepairScheduleById, new { Id = IdRepairSchedule[i] }, tran);

                    if (RepairSchedule is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = RepairSchedule is null ? "Đã có ID không tồn tại !" : ""
                        };
                    }

                    await connection.ExecuteAsync(QueryRepairSchedule.queryAdjustWaitingRepairSchedule, new { Id = IdRepairSchedule[i], IsWaiting }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRepairSchedule[i], "Update", "RepairSchedule", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRepairSchedule.queryInsertDiary, diaries[i], tran);
                }

                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = $"{content} thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> InsertRepairScheduleClient(RepairScheduleDto RepairScheduleDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var RepairSchedule = RepairScheduleDto.Adapt<RepairSchedule>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                var customer = await connection.QueryFirstOrDefaultAsync<Customer>(QueryRepairSchedule.queryGetCustomerById, new { Id = RepairScheduleDto.IdCustomer }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = customer.Id,
                    Fullname = customer.Name ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryRepairSchedule.queryInsertRepairSchedule, RepairSchedule, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RepairSchedule.Id, "Create", "RepairSchedule", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRepairSchedule.queryInsertDiary, diaries[i], tran);
                }
                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Thêm mới thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateRepairScheduleClient(RepairScheduleDto RepairScheduleDto, string cusCode)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var RepairSchedule = RepairScheduleDto.Adapt<RepairSchedule>();

            try
            {

                //get RepairSchedule by id
                var RepairScheduleById = await connection.QueryFirstOrDefaultAsync<RepairSchedule>(QueryRepairSchedule.queryGetRepairScheduleById, new { RepairScheduleDto.Id }, tran);
                var customerById = await connection.QueryFirstOrDefaultAsync<Customer>(QueryRepairSchedule.queryGetCustomerByCode, new { Code = cusCode }, tran);
               
                if(RepairScheduleById.IsWaiting == false)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Lịch hẹn này đã hoàn thành hoặc bị hủy không thể cập nhật !"
                    };
                }
                if (RepairScheduleById == null || customerById.Id != RepairScheduleById.IdCustomer)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                }

                var customer = await connection.QueryFirstOrDefaultAsync<Customer>(QueryRepairSchedule.queryGetCustomerById, new { Id = customerById.Id }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = customer.Id,
                    Fullname = customer.Name ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryRepairSchedule.queryUpdateRepairSchedule, RepairSchedule, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RepairSchedule.Id, "Update", "RepairSchedule", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRepairSchedule.queryInsertDiary, diaries[i], tran);
                }
                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Cập nhật thành công !"
                };
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<Guid> GetIdCustomerByCode(string Code)
        {
            using var con = _DbContext.CreateConnection();
            var customer = await con.QueryFirstOrDefaultAsync<RepairSchedule>(QueryRepairSchedule.queryGetCustomerByCode, new { Code });
            return customer.Id;
        }
        #endregion
    }
    internal static class QueryRepairSchedule
    {
        public const string queryInsertRepairSchedule = @"INSERT INTO [dbo].[RepairSchedule]
                                                                       ([Id]
                                                                       ,[DaySchedule]
                                                                       ,[IsAccepted]
                                                                       ,[IsCancel]
                                                                       ,[IsWaiting]
                                                                       ,[Status]
                                                                       ,[CreatedDate]
                                                                       ,[IdCustomer]
                                                                       ,[Note]
                                                                       ,[NoteGarage])
                                                                 VALUES (@Id, @DaySchedule, @IsAccepted, @IsCancel,
                                                                 @IsWaiting, @Status, @CreatedDate, @IdCustomer, @Note, @NoteGarage)";
        public const string queryUpdateRepairSchedule = "update RepairSchedule set DaySchedule = @DaySchedule, Note = @Note where Id = @Id";
        public const string queryUpdateRepairScheduleAD = "update RepairSchedule set DaySchedule = @DaySchedule, Note = @Note, IdCustomer = @IdCustomer where Id = @Id";

        public const string queryGetAllRepairSchedule = "select re.*, c.Name, c.Phone from RepairSchedule re left join Customer c on re.IdCustomer = c.Id order by re.CreatedDate desc";
        public const string queryGetAllRepairScheduleByIdCustomer = "select *from [dbo].[RepairSchedule] where IdCustomer = @IdCustomer order by CreatedDate desc";
        public const string queryGetRepairScheduleById = "select * from RepairSchedule where Id = @Id";
        public const string queryGetCustomerByCode = "select * from Customer where Code = @Code";
        public const string queryGetCustomerById = "select * from Customer where Id = @Id";
        public const string queryAdjustAcceptRepairSchedule = "update RepairSchedule set IsAccepted = @IsAccepted, IsCancel = 0, IsWaiting = 0 where Id = @Id";
        public const string queryAdjustCancelRepairSchedule = "update RepairSchedule set IsAccepted = 0, IsCancel = @IsCancel, IsWaiting = 0, NoteGarage = @NoteGarage where Id = @Id";
        public const string queryAdjustWaitingRepairSchedule = "update RepairSchedule set IsAccepted = 0, IsCancel = 0, IsWaiting = @IsWaiting where Id = @Id";

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
