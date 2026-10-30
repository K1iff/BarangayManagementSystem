using Microsoft.AspNetCore.StaticFiles;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

builder.Services.AddScoped<Supabase.Client>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

    var url = configuration["Supabase:Url"] ?? string.Empty;
    var key = configuration["Supabase:Key"] ?? string.Empty;

    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };

    var client = new Supabase.Client(url, key, options);
    client.InitializeAsync().Wait();
    return client;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Accounts}/{action=Login}/{id?}");

app.Run();
