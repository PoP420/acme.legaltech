using System;

namespace Acme.LegalTech.HttpApi.Tests;

public static class TestConfiguration
{
    private static string Env(string key, string fallback)
        => Environment.GetEnvironmentVariable(key) ?? fallback;

    public static string BaseUrl => Env("ApiBaseUrl", "https://localhost:44334");

    public static string AdminUser => Env("ApiUser", "admin@abp.io");

    public static string AdminPassword => Env("ApiPassword", "1q2w3E*");

    public static bool IgnoreSsl
    {
        get
        {
            var raw = Environment.GetEnvironmentVariable("ApiIgnoreSsl");
            if (bool.TryParse(raw, out var explicitValue))
            {
                return explicitValue;
            }

            return BaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                || BaseUrl.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
