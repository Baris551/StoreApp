using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StoreApp.Data.Abstract;
using StoreApp.Data.Concrete;
using StoreApp.Web.Models;
using StoreApp.Web;

var builder = WebApplication.CreateBuilder(args);

// --- Servis Tanımlamaları ---
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<Cart>(sc => SessionCart.GetCart(sc));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("StoreApp.Web")));

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
})
.AddEntityFrameworkStores<StoreDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSecret = builder.Configuration["AppSettings:Secret"];
    if (string.IsNullOrEmpty(jwtSecret))
        throw new InvalidOperationException("JWT Secret is missing in configuration.");

    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Users/Login";
    options.LogoutPath = "/Users/Logout";
    options.AccessDeniedPath = "/Users/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// E-Posta Servisi KALDIRILDI
// builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "StoreApp API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- Seed İşlemleri ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var roleResult = await roleManager.CreateAsync(new AppRole { Name = "Admin" });
        Console.WriteLine(roleResult.Succeeded
            ? "Admin role created successfully."
            : "Error creating Admin role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
    }

    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            FullName = "Admin User",
            DateAdded = DateTime.Now,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
        if (createResult.Succeeded)
        {
            var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine(roleAssignResult.Succeeded
                ? "Admin user created and assigned to Admin role successfully."
                : "Error assigning Admin role to user: " + string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
        }
        else
        {
            Console.WriteLine("Error creating admin user: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }
    else
    {
        var roles = await userManager.GetRolesAsync(adminUser);
        if (!roles.Contains("Admin"))
        {
            var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine(roleAssignResult.Succeeded
                ? "Existing admin user assigned to Admin role successfully."
                : "Error assigning Admin role to existing user: " + string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
        }
        else
        {
            Console.WriteLine("Admin user already exists and has Admin role.");
        }
    }
}

// --- Middleware’ler ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StoreApp API v1");
    c.RoutePrefix = "swagger";
});

// --- Route Ayarları ---
app.MapRazorPages();
app.MapControllerRoute("register", "Register", new { controller = "Users", action = "Register" });
app.MapControllerRoute("login", "Login", new { controller = "Users", action = "Login" });
app.MapControllerRoute("products_in_category", "products/{category?}", new { controller = "Product", action = "Index" });
app.MapControllerRoute("product_details", "product/{name}", new { controller = "Home", action = "Details" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
