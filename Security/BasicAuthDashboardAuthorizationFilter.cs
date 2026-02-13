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

        var providedUser = parts[0];
        var providedPassword = parts[1];

        var isValid = FixedTimeEquals(providedUser, _username) && FixedTimeEquals(providedPassword, _password);

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
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static void Challenge(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
    }
}
