using System;
using System.Text;
using System.IO;
using JwtAuth.Data;
using JwtAuth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Resolve connection string and make sure SQLite file path is absolute
var configuredConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=usuarios.db";
string sqliteConn;
if (configuredConn.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    var source = configuredConn.Substring("Data Source=".Length).Trim();
    if (!Path.IsPathRooted(source))
    {
        // Put the DB file in the current working directory (output folder)
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), source);
        sqliteConn = $"Data Source={fullPath}";
    }
    else
    {
        sqliteConn = configuredConn;
    }
}
else
{
    sqliteConn = configuredConn;
}

// Banco SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConn));

// Serviço de usuários
builder.Services.AddScoped<UserService>();

// Configuração JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"];
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(policy =>
    policy
        .WithOrigins("https://jwt-auth-frontend-navy.vercel.app") 
        .AllowAnyHeader()
        .AllowAnyMethod()
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Cria o banco automaticamente (somente em desenvolvimento)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Configured connection string: {Conn}", configuredConn);
    logger.LogInformation("Effective SQLite connection: {Conn}", sqliteConn);
    logger.LogInformation("CurrentDirectory: {Dir}", Directory.GetCurrentDirectory());

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
        logger.LogInformation("Database ensured/created successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao criar o banco de dados SQLite.");
        throw;
    }
}

app.Run();