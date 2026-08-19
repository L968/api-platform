using ApiPlatform.Gateway.Authentication;

namespace ApiPlatform.Gateway.Tests.Authentication;

public sealed class Pbkdf2SecretVerifierTests
{
    private const string EncodedHash = "pbkdf2-sha256$120000$+uiKCOn1b1MoH1XMkea62g==$lt3bfCjYY5FqUl9lFScL64J/5/q5ZBp1aKFWzB1fPD0=";

    [Fact]
    public void Verify_WithMatchingSecret_ReturnsTrue()
    {
        bool result = ApiKeyValidator.VerifySecret("DemoAccess123!", EncodedHash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WithWrongSecret_ReturnsFalse()
    {
        bool result = ApiKeyValidator.VerifySecret("wrong", EncodedHash);

        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("pbkdf2-sha256$invalid$salt$hash")]
    [InlineData("pbkdf2-sha256$120000$invalid$invalid")]
    public void Verify_WithMalformedHash_ReturnsFalse(string encodedHash)
    {
        bool result = ApiKeyValidator.VerifySecret("secret", encodedHash);

        Assert.False(result);
    }
}
