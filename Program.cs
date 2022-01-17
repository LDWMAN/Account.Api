using Microsoft.EntityFrameworkCore;
using AccountApi.Data;
using AccountApi.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using AccountApi.Model.Configuration;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


/// Builder ConfigurationManager  
ConfigurationManager configuration = builder.Configuration;


#region ConnectionString 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
);
#endregion


#region Controller Versioning
builder.Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;

        options.AssumeDefaultVersionWhenUnspecified = true;

        options.DefaultApiVersion = ApiVersion.Default;
    }
);
#endregion


#region JWT / Authentication 설정

builder.Services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));

var key = Encoding.ASCII.GetBytes(configuration["JwtConfig:Secret"]);

// TokenParameter
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false, // TODO: UPDATE
    ValidateAudience = false, // TODO: UPDATE
    RequireExpirationTime = false,  // TODO: UPDATE
    ValidateLifetime = true
};

builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(jwt =>
                {
                    var key = Encoding.ASCII.GetBytes(configuration["JwtConfig:Secret"]);
                    jwt.SaveToken = true;
                    jwt.TokenValidationParameters = tokenValidationParameters;
                }
            );

#endregion


#region UnitOfWork DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
#endregion


#region wwwroot 
IWebHostEnvironment environment = builder.Environment;
#endregion








var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//사용자 인증 설정
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
