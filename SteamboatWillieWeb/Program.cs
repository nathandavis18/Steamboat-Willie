using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
var connectionString = builder.Configuration.GetConnectionString("LocalConnection") ?? throw new InvalidOperationException("Connection string 'SmarterASP' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<AppUser, IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    IConfigurationSection googleAuthSection = builder.Configuration.GetSection("Authentication:Google");
    googleOptions.ClientId = googleAuthSection["ClientId"];
    googleOptions.ClientSecret = googleAuthSection["ClientSecret"];
});

builder.Services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
{
    IConfigurationSection microsoftAuthSection = builder.Configuration.GetSection("Authentication:Microsoft");
    microsoftOptions.ClientId = microsoftAuthSection["ClientId"];
    microsoftOptions.ClientSecret = microsoftAuthSection["ClientSecret"];
});

builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<DbInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=User}/{controller=Home}/{action=Index}/{id?}"
);

SeedDatabase();

app.Run();


void SeedDatabase()
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.Initialize();
}
