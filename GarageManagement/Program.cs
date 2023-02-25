using GarageManagement.Data.Context;
using GarageManagement.Data.Entity;
using GarageManagement.Middleware;
using GarageManagement.Services.Common.Function;
using GarageManagement.Services.IRepository;
using GarageManagement.Services.Repository;
using GarageManagement.Utility;
using Serilog;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Register validator with service provider (or use one of the automatic registration methods)

// Add services to the container.
builder.Services.AddSingleton<DataContext>();
builder.Services.AddTransient<SaveToDiary>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUnitRepository, UnitRepository>();
builder.Services.AddTransient<ICategoryBrandVehicleRepository, CategoryBrandVehicleRepository>();
builder.Services.AddTransient<ICategoryCityRepository, CategoryCityRepository>();
builder.Services.AddTransient<ICategoryDistrictRepository, CategoryDistrictRepository>();
builder.Services.AddTransient<ICategoryGearBoxRepository, CategoryGearBoxRepository>();
builder.Services.AddTransient<ICategoryModelRepository, CategoryModelRepository>();
builder.Services.AddTransient<ICategoryTypeRepository, CategoryTypeRepository>();
builder.Services.AddTransient<ICategoryWardRepository, CategoryWardRepository>();
builder.Services.AddTransient<ICustomerGroupRepository, CustomerGroupRepository>();
builder.Services.AddTransient<IVehicleRepository, VehicleRepository>();
builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
builder.Services.AddTransient<IRO_RepairOdersRepository, RO_RepairOdersRepository>();
builder.Services.AddTransient<IRO_RepairOdersRepository, RO_RepairOdersRepository>();
builder.Services.AddTransient<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddTransient<IEmployeeGroupRepository, EmployeeGroupRepository>();
builder.Services.AddTransient<ICustomer_VehicleRepository, Customer_VehicleRepository>();
builder.Services.AddTransient<IAccessaryGroupRepository, AccessaryGroupRepository>();
builder.Services.AddTransient<IRequestListRepository, RequestListRepository>();
builder.Services.AddTransient<ICategorySupplierRepository, CategorySupplierRepository>();
builder.Services.AddTransient<IAccessaryRepository, AccessaryRepository>();
builder.Services.AddTransient<IImportExportAccessaryReceiptRepository, ImportExportAccessaryReceiptRepository>();
builder.Services.AddTransient<IOrtherCostRepository, OrtherCostRepository>();
builder.Services.AddTransient<IDebtRepository, DebtRepository>();
builder.Services.AddTransient<IStaticsRepository, StaticsRepository>();
builder.Services.AddTransient<IRepairScheduleRepository, RepairScheduleRepository>();
builder.Services.AddTransient<IHolidayRepository, HolidayRepository>();
builder.Services.AddTransient<IAllowanceRepository, AllowanceRepository>();
builder.Services.AddTransient<IEmployeeDayOffRepository, EmployeeDayOffRepository>();
builder.Services.AddTransient<IEmployee_Salary_HistoryRepository, Employee_Salary_HistoryRepository>();

//add cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
                          policy =>
                          {
                              policy.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          });
});

// Add services to the container.
var logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//get secret key from application.json
builder.Services.Configure<AppSettingModel>(
builder.Configuration.GetSection("AppSettings"));

builder.Services.Configure<ConnectionStringOptions>(builder.Configuration.GetSection(ConnectionStringOptions.Position));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//UseMiddleware
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
