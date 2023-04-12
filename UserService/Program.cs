using Jose;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.Text;
using UserService.Core;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService.Core.UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("UserDb");
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("UserService.Data"))); ;


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", jwtBearerOptions =>
{
    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});


// Register the JwtSettings with the dependency injection container
JwtSettings jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

//RabbitMQ connection
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration.GetSection("RabbitMQ:Host").Value,
        Port = Convert.ToInt32(builder.Configuration.GetSection("RabbitMQ:Port").Value),
        UserName = builder.Configuration.GetSection("RabbitMQ:UserName").Value,
        Password = builder.Configuration.GetSection("RabbitMQ:Password").Value
    };
    return factory.CreateConnection();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<UserDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
