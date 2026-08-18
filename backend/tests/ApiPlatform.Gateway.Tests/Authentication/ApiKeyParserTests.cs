using ApiPlatform.Gateway.Authentication;

namespace ApiPlatform.Gateway.Tests.Authentication;

public sealed class ApiKeyParserTests
{
    [Fact]
    public void Parse_WithValidKey_ReturnsBothParts()
    {
        ApiKeyParts? result = ApiKeyValidator.ParseApiKey("app_public.sk_secret");

        Assert.NotNull(result);
        Assert.Equal("app_public", result.ClientId);
        Assert.Equal("sk_secret", result.Secret);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("app_public")]
    [InlineData("app_public.invalid")]
    [InlineData("invalid.sk_secret")]
    [InlineData("app_public.sk_secret.extra")]
    public void Parse_WithInvalidKey_ReturnsNull(string apiKey)
    {
        ApiKeyParts? result = ApiKeyValidator.ParseApiKey(apiKey);

        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithOversizedPart_ReturnsNull()
    {
        string apiKey = $"app_{new string('a', 100)}.sk_secret";

        ApiKeyParts? result = ApiKeyValidator.ParseApiKey(apiKey);

        Assert.Null(result);
    }
}
