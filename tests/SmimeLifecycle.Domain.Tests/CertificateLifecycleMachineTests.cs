using FluentAssertions;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;
using SmimeLifecycle.Domain.Lifecycle;

namespace SmimeLifecycle.Domain.Tests;

public class CertificateLifecycleMachineTests
{
    [Fact]
    public void Phase1_happy_path_reaches_active()
    {
        var status = LifecycleStatus.Eligible;
        status = CertificateLifecycleMachine.Transition(status, LifecycleStatus.CsrGenerated);
        status = CertificateLifecycleMachine.Transition(status, LifecycleStatus.SubmittedToCa);
        status = CertificateLifecycleMachine.Transition(status, LifecycleStatus.Issued);
        status = CertificateLifecycleMachine.Transition(status, LifecycleStatus.Installed);
        status = CertificateLifecycleMachine.Transition(status, LifecycleStatus.PublishedToDirectory);
        status = CertificateLifecycleMachine.Transition(status, LifecycleStatus.DirectorySyncPending);
        status = CertificateLifecycleMachine.Transition(status, LifecycleStatus.Active);
        status.Should().Be(LifecycleStatus.Active);
    }

    [Fact]
    public void Cannot_skip_from_eligible_to_active()
    {
        var act = () => CertificateLifecycleMachine.Transition(LifecycleStatus.Eligible, LifecycleStatus.Active);
        act.Should().Throw<InvalidLifecycleTransitionException>();
    }
}
