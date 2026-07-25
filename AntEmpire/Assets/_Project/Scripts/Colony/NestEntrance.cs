using UnityEngine;

namespace AntEmpire.Colony
{
    /// <summary>
    /// Marker for the colony's surface entrance — where ants deposit
    /// resources and where new ants appear. One per scene.
    /// </summary>
    public class NestEntrance : MonoBehaviour
    {
        public static NestEntrance Instance { get; private set; }

        [Tooltip("How close an ant must get to count as 'home'.")]
        [SerializeField] private float arrivalRadius = 0.4f;

        public float ArrivalRadius => arrivalRadius;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[NestEntrance] More than one nest entrance in the scene; using the newest.");
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
