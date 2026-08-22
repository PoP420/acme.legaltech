using System;
using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Acme.LegalTech.HttpApi.Tests;

public class AuthApiTest
{
    [Fact]
    public async Task Login_Success_ReturnsToken()
    {
        var client = new ApiClient(TestConfiguration.BaseUrl, TestConfiguration.IgnoreSsl);

        await client.LoginAsync(TestConfiguration.AdminUser, TestConfiguration.AdminPassword);

        client.Token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_BadPassword_ReturnsUnauthorized()
    {
        var client = new ApiClient(TestConfiguration.BaseUrl, TestConfiguration.IgnoreSsl);

        var response = await client.LoginRawAsync(TestConfiguration.AdminUser, "NotTheRightPassword123!");

        // The token endpoint rejects invalid credentials with 400 (invalid_grant)
        // or 401 depending on configuration; both indicate failed authentication.
        ((int)response.StatusCode).ShouldBeOneOf(400, 401);
    }
}
