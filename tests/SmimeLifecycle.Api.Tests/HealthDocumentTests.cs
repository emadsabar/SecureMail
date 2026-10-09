using FluentAssertions;

namespace SmimeLifecycle.Api.Tests;

public class HealthDocumentTests
{
    [Fact]
    public void Product_name_is_lifecycle_not_gateway()
    {
        const string product = "SmimeLifecycle";
        product.Should().NotContain("gateway");
        product.Should().NotContain("DLP");
    }
}
