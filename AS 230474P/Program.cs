using AS_230474P.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Make cookie HTTP-only for security
    options.Cookie.IsEssential = true;
});

// Load environment variables
DotNetEnv.Env.Load();

var recaptchaSiteKey = Environment.GetEnvironmentVariable("RECAPTCHA_SITE_KEY");

if (string.IsNullOrEmpty(recaptchaSiteKey))
{
    throw new InvalidOperationException("reCAPTCHA site key is not configured. Set the RECAPTCHA_SITE_KEY environment variable.");
}

// Register the reCAPTCHA site key in DI container
builder.Services.AddSingleton(recaptchaSiteKey);


// Retrieve the encryption key from environment variables
var encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
if (string.IsNullOrEmpty(encryptionKey))
{
    throw new InvalidOperationException("Encryption key is not configured. Set the ENCRYPTION_KEY environment variable.");
}



builder.Services.AddHttpClient<ReCaptchaService>();

// Add the encryption key to the dependency injection container
builder.Services.AddSingleton(encryptionKey);

// Configure services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddRazorPages();

var app = builder.Build();



// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
