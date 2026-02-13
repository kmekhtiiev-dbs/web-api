using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace WebApi.Security;

public class BasicAuthDashboardAuthorizationFilter(string username, string password) : IDashboardAuthorizationFilter
{
    private readonly string _username = username;
    private readonly string _password = password;

    public bool Authorize(DashboardContext context)
    {
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            return false;
        }

        var httpContext = context.GetHttpContext();
        var authHeader = httpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        var encodedCredentials = authHeader["Basic ".Length..].Trim();
        string decodedCredentials;

        try
        {
            decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
        }
        catch (FormatException)
        {
            Challenge(httpContext);
            return false;
        }

        var parts = decodedCredentials.Split(':', 2);
        if (parts.Length != 2)
        {
            Challenge(httpContext);
            return false;
        }

        var isValid = FixedTimeEquals(parts[0], _username) && FixedTimeEquals(parts[1], _password);
        if (!isValid)
        {
            Challenge(httpContext);
        }

        return isValid;
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        if (leftBytes.Length != rightBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static void Challenge(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
    }
}
