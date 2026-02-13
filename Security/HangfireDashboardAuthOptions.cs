namespace WebApi.Security;

public class HangfireDashboardAuthOptions
{
    public const string SectionName = "Hangfire:Dashboard";

    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
