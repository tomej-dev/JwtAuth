using System;
using System.Text;
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

// ---------------------------
// Configura��o do MySQL
// ---------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=localhost;Database=JwtAuthDB;User=root;Password=Unusualfpss#;";

var serverVersion = new MySqlServerVersion(new Version(8, 0, 39)); // ajuste para a vers�o do seu MySQL

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion)
           .LogTo(Console.WriteLine, LogLevel.Information); // log SQL para debug
});

// Servi�o de usu�rios
builder.Services.AddScoped<UserService>();

// Configura��o JWT
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

// ---------------------------
// Cria o banco MySQL automaticamente
// ---------------------------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("MySQL connection string: {Conn}", connectionString);

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Cria o banco e aplica migrations se n�o existir
        db.Database.Migrate();
        logger.LogInformation("Database migrated/created successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao criar ou migrar o banco de dados MySQL.");
        throw;
    }
}

app.Run();
