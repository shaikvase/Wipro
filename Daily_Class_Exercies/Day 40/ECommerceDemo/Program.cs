using ECommerceDemo.Filters;
using ECommerceDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + Filters
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CustomExceptionFilter>();
    options.Filters.Add<LoggingActionFilter>();
});

// Session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ECommerceDemo.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

// DI
builder.Services.AddSingleton<ProductService>();
builder.Services.AddScoped<CartService>();

// For accessing HttpContext in views if needed
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Default route -> Login, then Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
