namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represents the health-state of a container as reported by "docker inspect".
    /// </summary>
    /// <remarks>
    /// The values are ordered from "least usable" to "fully usable" and have explicit numeric values because they can
    /// get persisted respectively transferred.
    /// </remarks>
    public enum ContainerHealthState
    {
        /// <summary>
        /// The container does not exist or its state could not get queried.
        /// </summary>
        NotAvailable = 0,

        /// <summary>
        /// The container exists but neither its image nor its container-definition declares a health-check.
        /// Docker does not report any health-state for such a container, so waiting for it to become
        /// <see cref="Healthy"/> can never succeed.
        /// </summary>
        NoHealthCheckDefined = 1,

        /// <summary>
        /// A health-check is defined and is still within its start-period respectively did not yet succeed.
        /// </summary>
        Starting = 2,

        /// <summary>
        /// A health-check is defined and currently fails.
        /// </summary>
        Unhealthy = 3,

        /// <summary>
        /// The container is usable but not in its full desired state, for example because only a part of its
        /// functionality is available respectively because it is not fully replicated.
        /// </summary>
        /// <remarks>
        /// Docker itself does not report this state (its health-status only knows "starting", "healthy" and
        /// "unhealthy"), so it is currently never returned by <see cref="Utilities.GetContainerHealthState(string)"/>.
        /// It exists for orchestrators which do distinguish this state (comparable to a partially-available workload
        /// in Kubernetes).
        /// </remarks>
        Degraded = 4,

        /// <summary>
        /// A health-check is defined and currently succeeds.
        /// </summary>
        Healthy = 5
    }
}
