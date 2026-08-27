using Ct.Stu.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure: EF Core, PostgreSQL and application services
builder.Services.AddStuInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/api/error");

app.UseHttpsRedirection();

app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();
