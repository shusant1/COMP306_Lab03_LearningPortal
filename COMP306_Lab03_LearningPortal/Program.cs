
using COMP306_Lab03_LearningPortal.Data;
using COMP306_Lab03_LearningPortal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.Runtime.CredentialManagement;



var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();


// Get database connection from AWS Parameter Store
string connectionString;

AmazonSimpleSystemsManagementClient ssm;

if (builder.Environment.IsDevelopment())
{
    // Each developer sets their own profile using local User Secrets.
    string? profileName = builder.Configuration["AWS:Profile"];

    if (string.IsNullOrWhiteSpace(profileName))
    {
        throw new Exception("Set AWS:Profile in your local User Secrets.");
    }

    var chain = new CredentialProfileStoreChain();

    if (!chain.TryGetAWSCredentials(profileName, out var credentials))
    {
        throw new Exception("AWS profile not found: " + profileName);
    }

    ssm = new AmazonSimpleSystemsManagementClient(
        credentials, RegionEndpoint.USEast2);
}
else
{
    // Use the AWS role when deployed
    ssm = new AmazonSimpleSystemsManagementClient(
        RegionEndpoint.USEast2);
}

using (ssm)
{
    var response = await ssm.GetParameterAsync(
        new GetParameterRequest
        {
            Name = "/comp306/lab03/rds-connection",
            WithDecryption = true
        });

    connectionString = response.Parameter?.Value
        ?? throw new Exception("Database parameter is empty.");
}

// Connect to SQL Server
builder.Services.AddDbContext<LearningPortalContext>(options =>
    options.UseSqlServer(connectionString)
);


// Password hashing
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Login authentication
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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

app.Run();
