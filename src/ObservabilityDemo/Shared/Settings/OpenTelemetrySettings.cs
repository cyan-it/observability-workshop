namespace Settings;

public class OpenTelemetrySettings
{
    public static string Section = "OpenTelemetry";
    public string Endpoint { get; set; } = "http://localhost:4318";
    public string ServiceName { get; set; } = "ApplicationName";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string ServiceEnvironment { get; set; } = "Development";
    public string ServiceTenant { get; set; } = "Developers";
    
}