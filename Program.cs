using EXE.Models;
using EXE.Services;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using YourProjectNamespace.Services;
using EXE.Hubs;
using EXE.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IVisitorService, VisitorService>();
// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

//builder.Services.RegisterGlobalizationAndLocalization();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<IVnPayService, VnPayService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddDbContext<Exe101Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton(x => new PaypalClient(
    builder.Configuration["PaypalOptions:AppId"],
    builder.Configuration["PaypalOptions:AppSecret"],
    builder.Configuration["PaypalOptions:Mode"]
));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
 {
     IConfigurationSection googleAuthSection = builder.Configuration.GetSection("Authentication:Google");
     options.ClientId = googleAuthSection["ClientId"];
     options.ClientSecret = googleAuthSection["ClientSecret"];
     options.CallbackPath = "/signin-google";
 })
.AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
 {
	 IConfigurationSection facebookAuthSection = builder.Configuration.GetSection("Authentication:Facebook");
	 options.AppId = facebookAuthSection["AppId"];
	 options.AppSecret = facebookAuthSection["AppSecret"];
	 options.CallbackPath = "/signin-facebook"; 
 });




var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next)=>
{
    string cookie = string.Empty;
    if (context.Request.Cookies.TryGetValue("Language", out cookie))
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cookie);
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cookie);
    }
    else
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en");
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");

    }

    await next.Invoke();

});




app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
//app.UseMiddleware<VisitorTrackingMiddleware>();
app.MapHub<ChatHub>("/chathub");
app.MapHub<VisitorHub>("/visitorhub"); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}");

app.Run();
