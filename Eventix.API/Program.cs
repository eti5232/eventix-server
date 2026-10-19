using System.Text.Json.Serialization;
using Eventix.API.Errors;
using Eventix.API.Extensions;
using Eventix.API.Middleware;
using Eventix.Application;
using Eventix.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NLog.Web;

// Logger available before the host exists, so a startup failure can still be written to the NLog targets.
var startupLogger = NLog.LogManager.GetCurrentClassLogger();

WebApplication app;
try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog replaces the default logging providers; code keeps using the standard ILogger.
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        .ConfigureApiBehaviorOptions(options =>
        {
            // Validation errors (400) get the same uniform error format as every other error.
            options.InvalidModelStateResponseFactory = actionContext =>
                new BadRequestObjectResult(ErrorResponseFactory.CreateValidation(actionContext.HttpContext, actionContext.ModelState))
                {
                    ContentTypes = { ErrorResponseFactory.ContentType }
                };
        });
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddClientCors(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    app = builder.Build();

    await app.Services.MigrateDatabaseAsync();
    await app.Services.SeedDatabaseAsync();
}
// HostAbortedException is how the EF tools (dotnet ef) stop the app after building the host; it is not a failure.
catch (Exception exception) when (exception is not HostAbortedException)
{
    // Startup failed before the server could accept requests (bad configuration, no database, failed migration).
    // Log it once with the full exception, flush the log, and rethrow so the process still stops with an error.
    startupLogger.Fatal(exception, "The application failed to start.");
    NLog.LogManager.Shutdown();
    throw;
}

// Configure the HTTP request pipeline. The order matters:
// errors are caught first, and the correlation id must exist before anything is logged.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger is on in every environment, so the API can be explored on the live site too.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS before authentication, so the browser's preflight (OPTIONS) request is answered without a token.
app.UseClientCors();

// Authentication (who are you?) must come before authorization (what may you do?).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
