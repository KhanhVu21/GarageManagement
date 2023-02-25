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
    public class RequestListRepository: IRequestListRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public RequestListRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> DeleteRequestList(Guid IdRequestList, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);

                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query RequestList by id
                RequestList RequestList = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList }, tran);

                if (RequestList is null)
                {
                    tran.Rollback();

                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = RequestList is null ? "Đã có ID không tồn tại !" : ""
                    };
                }

                //RequestList
                await connection.ExecuteAsync(QueryRequestList.queryDeleteRequestList, new { Id = IdRequestList }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRequestList, "Delete", "RequestList", ""));

                //save to RequestList_Deleted
                await connection.ExecuteAsync(QueryRequestList.queryInsertRequestList_Deleted, RequestList, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRequestList, "Create", "RequestList_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diaries[i], tran);
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
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> DeleteRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRequestList.Count; i++)
                {
                    //query RequestList by id
                    RequestList RequestListId = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList[i] }, tran);

                    if (RequestListId is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = RequestListId is null ? "Đã có ID không tồn tại !" : ""
                        };
                    }

                    await connection.ExecuteAsync(QueryRequestList.queryDeleteRequestList, new { Id = IdRequestList[i] }, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRequestList[i], "Delete", "RequestList", ""));

                    //save to RequestList_Deleted
                    await connection.ExecuteAsync(QueryRequestList.queryInsertRequestList_Deleted, RequestListId, tran);
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RequestListId.Id, "Create", "RequestList_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> GetAllRequestList(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<RequestList> RequestLists = await connection.QueryAsync<RequestList>(QueryRequestList.queryGetAllRequestList);
            int coutRecord = RequestLists.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                RequestLists = RequestLists.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<RequestListDto> RequestListDtos = RequestLists.Adapt<IEnumerable<RequestListDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            return new TemplateApi()
            {
                ListPayload = RequestListDtos.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        public async Task<TemplateApi> GetAllRequestListById_RO_RepairOrders(int pageNumber, int pageSize, Guid Id_RO_RepairOrders)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<RequestList> RequestLists = await connection.QueryAsync<RequestList>(QueryRequestList.queryGetAllRequestListById_RO_RepairOrders, new { Id_RO_RepairOrders = Id_RO_RepairOrders });
            int coutRecord = RequestLists.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                RequestLists = RequestLists.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<RequestListDto> RequestListDtos = RequestLists.Adapt<IEnumerable<RequestListDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            return new TemplateApi()
            {
                ListPayload = RequestListDtos.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        public async Task<TemplateApi> GetRequestListById(Guid IdRequestList)
        {
            using var con = _DbContext.CreateConnection();
            var RequestList = await con.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList });

            if(RequestList == null)
            {
                return new TemplateApi()
                {
                    Message = "Không tìm thấy kết quả",
                    Success = false,
                    Fail = true,
                };
            }

            var RequestListDto = RequestList.Adapt<RequestListDto>();

            return new TemplateApi()
            {
                Payload = RequestListDto,
                Message = "Lấy thông tin thành công",
                Success = true,
                Fail = false,
                TotalElement = 1,
            };
        }
        public async Task<TemplateApi> InsertRequestList(RequestListDto RequestListDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var RequestList = RequestListDto.Adapt<RequestList>();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = RequestListDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryRequestList.queryInsertRequestList, RequestList, tran);
                // save to table diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RequestList.Id, "Create", "RequestList", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateRequestList(RequestListDto RequestListDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            var RequestList = RequestListDto.Adapt<RequestList>();

            try
            {
                //get RequestList by id
                RequestList RequestListById = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { RequestListDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = RequestListDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (RequestListById == null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                }

                await connection.ExecuteAsync(QueryRequestList.queryUpdateRequestList, RequestList, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RequestList.Id, "Update", "RequestList", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> ProcessRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent, bool IsProcessing)
        {
            var content = IsProcessing ? "Yêu cầu đang được thực hiện" : "Yêu cầu không được thực hiện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRequestList.Count; i++)
                {
                    //query RequestList by id
                    RequestList RequestList = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList[i] }, tran);

                    if (RequestList is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = RequestList is null ? "Đã có ID không tồn tại !" : ""
                        };
                    }

                    await connection.ExecuteAsync(QueryRequestList.queryUpdateStatusRequestList, new { Id = IdRequestList[i], IsProcessing = IsProcessing, IsCompleted = false, IsCanceled = false }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRequestList[i], "Update", "RequestList", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> ProcessRequestList(Guid IdRequestList, Guid IdUserCurrent, bool IsProcessing)
        {
            var content = IsProcessing ? "Yêu cầu đang được thực hiện" : "Yêu cầu không được thực hiện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query RequestList by id
            RequestList RequestList = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList }, tran);

            if (RequestList is null)
            {
                tran.Rollback();

                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = RequestList is null ? "Đã có ID không tồn tại !" : ""
                };
            }

            await connection.ExecuteAsync(QueryRequestList.queryUpdateStatusRequestList, new { Id = IdRequestList, IsProcessing = IsProcessing, IsCompleted = false, IsCanceled = false }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diary, tran);

            tran.Commit();

            return new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = $"{content} thành công !"
            };
        }
        public async Task<TemplateApi> CompleteRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent, bool IsCompleted)
        {
            var content = IsCompleted ? "Yêu cầu đã được hoàn thành" : "Yêu cầu chưa được hoàn thành";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRequestList.Count; i++)
                {
                    //query RequestList by id
                    RequestList RequestList = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList[i] }, tran);

                    if (RequestList is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = RequestList is null ? "Đã có ID không tồn tại !" : ""
                        };
                    }

                    await connection.ExecuteAsync(QueryRequestList.queryUpdateStatusRequestList, new { Id = IdRequestList[i], IsProcessing = false, IsCompleted = IsCompleted, IsCanceled = false }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRequestList[i], "Update", "RequestList", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> CompleteRequestList(Guid IdRequestList, Guid IdUserCurrent, bool IsCompleted)
        {
            var content = IsCompleted ? "Yêu cầu đã được hoàn thành" : "Yêu cầu chưa được hoàn thành";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query RequestList by id
            RequestList RequestList = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList }, tran);

            if (RequestList is null)
            {
                tran.Rollback();

                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = RequestList is null ? "Đã có ID không tồn tại !" : ""
                };
            }

            await connection.ExecuteAsync(QueryRequestList.queryUpdateStatusRequestList, new { Id = IdRequestList, IsProcessing = false, IsCompleted = IsCompleted, IsCanceled = false }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diary, tran);

            tran.Commit();

            return new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = $"{content} thành công !"
            };
        }
        public async Task<TemplateApi> CancelRequestListByList(List<Guid> IdRequestList, Guid IdUserCurrent, bool IsCanceled)
        {
            var content = IsCanceled ? "Yêu cầu đã bị dừng" : "Yêu cầu tiếp tục thực hiện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRequestList.Count; i++)
                {
                    //query RequestList by id
                    RequestList RequestList = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList[i] }, tran);

                    if (RequestList is null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = RequestList is null ? "Đã có ID không tồn tại !" : ""
                        };
                    }

                    await connection.ExecuteAsync(QueryRequestList.queryUpdateStatusRequestList, new { Id = IdRequestList[i], IsProcessing = false, IsCompleted = false, IsCanceled = IsCanceled }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRequestList[i], "Update", "RequestList", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> CancelRequestList(Guid IdRequestList, Guid IdUserCurrent, bool IsCanceled)
        {
            var content = IsCanceled ? "Yêu cầu đã bị dừng" : "Yêu cầu tiếp tục thực hiện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRequestList.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query RequestList by id
            RequestList RequestList = await connection.QueryFirstOrDefaultAsync<RequestList>(QueryRequestList.queryGetByIdRequestList, new { Id = IdRequestList }, tran);

            if (RequestList is null)
            {
                tran.Rollback();

                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = RequestList is null ? "Đã có ID không tồn tại !" : ""
                };
            }

            await connection.ExecuteAsync(QueryRequestList.queryUpdateStatusRequestList, new { Id = IdRequestList, IsProcessing = false, IsCompleted = false, IsCanceled = IsCanceled }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryRequestList.queryInsertDiary, diary, tran);

            tran.Commit();

            return new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = $"{content} thành công !"
            };
        }
        #endregion
    }
    internal static class QueryRequestList
    {
        public const string queryInsertRequestList = @"INSERT INTO [dbo].[RequestList]
                                                                       ([Id]
                                                                       ,[Id_RO_RepairOrders]
                                                                       ,[RequestContent]
                                                                       ,[IsProcessing]
                                                                       ,[IsCompleted]
                                                                       ,[IsCanceled]
                                                                       ,[Note]
                                                                       ,[Status]
                                                                       ,[CreatedDate])
                                                                 VALUES (@Id, @Id_RO_RepairOrders, @RequestContent, @IsProcessing, @IsCompleted, @IsCanceled,
                                                                        @Note, @Status, @CreatedDate)";
        public const string queryUpdateRequestList = @"UPDATE [dbo].[RequestList] SET RequestContent = @RequestContent,
                                                                        IsProcessing = @IsProcessing,
                                                                        IsCompleted = @IsCompleted,
                                                                        IsCanceled = @IsCanceled,
                                                                        Note = @Note
                                                                        WHERE Id = @Id";
        public const string queryDeleteRequestList = "DELETE FROM [dbo].[RequestList] WHERE Id = @Id";
        public const string queryGetByIdRequestList = "select * from [dbo].[RequestList] where Id = @Id";
        public const string queryGetAllRequestList = "select *from [dbo].[RequestList] order by CreatedDate desc";
        public const string queryGetAllRequestListById_RO_RepairOrders = "select *from [dbo].[RequestList] where Id_RO_RepairOrders = Id_RO_RepairOrders order by CreatedDate desc";
        public const string queryInsertRequestList_Deleted = @"INSERT INTO [dbo].[Delete_RequestList]
                                                                       ([Id]
                                                                       ,[Id_RO_RepairOrders]
                                                                       ,[RequestContent]
                                                                       ,[IsProcessing]
                                                                       ,[IsCompleted]
                                                                       ,[IsCanceled]
                                                                       ,[Note]
                                                                       ,[Status]
                                                                       ,[CreatedDate])
                                                                 VALUES (@Id, @Id_RO_RepairOrders, @RequestContent, @IsProcessing, @IsCompleted, @IsCanceled,
                                                                        @Note, @Status, @CreatedDate)";
        public const string queryGetAllIdRequestList = "select Id from [dbo].[RequestList]";
        public const string queryUpdateStatusRequestList = "UPDATE [dbo].[RequestList] SET IsProcessing = @IsProcessing, IsCompleted = @IsCompleted, IsCanceled = @IsCanceled WHERE Id = @Id";

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
