using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Heldom_SYS.Interface;
using Heldom_SYS.Service;
using Heldom_SYS.Models;
using Microsoft.Data.SqlClient;
using Heldom_SYS.Filter;

var builder = WebApplication.CreateBuilder(args);

// 配置 JSON 序列化選項（支援中文）
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Encoder = JavaScriptEncoder.Create(
        UnicodeRanges.BasicLatin,
        UnicodeRanges.Cyrillic,
        UnicodeRanges.CjkUnifiedIdeographs);
    options.WriteIndented = true;
});

// 添加 MVC 服務
builder.Services.AddControllersWithViews();

// 添加 role 服務
builder.Services.AddSingleton<IUserStoreService, UserStoreService>();

//  加入 Session 設定
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 有效時間
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSession(); // 啟用 Session

builder.Services.AddScoped<IAccidentService, AccidentService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAttendanceExcelService, AttendanceExcelService>();

// 添加身份驗證
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Profile/Login"; // 明確指向 Login 頁面
//        options.AccessDeniedPath = "/Profile/AccessDenied";
//    });


// 資料庫連線 從 appsettings.json 中讀取 Connection 連接字串 目前有3種
string? primaryConnection = builder.Configuration.GetConnectionString("ConstructionDB");
string? secondaryConnection = builder.Configuration.GetConnectionString("ConstructionDB2");
string? thirdConnection = builder.Configuration.GetConnectionString("ConstructionDB3");

// 測試並選擇可用的連線字串  
string? finalConnection = TestDatabaseConnection(primaryConnection) ? primaryConnection :
                         TestDatabaseConnection(secondaryConnection) ? secondaryConnection :
                         TestDatabaseConnection(thirdConnection) ? thirdConnection :
                         throw new InvalidOperationException("連線失敗");

// 可以將連接字串註冊到依賴注入容器 (直接使用資料庫)
builder.Services.AddScoped<SqlConnection>(provider => new SqlConnection(finalConnection));



// 資料庫連線 FEcore(快取)   
builder.Services.AddDbContext<ConstructionDbContext>(
            options => options.UseSqlServer(finalConnection));

// user驗證
builder.Services.AddControllers(options =>
{
    options.Filters.Add<UserFilter>();
});

var app = builder.Build();

app.UseSession(); // 使用 Session

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}");

//    pattern: "{controller=Project}/{action=BlueprintsCategories}/{id?}");

//    pattern: "{controller=Home}/{action=Index}/{id?}");

//    pattern: "{controller=Dashboard}/{action=Dashboard}/{id?}");




app.Run();

//檢測連線
static bool TestDatabaseConnection(string? connectionString)
{
    if (string.IsNullOrEmpty(connectionString))
        return false;

    try
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            return true;
        }
    }
    catch
    {
        Console.WriteLine($"資料庫連線失敗: {connectionString}");
        return false;
    }
}