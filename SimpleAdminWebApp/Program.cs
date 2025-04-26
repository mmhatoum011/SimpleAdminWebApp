using SimpleAdminWebApp.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSession(); // Required to use Session
builder.Services.AddSingleton<UserService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<UserService>();

// ✅ Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session and authorization
app.UseSession();
app.UseAuthorization();

// Set the default route to the Dashboard controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.UseStaticFiles();


app.Run();

