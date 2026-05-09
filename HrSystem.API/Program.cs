using HrSystem.Application;
using HrSystem.API.Common.Localization;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure;
using HrSystem.Infrustructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Storage.AWS3;
using HrSystem.Shared.CurrentUser;
using HrSystem.Shared.Common;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<LocalizeApiResponseFilter>();
});
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IApiMessageLocalizer, ApiMessageLocalizer>();
builder.Services.AddScoped<LocalizeApiResponseFilter>();
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = LanguageDefaults.SupportedLanguages
        .Select(language => new CultureInfo(language))
        .ToList();

    options.DefaultRequestCulture = new RequestCulture(LanguageDefaults.English);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders =
    [
        new AcceptLanguageHeaderRequestCultureProvider
        {
            MaximumAcceptLanguageHeaderValuesToTry = 2
        }
    ];
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAmazonS3(builder.Configuration);
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HR System API",
        Version = "v1",
        Description = "Comprehensive HR Management System API with features for employee management, attendance tracking, leave management, performance reviews, and more.",
        Contact = new OpenApiContact
        {
            Name = "HR System Support",
            Email = "support@hrsystem.com"
        }
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });

    // Enable XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
var requestLocalizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var env = services.GetRequiredService<IWebHostEnvironment>();

    CurrentUser.BypassScopeFilters = true;
    try
    {
        await dbContext.Database.MigrateAsync();
        await AppDbContextSeed.SeedAsync(dbContext, userManager, roleManager, env);
    }
    finally
    {
        CurrentUser.BypassScopeFilters = false;
    }

    // Initialize CurrentUser accessor with the app's IHttpContextAccessor
    var accessor = services.GetRequiredService<IHttpContextAccessor>();
    HrSystem.Shared.CurrentUser.CurrentUser.Initialize(accessor);
}

// Configure the HTTP request pipeline.
app.UseRequestLocalization(requestLocalizationOptions);
app.UseMiddleware<HrSystem.API.Middleware.ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HR System API v1");
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.EnableFilter();
    options.ShowExtensions();
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
