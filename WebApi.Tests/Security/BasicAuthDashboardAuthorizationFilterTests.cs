using System.Text;
using FluentAssertions;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Security;

namespace WebApi.Tests.Security;

public class BasicAuthDashboardAuthorizationFilterTests
{
    private const string ValidUsername = "admin";
    private const string ValidPassword = "secret";

    private static string EncodeCredentials(string username, string password)
    {
        var raw = $"{username}:{password}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    private static (AspNetCoreDashboardContext dashboardContext, DefaultHttpContext httpContext)
        CreateDashboardContext(string? authorizationHeader)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };

        if (authorizationHeader is not null)
        {
            httpContext.Request.Headers.Authorization = authorizationHeader;
        }

        var storage = new Hangfire.MemoryStorage.MemoryStorage();
        var options = new DashboardOptions();
        var dashboardContext = new AspNetCoreDashboardContext(storage, options, httpContext);
        return (dashboardContext, httpContext);
    }

    private static BasicAuthDashboardAuthorizationFilter CreateFilter(
        string username = ValidUsername, string password = ValidPassword) =>
        new(username, password);

    // ── Empty / missing credentials configuration ──────────────────────

    [Theory]
    [InlineData("", "secret")]
    [InlineData("admin", "")]
    [InlineData("", "")]
    [InlineData("   ", "secret")]
    [InlineData("admin", "   ")]
    public void Authorize_Returns_False_When_Filter_Has_Empty_Credentials(string username, string password)
    {
        var filter = CreateFilter(username, password);
        var (context, _) = CreateDashboardContext($"Basic {EncodeCredentials("admin", "secret")}");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    // ── Missing / malformed Authorization header ───────────────────────

    [Fact]
    public void Authorize_Returns_False_When_No_Auth_Header()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext(authorizationHeader: null);

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Authorize_Sets_401_And_WWWAuthenticate_When_No_Auth_Header()
    {
        var filter = CreateFilter();
        var (context, httpContext) = CreateDashboardContext(authorizationHeader: null);

        filter.Authorize(context);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        httpContext.Response.Headers.WWWAuthenticate.ToString()
            .Should().Contain("Basic realm=");
    }

    [Fact]
    public void Authorize_Returns_False_When_Auth_Header_Is_Empty()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext(authorizationHeader: "");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Authorize_Returns_False_When_Auth_Scheme_Is_Not_Basic()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext($"Bearer {EncodeCredentials(ValidUsername, ValidPassword)}");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Authorize_Returns_False_When_Credentials_Are_Not_Valid_Base64()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext("Basic not-valid-base64!!!");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Authorize_Returns_False_When_Decoded_Credentials_Have_No_Colon()
    {
        var filter = CreateFilter();
        var noColon = Convert.ToBase64String(Encoding.UTF8.GetBytes("usernamepassword"));
        var (context, _) = CreateDashboardContext($"Basic {noColon}");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    // ── Valid credentials ──────────────────────────────────────────────

    [Fact]
    public void Authorize_Returns_True_When_Credentials_Are_Correct()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext($"Basic {EncodeCredentials(ValidUsername, ValidPassword)}");

        var result = filter.Authorize(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void Authorize_Is_Case_Insensitive_For_Basic_Scheme()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext($"BASIC {EncodeCredentials(ValidUsername, ValidPassword)}");

        var result = filter.Authorize(context);

        result.Should().BeTrue();
    }

    // ── Invalid credentials ────────────────────────────────────────────

    [Fact]
    public void Authorize_Returns_False_When_Username_Is_Wrong()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext($"Basic {EncodeCredentials("wronguser", ValidPassword)}");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Authorize_Returns_False_When_Password_Is_Wrong()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext($"Basic {EncodeCredentials(ValidUsername, "wrongpassword")}");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Authorize_Returns_False_When_Both_Credentials_Are_Wrong()
    {
        var filter = CreateFilter();
        var (context, _) = CreateDashboardContext($"Basic {EncodeCredentials("wrong", "wrong")}");

        var result = filter.Authorize(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void Authorize_Sets_401_When_Credentials_Are_Wrong()
    {
        var filter = CreateFilter();
        var (context, httpContext) = CreateDashboardContext($"Basic {EncodeCredentials("wrong", "wrong")}");

        filter.Authorize(context);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    // ── Password with colon ────────────────────────────────────────────

    [Fact]
    public void Authorize_Handles_Password_With_Colon()
    {
        var passwordWithColon = "p:a:s:s:w:o:r:d";
        var filter = CreateFilter(ValidUsername, passwordWithColon);
        var (context, _) = CreateDashboardContext($"Basic {EncodeCredentials(ValidUsername, passwordWithColon)}");

        var result = filter.Authorize(context);

        result.Should().BeTrue();
    }
}

