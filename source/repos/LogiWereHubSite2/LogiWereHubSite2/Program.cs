using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient();

// Настройка маршрутизации
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/Views/Home/MainSiteWindows/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Home/AdminWindow/AddPages/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Home/AdminWindow/EditPages/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Home/AdminWindow/ViewPages/{0}.cshtml");
    options.ViewLocationFormats.Add("Views/Home/UserWindows/{0}.cshtml");
    // Добавьте другие пути, если у вас есть представления в других папках
});

var app = builder.Build();
 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

// app.UseMiddleware<TokenRefreshMiddleware>();

app.UseRouting();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=MainPage}/{id?}");



app.Run();
