using System.Text;
using HerpControllerService.Database;
using HerpControllerService.mqtt;
using HerpControllerService.mqtt.Processors;
using HerpControllerService.Services;
using HerpControllerService.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:7001");
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSignalR().AddMessagePackProtocol();

// Add services to the container.
builder.Services.AddDbContextFactory<HerpControllerDbContext>(
    options => options.UseNpgsql(builder.Configuration["ConnectionString"],
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
);

builder.Services.AddScoped(
    provider => provider.GetRequiredService<IDbContextFactory<HerpControllerDbContext>>().CreateDbContext()
);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:Secret"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/deviceCommands"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

//Register Services
builder.Services.AddSingleton<MqttService>();
// builder.Services.AddHostedService<MqttService>(p => p.GetRequiredService<MqttService>());

builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<SensorService>();
builder.Services.AddScoped<SensorReadingService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<TimerPinStateService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddSingleton<TelegramService>();

//MQTT processors
builder.Services.AddScoped<ReceivedSensorDataProcessor>();
builder.Services.AddScoped<ReceivedTimerDataProcessor>();
builder.Services.AddScoped<ReceivedDirectResponseDataProcessor>();
builder.Services.AddScoped<ReceivedPresenceDataProcessor>();

//SignalR
builder.Services.AddScoped<RealTimeHubSender>();
builder.Services.AddScoped<DeviceCommandResponseSender>();

var app = builder.Build();

app.MapHub<RealTimeHub>("/sensorData");
app.MapHub<DeviceCommandHub>("/deviceCommands");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();