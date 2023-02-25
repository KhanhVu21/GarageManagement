using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using Mapster;
using Dapper;
using GarageManagement.Services.Dtos;

namespace GarageManagement.Services.Repository
{
    public class CustomerGroupRepository: ICustomerGroupRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CustomerGroupRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCustomerGroupAndCustomer(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            var CustomerGroups = await connection.QueryAsync<CustomerGroup>(QueryCustomerGroup.queryGetAllCustomerGroupAvailable);
            int coutRecord = CustomerGroups.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CustomerGroups = CustomerGroups.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            var customerGroupDtos = CustomerGroups.Adapt<IEnumerable<CustomerGroupDto>>().ToList();

            for(int i = 0;i< customerGroupDtos.Count;i++)
            {
                var customers = await connection.QueryAsync<Customer>(QueryCustomerGroup.queryGetFullCustomerByIdCustomerGroup, new { IdGroup = customerGroupDtos[i].Id});
                customerGroupDtos[i].customers = customers.ToList();
            }
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = customerGroupDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCustomerGroupAvailable(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CustomerGroup> CustomerGroups = await connection.QueryAsync<CustomerGroup>(QueryCustomerGroup.queryGetAllCustomerGroupAvailable);
            int coutRecord = CustomerGroups.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CustomerGroups = CustomerGroups.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CustomerGroupDto> CustomerGroupDtos = CustomerGroups.Adapt<IEnumerable<CustomerGroupDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CustomerGroupDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCustomerGroup(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<CustomerGroup> CustomerGroups = await connection.QueryAsync<CustomerGroup>(QueryCustomerGroup.queryGetAllCustomerGroup);
            int coutRecord = CustomerGroups.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                CustomerGroups = CustomerGroups.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CustomerGroupDto> CustomerGroupDtos = CustomerGroups.Adapt<IEnumerable<CustomerGroupDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CustomerGroupDtos.ToArray(),
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
        public async Task<TemplateApi> GetCustomerGroupById(Guid IdCustomerGroup)
        {
            using var con = _DbContext.CreateConnection();
            CustomerGroup CustomerGroup = await con.QueryFirstOrDefaultAsync<CustomerGroup>(QueryCustomerGroup.queryGetByIdCustomerGroup, new { Id = IdCustomerGroup });

            if (CustomerGroup != null)
            {
                CustomerGroupDto CustomerGroupDto = CustomerGroup.Adapt<CustomerGroupDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = CustomerGroupDto,
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
        public async Task<TemplateApi> DeleteCustomerGroup(Guid IdCustomerGroup, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomerGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table CustomerGroup
                CustomerGroup CustomerGroup = await connection.QueryFirstOrDefaultAsync<CustomerGroup>(QueryCustomerGroup.queryGetByIdCustomerGroup, new { Id = IdCustomerGroup }, tran);

                //query Customer by idGroup
                Guid customerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCustomerGroup.queryGetCustomerByIdCustomerGroup, new { IdGroup = IdCustomerGroup }, tran);

                if (CustomerGroup is null || customerId != Guid.Empty)
                {
                    tran.Rollback();

                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = CustomerGroup is null ? "Đã có ID không tồn tại !" : "Đã có khách hàng trong nhóm này !"
                    };
                    return responseFalse;
                }

                await connection.ExecuteAsync(QueryCustomerGroup.queryDeleteCustomerGroup, new { Id = CustomerGroup.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CustomerGroup.Id, "Delete", "CustomerGroup", ""));


                //save to CustomerGroup_Deleted
                await connection.ExecuteAsync(QueryCustomerGroup.queryInsertCustomerGroup_Deleted, CustomerGroup, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CustomerGroup.Id, "Create", "CustomerGroup_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCustomerGroup.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCustomerGroupByList(List<Guid> IdCustomerGroup, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomerGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCustomerGroup.Count; i++)
                {
                    //query CustomerGroup by id
                    CustomerGroup CustomerGroupId = await connection.QueryFirstOrDefaultAsync<CustomerGroup>(QueryCustomerGroup.queryGetByIdCustomerGroup, new { Id = IdCustomerGroup[i] }, tran);

                    //query Customer by idGroup
                    Guid customerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCustomerGroup.queryGetCustomerByIdCustomerGroup, new { IdGroup = IdCustomerGroup[i] }, tran);

                    if (CustomerGroupId is null || customerId != Guid.Empty)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CustomerGroupId is null ? "Đã có ID không tồn tại !" : "Đã có khách hàng trong nhóm này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCustomerGroup.queryDeleteCustomerGroup, new { Id = IdCustomerGroup[i] }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCustomerGroup[i], "Delete", "CustomerGroup", ""));

                    //save to CustomerGroup_Deleted
                    await connection.ExecuteAsync(QueryCustomerGroup.queryInsertCustomerGroup_Deleted, CustomerGroupId, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, CustomerGroupId.Id, "Create", "CustomerGroup_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCustomerGroup.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> InsertCustomerGroup(CustomerGroupDto CustomerGroupDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CustomerGroup = CustomerGroupDto.Adapt<CustomerGroup>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomerGroup.queryGetUserById, new { Id = CustomerGroupDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CustomerGroupDto.Id,
                    Operation = "Create",
                    Table = "CustomerGroup",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCustomerGroup.queryInsertCustomerGroup, CustomerGroup, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCustomerGroup.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCustomerGroup(CustomerGroupDto CustomerGroupDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var CustomerGroup = CustomerGroupDto.Adapt<CustomerGroup>();

                //get CustomerGroup by id
                CustomerGroup CustomerGroupById = await connection.QueryFirstOrDefaultAsync<CustomerGroup>(QueryCustomerGroup.queryGetByIdCustomerGroup, new { CustomerGroupDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomerGroup.queryGetUserById, new { Id = CustomerGroupDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CustomerGroupDto.Id,
                    Operation = "Update",
                    Table = "CustomerGroup",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CustomerGroupById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCustomerGroup.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCustomerGroup.queryUpdateCustomerGroup, CustomerGroup, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCustomerGroup.queryInsertDiary, diary, tran);

                tran.Commit();

                response = new TemplateApi()
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
        public async Task<TemplateApi> HideCustomerGroupByList(List<Guid> IdCustomerGroup, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin nhóm khách hàng" : "Kích hoạt thông tin nhóm khách hàng";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomerGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdCustomerGroup.Count; i++)
                {
                    //query CustomerGroup by id
                    CustomerGroup CustomerGroup = await connection.QueryFirstOrDefaultAsync<CustomerGroup>(QueryCustomerGroup.queryGetByIdCustomerGroup, new { Id = IdCustomerGroup[i] }, tran);

                    //query Customer by idGroup
                    Guid customerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCustomerGroup.queryGetCustomerByIdCustomerGroup, new { IdGroup = IdCustomerGroup[i] }, tran);

                    if (CustomerGroup is null || customerId != Guid.Empty && IsHide)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CustomerGroup is null ? "Đã có ID không tồn tại !" : "Đã có khách hàng trong nhóm này !"
                        };
                        return responseFalse;
                    }

                    await connection.ExecuteAsync(QueryCustomerGroup.queryHideCustomerGroup, new { Id = IdCustomerGroup[i], IsHide = IsHide }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCustomerGroup[i], "Update", "CustomerGroup", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCustomerGroup.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideCustomerGroup(Guid IdCustomerGroup, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin nhóm khách hàng" : "Hiển thị thông tin nhóm khách hàng";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomerGroup.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            //query CustomerGroup by id
            CustomerGroup CustomerGroup = await connection.QueryFirstOrDefaultAsync<CustomerGroup>(QueryCustomerGroup.queryGetByIdCustomerGroup, new { Id = IdCustomerGroup }, tran);

            //query Customer by idGroup
            Guid customerId = await connection.QueryFirstOrDefaultAsync<Guid>(QueryCustomerGroup.queryGetCustomerByIdCustomerGroup, new { IdGroup = IdCustomerGroup }, tran);

            if (CustomerGroup is null || customerId != Guid.Empty && IsHide)
            {
                tran.Rollback();

                var responseFalse = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = CustomerGroup is null ? "Đã có ID không tồn tại !" : "Đã có khách hàng trong nhóm này !"
                };
                return responseFalse;
            }

            await connection.ExecuteAsync(QueryCustomerGroup.queryHideCustomerGroup, new { Id = IdCustomerGroup, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryCustomerGroup.queryInsertDiary, diary, tran);

            tran.Commit();

            var response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Ẩn thành công !"
            };
            return response;
        }
        #endregion
    }
    internal static class QueryCustomerGroup
    {
        public const string queryInsertCustomerGroup = @"INSERT INTO [dbo].[CustomerGroup]
                               ([Id]
                               ,[GroupName]
                               ,[GroupCode]
                               ,[Status]
                               ,[CreateDate]
                               ,[IsHide])
                         VALUES (@Id, @GroupName, @GroupCode, @Status,
                                @CreateDate, @IsHide)";
        public const string queryUpdateCustomerGroup = @"UPDATE [dbo].[CustomerGroup] SET GroupName = @GroupName
                                        WHERE Id = @Id";
        public const string queryDeleteCustomerGroup = "DELETE FROM [dbo].[CustomerGroup] WHERE Id = @Id";
        public const string queryGetByIdCustomerGroup = "select * from [dbo].[CustomerGroup] where Id = @Id";
        public const string queryGetAllCustomerGroup = "select *from [dbo].[CustomerGroup] order by CreateDate desc";
        public const string queryGetAllCustomerGroupAvailable = "select *from [dbo].[CustomerGroup] order by CreateDate desc";
        public const string queryHideCustomerGroup = "UPDATE [dbo].[CustomerGroup] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCustomerGroup_Deleted = @"INSERT INTO [dbo].[CustomerGroup_Deleted]
                               ([Id]
                               ,[GroupName]
                               ,[GroupCode]
                               ,[Status]
                               ,[CreateDate]
                               ,[IsHide])
                         VALUES (@Id, @GroupName, @GroupCode, @Status,
                                @CreateDate, @IsHide)";
        public const string queryGetAllIdCustomerGroup = "select Id from [dbo].[CustomerGroup]";
        public const string queryGetCustomerByIdCustomerGroup = "select Id from [dbo].[Customer] where IdGroup = @IdGroup";
        public const string queryGetFullCustomerByIdCustomerGroup = "select * from [dbo].[Customer] where IdGroup = @IdGroup";

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
