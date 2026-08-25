using Microsoft.EntityFrameworkCore;
using StudentMigration.Postgres;

var builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration["Postgres:ConnectionString"] ?? "Host=localhost;Port=5432;Database=campustrack;Username=campustrack;Password=campustrack";
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs));
builder.Services.AddScoped<StudentRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();

app.MapGet("/api/students/{id:guid}", async (Guid id, StudentRepository repo, CancellationToken ct) => await repo.GetByIdAsync(id, ct) is { } s ? Results.Ok(s) : Results.NotFound());
app.MapGet("/api/students", async (string? search, int skip, int take, StudentRepository repo, CancellationToken ct) => Results.Ok(await repo.SearchAsync(search, Math.Max(0, skip), Math.Clamp(take == 0 ? 50 : take, 1, 500), ct)));
app.MapPost("/api/students", async (Student student, StudentRepository repo, CancellationToken ct) => { if (student.Id == Guid.Empty) student.Id = Guid.NewGuid(); await repo.AddAsync(student, ct); return Results.Created($"/api/students/{student.Id}", student); });
app.MapPut("/api/students/{id:guid}/disable", async (Guid id, StudentRepository repo, CancellationToken ct) => await ChangeStatus(id, StudentStatus.Disabled, repo, ct));
app.MapPut("/api/students/{id:guid}/enable", async (Guid id, StudentRepository repo, CancellationToken ct) => await ChangeStatus(id, StudentStatus.Active, repo, ct));
app.Run();
static async Task<IResult> ChangeStatus(Guid id, StudentStatus status, StudentRepository repo, CancellationToken ct) { var s = await repo.GetByIdAsync(id, ct); if (s is null) return Results.NotFound(); s.Status = status; await repo.SaveAsync(s, ct); return Results.Ok(s); }
