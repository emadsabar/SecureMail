using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmimeLifecycle.Application.Audit;

namespace SmimeLifecycle.Api.Controllers;

[ApiController]
[Route("api/audit")]
public sealed class AuditController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<AuditLogItem>> Get(CancellationToken cancellationToken) =>
        mediator.Send(new GetAuditLogQuery(), cancellationToken);
}
