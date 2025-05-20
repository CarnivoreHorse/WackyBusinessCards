using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WackyBusinessCards.Constants;
using WackyBusinessCards.Data;
using WackyBusinessCards.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole(Roles.User, Roles.Admin));
});

// Configure cookie policy
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Register services
builder.Services.AddScoped<WackyBusinessCards.Services.BusinessCardService>();
builder.Services.AddScoped<WackyBusinessCards.Services.FileUploadService>();
builder.Services.AddScoped<WackyBusinessCards.Services.ActivityLogService>();
builder.Services.AddScoped<WackyBusinessCards.Services.UserManagementService>();
builder.Services.AddScoped<WackyBusinessCards.Services.StatisticsService>();
builder.Services.AddScoped<WackyBusinessCards.Services.AdminInitializationService>();
builder.Services.AddScoped<WackyBusinessCards.Services.EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Add Identity endpoints
app.MapRazorPages();

// Initialize admin user
using (var scope = app.Services.CreateScope())
{
    var adminInitService = scope.ServiceProvider.GetRequiredService<WackyBusinessCards.Services.AdminInitializationService>();
    await adminInitService.InitializeAsync();
}


app.Run();
