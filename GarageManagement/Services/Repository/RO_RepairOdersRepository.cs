using Azure;
using Azure.Core;
using Dapper;
using GarageManagement.Controllers.Payload.RO_RepairOders;
using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using GarageManagement.Services.IRepository;
using Mapster;
using System.ComponentModel;
using System.Data;

namespace GarageManagement.Services.Repository
{
    public class RO_RepairOdersRepository : IRO_RepairOdersRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public RO_RepairOdersRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllRO_RepairOders(int pageNumber, int pageSize, bool IsPaid)
        {
            using var connection = _DbContext.CreateConnection();
            var RO_RepairOderss = await connection.QueryAsync<RO_RepairOders>(QueryRO_RepairOders.queryGetAllRO_RepairOdersIsPaid);
            if(!IsPaid)
            {
                RO_RepairOderss = await connection.QueryAsync<RO_RepairOders>(QueryRO_RepairOders.queryGetAllRO_RepairOdersNotPaid);
            }
            int coutRecord = RO_RepairOderss.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                RO_RepairOderss = RO_RepairOderss.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var repairOders = RO_RepairOderss.Adapt<IEnumerable<RO_RepairOdersDto>>().ToList();
            for (int i = 0; i < repairOders.Count(); i++)
            {
                var debts = await connection.QueryAsync<Debt>(QueryRO_RepairOders.queryGetAllDebtIdRepairOrders, new { IdRepairOrders = repairOders[i].Id });
                if (debts.Count() > 1) { repairOders[i].IsPaid = true; }
            }

            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            return new TemplateApi()
            {
                ListPayload = repairOders.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        public async Task<TemplateApi> GetRO_RepairOdersById(Guid IdRO_RepairOders)
        {
            using var con = _DbContext.CreateConnection();
            var datas = new LoadAndUpdateRO_RepairOdersBy();

            RO_RepairOders RO_RepairOders = await con.QueryFirstOrDefaultAsync<RO_RepairOders>(QueryRO_RepairOders.queryGetByIdRO_RepairOders, new { Id = IdRO_RepairOders });
            if(RO_RepairOders is null)
            {
                return new TemplateApi()
                {
                    Message = "Không tìm thấy kết quả",
                    Success = false,
                    Fail = true,
                };
            }

            datas.RO_RepairOders = RO_RepairOders;
            datas.ListEmployee = await con.QueryAsync<RepairOrders_Employee>(
                QueryRO_RepairOders.queryGetAllRepairOrders_EmployeeByIdRO_RepairOders,
                new { IdRepairOrders = RO_RepairOders.Id }
            );
            datas.ListRepairOrders_Accessary = await con.QueryAsync<RepairOrders_Accessary>(
                QueryRO_RepairOders.queryGetAllRepairOrders_AccessaryIdRepairOrders,
                new { IdRepairOrders = RO_RepairOders.Id }
            );
            datas.OrtherCosts = await con.QueryAsync<OrtherCost>(
                QueryRO_RepairOders.queryGetAllOrtherCostIdRepairOrders,
                new { IdRepairOders = RO_RepairOders.Id }
            );
            datas.Debts = await con.QueryAsync<Debt>(
                QueryRO_RepairOders.queryGetAllDebtIdRepairOrders,
                new { IdRepairOrders = RO_RepairOders.Id }
            );
            datas.RequestLists = await con.QueryAsync<RequestList>(
                QueryRO_RepairOders.queryGetAllRequestListIdRepairOrders,
                new { Id_RO_RepairOrders = RO_RepairOders.Id }
            );

            return new TemplateApi()
            {
                Payload = datas,
                Message = "Lấy thông tin thành công",
                Success = true,
                Fail = false,
                TotalElement = 1,
            };
        }
        public async Task<TemplateApi> DeleteRO_RepairOders(Guid IdRO_RepairOders, Guid IdUserCurrent)
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
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRO_RepairOders.queryGetUserById, new { Id = IdUserCurrent }, tran);
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                //query table RO_RepairOders
                RO_RepairOders RO_RepairOders = await connection.QueryFirstOrDefaultAsync<RO_RepairOders>(QueryRO_RepairOders.queryGetByIdRO_RepairOders, new { Id = IdRO_RepairOders }, tran);

