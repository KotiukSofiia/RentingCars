using KursDb.Context;
using KursDb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Додати DbContext із підключенням до SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IRatingService, RatingService>();
    builder.Services.AddScoped<IReservationService, ReservationService>();
    builder.Services.AddScoped<ICarService, CarService>();

// Додати Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = false;         
    options.Password.RequiredLength = 4;           
    options.Password.RequireNonAlphanumeric = false; 
    options.Password.RequireUppercase = false;     
    options.Password.RequireLowercase = false;     
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminEmail = "admin@admin.com";
    var adminPassword = "Admin1234!";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Заповнення тестовими даними 
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.CarCategories.Any())
    {
        context.CarCategories.AddRange(
            new CarCategory { Name = "Преміум" },
            new CarCategory { Name = "Комфорт" },
            new CarCategory { Name = "Бізнес" },
            new CarCategory { Name = "Кроссовери" }
        );
        context.SaveChanges();
    }

    if (!context.Cars.Any())
    {
        var comfortCategory = context.CarCategories.FirstOrDefault(c => c.Name == "Комфорт");
        var premiumCategory = context.CarCategories.FirstOrDefault(c => c.Name == "Преміум");

        context.Cars.AddRange(
    new Car
    {
        Brand = "Toyota",
        Model = "Camry",
        Year = 2020,
        PricePerDay = 50,
        Status = "Доступно",
        Description = "Сучасний седан із комфортним салоном",
        CategoryId = comfortCategory.CategoryId
    },
    new Car
    {
        Brand = "BMW",
        Model = "X5",
        Year = 2019,
        PricePerDay = 80,
        Status = "Доступно",
        Description = "Потужний позашляховик преміум-класу",
        CategoryId = premiumCategory.CategoryId
    }
);
        context.SaveChanges();
    }
}

app.Run();
