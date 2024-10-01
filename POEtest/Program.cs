using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POEtest.Controllers;
using POEtest.Services;

var builder = WebApplication.CreateBuilder(args);

// Access the configuration object
var configuration = builder.Configuration;

// Add services to the container
builder.Services.AddControllersWithViews();

// Register session support 
builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory cache to store session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie accessible only by the server
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

// Register HttpClient for BlobService with configuration
builder.Services.AddHttpClient<BlobService>((sp, client) =>
{
    var functionBaseUrl = configuration["FunctionBaseUrl"];
    if (string.IsNullOrEmpty(functionBaseUrl))
    {
        throw new InvalidOperationException("Function base URL not configured in appsettings.json.");
    }
    client.BaseAddress = new Uri(functionBaseUrl);
});

// Register BlobService
builder.Services.AddSingleton<BlobService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new BlobService(configuration, httpClient); // Pass IConfiguration and HttpClient
});

// Register TableStorageService
builder.Services.AddSingleton<TableStorageService>(sp =>
{
    var connectionString = configuration.GetConnectionString("AzureStorage");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Azure Storage connection string not configured.");
    }
    return new TableStorageService(connectionString);
});

// Register QueueService
builder.Services.AddSingleton<QueueService>(sp =>
{
    var connectionString = configuration.GetConnectionString("AzureStorage");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Azure Storage connection string not configured.");
    }
    return new QueueService(connectionString, "orders");
});

// Register AzureFileShareService
builder.Services.AddSingleton<AzureFileShareService>(sp =>
{
    var connectionString = configuration.GetConnectionString("AzureStorage");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Azure Storage connection string not configured.");
    }
    return new AzureFileShareService(connectionString, "uploadfiles");
});

// Register HttpClient for UsersController
builder.Services.AddHttpClient<UsersController>(client =>
{
    var functionBaseUrl = configuration["FunctionBaseUrl"];
    if (string.IsNullOrEmpty(functionBaseUrl))
    {
        throw new InvalidOperationException("Function base URL not configured in appsettings.json.");
    }
    client.BaseAddress = new Uri($"{functionBaseUrl}/Users");
});

// Register HttpClient for FilesController
builder.Services.AddHttpClient<FilesController>(client =>
{
    var functionBaseUrl = configuration["FunctionBaseUrl"];
    if (string.IsNullOrEmpty(functionBaseUrl))
    {
        throw new InvalidOperationException("Function base URL not configured in appsettings.json.");
    }
    client.BaseAddress = new Uri($"{functionBaseUrl}/UploadFile");
});

// Register HttpClient for OrdersController
builder.Services.AddHttpClient<OrdersController>(client =>
{
    var functionBaseUrl = configuration["FunctionBaseUrl"];
    if (string.IsNullOrEmpty(functionBaseUrl))
    {
        throw new InvalidOperationException("Function base URL not configured in appsettings.json.");
    }
    client.BaseAddress = new Uri($"{functionBaseUrl}/Orders");
});

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); 
    app.UseHsts(); // Enable HSTS for security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files

app.UseRouting(); // Enable routing

// Enable session middleware
app.UseSession();

// Enable authorization middleware
app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}"); 

app.Run();

//Mrzyg?ód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]