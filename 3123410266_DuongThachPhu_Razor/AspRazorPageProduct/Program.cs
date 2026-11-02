using AspRazorPageProduct.Data;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Mỗi khu vực Admin được bảo vệ bằng đúng Permission cần cho khu vực đó
    // (phân quyền chi tiết theo chức năng, giống Blazor/MVC) thay vì 1 cờ
    // Admin/Không-Admin chung cho cả thư mục /Admin.
    options.Conventions.AuthorizePage("/Admin/Dashboard/Index");
    options.Conventions.AuthorizeFolder("/Admin/Products", Permissions.ProductsManage);
    options.Conventions.AuthorizeFolder("/Admin/Categories", Permissions.CategoriesManage);
    options.Conventions.AuthorizeFolder("/Admin/Orders", Permissions.OrdersManage);
    options.Conventions.AuthorizeFolder("/Admin/Users", Permissions.UsersManage);
    options.Conventions.AuthorizeFolder("/Admin/Customers", Permissions.CustomersManage);
    options.Conventions.AuthorizePage("/Checkout");
    options.Conventions.AuthorizePage("/OrderHistory");
});

var authorizationBuilder = builder.Services.AddAuthorizationBuilder();
foreach (var code in Permissions.All)
{
    authorizationBuilder.AddPolicy(code, policy => policy.RequireClaim(Permissions.ClaimType, code));
}

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Add HttpContextAccessor & Session services
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Kết nối MySQL qua Entity Framework Core (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register custom services (Scoped vì dùng chung AppDbContext theo request)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

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
    .AddEntityFrameworkStores<AppDbContext>()
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "TechStore.Auth";
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

var app = builder.Build();

// Tự động áp dụng migration vào MySQL khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    db.Database.Migrate();
    await DataSeeder.SeedAccountsAsync(db, userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
