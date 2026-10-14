using System.Reflection;
using Ct.Shared.Extensions;
using Ct.Fee;
using Ct.Acad;
using Ct.Arte;
using Ct.Staff;
using Ct.Scheduler;
using Ct.Asset;
using Ct.Accounting;
using Ct.Admission;
using Ct.Connect;
using Ct.Library;
using Ct.Stu;
using Ct.Gr;

var builder = WebApplication.CreateBuilder(args);

// Shared infrastructure
builder.Services.AddCtShared();

// Modular domain registrations
builder.Services.AddFeeModule(builder.Configuration);
builder.Services.AddAcadModule(builder.Configuration);
builder.Services.AddArteModule(builder.Configuration);
builder.Services.AddConnectModule(builder.Configuration);
builder.Services.AddLibraryModule(builder.Configuration);
builder.Services.AddStuModule(builder.Configuration);
builder.Services.AddStaffModule(builder.Configuration);
builder.Services.AddSchedulerModule(builder.Configuration);
builder.Services.AddAssetModule(builder.Configuration);
builder.Services.AddAccountingModule(builder.Configuration);
builder.Services.AddAdmissionModule(builder.Configuration);
builder.Services.AddGrModule(builder.Configuration);

// Controllers from all module assemblies
var moduleAssemblies = new[]
{
    typeof(Ct.Fee.FeeModule).Assembly,
    typeof(Ct.Acad.AcadModule).Assembly,
    typeof(Ct.Arte.ArteModule).Assembly,
    typeof(Ct.Connect.ConnectModule).Assembly,
    typeof(Ct.Library.LibraryModule).Assembly,
    typeof(Ct.Stu.StuModule).Assembly,
    typeof(Ct.Gr.GrModule).Assembly,
    typeof(Ct.Staff.StaffModule).Assembly,
    typeof(Ct.Scheduler.SchedulerModule).Assembly,
    typeof(Ct.Asset.AssetModule).Assembly,
    typeof(Ct.Accounting.AccountingModule).Assembly,
    typeof(Ct.Admission.AdmissionModule).Assembly,
};

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Ct.Fee.FeeModule).Assembly)
    .AddApplicationPart(typeof(Ct.Acad.AcadModule).Assembly)
    .AddApplicationPart(typeof(Ct.Arte.ArteModule).Assembly)
    .AddApplicationPart(typeof(Ct.Connect.ConnectModule).Assembly)
    .AddApplicationPart(typeof(Ct.Library.LibraryModule).Assembly)
    .AddApplicationPart(typeof(Ct.Stu.StuModule).Assembly)
    .AddApplicationPart(typeof(Ct.Gr.GrModule).Assembly)
    .AddApplicationPart(typeof(Ct.Staff.StaffModule).Assembly)
    .AddApplicationPart(typeof(Ct.Scheduler.SchedulerModule).Assembly)
    .AddApplicationPart(typeof(Ct.Asset.AssetModule).Assembly)
    .AddApplicationPart(typeof(Ct.Accounting.AccountingModule).Assembly)
    .AddApplicationPart(typeof(Ct.Admission.AdmissionModule).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "CT Modular Platform",
        Version = "v1",
        Description = "Combined modular .NET 10 + PostgreSQL application (Fee, Stu, Staff, Scheduler, Asset, Accounting, Admission, Acad, Arte, Connect, Library, Gr)"
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CT Modular v1"));
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.MapControllers();

app.MapFeeEndpoints();
app.MapAcadEndpoints();
app.MapArteEndpoints();
app.MapConnectEndpoints();
app.MapLibraryEndpoints();
app.MapStuEndpoints();
app.MapStaffEndpoints();
app.MapSchedulerEndpoints();
app.MapAssetEndpoints();
app.MapAccountingEndpoints();
app.MapAdmissionEndpoints();
app.MapGrEndpoints();

app.Run();

public partial class Program;
