using App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using App.Services;
using App.Data;
using Microsoft.AspNetCore.Builder;
using App.ExtendMethods;

var builder = WebApplication.CreateBuilder(args);
// var connectionString = builder.Configuration.GetConnectionString("AirlineReservationDb") ?? throw new InvalidOperationException("Connection string 'AirlineReservationDb' not found.");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Load appsettings.json configurations
IConfiguration configuration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json")
               .Build();

// Register the MyBlogContext with the dependency injection container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AirlineReservationDb")));

builder.Services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultUI()
                    .AddDefaultTokenProviders();

// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;         //Xác thực tài khoản
});

var mailSetting = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSetting);
builder.Services.AddSingleton<IEmailSender, SendMailService>();
builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login/";
            options.LogoutPath = "/logout/";
            options.AccessDeniedPath = "/khongduoctruycap.html";
        });

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Trên 30 giây truy cập lại sẽ nạp lại thông tin User (Role)
    // SecurityStamp trong bảng User đổi -> nạp lại thông tinn Security
    options.ValidationInterval = TimeSpan.FromSeconds(5);
});

builder.Services.AddAuthentication().AddCookie()
                .AddGoogle(options =>
                {
                    var gconfig = configuration.GetSection("Authentication:Google");
                    options.ClientId = gconfig["ClientId"];
                    options.ClientSecret = gconfig["ClientSecret"];
                    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    options.CallbackPath = "/dang-nhap-tu-google"; // Relative path instead of absolute URL
                })
                .AddFacebook(options =>
                {
                    var fconfig = configuration.GetSection("Authentication:Facebook");
                    options.AppId = fconfig["AppId"];
                    options.AppSecret = fconfig["AppSecret"];
                    options.CallbackPath = "/dang-nhap-tu-facebook";
                });
// .AddTwitter()
// .AddMicrosoftAccount();

builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", builder =>
    {
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});

builder.Services.AddMvc().AddViewOptions(options =>
{
    options.HtmlHelperOptions.ClientValidationEnabled = false;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//builder.Services.AddSingleton(typeof(ProductService), typeof(ProductService));

app.UseHttpsRedirection();
app.UseStaticFiles();


app.AddStatusCodePage(); // Tuy bien Response loi: 400 - 599

app.UseRouting();        // EndpointRoutingMiddleware

app.UseAuthentication(); // xac dinh danh tinh 
app.UseAuthorization();  // xac thuc  quyen truy  cap

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

app.UseEndpoints(endpoints =>
            {
                // /sayhi
                endpoints.MapGet("/sayhi", async (context) =>
                {
                    await context.Response.WriteAsync($"Hello ASP.NET MVC {DateTime.Now}");
                });

                // endpoints.MapControllers
                // endpoints.MapControllerRoute
                // endpoints.MapDefaultControllerRoute
                // endpoints.MapAreaControllerRoute

                // [AcceptVerbs]

                // [Route]

                // [HttpGet]
                // [HttpPost]
                // [HttpPut]
                // [HttpDelete]
                // [HttpHead]
                // [HttpPatch]

                // Controller khong co Area
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/{controller=Home}/{action=Index}/{id?}"
                );

                endpoints.MapRazorPages();
            });

/*
dotnet aspnet-codegenerator area Database
dotnet aspnet-codegenerator controller -name DbManage -outDir Areas/Database/Controllers/ -namespace App.Areas.Database.Controllers

-Tạo các trang Index/CRUD trong Areas/Contact/Views, trang ContactController trong Areas/Contact/Controllers dựa vào trang Contact.cs trong Models/Contact 
  - dotnet aspnet-codegenerator controller -name ContactController -namespace App.Areas.Contact.Controllers -m App.Models.Contacts.Contact -udl -dc App.Models.AppDBContext -outDir Areas/Contact/Controllers
  - mv Views/Contact Areas/Contact/Views/

  dotnet aspnet-codegenerator controller -name CategoryCotrller -m App.Models.Blog.Category -dc App.Models.AppDbContext -udl -outDir Areas/Blog/Controllers/

JSON
    "AirlineReservationDb": "Data Source=127.0.0.1,1433; Initial Catalog=AirlineReservationDB; User ID=SA;Password=yunbrayyunh; TrustServerCertificate=true;",
    "AirlineReservationConnectionString": "Server=(localdb)\\mssqllocaldb;Database=App;Trusted_Connection=True;MultipleActiveResultSets=true"
*/
