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

        private Vector3 _fullScale;

        public ResourceType Type => type;
        public double Remaining { get; private set; }
        public bool IsDepleted => Remaining <= 0;

        /// <summary>Configure a runtime-created node (used by the sandbox builder).</summary>
        public void Configure(ResourceType resourceType, double amount, float regrowDelay)
        {
            type = resourceType;
            startingAmount = amount;
            regrowSeconds = regrowDelay;
            Remaining = amount;
            UpdateVisual();
        }

        private void Awake()
        {
            _fullScale = transform.localScale;
            Remaining = startingAmount;
        }

        private void OnEnable()
        {
            Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        /// <summary>Nearest node with resources left, or null. Plain list scan — cheap for MVP node counts.</summary>
        public static ResourceNode FindNearest(Vector3 position)
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

            if (IsDepleted && regrowSeconds > 0 && isActiveAndEnabled)
            {
                StartCoroutine(RegrowAfterDelay());
            }

            return taken;
        }

        private IEnumerator RegrowAfterDelay()
        {
            yield return new WaitForSeconds(regrowSeconds);
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
