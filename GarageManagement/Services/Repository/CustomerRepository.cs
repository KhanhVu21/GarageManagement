using Dapper;
using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Numerics;
using static GarageManagement.Services.Repository.StaticsRepository;
using static System.Net.WebRequestMethods;

namespace GarageManagement.Services.Repository
{
    public class CustomerRepository: ICustomerRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public CustomerRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetCustomerByPhoneOrByEmail(String filter, int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            var Customers = await connection.QueryAsync<Customer>(QueryCustomer.queryCustomerByCode, new { Code = "%" + filter + "%" });
            if(Customers.Count() == 0)
            {
                Customers =  await connection.QueryAsync<Customer>(QueryCustomer.queryCustomerLikePhone, new { Phone = "%" + filter + "%" });
            }

            int coutRecord = Customers.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Customers = Customers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CustomerDto> CustomerDtos = Customers.Adapt<IEnumerable<CustomerDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CustomerDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCustomerByIdGroup(int pageNumber, int pageSize, Guid IdGroup)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Customer> Customers = await connection.QueryAsync<Customer>(QueryCustomer.queryGetAllCustomerByIdGroup, new { IdGroup = IdGroup });
            int coutRecord = Customers.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Customers = Customers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<CustomerDto> CustomerDtos = Customers.Adapt<IEnumerable<CustomerDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CustomerDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllCustomer(int pageNumber, int pageSize)
        {
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Customer> Customers = await connection.QueryAsync<Customer>(QueryCustomer.queryGetAllCustomer);
            int coutRecord = Customers.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                Customers = Customers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var  CustomerDtos = Customers.Adapt<IEnumerable<CustomerDto>>().ToList();
            for(int i = 0; i < CustomerDtos.Count();i++)
            {
                var deposit = await connection.QueryFirstOrDefaultAsync<DepositAfterPay>(QueryCustomer.queryDepositAfterPayByIdCustomer, new { IdCustomer = CustomerDtos[i].Id });
                if (deposit != null) { CustomerDtos[i].Deposit = deposit.TotalMoney; }
            }

            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = CustomerDtos.ToArray(),
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
        public async Task<TemplateApi> GetCustomerById(Guid IdCustomer)
        {
            using var con = _DbContext.CreateConnection();
            Customer Customer = await con.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetByIdCustomer, new { Id = IdCustomer });

            if (Customer != null)
            {
                var CustomerDto = Customer.Adapt<CustomerDto>();
                var deposit = await con.QueryFirstOrDefaultAsync<DepositAfterPay>(QueryCustomer.queryDepositAfterPayByIdCustomer, new { IdCustomer = IdCustomer });
                if (deposit != null) { CustomerDto.Deposit = deposit.TotalMoney; }

                var templateApi = new TemplateApi()
                {
                    Payload = CustomerDto,
                    Message = "Lấy thông tin khách hàng thành công",
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
                    Message = "Không tìm thấy khách hàng",
                    Success = false,
                    Fail = true,
                };
                return templateApi;
            }
        }
        public async Task<TemplateApi> DeleteCustomer(Guid IdCustomer, Guid IdUserCurrent)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            try
            {
                //query table Customer
                Customer Customer = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetByIdCustomer, new { Id = IdCustomer }, tran);

                if (Customer is null)
                {
                    var responseFalse = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return responseFalse;
                }
                //call store procedure to delete customer
                var values = new { IdCustomer = IdCustomer, IdUser = IdUserCurrent };
                await connection.ExecuteAsync(QueryCustomer.queryStoreDeleteCustomersByList, values, tran, commandType: CommandType.StoredProcedure);

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
        public async Task<TemplateApi> DeleteCustomerByList(List<Guid> IdCustomer, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            try
            {
                for (int i = 0; i < IdCustomer.Count; i++)
                {
                    //query Customer by id
                    var CustomerId = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetByIdCustomer, new { Id = IdCustomer[i] }, tran);

                    var repairOders = await connection.QueryFirstOrDefaultAsync<RO_RepairOders>(QueryCustomer.queryRO_RepairOdersByIdCustomer, new { IdCustomer = IdCustomer[i] }, tran);

                    if (CustomerId is null || repairOders is not null)
                    {
                        tran.Rollback();

                        var responseNotFound = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = CustomerId is null ? "Đã có ID không tồn tại !" : "Khách hàng đã có trong hóa đơn sửa chữa không thể xóa !"
                        };
                        return responseNotFound;
                    }

                    var values = new { IdCustomer = IdCustomer[i], IdUser = IdUserCurrent };
                    await connection.ExecuteAsync(QueryCustomer.queryStoreDeleteCustomersByList, values, tran, commandType: CommandType.StoredProcedure);
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
        public async Task<TemplateApi> InsertCustomer(CustomerDto CustomerDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var checkExistEmail = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByEmail, new { Email = CustomerDto.Email }, tran);
                if (checkExistEmail != null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Email này đã tồn tại !"
                    };
                }

                var checkExistPhone = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByPhone, new { Phone = CustomerDto.Phone }, tran);
                if (checkExistPhone != null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Số điện thoại này đã tồn tại !"
                    };
                }
                var Customer = CustomerDto.Adapt<Customer>();

                //query user by iduserCurrentLogin
                var userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer.queryGetUserById, new { Id = CustomerDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = CustomerDto.Id,
                    Operation = "Create",
                    Table = "Customer",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(QueryCustomer.queryInsertCustomer, Customer, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(QueryCustomer.queryInsertDiary, diary, tran);

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
        public async Task<TemplateApi> InsertCustomerNotToken(CustomerDto CustomerDto)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            try
            {
                var checkExitsEmail = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByEmail, new { Email = CustomerDto.Email }, tran);
                if(checkExitsEmail != null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Email này đã tồn tại !"
                    };
                }
                var checkExistPhone = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByPhone, new { Phone = CustomerDto.Phone }, tran);
                if (checkExistPhone != null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Số điện thoại này đã tồn tại !"
                    };
                }
                var checkExitsCustomerGr = await connection.QueryFirstOrDefaultAsync<CustomerGroup>(QueryCustomer.queryCustomerGroupByCode, new { GroupCode = "GR - 1669349753201" }, tran);
                if(checkExitsCustomerGr is null)
                {
                    // define some col with data concrete
                    var customerGroup = new CustomerGroup()
                    {
                        Id = Guid.NewGuid(),
                        CreateDate = DateTime.Now,
                        Status = 0,
                        IsHide = false,
                        GroupCode = "GR - 1669349753201",
                        GroupName = "Khách hàng mới"
                    };
                    await connection.ExecuteAsync(QueryCustomerGroup.queryInsertCustomerGroup, customerGroup, tran);

                    var customer = CustomerDto.Adapt<Customer>();
                    customer.IdGroup = customerGroup.Id;
                    await connection.ExecuteAsync(QueryCustomer.queryInsertCustomer, customer, tran);
                }
                else
                {
                    var Customer = CustomerDto.Adapt<Customer>();
                    Customer.IdGroup = checkExitsCustomerGr.Id;
                    await connection.ExecuteAsync(QueryCustomer.queryInsertCustomer, Customer, tran);

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
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        public async Task<TemplateApi> UpdateCustomer(CustomerDto CustomerDto)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                var checkExistEmail = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByEmail, new { Email = CustomerDto.Email }, tran);
                if (checkExistEmail != null && checkExistEmail.Id != CustomerDto.Id)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Email này đã tồn tại !"
                    };
                }

                var checkExistPhone = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByPhone, new { Phone = CustomerDto.Phone }, tran);
                if (checkExistPhone != null && checkExistPhone.Id != CustomerDto.Id)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Số điện thoại này đã tồn tại !"
                    };
                }

                var Customer = CustomerDto.Adapt<Customer>();
                //get Customer by id
                Customer CustomerById = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetByIdCustomer, new { CustomerDto.Id }, tran);

                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryCustomer.queryGetUserById, new { Id = CustomerDto.IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (CustomerById == null)
                {
                    return new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                }

                //check id file exits
                if (CustomerDto.idFile is not null)
                {
                    Customer.Avatar = CustomerById.Avatar;
                }

                await connection.ExecuteAsync(QueryCustomer.queryUpdateCustomer, Customer, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, Customer.Id, "Update", "Customer", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryCustomer.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> VerifyOtp(string cusCode, string email, string otp)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var checkEmail = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByEmail, new { Email = email });
            if(checkEmail == null)
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Email này không tồn tại !"
                };
            }
            if(checkEmail.Code != cusCode)
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Vui lòng kiểm tra lại mã khách hàng !"
                };
            }
            if (checkEmail.Otp != Int32.Parse(otp))
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Mã xác thực không chính xác !"
                };
            }

            return new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Xác thực thành công !"
            };
        }
        public async Task<TemplateApi> RefreshOtp(String email, String random, String cusCode)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            var checkEmail = await connection.QueryFirstOrDefaultAsync<Customer>(QueryCustomer.queryGetCustomerByEmail, new { Email = email });
            if (checkEmail == null)
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Email này không tồn tại !"
                };
            }
            if (checkEmail.Code != cusCode)
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Mã khách hàng không chính xác !"
                };
            }

            await connection.ExecuteAsync(QueryCustomer.queryUpdateOtpCustomer, new { Otp = random, Id = checkEmail.Id});

            return new TemplateApi()
            {
                Success = true,
                Fail = false,
                Message = "Mã OTP đã gửi về Gmail của bạn !"
            };
        }
        public async Task<TemplateApi> GetAllRepairOrderByCustomer(string cusCode, int pageNumber, int pageSize)
        {
            using var con = _DbContext.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@code", cusCode);

            var results = await con.QueryAsync<LoadDatasRepairOrderByCustomer>(
                QueryCustomer.queryStoreGetAllRepairOrderByCustomer,
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            if (results.Count() == 0)
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Mã khách hàng không chính xác !"
                };
            }


            var groupedList = results.GroupBy(a => a.IdCustomer)
            .Select(b => new LoadDatasRepairOrderByCustomerGr()
            {
                IdCustomer = b.First().IdCustomer,
                NameCustomer = b.First().NameCustomer,
                Code = b.First().Code,
                Sex = b.First().Sex,
                Birthday = b.First().Birthday,
                Phone = b.First().Phone,
                Email = b.First().Email,
                AddressCus = b.First().AddressCus,
                IdCity = b.First().IdCity,
                IdDistrict = b.First().IdDistrict,
                IdWard = b.First().IdWard,
                TaxNumber = b.First().TaxNumber,
                AccountNumber = b.First().AccountNumber,
                NoteCus = b.First().NoteCus,
                Avatar = b.First().Avatar,
                TypeOfCustomer = b.First().TypeOfCustomer,
                IdGroup = b.First().IdGroup,

                rcRepairOrders = b.Any(b => b.IdRepairOrder != null) ? b.GroupBy(c => c.IdRepairOrder).Select(d => new RcRepairOrders()
                {
                    IdRepairOrder = d.First().IdRepairOrder,
                    Kilometer = d.First().Kilometer,
                    TaxPercent = d.First().TaxPercent,
                    TotalMoneyRepair = d.First().TotalMoneyRepair,
                    RO_RepairOdersCode = d.First().RO_RepairOdersCode,
                    CreatedDate = d.First().CreatedDate,
                    rcDebts = d.Any(d => d.IdDebt != null) ? d.GroupBy(d => d.IdDebt).Select(e => new RcDebt()
                    {
                        IdDebt = e.First().IdDebt,
                        DebtContent = e.First().DebtContent,
                        Deposit = e.First().Deposit,
                        TotalPay = e.First().TotalPay,
                        DebtNumber = e.First().DebtNumber,
                        LastPay = e.First().LastPay
                    }).ToList() : null,
                    rcRequestLists = d.Any(d => d.IdRequest != null) ? d.GroupBy(d => d.IdRequest).Select(g => new RcRequestList()
                    {
                        IdRequest = g.First().IdRequest,
                        RequestContent = g.First().RequestContent,
                        IsProcessing = g.First().IsProcessing,
                        IsCompleted = g.First().IsCompleted,
                        IsCanceled = g.First().IsCanceled,
                        Note = g.First().Note
                    }).ToList() : null,
                    rcRepairOrdersAcs = d.Any(d => d.IdRepairOrdersAccessary != null) ? d.GroupBy(d => d.IdRepairOrdersAccessary).Select(h => new RcRepairOrdersAcs()
                    {
                        IdRepairOrdersAccessary = h.First().IdRepairOrdersAccessary,
                        IdCategoryAccessary = h.First().IdCategoryAccessary,
                        Quantity = h.First().Quantity,
                        Price = h.First().Price,
                        DiscountPercent = h.First().DiscountPercent,
                        DiscountPrice = h.First().DiscountPrice,
                        TotalMoneyAcs = h.First().TotalMoneyAcs
                    }).ToList() : null,
                    rcRepairOrdersEmps = d.Any(d => d.IdRepairOrdersEmployee != null) ? d.GroupBy(d => d.IdRepairOrdersEmployee).Select(k => new RcRepairOrdersEmp()
                    {
                        IdRepairOrdersEmployee = k.First().IdRepairOrdersEmployee,
                        IdEmployee = k.First().IdEmployee
                    }).ToList() : null,
                    rcOrtherCosts = d.Any(d => d.IdOrtherCost != null) ? d.GroupBy(d => d.IdOrtherCost).Select(m => new RcOrtherCost()
                    {
                        IdOrtherCost = m.First().IdOrtherCost,
                        CostContent = m.First().CostContent,
                        QuantityOrt = m.First().QuantityOrt,
                        PriceOrt = m.First().PriceOrt,
                        TotalMoneyOrt = m.First().TotalMoneyOrt
                    }).ToList() : null,
                }).ToList() : null
            });

            int coutRecord = groupedList.FirstOrDefault()?.rcRepairOrders?.Count() ?? 0;

            if (pageNumber != 0 && pageSize != 0 && groupedList.FirstOrDefault()?.rcRepairOrders != null)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                groupedList = groupedList.Select(a => new LoadDatasRepairOrderByCustomerGr
                {
                    IdCustomer = a.IdCustomer,
                    NameCustomer = a.NameCustomer,
                    Code = a.Code,
                    Sex = a.Sex,
                    Birthday = a.Birthday,
                    Phone = a.Phone,
                    Email = a.Email,
                    AddressCus = a.AddressCus,
                    IdCity = a.IdCity,
                    IdDistrict = a.IdDistrict,
                    IdWard = a.IdWard,
                    TaxNumber = a.TaxNumber,
                    AccountNumber = a.AccountNumber,
                    NoteCus = a.NoteCus,
                    Avatar = a.Avatar,
                    TypeOfCustomer = a.TypeOfCustomer,
                    IdGroup = a.IdGroup,
                    rcRepairOrders = a.rcRepairOrders
                        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                        .ToList(),
                });
                coutRecord = groupedList.FirstOrDefault()?.rcRepairOrders?.Count() ?? 0;
            }
            int NumPagesize = pageSize == 0 ? 1 : pageSize;    

            return new TemplateApi()
            {
                Payload= groupedList.FirstOrDefault(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        public async Task<TemplateApi> GetInforRepairOrderById(Guid IdRepairOrder, String cusCode)
        {
            using var con = _DbContext.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@code", cusCode);

            var results = await con.QueryAsync<LoadDatasRepairOrderByCustomer>(
                QueryCustomer.queryStoreGetAllRepairOrderByCustomer,
                param: parameters,
                commandType: CommandType.StoredProcedure
            );

            if (results.Count() == 0)
            {
                return new TemplateApi()
                {
                    Success = false,
                    Fail = true,
                    Message = "Mã khách hàng không chính xác !"
                };
            }


            var groupedList = results.GroupBy(a => a.IdCustomer)
            .Select(b => new LoadDatasRepairOrderByCustomerGr()
            {
                IdCustomer = b.First().IdCustomer,
                NameCustomer = b.First().NameCustomer,
                Code = b.First().Code,
                Sex = b.First().Sex,
                Birthday = b.First().Birthday,
                Phone = b.First().Phone,
                Email = b.First().Email,
                AddressCus = b.First().AddressCus,
                IdCity = b.First().IdCity,
                IdDistrict = b.First().IdDistrict,
                IdWard = b.First().IdWard,
                TaxNumber = b.First().TaxNumber,
                AccountNumber = b.First().AccountNumber,
                NoteCus = b.First().NoteCus,
                Avatar = b.First().Avatar,
                TypeOfCustomer = b.First().TypeOfCustomer,
                IdGroup = b.First().IdGroup,

                rcRepairOrders = b.Any(b => b.IdRepairOrder != null) ? b.GroupBy(c => c.IdRepairOrder).Select(d => new RcRepairOrders()
                {
                    IdRepairOrder = d.First().IdRepairOrder,
                    Kilometer = d.First().Kilometer,
                    TaxPercent = d.First().TaxPercent,
                    TotalMoneyRepair = d.First().TotalMoneyRepair,
                    RO_RepairOdersCode = d.First().RO_RepairOdersCode,
                    CreatedDate = d.First().CreatedDate,
                    rcDebts = d.Any(d => d.IdDebt != null) ? d.GroupBy(d => d.IdDebt).Select(e => new RcDebt()
                    {
                        IdDebt = e.First().IdDebt,
                        DebtContent = e.First().DebtContent,
                        Deposit = e.First().Deposit,
                        TotalPay = e.First().TotalPay,
                        DebtNumber = e.First().DebtNumber,
                        LastPay = e.First().LastPay
                    }).ToList() : null,
                    rcRequestLists = d.Any(d => d.IdRequest != null) ? d.GroupBy(d => d.IdRequest).Select(g => new RcRequestList()
                    {
                        IdRequest = g.First().IdRequest,
                        RequestContent = g.First().RequestContent,
                        IsProcessing = g.First().IsProcessing,
                        IsCompleted = g.First().IsCompleted,
                        IsCanceled = g.First().IsCanceled,
                        Note = g.First().Note
                    }).ToList() : null,
                    rcRepairOrdersAcs = d.Any(d => d.IdRepairOrdersAccessary != null) ? d.GroupBy(d => d.IdRepairOrdersAccessary).Select(h => new RcRepairOrdersAcs()
                    {
                        IdRepairOrdersAccessary = h.First().IdRepairOrdersAccessary,
                        IdCategoryAccessary = h.First().IdCategoryAccessary,
                        Quantity = h.First().Quantity,
                        Price = h.First().Price,
                        DiscountPercent = h.First().DiscountPercent,
                        DiscountPrice = h.First().DiscountPrice,
                        TotalMoneyAcs = h.First().TotalMoneyAcs
                    }).ToList() : null,
                    rcRepairOrdersEmps = d.Any(d => d.IdRepairOrdersEmployee != null) ? d.GroupBy(d => d.IdRepairOrdersEmployee).Select(k => new RcRepairOrdersEmp()
                    {
                        IdRepairOrdersEmployee = k.First().IdRepairOrdersEmployee,
                        IdEmployee = k.First().IdEmployee
                    }).ToList() : null,
                    rcOrtherCosts = d.Any(d => d.IdOrtherCost != null) ? d.GroupBy(d => d.IdOrtherCost).Select(m => new RcOrtherCost()
                    {
                        IdOrtherCost = m.First().IdOrtherCost,
                        CostContent = m.First().CostContent,
                        QuantityOrt = m.First().QuantityOrt,
                        PriceOrt = m.First().PriceOrt,
                        TotalMoneyOrt = m.First().TotalMoneyOrt
                    }).ToList() : null,
                }).ToList() : null
            });

            groupedList = groupedList.Select(a => new LoadDatasRepairOrderByCustomerGr
            {
                IdCustomer = a.IdCustomer,
                NameCustomer = a.NameCustomer,
                Code = a.Code,
                Sex = a.Sex,
                Birthday = a.Birthday,
                Phone = a.Phone,
                Email = a.Email,
                AddressCus = a.AddressCus,
                IdCity = a.IdCity,
                IdDistrict = a.IdDistrict,
                IdWard = a.IdWard,
                TaxNumber = a.TaxNumber,
                AccountNumber = a.AccountNumber,
                NoteCus = a.NoteCus,
                Avatar = a.Avatar,
                TypeOfCustomer = a.TypeOfCustomer,
                IdGroup = a.IdGroup,
                rcRepairOrders = a.rcRepairOrders
                        .Where(e => e.IdRepairOrder == IdRepairOrder)
            });

            return new TemplateApi()
            {
                Payload = groupedList.FirstOrDefault(),
                Message = "Lấy thông tin thành công !",
                Success = true,
                Fail = false,
                PageNumber = 0,
                PageSize = 0,
                TotalElement = 1,
                TotalPages = 1,
            };
        }
        #endregion
    }
    internal static class QueryCustomer
    {
        public const string queryInsertCustomer = @"INSERT INTO [dbo].[Customer]
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
                               ,[TypeOfCustomer]
                               ,[IdGroup]
                               ,[Status]
                               ,[CreatedDate]
                               ,[Otp])
                         VALUES (@Id, @Code, @Name, @Sex,
                                @Birthday, @Phone, @Email, @Address,
                                @IdCity, @IdDistrict, @IdWard, @TaxNumber,
                                @AccountNumber, @Note, @Avatar, @TypeOfCustomer, @IdGroup, @Status, @CreatedDate, @Otp)";
        public const string queryUpdateCustomer = @"UPDATE [dbo].[Customer] SET
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
                                        TypeOfCustomer = @TypeOfCustomer,
                                        IdGroup = @IdGroup
                                        WHERE Id = @Id";
        public const string queryUpdateOtpCustomer = @"UPDATE [dbo].[Customer] SET Otp = @Otp WHERE Id = @Id";
        public const string queryDeleteCustomer = "DELETE FROM [dbo].[Customer] WHERE Id = @Id";
        public const string queryGetByIdCustomer = "select * from [dbo].[Customer] where Id = @Id";
        public const string queryGetAllCustomer = "select *from [dbo].[Customer] order by CreatedDate desc";
        public const string queryHideCustomer = "UPDATE [dbo].[Customer] SET IsHide = @IsHide WHERE Id = @Id";
        public const string queryInsertCustomer_Deleted = @"INSERT INTO [dbo].[Customer_Deleted]
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
                               ,[TypeOfCustomer]
                               ,[IdGroup]
                               ,[Status]
                               ,[CreatedDate]
                               ,[Otp])
                         VALUES (@Id, @Code, @Name, @Sex,
                                @Birthday, @Phone, @Email, @Address,
                                @IdCity, @IdDistrict, @IdWard, @TaxNumber,
                                @AccountNumber, @Note, @Avatar, @TypeOfCustomer, @IdGroup, @Status, @CreatedDate, @Otp)";
        public const string queryGetAllIdCustomer = "select Id from [dbo].[Customer]";
        public const string queryGetAllCustomerByIdGroup = "select *from [dbo].[Customer] where IdGroup = @IdGroup order by CreatedDate desc";
        public const string queryDeleteCustomer_Vehicle = "DELETE FROM [dbo].[Customer_Vehicle] WHERE Id = @Id";
        public const string queryGetCustomer_VehicleByIdCustomer = "select * from [dbo].[Customer_Vehicle] where IdCustomer = @IdCustomer";
        public const string queryInsertCustomer_Vehicle_Deleted = @"INSERT INTO [dbo].[Customer_Vehicle_Deleted]
                               ([Id]
                               ,[IdCustomer]
                               ,[IdVehicle])
                         VALUES (@Id, @IdCustomer, @IdVehicle)";
        public const string queryRO_RepairOdersByIdCustomer = "select * from [dbo].[RO_RepairOders] where IdCustomer = @IdCustomer";
        public const string queryCustomerGroupByCode = "select * from [dbo].[CustomerGroup] where GroupCode = @GroupCode";
        public const string queryGetCustomerByEmail = "select * from [dbo].[Customer] where Email = @Email";
        public const string queryGetCustomerByPhone = "select * from [dbo].[Customer] where Phone = @Phone";
        public const string queryCustomerLikePhone = "select * from [dbo].[Customer] where Phone LIKE @Phone";
        public const string queryCustomerByCode = "select * from [dbo].[Customer] where Code LIKE @Code";
        public const string queryDepositAfterPayByIdCustomer = "select * from [dbo].[DepositAfterPay] where IdCustomer = @IdCustomer";

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

        //StoredProcedure 
        public const string queryStoreDeleteCustomersByList = "[dbo].[GM_SP_DeleteCustomerByList]";
        public const string queryStoreGetAllRepairOrderByCustomer = "[dbo].[GM_SP_GetAllRepairOrderByCustomer]";
    }
}
