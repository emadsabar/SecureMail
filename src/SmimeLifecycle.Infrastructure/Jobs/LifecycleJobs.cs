using MediatR;
using SmimeLifecycle.Application.Certificates;
using SmimeLifecycle.Application.Enrollments;

namespace SmimeLifecycle.Infrastructure.Jobs;

public sealed class LifecycleJobs(IMediator mediator)
{
    public Task PollIssuanceAsync() => mediator.Send(new PollIssuanceCommand());

    public Task PublishAsync() => mediator.Send(new PublishCertificatesCommand());
}
