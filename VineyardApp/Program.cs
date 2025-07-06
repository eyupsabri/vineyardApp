using Business.Services;
using DAL;
using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using System.Text;
using VineyardApp.ActionFilters;
using VineyardApp.BackgroundServices;
using VineyardApp.Hubs;
using VineyardApp.MQTT;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = jwt.Issuer,
//            ValidAudience = jwt.Issuer,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
//        };
//    });
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwt.Issuer,
          ValidAudience = jwt.Issuer,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
      };

      // 1b) Allow tokens passed as ?access_token=... on your hub endpoint
      options.Events = new JwtBearerEvents
      {
          OnMessageReceived = ctx =>
          {
              var token = ctx.Request.Query["access_token"];
              var path = ctx.HttpContext.Request.Path;
              if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs/pumpStatus"))
              {
                  ctx.Token = token;
              }
              return Task.CompletedTask;
          }
      };
  });


builder.Services
  .Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"))
  .AddSingleton<ITokenService, TokenService>();


//For local development only - open port in firewall settings -------------------------------------------------------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5018);  // <-- listen on 0.0.0.0:5018
    // If you also want HTTPS locally:
    // options.ListenAnyIP(5001, listenOpts => listenOpts.UseHttps());
});






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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIoTDevicesService, IoTDevicesService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

builder.Services.AddScoped<AuthActionFilter>();
builder.Services.AddSignalR();
builder.Services.AddAuthorization();
var app = builder.Build();



//---------------------Local Development Only-------------------------------------------------------------------------
app.UseForwardedHeaders();        // ← must come before everything that relies on proto
//app.UseHttpsRedirection();




app.UseAuthentication();
app.UseAuthorization();

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


app.MapControllers();
app.MapHub<PumpStatusHub>("/hub/pumpStatus");

app.Run();
