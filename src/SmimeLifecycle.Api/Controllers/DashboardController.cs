using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmimeLifecycle.Application.Dashboard;

namespace SmimeLifecycle.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<DashboardSummary> Get(CancellationToken cancellationToken) =>
        mediator.Send(new GetDashboardQuery(), cancellationToken);
}
