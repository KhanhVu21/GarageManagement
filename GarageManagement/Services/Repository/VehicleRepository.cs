using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.IRepository;
using Mapster;
using Dapper;
using GarageManagement.Services.Dtos;
using System.Data;

namespace GarageManagement.Services.Repository
{
    public class VehicleRepository: IVehicleRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public VehicleRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllVehicleByIdCustomer(int pageNumber, int pageSize, Guid IdCustomer)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Vehicle> vehicles = await connection.QueryAsync<Vehicle>(QueryVehicle.queryGetAllVehicleByIdCustomer, new { IdCustomer = IdCustomer });
            int coutRecord = vehicles.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                vehicles = vehicles.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<VehicleDto> vehicleDtos = vehicles.Adapt<IEnumerable<VehicleDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = vehicleDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCustomerByIdVehicle(int pageNumber, int pageSize, Guid IdVehicle)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Customer> customers = await connection.QueryAsync<Customer>(QueryVehicle.queryGetAllCustomerByIdVehicle, new { IdVehicle = IdVehicle });
            int coutRecord = customers.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                customers = customers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CustomerDto> customerDtos = customers.Adapt<IEnumerable<CustomerDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = customerDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllVehicle(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Vehicle> Vehicles = await connection.QueryAsync<Vehicle>(QueryVehicle.queryGetAllVehicle);
            int coutRecord = Vehicles.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Vehicles = Vehicles.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<VehicleDto> VehicleDtos = Vehicles.Adapt<IEnumerable<VehicleDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = VehicleDtos.ToArray(),
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
        public async Task<TemplateApi> GetVehicleById(Guid IdVehicle)
        {
            using var con = _DbContext.CreateConnection();
            Vehicle Vehicle = await con.QueryFirstOrDefaultAsync<Vehicle>(QueryVehicle.queryGetByIdVehicle, new { Id = IdVehicle });

            //get list customber by idvehicle
            IEnumerable<Customer> customers = await con.QueryAsync<Customer>(QueryVehicle.queryGetAllCustomerByIdVehicle, new { IdVehicle = IdVehicle });

            if (Vehicle != null)
            {
                VehicleDto VehicleDto = Vehicle.Adapt<VehicleDto>();
                VehicleDto.ListCustomers = customers.ToList();

                var templateApi = new TemplateApi()
                {
                    Payload = VehicleDto,
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
        public async Task<TemplateApi> DeleteVehicle(Guid IdVehicle, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            try
            {
                //query table Vehicle
                Vehicle Vehicle = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryVehicle.queryGetByIdVehicle, new { Id = IdVehicle }, tran);

                //query RO_RepairOders by IdVehicle
                var rO_RepairOdersByIdVehicle = await connection.QueryFirstOrDefaultAsync<RO_RepairOders>(QueryVehicle.queryRO_RepairOdersByIdVehicle, new { IdVehicle = IdVehicle }, tran);

                if (Vehicle is null || rO_RepairOdersByIdVehicle is not null)
                {
                    tran.Rollback();

                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = Vehicle is null ? "Đã có ID không tồn tại !" : "Đã có xe trong hóa đơn không thể xóa !"
                    };
                }

                var values = new { IdVehicle = IdVehicle, IdUser = IdUserCurrent };
                await connection.ExecuteAsync(QueryVehicle.queryStoreDeleteVehicleByList, values, tran, commandType: CommandType.StoredProcedure);

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
        public async Task<TemplateApi> DeleteVehicleByList(List<Guid> IdVehicle, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            try
            {
                for (int i = 0; i < IdVehicle.Count; i++)
                {
                    //query Vehicle by id
                    var VehicleId = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryVehicle.queryGetByIdVehicle, new { Id = IdVehicle[i] }, tran);

                    //query RO_RepairOders by IdVehicle
                    var rO_RepairOdersByIdVehicle = await connection.QueryFirstOrDefaultAsync<RO_RepairOders>(QueryVehicle.queryRO_RepairOdersByIdVehicle, new { IdVehicle = IdVehicle[i] }, tran);

                    if (VehicleId is null || rO_RepairOdersByIdVehicle is not null)
                    {
                        tran.Rollback();

                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = VehicleId is null ? "Đã có ID không tồn tại !" : "Đã có xe trong hóa đơn không thể xóa !"
                        };
                    }

                    var values = new { IdVehicle = IdVehicle[i], IdUser = IdUserCurrent };
                    await connection.ExecuteAsync(QueryVehicle.queryStoreDeleteVehicleByList, values, tran, commandType: CommandType.StoredProcedure);
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
        public async Task<TemplateApi> InsertVehicle(VehicleDto VehicleDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                /*var CategoryBrandVehicle = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = VehicleDto.IdBrandVehicleCategory }, tran);
                if (CategoryBrandVehicle.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Thương hiệu {CategoryBrandVehicle.Name} không có sẵn !"
                    };
                }

                var CategoryModel = await connection.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { Id = VehicleDto.IdModel }, tran);
                if (CategoryModel.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Mẫu {CategoryModel.ModelName} không có sẵn !"
                    };
                }

                var categoryType = await connection.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { Id = VehicleDto.IdType }, tran);
                if (categoryType.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Loại {categoryType.Name} không có sẵn !"
                    };
                }

                var categoryGearBox = await connection.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { Id = VehicleDto.IdGearBox }, tran);
                if (categoryGearBox.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Hộp số {categoryGearBox.Name} không có sẵn !"
                    };
                }*/

                var Vehicle = VehicleDto.Adapt<Vehicle>();

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryVehicle.queryGetUserById, new { Id = VehicleDto.IdUserCurrent }, tran);

                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryVehicle.queryInsertVehicle, Vehicle, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, VehicleDto.Id, "Create", "Vehicle", ""));

                //insert customer_vehicle
                for (int i = 0; i < VehicleDto?.ListIdCustomer?.Split(',').Length;i++)
                {
                    var customer_Vehicle = new Customer_Vehicle()
                    {
                        Id = Guid.NewGuid(),
                        IdCustomer = new Guid(VehicleDto.ListIdCustomer.Split(',')[i]),
                        IdVehicle = VehicleDto.Id
                    };

                    await connection.ExecuteAsync(QueryVehicle.queryInsertCustomer_Vehicle, customer_Vehicle, tran);

                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, customer_Vehicle.Id, "Create", "Customer_Vehicle", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryVehicle.queryInsertDiary, diaries[i], tran);
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
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateVehicle(VehicleDto VehicleDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
/*                var CategoryBrandVehicle = await connection.QueryFirstOrDefaultAsync<CategoryBrandVehicle>(QueryCategoryBrandVehicle.queryGetByIdCategoryBrandVehicle, new { Id = VehicleDto.IdBrandVehicleCategory }, tran);
                if (CategoryBrandVehicle.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Thương hiệu {CategoryBrandVehicle.Name} không có sẵn !"
                    };
                }

                var CategoryModel = await connection.QueryFirstOrDefaultAsync<CategoryModel>(QueryCategoryModel.queryGetByIdCategoryModel, new { Id = VehicleDto.IdModel }, tran);
                if (CategoryModel.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Mẫu {CategoryModel.ModelName} không có sẵn !"
                    };
                }

                var categoryType = await connection.QueryFirstOrDefaultAsync<CategoryType>(QueryCategoryType.queryGetByIdCategoryType, new { Id = VehicleDto.IdType }, tran);
                if (categoryType.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Loại {categoryType.Name} không có sẵn !"
                    };
                }

                var categoryGearBox = await connection.QueryFirstOrDefaultAsync<CategoryGearBox>(QueryCategoryGearBox.queryGetByIdCategoryGearBox, new { Id = VehicleDto.IdGearBox }, tran);
                if (categoryGearBox.IsHide == true)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = $"Hộp số {categoryGearBox.Name} không có sẵn !"
                    };
                }*/

                var Vehicle = VehicleDto.Adapt<Vehicle>();

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer.queryGetUserById, new { Id = VehicleDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //get Vehicle by id
                Vehicle VehicleById = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryVehicle.queryGetByIdVehicle, new { VehicleDto.Id }, tran);
                if (VehicleById == null)
                {
                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseFalse;
                }

                //check id file exits
                if (VehicleDto.idFile is not null)
                {
                    VehicleDto.Avatar = VehicleById.Avatar;
                }

                await connection.ExecuteAsync(QueryVehicle.queryUpdateVehicle, Vehicle, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Vehicle.Id, "Update", "Vehicle", ""));
      
                var customerVehicles = await connection.QueryAsync<Customer_Vehicle>(QueryVehicle.queryGetCustomer_VehicleByIdVehicle, new { IdVehicle = Vehicle .Id}, tran);
                if(customerVehicles.Count() != VehicleDto?.ListIdCustomer?.Split(',').Length)
                {
                    //delete data customer_vehicle
                    var customerVehicleIds = customerVehicles.Select(x => x.Id).ToList();
                    await connection.ExecuteAsync(QueryVehicle.queryDeleteCustomer_VehicleByIds, new {Id = customerVehicleIds }, tran);
                 
                    //insert customer_vehicle
                    for (int i = 0; i < VehicleDto?.ListIdCustomer?.Split(',').Length; i++)
                    {
                        var customer_Vehicle = new Customer_Vehicle()
                        {
                            Id = Guid.NewGuid(),
                            IdCustomer = new Guid(VehicleDto.ListIdCustomer.Split(',')[i]),
                            IdVehicle = VehicleDto.Id
                        };

                        await connection.ExecuteAsync(QueryVehicle.queryInsertCustomer_Vehicle, customer_Vehicle, tran);

                        //save diary if success
                        diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, customer_Vehicle.Id, "Create", "Customer_Vehicle", ""));
                    }
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCategoryBrandVehicle.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideVehicleByList(List<Guid> IdVehicle, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Khóa thông tin phương tiện" : "Kích hoạt thông tin phương tiện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryVehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);
            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };
            try
            {
                for (int i = 0; i < IdVehicle.Count; i++)
                {
                    //query Vehicle by id
                    Vehicle Vehicle = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryVehicle.queryGetByIdVehicle, new { Id = IdVehicle[i] }, tran);
                    
                    var customerVehicle = await connection.QueryFirstOrDefaultAsync<Customer_Vehicle>(QueryVehicle.queryGetCustomer_VehicleByIdVehicle, new { IdVehicle = IdVehicle[i] }, tran);

                    if (Vehicle is null || customerVehicle is not null)
                    {
                        tran.Rollback();

                        var responseFalse = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = Vehicle is null ? "Đã có ID không tồn tại !" : "Xe này đã có người sử dụng không thể khóa !"
                        };
                        return responseFalse;
                    }
                    await connection.ExecuteAsync(QueryVehicle.queryHideVehicle, new { Id = IdVehicle[i], IsHide = IsHide }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdVehicle[i], "Update", "Vehicle", content));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryVehicle.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> HideVehicle(Guid IdVehicle, Guid IdUserCurrent, bool IsHide)
        {
            var content = IsHide ? "Ẩn thông tin phương tiện" : "Hiển thị thông tin phương tiện";

            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            //query Vehicle by id
            Vehicle Vehicle = await connection.QueryFirstOrDefaultAsync<Vehicle>(QueryVehicle.queryGetByIdVehicle, new { Id = IdVehicle }, tran);

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryVehicle.queryGetUserById, new { Id = IdUserCurrent }, tran);

            saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                IdWith = IdVehicle,
                Operation = "Update",
                Table = "Vehicle",
                Content = content,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            if (Vehicle is null)
            {
                //save to diary if false
                saveDiaryModel.IsSuccess = false;
                Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryVehicle.queryInsertDiary, diaryFalse, tran);
                tran.Commit();

                response = new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Không tìm thấy kết quả !"
                };
                return response;
            }
            await connection.ExecuteAsync(QueryVehicle.queryHideVehicle, new { Id = IdVehicle, IsHide = IsHide }, tran);

            //save diary if success
            Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
            await connection.ExecuteAsync(QueryVehicle.queryInsertDiary, diary, tran);

            tran.Commit();

            response = new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Ẩn thành công !"
            };
            return response;
        }
        #endregion
    }
    internal static class QueryVehicle
    {
        public const string queryInsertVehicle = @"INSERT INTO [dbo].[Vehicle]
                               ([Id]
                               ,[NameVehicle]
                               ,[ChassisNumber]
                               ,[EngineNumber]
                               ,[LicensePlates]
                               ,[IdBrandVehicleCategory]
                               ,[IdModel]
                               ,[IdType]
                               ,[IdGearBox]
                               ,[EngineCapacity]
                               ,[Color]
                               ,[Status]
                               ,[CreatedDate]
                               ,[IsHide]
                               ,[YearOfManufacture]
                               ,[Avatar])
                         VALUES (@Id, @NameVehicle, @ChassisNumber, @EngineNumber,
                                @LicensePlates, @IdBrandVehicleCategory, @IdModel, @IdType,
                                @IdGearBox, @EngineCapacity, @Color, @Status,
                                @CreatedDate, @IsHide, @YearOfManufacture, @Avatar)";
        public const string queryUpdateVehicle = @"UPDATE [dbo].[Vehicle] SET NameVehicle = @NameVehicle,
                                        ChassisNumber = @ChassisNumber,
                                        EngineNumber = @EngineNumber,
                                        LicensePlates = @LicensePlates,
                                        IdBrandVehicleCategory = @IdBrandVehicleCategory,
                                        IdModel = @IdModel,
                                        IdType = @IdType,
                                        IdGearBox = @IdGearBox,
                                        EngineCapacity = @EngineCapacity,
                                        Color = @Color,
                                        YearOfManufacture = @YearOfManufacture,
                                        Avatar = @Avatar
                                        WHERE Id = @Id";
        public const string queryDeleteVehicle = "DELETE FROM [dbo].[Vehicle] WHERE Id = @Id";
        public const string queryGetByIdVehicle = "select *from [dbo].[Vehicle] where Id = @Id";
        public const string queryGetAllVehicle = "select *from [dbo].[Vehicle] order by CreatedDate desc";
        public const string queryHideVehicle = "UPDATE [dbo].[Vehicle] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertVehicle_Deleted = @"INSERT INTO [dbo].[Vehicle_Deleted]
                               ([Id]
                               ,[NameVehicle]
                               ,[ChassisNumber]
                               ,[EngineNumber]
                               ,[LicensePlates]
                               ,[IdBrandVehicleCategory]
                               ,[IdModel]
                               ,[IdType]
                               ,[IdGearBox]
                               ,[EngineCapacity]
                               ,[Color]
                               ,[Status]
                               ,[CreatedDate]
                               ,[IsHide]
                               ,[YearOfManufacture]
                               ,[Avatar])
                         VALUES (@Id, @NameVehicle, @ChassisNumber, @EngineNumber,
                                @LicensePlates, @IdBrandVehicleCategory, @IdModel, @IdType,
                                @IdGearBox, @EngineCapacity, @Color, @Status,
                                @CreatedDate, @IsHide, @YearOfManufacture, @Avatar)";
        public const string queryGetAllIdVehicle = "select Id from [dbo].[Vehicle]";
        public const string queryDeleteCustomer_Vehicle = "DELETE FROM [dbo].[Customer_Vehicle] WHERE Id = @Id";
        public const string queryDeleteCustomer_VehicleByIds = "DELETE FROM [dbo].[Customer_Vehicle] WHERE Id in @Id";
        public const string queryGetCustomer_VehicleByIdVehicle = "select * from [dbo].[Customer_Vehicle] where IdVehicle = @IdVehicle";
        public const string queryInsertCustomer_Vehicle_Deleted = @"INSERT INTO [dbo].[Customer_Vehicle_Deleted]
                               ([Id]
                               ,[IdCustomer]
                               ,[IdVehicle])
                         VALUES (@Id, @IdCustomer, @IdVehicle)";
        public const string queryRO_RepairOdersByIdVehicle = "select * from [dbo].[RO_RepairOders] where IdVehicle = @IdVehicle";

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
        public const string queryInsertCustomer_Vehicle = @"INSERT INTO [dbo].[Customer_Vehicle]
                                                                   ([Id]
                                                                   ,[IdCustomer]
                                                                   ,[IdVehicle])
                                                             VALUES (@Id, @IdCustomer, @IdVehicle)";
        public const string queryGetAllCustomerByIdVehicle = @"select temp1.* from
                                            (
	                                            select cus.* from
	                                            (
	                                            select c.*
		                                            from Customer_Vehicle c left join Vehicle v on c.IdVehicle = v.Id
		                                            where v.Id = @IdVehicle
	                                            ) as temp left join Customer cus on temp.IdCustomer = cus.Id
                                            ) as temp1";
        public const string queryGetAllVehicleByIdCustomer = @"select temp1.* from
                                            (
	                                            select v.* from
	                                            (
													select c.*
		                                            from Customer_Vehicle c left join Customer cus on cus.Id = c.IdCustomer
		                                            where cus.Id = @IdCustomer
	                                            ) as temp left join Vehicle v on temp.IdVehicle = v.Id where v.IsHide = 0
                                            ) as temp1";

        //StoredProcedure 
        public const string queryStoreDeleteVehicleByList = "[dbo].[GM_SP_DeleteVehicleByList]";
    }
}
