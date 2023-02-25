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
    public class UnitRepository : IUnitRepository
    {
        #region Variables
        public DataContext _DbContext;
        #endregion

        #region Constructors
        public UnitRepository(DataContext DbContext)
        {
            _DbContext = DbContext;
        }
        #endregion

        #region METHOD
        public async Task<TemplateApi> GetAllUnit(int pageNumber, int pageSize)
        {
            string query = "select *from [dbo].[Unit] order by CreatedDate desc";
            using var connection = _DbContext.CreateConnection();
            IEnumerable<Unit> units = await connection.QueryAsync<Unit>(query);
            int coutRecord = units.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                units = units.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<UnitDto> unitDtos = units.Adapt<IEnumerable<UnitDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = unitDtos.ToArray(),
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
        public async Task<TemplateApi> GetUnitById(Guid IdUnit)
        {
            var query = "select *from [dbo].[Unit] where Id = @Id";
            using var con = _DbContext.CreateConnection();
            Unit unit = await con.QueryFirstOrDefaultAsync<Unit>(query, new { Id = IdUnit });

            if (unit != null)
            {
                UnitDto unitDto = unit.Adapt<UnitDto>();

                var templateApi = new TemplateApi()
                {
                    Payload = unitDto,
                    Message = "Lấy đơn vị thành công",
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
                    Message = "Không tìm thấy đơn vị",
                    Success = false,
                    Fail = true,
                };
                return templateApi;
            }
        }
        public async Task<TemplateApi> DeleteUnit(Guid IdUnit, Guid IdUserCurrent)
        {
            var query = "DELETE FROM [dbo].[Unit] WHERE Id IN @Ids";
            var queryUserById = "select Id, Fullname from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
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
            var queryUnitParentAndChill = @"WITH ret AS(
                                            SELECT  *
                                            FROM    Unit
                                            WHERE   ID = @ID
                                            UNION ALL
                                            SELECT  t.*
                                            FROM    Unit t INNER JOIN
                                                    ret r ON t.ParentId = r.ID
                                    )
                                    SELECT  Id
                                    FROM ret";
            var queryCheckUnitIsUsed = "select Id from [dbo].[User] where UnitId = @UnitId and IsDeleted = @IsDeleted";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(queryUserById, new { Id = IdUserCurrent }, tran);
                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = IdUnit,
                    Operation = "Delete",
                    Table = "Unit",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                // check user exits in unit
                Guid user = connection.QueryFirstOrDefault<Guid>(queryCheckUnitIsUsed, new { UnitId = IdUnit, IsDeleted = false }, tran);
                if (user != Guid.Empty)
                {
                    //save to diary if false    
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Đơn vị này đã có người dùng không thể xóa !"
                    };
                    return response;
                }

                //query table unit
                IEnumerable<Guid> unitsGuidId = await connection.QueryAsync<Guid>(queryUnitParentAndChill, new { ID = IdUnit }, tran);

                if (!unitsGuidId.Any())
                {
                    //save to diary if false    
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }
                await connection.ExecuteAsync(query, new { Ids = unitsGuidId }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

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
        public async Task<TemplateApi> InsertUnit(UnitDto unitDto)
        {
            //block query string
            var query = @"INSERT INTO [dbo].[Unit]
                               ([Id]
                               ,[UnitName]
                               ,[ParentId]
                               ,[Status]
                               ,[CreatedBy]
                               ,[CreatedDate]
                               ,[UnitCode]
                               ,[IsHide])
                         VALUES (@Id, @UnitName, @ParentId, @Status, @CreatedBy, @CreatedDate, @UnitCode, @IsHide)";
            var queryById = "select *from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
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

            //create connection
            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var unit = new Unit();
                unit = unitDto.Adapt<Unit>();

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryById, new { Id = unitDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = unitDto.Id,
                    Operation = "Create",
                    Table = "Unit",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                await connection.ExecuteAsync(query, unit, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

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
        public async Task<TemplateApi> UpdateUnit(UnitDto unitDto)
        {
            //block query string
            var query = "UPDATE Unit SET UnitName = @UnitName, UnitCode = @UnitCode, ParentId = @ParentId WHERE Id = @Id";
            var queryById = "select *from Unit where Id = @Id";
            var queryUserById = "select *from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
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

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                var unit = new Unit();
                unit = unitDto.Adapt<Unit>();

                //get unit by id
                Unit unitById = await connection.QueryFirstOrDefaultAsync<Unit>(queryById, new { unitDto.Id }, tran);

                //query user by iduserCurrentLogin
                User userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<User>(queryUserById, new { Id = unitDto.IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = unitDto.Id,
                    Operation = "Update",
                    Table = "Unit",
                    Content = "",
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                if (unitById == null)
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(query, unit, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

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
        public async Task<TemplateApi> HideUnit(Guid IdUnit, bool IsHide, Guid IdUserCurrent)
        {
            var query = "UPDATE Unit SET IsHide = @IsHide WHERE Id IN @Ids";
            var queryUserById = "select *from [dbo].[User] where Id = @Id";
            var querySaveDiary = @"INSERT INTO [dbo].[Diary]
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
            var queryUnitParentAndChill = @"WITH ret AS(
                                            SELECT  *
                                            FROM    Unit
                                            WHERE   ID = @ID
                                            UNION ALL
                                            SELECT  t.*
                                            FROM    Unit t INNER JOIN
                                                    ret r ON t.ParentId = r.ID
                                    )
                                    SELECT  Id
                                    FROM ret";
            var queryCheckUnitIsUsed = "select Id from [dbo].[User] where UnitId = @UnitId and IsDeleted = @IsDeleted";

            var content = IsHide ? "ẩn phòng ban" : "hiển thị phòng ban";

            using var connection = _DbContext.CreateConnection();
            connection.Open();
            using var tran = connection.BeginTransaction();

            var response = new TemplateApi();
            var saveToDiary = new SaveToDiary();
            var saveDiaryModel = new SaveDiaryModel();

            try
            {
                //query user by iduserCurrentLogin
                UserCurrentLoginModel userByIDCurrentLogin = await connection.QueryFirstOrDefaultAsync<UserCurrentLoginModel>(queryUserById, new { Id = IdUserCurrent }, tran);

                saveDiaryModel = new SaveDiaryModel()
                {
                    IdUserCurrent = userByIDCurrentLogin.Id,
                    IdWith = IdUnit,
                    Operation = "Update",
                    Table = "Unit",
                    Content = content,
                    Fullname = userByIDCurrentLogin.Fullname ?? "",
                    IsSuccess = true,
                };

                // check user exits in unit
                Guid user = connection.QueryFirstOrDefault<Guid>(queryCheckUnitIsUsed, new { UnitId = IdUnit, IsDeleted = false }, tran);
                if (user != Guid.Empty)
                {
                    //save to diary if false    
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();

                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Đơn vị này đã có người dùng không thể ẩn !"
                    };
                    return response;
                }

                //get UnitParentAndChill by id
                IEnumerable<Guid> unitsGuidId = await connection.QueryAsync<Guid>(queryUnitParentAndChill, new { ID = IdUnit }, tran);
                if (!unitsGuidId.Any())
                {
                    //save to diary if false
                    saveDiaryModel.IsSuccess = false;
                    Diary diaryFalse = saveToDiary.InsertDiary(saveDiaryModel);
                    await connection.ExecuteAsync(querySaveDiary, diaryFalse, tran);

                    tran.Commit();
                    response = new TemplateApi()
                    {
                        Success = false,
                        Fail = true,
                        Message = "Không tìm thấy kết quả !"
                    };
                    return response;
                }

                await connection.ExecuteAsync(query, new { Ids = unitsGuidId, IsHide }, tran);

                //save diary if success
                Diary diary = saveToDiary.InsertDiary(saveDiaryModel);
                await connection.ExecuteAsync(querySaveDiary, diary, tran);

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
        public async Task<TemplateApi> GetUnitNotHide(int pageNumber, int pageSize)
        {
            string query = "select * from Unit where IsHide = @IsHide order by CreatedDate desc";
            using var connection = _DbContext.CreateConnection();

            IEnumerable<Unit> units = await connection.QueryAsync<Unit>(query, new { IsHide = false });
            int coutRecord = units.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                units = units.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<UnitDto> unitDtos = units.Adapt<IEnumerable<UnitDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = unitDtos.ToArray(),
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
        public async Task<TemplateApi> GetAllUnitByIdParent(Guid IdUnit, int pageNumber, int pageSize)
        {
            string query = "select *from Unit where ParentId = @ParentId";
            using var connection = _DbContext.CreateConnection();

            IEnumerable<Unit> units = await connection.QueryAsync<Unit>(query, new { ParentId = IdUnit });
            int coutRecord = units.Count();

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                units = units.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            IEnumerable<UnitDto> unitDtos = units.Adapt<IEnumerable<UnitDto>>();
            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = unitDtos.ToArray(),
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
        public async Task<UnitDto> GetUnitByUnitCode(string UnitCode)
        {
            var query = "select *from Unit where UnitCode = @UnitCode";

            using var con = _DbContext.CreateConnection();
            Unit unit = await con.QueryFirstOrDefaultAsync<Unit>(query, new { UnitCode });
            var unitDto = new UnitDto();
            if (unit != null)
            {
                unitDto = unit.Adapt<UnitDto>();
                return unitDto;
            }
            return unitDto;
        }
        public async Task<TemplateApi> GetAllUnitAndUser(int pageNumber, int pageSize)
        {
            string queryTableUnit = "select * from Unit where IsHide = @IsHide order by CreatedDate desc";
            string query = "select u.Id, u.Fullname from [dbo].[User] u where u.UnitId = @UnitId";

            using var connection = _DbContext.CreateConnection();
            var getListUserByUnits = new List<GetListUserByUnit>();
            var units = await connection.QueryAsync<Unit>(queryTableUnit, new { IsHide = false });

            foreach (var item in units)
            {
                var lstUsers = await connection.QueryAsync<LstUser>(query, new { UnitId = item.Id });

                var getListUserByUnit = new GetListUserByUnit()
                {
                    NameUnit = item.UnitName,
                    LstUsers = lstUsers
                };
                getListUserByUnits.Add(getListUserByUnit);
            }
            int coutRecord = getListUserByUnits.Count;

            if (pageNumber != 0 && pageSize != 0)
            {
                if (pageNumber < 0) { pageNumber = 1; }
                getListUserByUnits = getListUserByUnits.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }

            var NumPagesize = pageSize == 0 ? 1 : pageSize;

            var templateApi = new TemplateApi()
            {
                ListPayload = getListUserByUnits.ToArray(),
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
        #endregion
    }
}
