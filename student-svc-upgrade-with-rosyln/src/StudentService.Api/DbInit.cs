using Microsoft.EntityFrameworkCore;
using StudentService.Api.Data;

namespace StudentService.Api;

public static class DbInit
{
    public static async Task InitializeAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
