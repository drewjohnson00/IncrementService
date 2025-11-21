using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using IncrementService.Middleware;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Repository;
using Serilog;
using Serilog.Core;

namespace IncrementService;

public class Program
{
    private static Logger StartupLogger = null!;
    private static Logger AppLogger = null!;

    public static void Main(string[] args)
    {
        WebApplication app;

        // Startup logger - only for initialization (always available, even if appsettings.json fails)
        StartupLogger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/startup.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        StartupLogger.Information("Starting IncrementService...");

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            string connectionString = ValidateConfigurationAndGetConnectionString(builder.Configuration, StartupLogger);

            // Application logger - for runtime (uses appsettings.json configuration)
            AppLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            // Set the global Serilog logger to the application logger
            Log.Logger = AppLogger;

            // Clear default logging providers and configure DI to use Serilog for ILogger<T>
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, dispose: true); 

            StartupLogger.Information("Startup complete, switching to application logger");
            AppLogger.Information("Application logger active");

            bool isDevelopment = builder.Environment.IsDevelopment();

            builder.Services.AddControllers(options =>
            {
                // TODO: Add global filters here
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // Don't get stuck in circular references...just set value to null
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Accept default property name capitalization -- PascalCase
                options.JsonSerializerOptions.WriteIndented = false; // false = don't add extra whitespace for readability
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault; // Ignore default values during serialization
            });

            // Configure consistent error response format for model binding/validation failures
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    Guid logId = Guid.NewGuid();

                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    string endpoint = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                    Log.Logger.ForContext<Program>()
                        .Warning("Model validation failed on {Endpoint}. Errors: {Errors}. LogId: {LogId}",
                            endpoint, string.Join("; ", errors), logId);

                    context.HttpContext.Response.Headers["X-Correlation-Id"] = logId.ToString();

                    var errorResponse = new ErrorResponse
                    {
                        Message = "Validation failed.",
                        Errors = errors,
                    };

                    return new BadRequestObjectResult(errorResponse);
                };
            });

            builder.Services.AddRepositoryServices(connectionString);
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); // include all types in IncrementService project
                cfg.RegisterServicesFromAssembly(typeof(IIncrementRepository).Assembly); // include all types in Repository project
                cfg.RegisterServicesFromAssembly(typeof(IncrementKey).Assembly); // include all types in Infrastructure project
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "IncrementService API", Version = "v1" });

                //var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                options.OrderActionsBy(x =>
                {
                    string method = x.HttpMethod switch
                    {
                        "GET" => "1",
                        "POST" => "2",
                        "PUT" => "3",
                        "DELETE" => "4",
                        _ => "5"
                    };

                    return $"{x.ActionDescriptor.RouteValues["controller"]}_{x.RelativePath}_{method}";
                    ;
                });
            });

            app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DefaultModelsExpandDepth(-1); // Disable schema at bottom of Swagger page
                options.EnableTryItOutByDefault(); // Disable "Try it out" by default for all endpoints
            });

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.MapControllers();
        }
        catch (Exception ex)
        {
            StartupLogger.Fatal(ex, "Application failed to start due to configuration or initialization error");
            throw;
        }
        finally
        {
            StartupLogger.Information("Shutting down startup logger");
            StartupLogger.Dispose();
        }

        Log.Logger.ForContext<Program>().Information("Setup Complete -- Application Starting...");

        app.Run();
    }

    private static string ValidateConfigurationAndGetConnectionString(ConfigurationManager configuration, Logger logger)
    {
        logger.Information("Validating configuration...");

        // Check if configuration is empty (would indicate appsettings.json didn't load)
        if (!configuration.AsEnumerable().Any())
        {
            const string errorMessage = "Configuration is empty. appsettings.json may not have loaded correctly.";
            logger.Error("{errorMessage}", errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        // Check for required configuration sections
        var requiredSections = new[] { "ConnectionStrings", "Serilog", "AllowedHosts" };
        List<string> missingSections = requiredSections.Where(section => !configuration.GetSection(section).Exists()).ToList();

        if (missingSections.Count != 0)
        {
            string errorMessage = $"Required configuration sections missing: {string.Join(", ", missingSections)}. " +
                                          "Verify appsettings.json is valid and contains all required sections.";
            logger.Error("{errorMessage}", errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        // Verify connection string is present
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            const string errorMessage = "Connection string 'DefaultConnection' is missing or empty. " +
                                        "Check appsettings.json ConnectionStrings section.";
            logger.Error("{errorMessage}", errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        int configCount = configuration.AsEnumerable().Count();
        Console.WriteLine("✓ Configuration validated successfully");
        Console.WriteLine($"✓ Found {configCount} configuration entries");
        logger.Information("Configuration validated successfully. Found {ConfigCount} configuration entries", configCount);

        // Log database name (helpful for diagnostics)
        if (connectionString.Contains("Database="))
        {
            string dbName = connectionString.Split("Database=")[1].Split(';')[0];
            logger.Information("Connection string configured for database: {Database}", dbName);
        }

        return connectionString;
    }
}