                if (RO_RepairOders is null)
                {
                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(QueryRO_RepairOders.queryDeleteRO_RepairOders, new { Id = RO_RepairOders.Id }, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RO_RepairOders.Id, "Delete", "RO_RepairOders", ""));

                //save to RO_RepairOders_Deleted
                await connection.ExecuteAsync(QueryRO_RepairOders.queryInsertRO_RepairOders_Deleted, RO_RepairOders, tran);
                //save diary if success
                diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RO_RepairOders.Id, "Create", "RO_RepairOders_Deleted", ""));

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRO_RepairOders.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> DeleteRO_RepairOdersByList(List<Guid> IdRO_RepairOders, Guid IdUserCurrent)
        {
            //open connection to sql
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            //query user by iduserCurrentLogin
            UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRO_RepairOders.queryGetUserById, new { Id = IdUserCurrent }, tran);

            var saveDiaryModel = new SaveDiaryModel()
            {
                IdUserCurrent = userByIDCurrentLogin.Id,
                Fullname = userByIDCurrentLogin.Fullname ?? "",
                IsSuccess = true,
            };

            try
            {
                for (int i = 0; i < IdRO_RepairOders.Count; i++)
                {
                    //query RO_RepairOders by id
                    RO_RepairOders RO_RepairOdersId = await connection.QueryFirstOrDefaultAsync<RO_RepairOders>(QueryRO_RepairOders.queryGetByIdRO_RepairOders, new { Id = IdRO_RepairOders[i] }, tran);

                    if (RO_RepairOdersId is null)
                    {
                        response = new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = "Không tìm thấy kết quả !"
                        };
                        return response;
                    }
                    await connection.ExecuteAsync(QueryRO_RepairOders.queryDeleteRO_RepairOders, new { Id = IdRO_RepairOders[i] }, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, IdRO_RepairOders[i], "Delete", "RO_RepairOders", ""));

                    //save to RO_RepairOders_Deleted
                    await connection.ExecuteAsync(QueryRO_RepairOders.queryInsertRO_RepairOders_Deleted, RO_RepairOdersId, tran);
                    //save diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, RO_RepairOdersId.Id, "Create", "RO_RepairOders_Deleted", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRO_RepairOders.queryInsertDiary, diaries[i], tran);
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
        public async Task<TemplateApi> UpdateRO_RepairOders(RepairOrderPayload repairOrderPayload, Guid IdUserCurrent)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            try
            {
                //prepare param to insert RO_RepairOders
                #region RO_RepairOders
                //map data from dtos to entity RO_RepairOders
                var rO_RepairOders = repairOrderPayload.Adapt<RO_RepairOders>();
                rO_RepairOders.TotalMoney = repairOrderPayload.TotalMoney;

                var data = new List<RO_RepairOders>{rO_RepairOders};
                #endregion

                //prepare param to insert RepairOrders_Employee
                #region RepairOrders_Employee
                var repairOrders_Employees = new List<RepairOrders_Employee>();

                for (int i = 0; i < repairOrderPayload?.RepairOrderEmployee?.Count; i++)
                {
                    repairOrders_Employees.Add(new RepairOrders_Employee
                    {
                        Id = repairOrderPayload.RepairOrderEmployee[i].Id ?? Guid.NewGuid(),
                        IdRepairOrders = rO_RepairOders.Id,
                        IdEmployee = repairOrderPayload.RepairOrderEmployee[i].IdEmployee,
                        Note = repairOrderPayload.RepairOrderEmployee[i].Note,
                        CreatedDate = DateTime.Now
                    });
                }
                var tableRepairOrders_Employee = ConvertToDataTable(repairOrders_Employees);
                #endregion

                //prepare param to insert OrtherCost
                #region OrtherCost
                var ortherCostDatas = new List<OrtherCost>();

                for (int i = 0; i < repairOrderPayload?.OrtherCost?.Count; i++)
                {
                    ortherCostDatas.Add(new OrtherCost()
                    {
                        Id = repairOrderPayload?.OrtherCost[i].Id ?? Guid.NewGuid(),
                        IdRepairOders = rO_RepairOders.Id,
                        CostContent = repairOrderPayload?.OrtherCost[i].CostContent,
                        Quantity = repairOrderPayload?.OrtherCost[i].Quantity,
                        Price = repairOrderPayload?.OrtherCost[i].Price,
                        TotalMoney = repairOrderPayload?.OrtherCost[i].TotalMoney,
                        CreatedDate = DateTime.Now,
                        Status = 0
                    });
                    //data[0].TotalMoney += repairOrderPayload?.OrtherCost[i].TotalMoney;
                }
                var tableOrtherCost = ConvertToDataTable(ortherCostDatas);
                #endregion

                //prepare param to insert RequestList
                #region RequestList
                var requestDatas = new List<RequestList>();

                for (int i = 0; i < repairOrderPayload?.RequestList?.Count; i++)
                {
                    requestDatas.Add(new RequestList()
                    {
                        Id = repairOrderPayload?.RequestList[i].Id ?? Guid.NewGuid(),
                        Id_RO_RepairOrders = rO_RepairOders.Id,
                        RequestContent = repairOrderPayload?.RequestList[i].RequestContent,
                        IsProcessing = true,
                        IsCompleted = false,
                        IsCanceled = false,
                        CreatedDate = DateTime.Now,
                        Status = 0,
                        Note = repairOrderPayload?.RequestList[i].Note
                    });
                }
                var tableRequestList = ConvertToDataTable(requestDatas);
                #endregion

                //prepare param to insert RepairOrders_Accessary
                #region RepairOrders_Accessary
                var repairOrders_Accessaries = new List<RepairOrders_Accessary>();

                for (int i = 0; i < repairOrderPayload?.AccessaryOrder?.Count; i++)
                {
                    repairOrders_Accessaries.Add(new RepairOrders_Accessary()
                    {
                        Id = repairOrderPayload?.AccessaryOrder[i].Id ?? Guid.NewGuid(),
                        IdCategoryAccessary = repairOrderPayload?.AccessaryOrder[i].IdCategoryAccessary,
                        IdRepairOrders = rO_RepairOders?.Id,
                        Quantity = repairOrderPayload?.AccessaryOrder[i].Quantity,
                        Price = repairOrderPayload?.AccessaryOrder[i].Price,
                        DiscountPercent = repairOrderPayload?.AccessaryOrder[i].DiscountPercent,
                        DiscountPrice = repairOrderPayload?.AccessaryOrder[i].DiscountPrice,
                        TotalMoney = repairOrderPayload?.AccessaryOrder[i].TotalMoney,
                        CreatedDate = DateTime.Now,
                    });

                    var checkInventory = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryGetByIdAccessary, new { Id = repairOrderPayload?.AccessaryOrder[i].IdCategoryAccessary });
                    if (checkInventory.Inventory < repairOrderPayload?.AccessaryOrder[i].Quantity)
                    {
                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = $"Số lượng phụ tùng '{checkInventory.Name}' trong kho không đủ !"
                        };
                    }
                   //data[0].TotalMoney += repairOrderPayload?.AccessaryOrder[i].TotalMoney;
                }
                var tableRepairOrders_Accessary = ConvertToDataTable(repairOrders_Accessaries);
                #endregion

                //prepare param to insert Debt
                #region Debt
                //map data from dtos to entity Debt
                var debt = new Debt()
                {
                    Id = Guid.NewGuid(),
                    DebtContent = null,
                    Deposit = repairOrderPayload?.Deposit,
                    TotalPay = null,
                    DebtNumber = null,
                    LastPay = false,
                    IdRepairOrders = (Guid)rO_RepairOders.Id,
                    CreatedDate = DateTime.Now,
                    Status = 0
                };

                var debts = new List<Debt>
                {
                    debt
                };
                var tableDebt = ConvertToDataTable(debts);
                #endregion

                //call store procedure
                var tableRO_RepairOders = ConvertToDataTable(data);

                var temp = await connection.ExecuteAsync(
                sql: QueryRO_RepairOders.queryGM_SP_UpdateRO_RepairOders,
                param: new
                {
                    RO_RepairOders = tableRO_RepairOders.AsTableValuedParameter("dbo.RO_RepairOders_TableType"),
                    RepairOrders_Employee = tableRepairOrders_Employee.AsTableValuedParameter("dbo.RepairOrders_Employee_DataTable"),
                    RepairOrders_Accessary = tableRepairOrders_Accessary.AsTableValuedParameter("dbo.RepairOrders_Accessary_DataTable"),
                    OrtherCost = tableOrtherCost.AsTableValuedParameter("dbo.OrtherCost_DataTable"),
                    RequestList = tableRequestList.AsTableValuedParameter("dbo.RequestList_DataTable"),
                    Debt = tableDebt.AsTableValuedParameter("dbo.Debt_DataTable"),
                    //Accessary = tableAccessary.AsTableValuedParameter("[dbo].[Accessary_DataTable]"),
                    IdUser = IdUserCurrent
                },
                commandType: CommandType.StoredProcedure
                );

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Cập nhật thành công !"
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<TemplateApi> InsertRO_RepairOders(RepairOrderPayload repairOrderPayload, Guid IdUserCurrent)
        {
            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();

            try
            {
                //prepare param to insert RO_RepairOders
                #region RO_RepairOders
                //map data from dtos to entity RO_RepairOders
                var rO_RepairOders = repairOrderPayload.Adapt<RO_RepairOders>();
                // define some col with data concrete
                rO_RepairOders.Id = Guid.NewGuid();
                rO_RepairOders.CreatedDate = DateTime.Now;
                rO_RepairOders.RO_RepairOdersCode = "RO - " + DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
                rO_RepairOders.TotalMoney = repairOrderPayload.TotalMoney;
                rO_RepairOders.Status = 0;

                var data = new List<RO_RepairOders>
                {
                    rO_RepairOders
                };
                #endregion

                //prepare param to insert OrtherCost
                #region OrtherCost
                var ortherCostDatas = new List<OrtherCost>();

                for (int i = 0; i < repairOrderPayload?.OrtherCost?.Count; i++)
                {
                    var ortherCostData = new OrtherCost()
                    {
                        Id = Guid.NewGuid(),
                        IdRepairOders = rO_RepairOders.Id,
                        CostContent = repairOrderPayload?.OrtherCost[i].CostContent,
                        Quantity = repairOrderPayload?.OrtherCost[i].Quantity,
                        Price = repairOrderPayload?.OrtherCost[i].Price,
                        TotalMoney = repairOrderPayload?.OrtherCost[i].TotalMoney,
                        CreatedDate = DateTime.Now,
                        Status = 0
                    };
                    ortherCostDatas.Add(ortherCostData);

                    //data[0].TotalMoney += repairOrderPayload?.OrtherCost[i].TotalMoney;
                }
                var tableOrtherCost = ConvertToDataTable(ortherCostDatas);
                #endregion

                //prepare param to insert RequestList
                #region RequestList
                var requestDatas = new List<RequestList>();

                for (int i = 0; i < repairOrderPayload?.RequestList?.Count; i++)
                {
                    var requestData = new RequestList()
                    {
                        Id = Guid.NewGuid(),
                        Id_RO_RepairOrders = rO_RepairOders.Id,
                        RequestContent = repairOrderPayload?.RequestList[i].RequestContent,
                        IsProcessing = true,
                        IsCompleted = false,
                        IsCanceled = false,
                        CreatedDate = DateTime.Now,
                        Status = 0,
                        Note = repairOrderPayload?.RequestList[i].Note
                    };
                    requestDatas.Add(requestData);
                }
                var tableRequestList = ConvertToDataTable(requestDatas);
                #endregion

                //prepare param to insert RepairOrders_Employee
                #region RepairOrders_Employee
                var repairOrders_Employees = new List<RepairOrders_Employee>();

                for (int i = 0; i < repairOrderPayload?.RepairOrderEmployee?.Count; i++)
                {
                    var repairOrders_Employee = new RepairOrders_Employee()
                    {
                        Id = Guid.NewGuid(),
                        IdRepairOrders = rO_RepairOders.Id,
                        IdEmployee = repairOrderPayload.RepairOrderEmployee[i].IdEmployee,
                        Note = repairOrderPayload.RepairOrderEmployee[i].Note,
                        CreatedDate = DateTime.Now,
                    };
                    repairOrders_Employees.Add(repairOrders_Employee);
                }
                var tableRepairOrders_Employee = ConvertToDataTable(repairOrders_Employees);
                #endregion

                //prepare param to insert RepairOrders_Accessary
                #region RepairOrders_Accessary
                var repairOrders_Accessaries = new List<RepairOrders_Accessary>();
                var accessarys = new List<Accessary>();

                for (int i = 0; i < repairOrderPayload?.AccessaryOrder?.Count; i++)
                {
                    var repairOrders_Accessary = new RepairOrders_Accessary()
                    {
                        Id = Guid.NewGuid(),
                        IdCategoryAccessary = repairOrderPayload?.AccessaryOrder[i].IdCategoryAccessary,
                        IdRepairOrders = rO_RepairOders?.Id,
                        Quantity = repairOrderPayload?.AccessaryOrder[i].Quantity,
                        Price = repairOrderPayload?.AccessaryOrder[i].Price,
                        DiscountPercent = repairOrderPayload?.AccessaryOrder[i].DiscountPercent,
                        DiscountPrice = repairOrderPayload?.AccessaryOrder[i].DiscountPrice,
                        TotalMoney = repairOrderPayload?.AccessaryOrder[i].TotalMoney,
                        CreatedDate = DateTime.Now,
                    };
                    repairOrders_Accessaries.Add(repairOrders_Accessary);

                    var checkInventory = await connection.QueryFirstOrDefaultAsync<Accessary>(QueryAccessary.queryGetByIdAccessary, new { Id = repairOrderPayload?.AccessaryOrder[i].IdCategoryAccessary });
                    if(checkInventory.Inventory < repairOrderPayload?.AccessaryOrder[i].Quantity)
                    {
                        return new TemplateApi()
                        {
                            Success = false,
                            Fail = true,
                            Message = $"Số lượng phụ tùng '{checkInventory.Name}' trong kho không đủ !"
                        };
                    }

                    accessarys.Add(new Accessary()
                    {
                        Id = (Guid)repairOrderPayload?.AccessaryOrder[i].IdCategoryAccessary,
                        Inventory = repairOrderPayload?.AccessaryOrder[i].Quantity,
                    });

                    ///data[0].TotalMoney += repairOrderPayload?.AccessaryOrder[i].TotalMoney;
                }
                var tableRepairOrders_Accessary = ConvertToDataTable(repairOrders_Accessaries);
                var tableAccessary = ConvertToDataTable(accessarys);
                #endregion

                //prepare param to insert Debt
                #region Debt
                //map data from dtos to entity Debt
                var debt = new Debt()
                {
                    Id = Guid.NewGuid(),
                    DebtContent = null,
                    Deposit = repairOrderPayload?.Deposit,
                    TotalPay = null,
                    DebtNumber = null,
                    LastPay = false,
                    IdRepairOrders = (Guid)rO_RepairOders.Id,
                    CreatedDate = DateTime.Now,
                    Status = 0
                };

                var debts = new List<Debt>
                {
                    debt
                };
                var tableDebt = ConvertToDataTable(debts);
                #endregion

                var tableRO_RepairOders = ConvertToDataTable(data);
                //call store procedure
                await connection.ExecuteAsync(
                sql: QueryRO_RepairOders.queryGM_SP_InsertRO_RepairOders,
                param: new
                {
                    RO_RepairOders = tableRO_RepairOders.AsTableValuedParameter("dbo.RO_RepairOders_TableType"),
                    RepairOrders_Employee = tableRepairOrders_Employee.AsTableValuedParameter("dbo.RepairOrders_Employee_DataTable"),
                    RepairOrders_Accessary = tableRepairOrders_Accessary.AsTableValuedParameter("dbo.RepairOrders_Accessary_DataTable"),
                    OrtherCost = tableOrtherCost.AsTableValuedParameter("dbo.OrtherCost_DataTable"),
                    RequestList = tableRequestList.AsTableValuedParameter("dbo.RequestList_DataTable"),
                    Debt = tableDebt.AsTableValuedParameter("dbo.Debt_DataTable"),
                    Accessary = tableAccessary.AsTableValuedParameter("[dbo].[Accessary_DataTable]"),
                    IdUser = IdUserCurrent
                },
                commandType: CommandType.StoredProcedure
                );

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Thêm mới thành công !"
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<TemplateApi> GetAllRO_RepairOdersByIdCustomer(int pageNumber, int pageSize, Guid CustomerId)
        {
            using var connection = _DbContext.CreateConnection();
            var RO_RepairOderss = await connection.QueryAsync<RO_RepairOders>(QueryRO_RepairOders.queryGetAllRO_RepairOdersByIdCustomer, new { IdCustomer = CustomerId});
            int coutRecord = RO_RepairOderss.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                RO_RepairOderss = RO_RepairOderss.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var repairOders = RO_RepairOderss.Adapt<IEnumerable<RO_RepairOdersDto>>().ToList();
            for (int i = 0; i < repairOders.Count(); i++)
            {
                var debts = await connection.QueryAsync<Debt>(QueryRO_RepairOders.queryGetAllDebtIdRepairOrders, new { IdRepairOrders = repairOders[i].Id });
                if (debts.Count() > 1) { repairOders[i].IsPaid = true; }
            }
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            return new TemplateApi()
            {
                ListPayload = repairOders.ToArray(),
                Message = "Lấy danh sách thành công",
                Success = true,
                Fail = false,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElement = coutRecord,
                TotalPages = coutRecord / NumPagesize,
            };
        }
        public async Task<TemplateApi> PayRepairOders(float totalMoneys, Guid IdRepairOrder, Guid IdUserCurrent, bool continueDeposit)
        {
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var diaries = new List<Diary>();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(QueryRO_RepairOders.queryGetUserById, new { Id = IdUserCurrent }, tran);
                var saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                var RO_RepairOders = await connection.QueryFirstOrDefaultAsync<RO_RepairOders>(QueryRO_RepairOders.queryGetByIdRO_RepairOders, new { Id = IdRepairOrder },tran);
                var debt = await connection.QueryFirstOrDefaultAsync<Debt>(QueryRO_RepairOders.queryGetByIdDebt, new { IdRepairOrders = IdRepairOrder }, tran);
                
                if (RO_RepairOders is null || debt is null)
                {
                    return new TemplateApi()
                    {
                        Message = "Không tìm thấy kết quả",
                        Success = false,
                        Fail = true,
                    };
                }
                if (debt.DebtNumber is not null && totalMoneys > debt.DebtNumber)
                {
                    return new TemplateApi()
                    {
                        Message = "Số tiền trả không lớn hơn số tiền còn nợ !",
                        Success = false,
                        Fail = true,
                    };
                }

                if (RO_RepairOders.TotalMoney <= debt.Deposit || RO_RepairOders.TotalMoney <= debt.Deposit + totalMoneys)
                {
                    if (RO_RepairOders.TotalMoney <= debt.Deposit && totalMoneys != 0)
                    {
                        return new TemplateApi()
                        {
                            Message = "Số tiền trả không lớn hơn số tiền còn nợ !",
                            Success = false,
                            Fail = true,
                        };
                    }
                    if (RO_RepairOders.TotalMoney < debt.Deposit + totalMoneys && totalMoneys != 0)
                    {
                        return new TemplateApi()
                        {
                            Message = "Số tiền trả không lớn hơn số tiền còn nợ !",
                            Success = false,
                            Fail = true,
                        };
                    }
                    var debtData = new Debt
                    {
                        Id = Guid.NewGuid(),
                        DebtContent = null,
                        Deposit = 0,
                        TotalPay = RO_RepairOders.TotalMoney <= debt.Deposit ? RO_RepairOders.TotalMoney : totalMoneys,
                        DebtNumber = 0,
                        LastPay = true,
                        CreatedDate = DateTime.Now,
                        Status = 0,
                        IdRepairOrders = IdRepairOrder,
                    };

                    await connection.ExecuteAsync(QueryDebt.queryInsertDebt, debtData, tran);
                    // save to table diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, debtData.Id, "Create", "Debt", ""));

                    if(continueDeposit)
                    {
                        var depositAfterPay = new DepositAfterPay
                        {
                            Id = Guid.NewGuid(),
                            IdRepairOders = IdRepairOrder,
                            IdCustomer = (Guid)RO_RepairOders.IdCustomer,
                            IdDebt = debtData.Id,
                            TotalMoney = (float)debt.Deposit - (float)RO_RepairOders.TotalMoney,
                            CreatedDate = DateTime.Now,
                            Status = 0,
                            Note = null
                        };
                        await connection.ExecuteAsync(QueryRO_RepairOders.queryInsertDepositAfterPay, depositAfterPay, tran);
                        // save to table diary if success
                        diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, depositAfterPay.Id, "Create", "DepositAfterPay", "Tiếp tục gửi tiền kí gửi"));
                    }
                }
                else
                {
                    var dataDebt = new Debt();
                    dataDebt.Id = Guid.NewGuid();
                    dataDebt.DebtContent = null;
                    dataDebt.Deposit = 0;
                    dataDebt.TotalPay = totalMoneys;
                    dataDebt.DebtNumber = debt.DebtNumber is null ? RO_RepairOders.TotalMoney - (debt.Deposit + totalMoneys) : debt.DebtNumber - totalMoneys;
                    dataDebt.LastPay = debt.DebtNumber is null ? false : debt.DebtNumber - totalMoneys == 0 ? true : false;
                    dataDebt.CreatedDate = DateTime.Now;
                    dataDebt.Status = 0;
                    dataDebt.IdRepairOrders = IdRepairOrder;

                    await connection.ExecuteAsync(QueryDebt.queryInsertDebt, dataDebt, tran);
                    // save to table diary if success
                    diaries.Add(saveToDiary.ConvertDriaryToListDiary(saveDiaryModel, dataDebt.Id, "Create", "Debt", ""));
                }

                //save to table diary
                for (int i = 0; i < diaries.Count; i++)
                {
                    await connection.ExecuteAsync(QueryRO_RepairOders.queryInsertDiary, diaries[i], tran);
                }
                tran.Commit();

                return new TemplateApi()
                {
                    Success = true,
                    Fail = false,
                    Message = "Thanh toán thành công !"
                };
            }
            catch (Exception)
            {
                // roll the transaction back
                tran.Rollback();
                throw;
            }
        }
        #endregion

        #region Method convert class to datatable
        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        #endregion
    }
    internal static class QueryRO_RepairOders
    {
        public const string queryInsertRO_RepairOders = @"INSERT INTO [dbo].[RO_RepairOders]
                                               ([Id]
                                               ,[RO_RepairOdersCode]
                                               ,[IdCustomer]
                                               ,[IdVehicle]
                                               ,[Kilometer]
                                               ,[TaxPercent]
                                               ,[TotalMoney]
                                               ,[CreatedDate]
                                               ,[Status])
                   VALUES (@Id, @RO_RepairOdersCode, @IdCustomer, @IdVehicle, @Kilometer, @TaxPercent, @TotalMoney, @CreatedDate, @Status)";
        public const string queryInsertDepositAfterPay = @"INSERT INTO [dbo].[DepositAfterPay]
                                   ([Id]
                                   ,[IdRepairOders]
                                   ,[IdCustomer]
                                   ,[Status]
                                   ,[CreatedDate]
                                   ,[TotalMoney]
                                   ,[Note]
                                   ,[IdDebt])
                   VALUES (@Id, @IdRepairOders, @IdCustomer, @Status, @CreatedDate, @TotalMoney, @Note, @IdDebt)";

        public const string queryUpdateRO_RepairOders = @"UPDATE [dbo].[RO_RepairOders] SET
                                                                        IdCustomer = @IdCustomer,
                                                                        IdVehicle = @IdVehicle,
                                                                        Kilometer = @Kilometer,
                                                                        TaxPercent = @TaxPercent
                                                                        WHERE Id = @Id";
        public const string queryDeleteRO_RepairOders = "DELETE FROM [dbo].[RO_RepairOders] WHERE Id = @Id";
        public const string queryGetByIdRO_RepairOders = "select * from [dbo].[RO_RepairOders] where Id = @Id";
        public const string queryGetByIdDebt = @"select top 1 * from Debt where IdRepairOrders = @IdRepairOrders
                                                order by CreatedDate desc";
        public const string queryGetAllRO_RepairOdersIsPaid = @"select ro.*
                                                                from [dbo].[RO_RepairOders] ro right join Debt d on d.IdRepairOrders = ro.Id
                                                                where d.LastPay = 1
                                                                order by ro.CreatedDate desc";
        public const string queryGetAllRO_RepairOdersNotPaid = @"select * from RO_RepairOders 
                                                                where Id not in (
                                                                select ro.Id
                                                                from [dbo].[RO_RepairOders] ro right join Debt d on d.IdRepairOrders = ro.Id
                                                                where d.LastPay = 1)
                                                                order by CreatedDate desc";
        public const string queryGetAllRequestListById_RO_RepairOrders = "select * from RequestList where Id_RO_RepairOrders = @Id_RO_RepairOrders";
        public const string queryGetAllRO_RepairOdersByIdCustomer = "select *from [dbo].[RO_RepairOders] where IdCustomer = @IdCustomer order by CreatedDate desc";
        public const string queryInsertRO_RepairOders_Deleted = @"INSERT INTO [dbo].[Delete_RO_RepairOders]
                                               ([Id]
                                               ,[RO_RepairOdersCode]
                                               ,[IdCustomer]
                                               ,[IdVehicle]
                                               ,[Kilometer]
                                               ,[TaxPercent]
                                               ,[TotalMoney]
                                               ,[CreatedDate]
                                               ,[Status])
                   VALUES (@Id, @RO_RepairOdersCode, @IdCustomer, @IdVehicle, @Kilometer, @TaxPercent, @TotalMoney, @CreatedDate, @Status)";
        public const string queryGetAllIdRO_RepairOders = "select Id from [dbo].[RO_RepairOders]";
        public const string queryInsertRepairOrders_Employee = @"INSERT INTO [dbo].[RepairOrders_Employee]
                                                                   ([Id]
                                                                   ,[IdRepairOrders]
                                                                   ,[IdEmployee]
                                                                   ,[CreatedDate])
                                                                VALUES (@Id, @IdRepairOrders, @IdEmployee, @CreatedDate)";
        public const string queryDeleteRepairOrders_Employee = "DELETE FROM [dbo].[RepairOrders_Employee] WHERE Id = @Id";
        public const string queryUpdateRepairOrders_Employee = @"UPDATE [dbo].[RepairOrders_Employee] SET
                                                                        IdEmployee = @IdEmployee,
                                                                        Note = @Note
                                                                        WHERE Id = @Id";
        public const string queryInsertRepairOrders_Accessary = @"INSERT INTO [dbo].[RepairOrders_Accessary]
                                                           ([Id]
                                                           ,[IdCategoryAccessary]
                                                           ,[IdRepairOrders]
                                                           ,[Quantity]
                                                           ,[Price]
                                                           ,[DiscountPercent]
                                                           ,[DiscountPrice]
                                                           ,[TotalMoney]
                                                           ,[CreatedDate])
              VALUES (@Id, @IdCategoryAccessary, @IdRepairOrders, @Quantity, @Price, @DiscountPercent, @DiscountPrice, @TotalMoney, @CreatedDate)";
        public const string queryDeleteRepairOrders_Accessary = "DELETE FROM [dbo].[RepairOrders_Accessary] WHERE Id = @Id";
        public const string queryUpdateRepairOrders_Accessary = @"UPDATE [dbo].[RepairOrders_Accessary] SET
                                                                        IdCategoryAccessary = @IdCategoryAccessary,
                                                                        Quantity = @Quantity,
                                                                        Price = @Price,         
                                                                        DiscountPercent = @DiscountPercent,
                                                                        DiscountPrice = @DiscountPrice,
                                                                        TotalMoney = @TotalMoney
                                                                        WHERE Id = @Id";
        public const string queryGetAllRepairOrders_EmployeeByIdRO_RepairOders = "select * from [dbo].[RepairOrders_Employee] where IdRepairOrders = @IdRepairOrders";
        public const string queryGetAllRepairOrders_AccessaryIdRepairOrders = "select * from [dbo].[RepairOrders_Accessary] where IdRepairOrders = @IdRepairOrders";

        public const string queryGetAllDebtIdRepairOrders = "select * from [dbo].[Debt] where IdRepairOrders = @IdRepairOrders order by CreatedDate desc";
        public const string queryGetAllRequestListIdRepairOrders = "select * from [dbo].[RequestList] where Id_RO_RepairOrders = @Id_RO_RepairOrders";
        public const string queryGetAllOrtherCostIdRepairOrders = "select * from [dbo].[OrtherCost] where IdRepairOders = @IdRepairOders";

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

        //call store procedure
        public const string queryGM_SP_InsertRO_RepairOders = "[dbo].[GM_SP_InsertRO_RepairOders]";
        public const string queryGM_SP_UpdateRO_RepairOders = "[dbo].[GM_SP_UpdateRO_RepairOders]";
    }
}
