using FluentAssertions;
using SmimeLifecycle.Application.Enrollments;

namespace SmimeLifecycle.Application.Tests;

public class SubmitCsrCommandValidatorTests
{
    [Fact]
    public void Rejects_private_key_pem()
    {
        var validator = new SubmitCsrCommandValidator();
        var result = validator.Validate(new SubmitCsrCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "-----BEGIN PRIVATE KEY-----\nMII\n-----END PRIVATE KEY-----"));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Accepts_certificate_request_pem()
    {
        var validator = new SubmitCsrCommandValidator();
        var result = validator.Validate(new SubmitCsrCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "-----BEGIN CERTIFICATE REQUEST-----\nMII\n-----END CERTIFICATE REQUEST-----"));
        result.IsValid.Should().BeTrue();
    }
}
