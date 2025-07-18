namespace Api.Extensions;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        return builder;
    }
}