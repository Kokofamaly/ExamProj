using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using WordCardsApi.Infrastructure.Data;
using WordCardsApi.Infrastructure.Providers;
using WordCardsApi.Infrastructure.Settings;
using WordCardsApi.Middleware;
using WordCardsApi.Models;
using WordCardsApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<MongoDbContext>();


builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LearningSessionService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<UserWordService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<LearningSessionProvider>();
builder.Services.AddScoped<RefreshTokenProvider>();
builder.Services.AddScoped<SessionWordProvider>();
builder.Services.AddScoped<UserProvider>();
builder.Services.AddScoped<UserWordProvider>();

var allowedOrigins = 
    builder.Configuration.GetSection("CorsAllowedOrigins").Get<string[]>() 
    ?? throw new InvalidOperationException("Failed to configure allowed cors origins");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", 
    policy => 
        policy
        .WithOrigins(allowedOrigins)
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

    var jwtSettings = builder.Configuration
        .GetSection("JwtSettings")
        .Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SignKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("UserOnly", p => p.RequireRole("User"))
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/users", async (MongoDbContext context) => await context.Users.Find(x => true).ToListAsync()).AllowAnonymous();
}

app.Run();
