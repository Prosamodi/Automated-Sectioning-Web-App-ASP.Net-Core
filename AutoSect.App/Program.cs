using AutoSect.App.Models;
using AutoSect.App.Models.ViewModels;
using AutoSect.App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AutoSectDbContext>(opts =>
{
    opts.UseSqlServer(
        builder.Configuration["ConnectionStrings:AutoSectConnection"]);
    opts.EnableSensitiveDataLogging(false);
});

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:IdentityConnection"]));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<AppIdentityDbContext>();

builder.Services.AddAuthentication()
    .AddGoogle(opts =>
    {
        opts.ClientId = builder.Configuration["GoogleService:ClientId"];
        opts.ClientSecret = builder.Configuration["GoogleService:ClientSecret"];

    });

builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddScoped<IStudentRepository, EFStudentRepository>();
builder.Services.AddScoped<IProjectSettingRepository, EFProjectSettingsRepository>();

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<StudentsService>();
builder.Services.AddScoped<AutomationService>();
builder.Services.AddScoped<ExportExcelService>();

builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromDays(1);
    opts.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequireUppercase = false;
});


var app = builder.Build();

//Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();
app.MapRazorPages();


IdentitySeedData.EnsurePopulated(app);
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<AutoSectDbContext>();
SeedStudent.EnsurePopulated(context);

app.Run();
