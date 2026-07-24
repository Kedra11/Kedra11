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
    /// Everything the state machine needs from the world. Implemented by
    /// AntController (MonoBehaviour); kept as an interface so the state
    /// machine itself stays pure C# and unit-testable without a scene.
    /// </summary>
    public interface IAntAgent
    {
        /// <summary>Pick a resource node to walk to. False if none exist right now.</summary>
        bool TryAcquireResourceTarget();

        /// <summary>True while the acquired target still exists and has resources.</summary>
        bool TargetStillValid { get; }

        /// <summary>Advance towards the target. Returns true on arrival.</summary>
        bool StepTowardsTarget(float deltaTime);

        /// <summary>Advance towards the nest. Returns true on arrival.</summary>
        bool StepTowardsNest(float deltaTime);

        /// <summary>Take resources from the target into the ant's "backpack".</summary>
        void CollectFromTarget();

        /// <summary>Empty the backpack into the colony's ResourceManager.</summary>
        void DepositAtNest();

        /// <summary>Seconds the ant spends gathering at a node.</summary>
        float CollectDuration { get; }

        /// <summary>Seconds the ant rests before looking for work again.</summary>
        float IdleDuration { get; }
    }

    /// <summary>
    /// Plain C# state machine driving one ant. Cheap by design: one switch
    /// per tick, no allocations, no physics, no pathfinding — the heavy
    /// operation (nearest-node search) only happens in SearchResource.
    /// </summary>
    public class AntStateMachine
    {
        private readonly IAntAgent _agent;
        private float _stateTimer;

        public AntState CurrentState { get; private set; } = AntState.Idle;

        public AntStateMachine(IAntAgent agent)
        {
            _agent = agent;
        }

        public void Tick(float deltaTime)
        {
            switch (CurrentState)
            {
                case AntState.Idle:
                    _stateTimer += deltaTime;
                    if (_stateTimer >= _agent.IdleDuration)
                    {
                        TransitionTo(AntState.SearchResource);
                    }
                    break;

                case AntState.SearchResource:
                    // If nothing to gather, rest a bit and try again —
                    // avoids hammering the search every frame.
                    TransitionTo(_agent.TryAcquireResourceTarget()
                        ? AntState.MoveToResource
                        : AntState.Idle);
                    break;

                case AntState.MoveToResource:
                    if (!_agent.TargetStillValid)
                    {
                        // Another ant emptied the node while we walked.
                        TransitionTo(AntState.SearchResource);
                        break;
                    }
                    if (_agent.StepTowardsTarget(deltaTime))
                    {
                        TransitionTo(AntState.CollectResource);
                    }
                    break;

                case AntState.CollectResource:
                    if (!_agent.TargetStillValid)
                    {
                        TransitionTo(AntState.SearchResource);
                        break;
                    }
                    _stateTimer += deltaTime;
                    if (_stateTimer >= _agent.CollectDuration)
                    {
                        _agent.CollectFromTarget();
                        TransitionTo(AntState.ReturnToNest);
                    }
                    break;

                case AntState.ReturnToNest:
                    if (_agent.StepTowardsNest(deltaTime))
                    {
                        TransitionTo(AntState.DepositResource);
                    }
                    break;

                case AntState.DepositResource:
                    _agent.DepositAtNest();
                    TransitionTo(AntState.Idle);
                    break;
            }
        }

        private void TransitionTo(AntState next)
        {
            CurrentState = next;
            _stateTimer = 0f;
        }
    }
}
