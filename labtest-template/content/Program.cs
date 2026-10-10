using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TemplateProject.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<TemplateProject.Services.SampleItemRepository>();

var configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
var connection = new SqliteConnectionStringBuilder(configuredConnection);
if (!Path.IsPathRooted(connection.DataSource))
    connection.DataSource = Path.Combine(builder.Environment.ContentRootPath, connection.DataSource);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connection.ConnectionString));

var stateMode = builder.Configuration["StateMode"];
if (stateMode is not ("Cookie" or "Session"))
    throw new InvalidOperationException("StateMode must be Cookie or Session.");

if (stateMode == "Session")
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = ".TemplateProject.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.IdleTimeout = TimeSpan.FromMinutes(30);
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
if (stateMode == "Session")
    app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
