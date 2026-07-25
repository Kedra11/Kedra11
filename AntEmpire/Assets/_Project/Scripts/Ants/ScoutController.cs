using AntEmpire.Colony;
using AntEmpire.Core;
using AntEmpire.Economy;
using AntEmpire.Events;
using AntEmpire.ResourceNodes;
using AntEmpire.UI;
using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// Scout ant: doesn't gather or fight. Roams far from the nest and
    /// periodically discovers a rich one-time find where it stands:
    /// a Golden Leaf (big pile of Leaves) or spilled Honey (big pile of
    /// Food). Finds vanish once emptied — workers should hurry.
    /// </summary>
    public class ScoutController : MonoBehaviour
    {
        private const float MoveSpeed = 2.7f;
        private const float MinWanderRadius = 8f;
        private const float MaxWanderRadius = 13f;
        private const float MinDiscoverySeconds = 40f;
        private const float MaxDiscoverySeconds = 75f;
        private const float MinFindDistanceFromNest = 7f;

        private Vector3 _wanderTarget;
        private float _discoveryTimer;

        private void Start()
        {
            PickWanderPoint();
            _discoveryTimer = Random.Range(MinDiscoverySeconds, MaxDiscoverySeconds) * 0.5f; // first find sooner
        }

        private void Update()
        {
            if (StepTowards(_wanderTarget, 0.4f))
            {
                PickWanderPoint();
            }

            _discoveryTimer -= Time.deltaTime;
            if (_discoveryTimer <= 0f && FarEnoughFromNest())
            {
                _discoveryTimer = Random.Range(MinDiscoverySeconds, MaxDiscoverySeconds);
                Discover();
            }
        }

        private void Discover()
        {
            bool honey = Random.value < 0.3f;

            GameObject nodeObject = GameObject.CreatePrimitive(honey ? PrimitiveType.Sphere : PrimitiveType.Cube);
            nodeObject.name = honey ? "HoneyFind" : "GoldenLeafFind";
            nodeObject.transform.position = transform.position + new Vector3(0.4f, 0f, 0.4f);
            nodeObject.transform.localScale = honey
                ? new Vector3(0.8f, 0.6f, 0.8f)
                : new Vector3(0.9f, 0.3f, 0.9f);
            Destroy(nodeObject.GetComponent<Collider>());

            Color color = honey ? new Color(0.95f, 0.6f, 0.1f) : new Color(1f, 0.85f, 0.25f);
            var material = new Material(Shader.Find("Standard")) { color = color };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.4f);
            nodeObject.GetComponent<Renderer>().sharedMaterial = material;

            // regrowDelay 0 = one-time node, destroyed when emptied.
            nodeObject.AddComponent<ResourceNode>().Configure(
                honey ? ResourceType.Food : ResourceType.Leaves,
                honey ? 80 : 90,
                regrowDelay: 0f,
                respawnCenter: nodeObject.transform.position,
                respawnMinDistance: 0f,
                respawnMaxDistance: 0f);

            FloatingWorldText.Spawn(
                nodeObject.transform.position + Vector3.up * 1.1f,
                honey ? "Honey!" : "Golden Leaf!",
                color);
        }

        private bool FarEnoughFromNest()
        {
            Vector3 nest = NestEntrance.Instance != null ? NestEntrance.Instance.Position : Vector3.zero;
            return (transform.position - nest).sqrMagnitude >=
                   MinFindDistanceFromNest * MinFindDistanceFromNest;
        }

        private void PickWanderPoint()
        {
            Vector3 center = NestEntrance.Instance != null ? NestEntrance.Instance.Position : Vector3.zero;
            Vector2 direction = Random.insideUnitCircle.normalized;
            float distance = Random.Range(MinWanderRadius, MaxWanderRadius);
            _wanderTarget = center + new Vector3(direction.x * distance, 0f, direction.y * distance);
        }

        private bool StepTowards(Vector3 destination, float arriveDistance)
        {
            destination.y = transform.position.y;
            Vector3 toTarget = destination - transform.position;
            if (toTarget.sqrMagnitude <= arriveDistance * arriveDistance)
            {
                return true;
            }

            float speed = MoveSpeed * RainEvent.SpeedFactor *
                (GameManager.Instance != null ? GameManager.Instance.RoomService.SpeedBonus : 1f);
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            Quaternion look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 540f * Time.deltaTime);
            return false;
        }
    }
}
