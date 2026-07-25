using System.Collections;
using System.Collections.Generic;
using AntEmpire.Economy;
using UnityEngine;

namespace AntEmpire.ResourceNodes
{
    /// <summary>
    /// A gatherable spot on the surface (leaf pile, seed, soil patch...).
    ///
    /// Nodes keep a static registry so ants can find the nearest one with a
    /// simple list scan — no physics queries, no FindObjectsOfType. The node
    /// shrinks visually as it is emptied and regrows after a delay, so the
    /// prototype world never runs dry.
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        private static readonly List<ResourceNode> Active = new List<ResourceNode>();

        [SerializeField] private ResourceType type = ResourceType.Leaves;
        [SerializeField] private double startingAmount = 20;
        [SerializeField] private float regrowSeconds = 25f;
        [Tooltip("When the node regrows, move it to a new random spot instead of the same place.")]
        [SerializeField] private bool relocateOnRegrow = true;

        private Vector3 _fullScale;
        private Vector3 _respawnCenter;
        private float _respawnMinDistance = 4f;
        private float _respawnMaxDistance = 9f;

        public ResourceType Type => type;
        public double Remaining { get; private set; }
        public bool IsDepleted => Remaining <= 0;

        /// <summary>Configure a runtime-created node (used by the sandbox builder).
        /// The respawn ring defines where the node may reappear after depletion.</summary>
        public void Configure(
            ResourceType resourceType, double amount, float regrowDelay,
            Vector3 respawnCenter, float respawnMinDistance, float respawnMaxDistance)
        {
            type = resourceType;
            startingAmount = amount;
            regrowSeconds = regrowDelay;
            _respawnCenter = respawnCenter;
            _respawnMinDistance = respawnMinDistance;
            _respawnMaxDistance = respawnMaxDistance;
            Remaining = amount;
            UpdateVisual();
        }

        private void Awake()
        {
            _fullScale = transform.localScale;
            Remaining = startingAmount;
            // Editor-placed nodes respawn around their own starting spot.
            _respawnCenter = transform.position;
        }

        private void OnEnable()
        {
            Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        /// <summary>Nearest node with resources left, or null. Pass a
        /// <paramref name="filter"/> to only consider one resource type
        /// (used by assigned workers). Plain list scan — cheap for MVP node counts.</summary>
        public static ResourceNode FindNearest(Vector3 position, ResourceType? filter = null)
        {
            ResourceNode nearest = null;
            float nearestSqr = float.MaxValue;

            for (int i = 0; i < Active.Count; i++)
            {
                ResourceNode node = Active[i];
                if (node.IsDepleted)
                {
                    continue;
                }
                if (filter.HasValue && node.type != filter.Value)
                {
                    continue;
                }

                float sqr = (node.transform.position - position).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = node;
                }
            }

            return nearest;
        }

        /// <summary>Remove up to <paramref name="amount"/> from the node; returns what was actually taken.</summary>
        public double TakeUpTo(double amount)
        {
            if (amount <= 0 || IsDepleted)
            {
                return 0;
            }

            double taken = amount < Remaining ? amount : Remaining;
            Remaining -= taken;
            UpdateVisual();

            if (IsDepleted)
            {
                if (regrowSeconds > 0 && isActiveAndEnabled)
                {
                    StartCoroutine(RegrowAfterDelay());
                }
                else
                {
                    // One-time nodes (scout finds like Golden Leaf / Honey)
                    // vanish once emptied.
                    Destroy(gameObject, 0.8f);
                }
            }

            return taken;
        }

        private IEnumerator RegrowAfterDelay()
        {
            yield return new WaitForSeconds(regrowSeconds);

            if (relocateOnRegrow)
            {
                // Reappear somewhere new so gathering routes keep changing.
                Vector2 direction = Random.insideUnitCircle.normalized;
                float distance = Random.Range(_respawnMinDistance, _respawnMaxDistance);
                transform.position = new Vector3(
                    _respawnCenter.x + direction.x * distance,
                    transform.position.y,
                    _respawnCenter.z + direction.y * distance);
            }

            Remaining = startingAmount;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            // Shrink towards 20% as the node empties so players see depletion.
            float fraction = startingAmount > 0 ? (float)(Remaining / startingAmount) : 0f;
            transform.localScale = _fullScale * Mathf.Lerp(0.2f, 1f, fraction);
        }
    }
}
