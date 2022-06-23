using TeamMessenger.Server.Data;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (databaseUrl != null)
{
    // Получение адреса базы данных PostgreSQL из ENV-переменной, проставляемой Heroku, если она есть
    databaseUrl = databaseUrl.Replace("postgres://", "");
    string[] databaseCred = databaseUrl.Split(new char[] { '@', '/', ':' });
    string databaseCredString = $"Host={databaseCred[2]};Port={databaseCred[3]};User ID={databaseCred[0]};Password={databaseCred[1]};Database={databaseCred[4]};Pooling=true;TrustServerCertificate=True";
    builder.Services.AddDbContext<MessagesContext>(options =>
                    options.UseNpgsql(databaseCredString));
}
else
{
    builder.Services.AddDbContext<MessagesContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MessagesContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapHub<TeamMessenger.Server.Hubs.ChatHub>(TeamMessenger.Shared.Chat.HubUrl);
app.MapFallbackToFile("index.html");

app.Run();
