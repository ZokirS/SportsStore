using Microsoft.EntityFrameworkCore;
using SportsStore.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StoreDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration["ConnectionStrings:SportsStoreConnection"]);
});
builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.UseStaticFiles();
app.MapControllerRoute("pagination",
    "Products/Page{productPage}",
    new {Controller = "Home", Action = "Index"});
app.MapDefaultControllerRoute();

SeedData.EnsurePopulated(app);
app.Run();
