using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmimeLifecycle.Application.Certificates;

namespace SmimeLifecycle.Api.Controllers;

[ApiController]
[Route("api/certificates")]
public sealed class CertificatesController(IMediator mediator) : ControllerBase
{
    public sealed record ConfirmSyncRequest(Guid CertificateId);
    public sealed record RevokeRequest(Guid CertificateId, string Reason);

    [HttpPost("confirm-directory-sync")]
    public async Task<IActionResult> ConfirmSync([FromBody] ConfirmSyncRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ConfirmDirectorySyncCommand(request.CertificateId), cancellationToken);
        return NoContent();
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RevokeCertificateCommand(request.CertificateId, request.Reason), cancellationToken);
        return NoContent();
    }
}
