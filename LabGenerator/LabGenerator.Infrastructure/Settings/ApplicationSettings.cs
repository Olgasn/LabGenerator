namespace LabGenerator.Infrastructure.Settings;

public class ApplicationSettings
{
    public bool PgAutoMigrations { get; set; } = false;

    public bool LogLlmRequests { get; set; } = false;

    public int LogLlmMaxChars { get; set; } = 50000;

    public int LlmRequestTimeoutSeconds { get; set; } = 300;

    public int LlmRetryCount { get; set; } = 3;

    public int LlmRetryMaxDelaySeconds { get; set; } = 120;
}