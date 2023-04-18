using Microsoft.EntityFrameworkCore;
using NotificationService.Core;
using NotificationService.Data;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<INotificationService, NotificationService.Core.NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("NotificationDb");
builder.Services.AddDbContext<NotificationDbContext>(options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NotificationService.Data")));


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

        var context = services.GetRequiredService<NotificationDbContext>();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
