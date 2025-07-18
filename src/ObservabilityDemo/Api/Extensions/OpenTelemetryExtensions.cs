using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.ResourceDetectors.Container;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Settings;

namespace Api.Extensions;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        var openTelemetrySettings =
            builder.Configuration.GetSection(OpenTelemetrySettings.Section).Get<OpenTelemetrySettings>();
        void AppResourceBuilder(ResourceBuilder resource) => resource.AddDetector(new ContainerResourceDetector());

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(AppResourceBuilder)
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(openTelemetrySettings!.ServiceName)
                            .AddAttributes(new[]
                            {
                                new KeyValuePair<string, object>("service.version",
                                    openTelemetrySettings.ServiceVersion),
                                new KeyValuePair<string, object>("service.environment",
                                    openTelemetrySettings.ServiceEnvironment),
                                new KeyValuePair<string, object>("service.tenant",
                                    openTelemetrySettings.ServiceTenant)
                            }))
                    .SetSampler(new TraceIdRatioBasedSampler(1.0))
                    .AddSource("Api.Services.Delay")
                    .AddSource("Api.Services.Random")
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.EnrichWithHttpRequestMessage = (activity, message) =>
                        {
                            activity.DisplayName = message.RequestUri?.ToString() ?? "unknown";
                        };
                    })
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.EnrichWithException = (activity, exception) =>
                        {
                            activity.SetStatus(ActivityStatusCode.Error);
                            activity.AddTag("Exception", exception.Message);
                        };
                    })
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"{openTelemetrySettings.Endpoint}/v1/traces");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(openTelemetrySettings!.ServiceName)
                            .AddAttributes(new[]
                            {
                                new KeyValuePair<string, object>("service.version",
                                    openTelemetrySettings.ServiceVersion),
                                new KeyValuePair<string, object>("service.environment",
                                    openTelemetrySettings.ServiceEnvironment),
                                new KeyValuePair<string, object>("service.tenant", openTelemetrySettings.ServiceTenant)
                            }))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http")
                    .AddOtlpExporter((otlpOptions, readerOptions) =>
                    {
                        otlpOptions.Endpoint = new Uri($"{openTelemetrySettings!.Endpoint}/v1/metrics");
                        otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                        readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 2000;
                    });
            });
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = $"{openTelemetrySettings!.Endpoint}/v1/logs";
                options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = openTelemetrySettings.ServiceName,
                    ["service.version"] = openTelemetrySettings.ServiceVersion,
                    ["service.environment"] = openTelemetrySettings.ServiceEnvironment,
                    ["service.tenant"] = openTelemetrySettings.ServiceTenant
                };
            })
            .WriteTo.Console()
            .CreateLogger();

        builder.Services.AddSerilog();

        return builder;
    }
}