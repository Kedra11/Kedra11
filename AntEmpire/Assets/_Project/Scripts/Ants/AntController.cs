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

        private const float ArriveDistance = 0.35f;
        private const float TurnSpeedDegrees = 540f;

        public AntState CurrentState => _stateMachine?.CurrentState ?? AntState.Idle;
        public AntTypeData TypeData => typeData;

        /// <summary>Resource this worker is dedicated to by the player, or null
        /// for auto (nearest of any type). Set by AntSpawner from WorkforceService.</summary>
        public ResourceType? AssignedResource { get; set; }

        /// <summary>Inject config when spawning from code (sandbox / spawner).</summary>
        public void Initialize(AntTypeData data)
        {
            typeData = data;
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
            double taken = _target.TakeUpTo(typeData.carryCapacity);
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

            transform.position = Vector3.MoveTowards(
                transform.position, destination, typeData.moveSpeed * deltaTime);

            // Face the walking direction so the placeholder body reads as "running".
            Quaternion look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look, TurnSpeedDegrees * deltaTime);

            return false;
        }
    }
}
