using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmimeLifecycle.Application.Certificates;
using SmimeLifecycle.Application.Devices;
using SmimeLifecycle.Application.Enrollments;
using SmimeLifecycle.Application.Outlook;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Api.Controllers;

[ApiController]
[Route("api/agent")]
public sealed class AgentController(IMediator mediator) : ControllerBase
{
    public sealed record RegisterRequest(
        Guid DirectoryUserId,
        string Hostname,
        string OsVersion,
        string AgentVersion,
        string? AdComputerObjectGuid,
        string EnrollmentSecret);

    [HttpPost("devices")]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new RegisterDeviceCommand(
                request.DirectoryUserId,
                request.Hostname,
                request.OsVersion,
                request.AgentVersion,
                request.AdComputerObjectGuid,
                request.EnrollmentSecret),
            cancellationToken);
        return Ok(id);
    }

    [HttpGet("devices/{deviceId:guid}/pending-enrollment")]
    public async Task<ActionResult<PendingEnrollmentDto>> Pending(Guid deviceId, CancellationToken cancellationToken)
    {
        var pending = await mediator.Send(new GetPendingEnrollmentQuery(deviceId), cancellationToken);
        return pending is null ? NotFound() : Ok(pending);
    }

    public sealed record CsrRequest(Guid EnrollmentId, Guid DeviceId, string CsrPem);

    [HttpPost("csr")]
    public async Task<IActionResult> SubmitCsr([FromBody] CsrRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new SubmitCsrCommand(request.EnrollmentId, request.DeviceId, request.CsrPem), cancellationToken);
        return Accepted();
    }

    [HttpGet("enrollments/{enrollmentId:guid}/certificate")]
    public async Task<ActionResult<IssuedPublicCertificateDto>> Issued(Guid enrollmentId, CancellationToken cancellationToken)
    {
        var issued = await mediator.Send(new GetIssuedPublicCertificateQuery(enrollmentId), cancellationToken);
        return issued is null ? NotFound() : Ok(issued);
    }

    public sealed record InstalledRequest(Guid CertificateId, Guid DeviceId, string Thumbprint);

    [HttpPost("installed")]
    public async Task<IActionResult> Installed([FromBody] InstalledRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RecordInstallationCommand(request.CertificateId, request.DeviceId, request.Thumbprint), cancellationToken);
        return NoContent();
    }

    public sealed record OutlookCheckRequest(
        Guid DirectoryUserId,
        Guid DeviceId,
        OutlookClientProfile Profile,
        OutlookReadinessStatus Status,
        string? SigningThumbprint,
        string? Detail);

    [HttpPost("outlook-check")]
    public async Task<IActionResult> OutlookCheck([FromBody] OutlookCheckRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RecordOutlookCheckCommand(
                request.DirectoryUserId,
                request.DeviceId,
                request.Profile,
                request.Status,
                request.SigningThumbprint,
                request.Detail),
            cancellationToken);
        return NoContent();
    }
}
