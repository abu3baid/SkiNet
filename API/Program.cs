using API.Extensions;
using API.Helpers;
using API.Middleware;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(MappingProfiles));
builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApplicationServices();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = app.Services.GetService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<StoreContext>();
        
        await context.Database.MigrateAsync();
        await StoreContextSeed.SeedAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occured during migration");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();

app.UseRouting();
app.UseStaticFiles();

app.UseAuthorization();

app.UseSwaggerDocumentation();

app.MapControllers();

app.Run();
