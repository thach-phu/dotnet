using MVC.Data;
using MVC.Models;
using MVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddDbContext<StoreDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Store")
        ?? throw new InvalidOperationException("Connection string 'Store' is not configured.");
    options.UseMySQL(connectionString);
});
builder.Services.AddScoped<ProductCatalog>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SessionCart>();

// ASP.NET Core Identity quản lý User/Role (bảng AspNetUsers/AspNetRoles...),
// còn Permission/RolePermission vẫn là 2 bảng tự viết để phân quyền chi tiết
// theo từng chức năng (AppClaimsPrincipalFactory gắn thêm claim "permission").
builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<StoreDbContext>()
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/login";
    options.Events.OnRedirectToLogin = context =>
    {
        if (!context.Request.Path.StartsWithSegments("/Admin"))
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect($"{context.Request.PathBase}/admin/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (!context.Request.Path.StartsWithSegments("/Admin"))
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
        context.Response.Redirect($"{context.Request.PathBase}/admin/login");
        return Task.CompletedTask;
    };
});

var authorization = builder.Services.AddAuthorizationBuilder();
foreach (var code in Permissions.All)
    authorization.AddPolicy(code, policy => policy.RequireClaim(Permissions.ClaimType, code));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    db.Database.Migrate();
    await DataSeeder.SeedAccountsAsync(db, userManager, roleManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
