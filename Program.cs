
using App;
using App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AirlineReservationConnectionString") ?? throw new InvalidOperationException("Connection string 'AirlineReservationConnectionString' not found.");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Load appsettings.json configurations
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Register the MyBlogContext with the dependency injection container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    //options.UseSqlServer(connectionString);
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext"));
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

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();


/*
-Tạo các trang Index/CRUD trong Areas/Contact/Views, trang ContactController trong Areas/Contact/Controllers dựa vào trang Contact.cs trong Models/Contact 
  - dotnet aspnet-codegenerator controller -name ContactController -namespace App.Areas.Contact.Controllers -m App.Models.Contacts.Contact -udl -dc App.Models.AppDBContext -outDir Areas/Contact/Controllers
  - mv Views/Contact Areas/Contact/Views/
*/