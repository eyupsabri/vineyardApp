using Business.Services;
using DAL;
using Entities;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using VineyardApp.BackgroundServices;
using VineyardApp.MQTT;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers (so HTTPS redirection works behind Fly proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();  // trust any network
    options.KnownProxies.Clear();   // trust any proxy
});

// Configure HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});


// force console output of Info-level logs from *all* categories
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Host.ConfigureHostOptions(opts =>
{
    opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIoTDeviceRepository, IoTDeviceRepository>();
builder.Services.AddScoped<IPumpSessionRepository, PumpSessionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IIoTDevicesService, IoTDevicesService>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);


builder.Services.Configure<MqttOptions>(
    builder.Configuration.GetSection("MqttProxy"));

builder.Services.AddSingleton(sp =>
{
    var factory = new MqttFactory();
    return factory.CreateMqttClient();
});

builder.Services.AddHostedService<DesiredActualReconciler>();
builder.Services.AddHostedService<OfflinePumpChecker>();
// 6) Register your background MQTT subscriber
builder.Services.AddHostedService<MqttStatusSubscriber>();
builder.Services.AddSingleton<IMessagePublisher, MqttMessagePublisher>();


var app = builder.Build();

app.UseForwardedHeaders();        // ← must come before everything that relies on proto
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
