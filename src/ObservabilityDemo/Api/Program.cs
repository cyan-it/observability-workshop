
using Api.Extensions;
using Api.Services.Delay;
using Api.Services.Random;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    var origins = builder.Configuration["Cors:AllowedOrigins"] ?? "*";
    if (origins == "*")
    {
        options.AddDefaultPolicy(policyBuilder => policyBuilder
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    }
    else
    {
        var allowedOrigins = origins.Split(";", StringSplitOptions.RemoveEmptyEntries);
        options.AddDefaultPolicy(policyBuilder => policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    }
});

builder.Services
    .AddScoped<IRandomService, RandomService>()
    .AddScoped<IDelayService, DelayService>();

var app = builder.Build();
app.UseCors();

app.MapGet("/ok", async (IDelayService delayService, IRandomService randomService) =>
{
    await delayService.Delay(randomService.Next(5, 10));
    return Results.Ok();
});

app.MapGet("/unauthorized",
    async (IDelayService delayService, IRandomService randomService, ILogger<Program> logger) =>
    {
        await delayService.Delay(randomService.Next(10, 20));
        if (randomService.NextDouble() < 0.4)
        {
            logger.LogError("Error: Unauthorized access attempted.");
            return Results.Unauthorized();
        }

        return Results.Ok();
    });

app.MapGet("/not-found",
    async (IDelayService delayService, IRandomService randomService, ILogger<Program> logger) =>
    {
        await delayService.Delay(randomService.Next(20, 30));
        if (randomService.NextDouble() < 0.4)
        {
            logger.LogError("Error: Resource not found.");
            return Results.NotFound();
        }

        return Results.Ok();
    });

app.MapGet("/bad-request",
    async (IDelayService delayService, IRandomService randomService, ILogger<Program> logger) =>
    {
        await delayService.Delay(randomService.Next(30, 40));
        if (randomService.NextDouble() < 0.4)
        {
            logger.LogError("Error: Bad request made.");
            return Results.BadRequest();
        }

        return Results.Ok();
    });

app.MapGet("/internal-server-error",
    async (IDelayService delayService, IRandomService randomService, ILogger<Program> logger) =>
    {
        await delayService.Delay(randomService.Next(40, 50));
        if (randomService.NextDouble() < 0.4)
        {
            logger.LogError("Error: Internal server error occurred.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Results.Ok();
    });

app.Run();