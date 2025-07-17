using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Settings;
using System.Reflection;

namespace Migrator;

internal class Program
{
    static int Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting migration...");

        try
        {
            var migrationDirection = args.FirstOrDefault() ?? "up";
            logger.LogInformation("Migration direction: {Direction}", migrationDirection);

            var runner = host.Services.GetRequiredService<IMigrationRunner>();

            if (migrationDirection.Equals("down", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Rolling back one step...");
                runner.Rollback(1);
                logger.LogInformation("Rollback complete.");
            }
            else
            {
                logger.LogInformation("Applying migrations...");
                runner.MigrateUp();
                logger.LogInformation("Migrations applied successfully.");
            }

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Migration failed.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((configurationBuilder) =>
            {
                configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((hostBuilderContext, services) =>
            {
                var previewOnly = args.Contains("--preview", StringComparer.OrdinalIgnoreCase);
                services.AddFluentMigratorCore()
                    .ConfigureRunner(rb => rb
                        .AddPostgres()
                        .WithGlobalConnectionString(
                            hostBuilderContext.Configuration.GetConnectionString("DefaultConnection"))
                        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
                    .Configure<ProcessorOptions>(opt => opt.PreviewOnly = previewOnly);

                services.AddSerilog(configuration =>
                    configuration
                        .Enrich.FromLogContext()
                        .WriteTo.Console());
            });
}