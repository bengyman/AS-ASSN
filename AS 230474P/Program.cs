using AS_230474P.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using System;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
DotNetEnv.Env.Load();

// Retrieve the encryption key from environment variables
var encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
if (string.IsNullOrEmpty(encryptionKey))
{
    throw new InvalidOperationException("Encryption key is not configured. Set the ENCRYPTION_KEY environment variable.");
}

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
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
