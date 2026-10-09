using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Domain.Lifecycle;

public static class CertificateLifecycleMachine
{
    private static readonly Dictionary<LifecycleStatus, LifecycleStatus[]> Allowed = new()
    {
        [LifecycleStatus.Eligible] = [LifecycleStatus.CsrGenerated, LifecycleStatus.Failed],
        [LifecycleStatus.CsrGenerated] = [LifecycleStatus.SubmittedToCa, LifecycleStatus.Failed],
        [LifecycleStatus.SubmittedToCa] = [LifecycleStatus.Issued, LifecycleStatus.Failed],
        [LifecycleStatus.Issued] = [LifecycleStatus.Installed, LifecycleStatus.Failed, LifecycleStatus.Revoked],
        [LifecycleStatus.Installed] =
        [
            LifecycleStatus.PublishedToDirectory, LifecycleStatus.Failed, LifecycleStatus.Revoked
        ],
        [LifecycleStatus.PublishedToDirectory] =
        [
            LifecycleStatus.DirectorySyncPending, LifecycleStatus.Failed, LifecycleStatus.Revoked
        ],
        [LifecycleStatus.DirectorySyncPending] =
        [
            LifecycleStatus.Active, LifecycleStatus.Failed, LifecycleStatus.Revoked
        ],
        [LifecycleStatus.Active] =
        [
            LifecycleStatus.Renewing, LifecycleStatus.Replacing, LifecycleStatus.Revoked, LifecycleStatus.Expired
        ],
        [LifecycleStatus.Renewing] =
        [
            LifecycleStatus.CsrGenerated, LifecycleStatus.Issued, LifecycleStatus.Failed, LifecycleStatus.Active
        ],
        [LifecycleStatus.Replacing] =
        [
            LifecycleStatus.CsrGenerated, LifecycleStatus.Issued, LifecycleStatus.Failed, LifecycleStatus.Active
        ],
        [LifecycleStatus.Revoked] = [],
        [LifecycleStatus.Expired] = [LifecycleStatus.Renewing],
        [LifecycleStatus.Failed] = [LifecycleStatus.Eligible, LifecycleStatus.CsrGenerated, LifecycleStatus.SubmittedToCa]
    };

    public static bool CanTransition(LifecycleStatus from, LifecycleStatus to) =>
        Allowed.TryGetValue(from, out var next) && next.Contains(to);

    public static LifecycleStatus Transition(LifecycleStatus from, LifecycleStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidLifecycleTransitionException(from.ToString(), to.ToString());
        }

        return to;
    }
}
