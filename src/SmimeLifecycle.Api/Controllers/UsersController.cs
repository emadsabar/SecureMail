using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmimeLifecycle.Application.Enrollments;
using SmimeLifecycle.Application.Users;

namespace SmimeLifecycle.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<UserListItem>> List(CancellationToken cancellationToken) =>
        mediator.Send(new GetUsersQuery(), cancellationToken);

    public sealed record SyncUserRequest(string UserPrincipalName);

    [HttpPost("sync")]
    public async Task<ActionResult<Guid>> Sync([FromBody] SyncUserRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new SyncEligibleUserCommand(request.UserPrincipalName), cancellationToken);
        return Ok(id);
    }

    public sealed record StartEnrollmentRequest(Guid? DeviceId, bool IsRenewal);

    [HttpPost("{userId:guid}/enrollments")]
    public async Task<ActionResult<Guid>> StartEnrollment(
        Guid userId,
        [FromBody] StartEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new StartEnrollmentCommand(userId, request.DeviceId, request.IsRenewal), cancellationToken);
        return Ok(id);
    }
}
