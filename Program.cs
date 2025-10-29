using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

builder = WebApplication.CreateBuilder(args);

var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = jwtCofig["Key"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];

builder.Services.AddControllers();

builder.Services.AddAuthentication(options 
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = issuer,
         ValidAudience = audience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
         ClockSkew = TimeSpan.FromSeconds(30)
     };
});

builder.Services.AddAuthorization();


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

