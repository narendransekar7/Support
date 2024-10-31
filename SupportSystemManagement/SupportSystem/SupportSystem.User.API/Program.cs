using Microsoft.EntityFrameworkCore;
using SupportSystem.User.MSSQL.Backend.Deployment;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using SupportSystem.User.Model;
using SupportSystem.User.API.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<UserDatabase>(options => {


    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
 });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("https://localhost:7160")  // Allow the React app's origin (e.g., localhost:3000)
            .AllowAnyHeader()                         // Allow any headers (e.g., content-type)
            .AllowAnyMethod()                         // Allow any methods (GET, POST, etc.)
            .AllowCredentials();                      // If you're using authentication
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("hldiSW6BAHCCzY9Yy1zQLiN+MHYJ0Fm5InfQlPANUyM=")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });
builder.Services.AddScoped<IPasswordHasher<PersonModel>, PasswordHasher<PersonModel>>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
//app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();