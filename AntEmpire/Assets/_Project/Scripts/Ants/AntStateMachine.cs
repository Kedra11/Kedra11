namespace AntEmpire.Ants
{
    /// <summary>
    /// Worker ant behaviour states (see CLAUDE.md — Ant State Machine).
    /// </summary>
    public enum AntState
    {
        Idle,
        SearchResource,
        MoveToResource,
        CollectResource,
        ReturnToNest,
        DepositResource
    }

    /// <summary>
    /// Plain C# state machine driving one ant. No Unity API here so it can
    /// be unit-tested without a scene.
    /// </summary>
    public class AntStateMachine
    {
        public AntState CurrentState { get; private set; } = AntState.Idle;

        // TODO: Tick(deltaTime) — advance the current state.
        // TODO: transitions:
        //       Idle -> SearchResource when a resource node exists
        //       SearchResource -> MoveToResource when target picked
        //       MoveToResource -> CollectResource on arrival
        //       CollectResource -> ReturnToNest when carry capacity reached
        //       ReturnToNest -> DepositResource on nest arrival
        //       DepositResource -> Idle after depositing into ResourceManager
        // TODO: keep decisions cheap; no pathfinding in MVP.
    }
}
