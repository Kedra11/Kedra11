using AntEmpire.Colony;
using AntEmpire.Core;
using AntEmpire.Economy;
using AntEmpire.ResourceNodes;
using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// MonoBehaviour on every ant. Thin by design: all decisions live in
    /// AntStateMachine, all carry logic in WorkerAntCollector; this class
    /// only implements IAntAgent (movement + world queries) and ticks the
    /// state machine.
    ///
    /// Per-frame cost is one switch and at most one Vector3.MoveTowards —
    /// fine for hundreds of ants on mobile. The expensive nearest-node scan
    /// only runs in the SearchResource state.
    /// </summary>
    public class AntController : MonoBehaviour, IAntAgent
    {
        [SerializeField] private AntTypeData typeData;

        private AntStateMachine _stateMachine;
        private WorkerAntCollector _collector;
        private ResourceNode _target;
        private float _speedMultiplier = 1f;
        private int _carryCapacityOverride; // 0 = use typeData.carryCapacity

        private const float ArriveDistance = 0.35f;
        private const float TurnSpeedDegrees = 540f;

        public AntState CurrentState => _stateMachine?.CurrentState ?? AntState.Idle;
        public AntTypeData TypeData => typeData;

        /// <summary>Resource this worker is dedicated to by the player, or null
        /// for auto (nearest of any type). Set by AntSpawner from WorkforceService.</summary>
        public ResourceType? AssignedResource { get; set; }

        /// <summary>The enemy this ant is currently swarming, or null when working.
        /// While defending, the gathering state machine is paused.</summary>
        public Component CombatTarget { get; private set; }

        private const float MeleeStopDistance = 1.15f;

        /// <summary>Called by the enemy when it recruits this ant into the fight.</summary>
        public void EnterCombat(Component enemy)
        {
            CombatTarget = enemy;
        }

        /// <summary>Called when the enemy dies or flees; the ant resumes working.</summary>
        public void ExitCombat()
        {
            CombatTarget = null;
        }

        /// <summary>Inject config when spawning from code (sandbox / spawner).</summary>
        public void Initialize(AntTypeData data)
        {
            typeData = data;
        }

        /// <summary>Apply the colony's evolution level: stats plus the visual stage.
        /// Called by AntSpawner on spawn and whenever the colony evolves.</summary>
        public void ApplyEvolution(EvolutionLevelData data)
        {
            if (data == null)
            {
                return;
            }
            _speedMultiplier = data.speedMultiplier;
            _carryCapacityOverride = data.carryCapacity;

            var visual = GetComponent<PlaceholderAntVisual>();
            if (visual != null)
            {
                visual.ApplyLevel(data.level);
            }
        }

        private void Awake()
        {
            _collector = new WorkerAntCollector();
            _stateMachine = new AntStateMachine(this);
        }

        private void Start()
        {
            if (typeData == null)
            {
                // Fallback so a bare AntController in a test scene still works.
                typeData = ScriptableObject.CreateInstance<AntTypeData>();
            }
        }

        private void Update()
        {
            if (CombatTarget != null)
            {
                // Rush the enemy and crowd it in melee range; the enemy itself
                // counts nearby ants to take bite damage.
                StepTowards(CombatTarget.transform.position, Time.deltaTime, MeleeStopDistance);
                return;
            }

            _stateMachine.Tick(Time.deltaTime);
        }

        // ---- IAntAgent --------------------------------------------------------

        public float CollectDuration => typeData.collectSeconds;

        // Small random spread so ants don't all wake up in lockstep.
        public float IdleDuration => typeData.idleSeconds * Random.Range(0.8f, 1.3f);

        public bool TargetStillValid => _target != null && !_target.IsDepleted;

        public bool TryAcquireResourceTarget()
        {
            // Dedicated workers look for their assigned resource first, and
            // only fall back to "anything nearby" when that type has run dry —
            // an idle ant would read as a bug, not as information.
            _target = AssignedResource.HasValue
                ? ResourceNode.FindNearest(transform.position, AssignedResource.Value)
                : null;

            if (_target == null)
            {
                _target = ResourceNode.FindNearest(transform.position);
            }
            return _target != null;
        }

        public bool StepTowardsTarget(float deltaTime)
        {
            return StepTowards(_target.transform.position, deltaTime, ArriveDistance);
        }

        public bool StepTowardsNest(float deltaTime)
        {
            NestEntrance nest = NestEntrance.Instance;
            if (nest == null)
            {
                return false;
            }
            return StepTowards(nest.Position, deltaTime, nest.ArrivalRadius);
        }

        public void CollectFromTarget()
        {
            int capacity = _carryCapacityOverride > 0 ? _carryCapacityOverride : typeData.carryCapacity;
            double taken = _target.TakeUpTo(capacity);
            if (taken > 0)
            {
                _collector.Take(_target.Type, taken);
            }
        }

        public void DepositAtNest()
        {
            if (GameManager.Instance != null)
            {
                _collector.DepositInto(GameManager.Instance.ResourceManager);
            }
        }

        // ---- Movement ---------------------------------------------------------

        private bool StepTowards(Vector3 destination, float deltaTime, float arriveDistance)
        {
            // Ants walk on the ground plane; ignore height differences.
            destination.y = transform.position.y;

            Vector3 toTarget = destination - transform.position;
            if (toTarget.sqrMagnitude <= arriveDistance * arriveDistance)
            {
                return true;
            }

            float environmentFactor = Events.RainEvent.SpeedFactor *
                (GameManager.Instance != null ? GameManager.Instance.RoomService.SpeedBonus : 1f);
            transform.position = Vector3.MoveTowards(
                transform.position, destination,
                typeData.moveSpeed * _speedMultiplier * environmentFactor * deltaTime);

            // Face the walking direction so the placeholder body reads as "running".
            Quaternion look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look, TurnSpeedDegrees * deltaTime);

            return false;
        }
    }
}
