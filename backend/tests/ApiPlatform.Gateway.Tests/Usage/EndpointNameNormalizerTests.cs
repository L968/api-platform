using ApiPlatform.Gateway.Metering;
using Microsoft.AspNetCore.Http;

namespace ApiPlatform.Gateway.Tests.Usage;

public sealed class EndpointNameNormalizerTests
{
    [Theory]
    [InlineData("GET", "/orders", "GET /orders")]
    [InlineData("GET", "/orders/123", "GET /orders/{id}")]
    [InlineData("GET", "/orders/anything/else", "GET /orders/{id}")]
    [InlineData("POST", "/payments", "POST /payments")]
    public void Normalize_ReturnsLowCardinalityEndpoint(string method, string path, string expected)
    {
        DefaultHttpContext context = new();
        context.Request.Method = method;
        context.Request.Path = path;

        string result = UsageMeterMiddleware.NormalizeEndpoint(context.Request);

        Assert.Equal(expected, result);
    }
}
