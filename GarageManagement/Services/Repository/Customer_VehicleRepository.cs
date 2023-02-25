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
    public class Customer_VehicleRepository: ICustomer_VehicleRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public Customer_VehicleRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllCustomer_Vehicle(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Customer_Vehicle> Customer_Vehicles = await connection.QueryAsync<Customer_Vehicle>(QueryCustomer_Vehicle.queryGetAllCustomer_Vehicle);
            int coutRecord = Customer_Vehicles.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Customer_Vehicles = Customer_Vehicles.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<Customer_VehicleDto> Customer_VehicleDtos = Customer_Vehicles.Adapt<IEnumerable<Customer_VehicleDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = Customer_VehicleDtos.ToArray(),
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
        public async Task<TemplateApi> GetCustomer_VehicleById(Guid IdCustomer_Vehicle)
        {
            using var con = _DbContext.CreateConnection();
            Customer_Vehicle Customer_Vehicle = await con.QueryFirstOrDefaultAsync<Customer_Vehicle>(QueryCustomer_Vehicle.queryGetByIdCustomer_Vehicle, new { Id = IdCustomer_Vehicle });

            if (Customer_Vehicle != null)
            {
                Customer_VehicleDto Customer_VehicleDto = Customer_Vehicle.Adapt<Customer_VehicleDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = Customer_VehicleDto,
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
        public async Task<TemplateApi> DeleteCustomer_Vehicle(Guid IdCustomer_Vehicle, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer_Vehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table Customer_Vehicle
                Customer_Vehicle Customer_Vehicle = await connection.QueryFirstOrDefaultAsync<Customer_Vehicle>(QueryCustomer_Vehicle.queryGetByIdCustomer_Vehicle, new { Id = IdCustomer_Vehicle }, tran);

                if (Customer_Vehicle is null)
                {
                    //save to diary if false    
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(QueryCustomer_Vehicle.queryDeleteCustomer_Vehicle, new { Id = Customer_Vehicle.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Customer_Vehicle.Id, "Delete", "Customer_Vehicle", ""));

                //save to Customer_Vehicle_Deleted
                await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertCustomer_Vehicle_Deleted, Customer_Vehicle, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Customer_Vehicle.Id, "Create", "Customer_Vehicle_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteCustomer_VehicleByList(List<Guid> IdCustomer_Vehicle, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer_Vehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);

            try
            {
                for (int i = 0; i < IdCustomer_Vehicle.Count; i++)
                {
                    //query Customer_Vehicle by id
                    Customer_Vehicle Customer_VehicleId = await connection.QueryFirstOrDefaultAsync<Customer_Vehicle>(QueryCustomer_Vehicle.queryGetByIdCustomer_Vehicle, new { Id = IdCustomer_Vehicle[i] }, tran);

                    saveDiaryModel = new SaveDiaryModel()
                    {
                        IdUserCurrent = userByIDCurrentLogin.Id,
                        Fullname = userByIDCurrentLogin.Fullname ?? "",
                        IsSuccess = true,
                    };

                    if (Customer_VehicleId is null)
                    {
                        tran.Rollback();

                        response = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = "Đã có ID không tồn tại !"
                        };
                        return response;
                    }
                    await connection.ExecuteAsync(QueryCustomer_Vehicle.queryDeleteCustomer_Vehicle, new { Id = IdCustomer_Vehicle[i] }, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdCustomer_Vehicle[i], "Delete", "Customer_Vehicle", ""));

                    //save to Customer_Vehicle_Deleted
                    await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertCustomer_Vehicle_Deleted, Customer_VehicleId, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Customer_VehicleId.Id, "Create", "Customer_Vehicle_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertDiary, diaries[i], tran);
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
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> InsertCustomer_Vehicle(Customer_VehicleDto Customer_VehicleDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var Customer_Vehicle = Customer_VehicleDto.Adapt<Customer_Vehicle>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer_Vehicle.queryGetUserById, new { Id = Customer_VehicleDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = Customer_VehicleDto.Id,
                    Operation = "Create",
                    Table = "Customer_Vehicle",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertCustomer_Vehicle, Customer_Vehicle, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateCustomer_Vehicle(Customer_VehicleDto Customer_VehicleDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var Customer_Vehicle = Customer_VehicleDto.Adapt<Customer_Vehicle>();

                //get Customer_Vehicle by id
                Customer_Vehicle Customer_VehicleById = await connection.QueryFirstOrDefaultAsync<Customer_Vehicle>(QueryCustomer_Vehicle.queryGetByIdCustomer_Vehicle, new { Customer_VehicleDto.Id }, tran);

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer_Vehicle.queryGetUserById, new { Id = Customer_VehicleDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = Customer_VehicleDto.Id,
                    Operation = "Update",
                    Table = "Customer_Vehicle",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (Customer_VehicleById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(QueryCustomer_Vehicle.queryUpdateCustomer_Vehicle, Customer_Vehicle, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCustomer_Vehicle.queryInsertDiary, diary, tran);

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
        #endregion
    }
    internal static class QueryCustomer_Vehicle
    {
        public const string queryInsertCustomer_Vehicle = @"INSERT INTO [dbo].[Customer_Vehicle]
                               ([Id]
                               ,[IdCustomer]
                               ,[IdVehicle])
                         VALUES (@Id, @IdCustomer, @IdVehicle)";
        public const string queryUpdateCustomer_Vehicle = @"UPDATE [dbo].[Customer_Vehicle] SET IdCustomer = @IdCustomer,
                                        IdVehicle = @IdVehicle
                                        WHERE Id = @Id";
        public const string queryDeleteCustomer_Vehicle = "DELETE FROM [dbo].[Customer_Vehicle] WHERE Id = @Id";
        public const string queryGetByIdCustomer_Vehicle = "select * from [dbo].[Customer_Vehicle] where Id = @Id";
        public const string queryGetAllCustomer_Vehicle = "select *from [dbo].[Customer_Vehicle]";
        public const string queryHideCustomer_Vehicle = "UPDATE [dbo].[Customer_Vehicle] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCustomer_Vehicle_Deleted = @"INSERT INTO [dbo].[Customer_Vehicle_Deleted]
                               ([Id]
                               ,[IdCustomer]
                               ,[IdVehicle])
                         VALUES (@Id, @IdCustomer, @IdVehicle)";
        public const string queryGetAllIdCustomer_Vehicle = "select Id from [dbo].[Customer_Vehicle]";

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
