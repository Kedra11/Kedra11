using AntEmpire.Ants;
using AntEmpire.Colony;
using AntEmpire.Economy;
using AntEmpire.ResourceNodes;
using AntEmpire.UI;
using UnityEngine;

namespace AntEmpire.Core
{
    /// <summary>
    /// One-component playable prototype (version 0.1 goal: "ants run for
    /// leaves, bring them home, food grows, progress survives restarts").
    ///
    /// Usage: create an EMPTY scene, add an empty GameObject, attach this
    /// script, press Play. It builds the whole test world from primitives:
    /// ground, nest, resource nodes, ants, camera, light and HUD.
    ///
    /// This is a development sandbox — real scenes (Boot, Underground,
    /// Surface) will be authored in the editor later and this script is not
    /// part of them.
    /// </summary>
    public class PrototypeSandbox : MonoBehaviour
    {
        [Header("World")]
        [SerializeField] private int leafNodes = 5;
        [SerializeField] private int foodNodes = 3;
        [SerializeField] private float minNodeDistance = 4f;
        [SerializeField] private float maxNodeDistance = 9f;

        // The underground cutaway lives far from the meadow; the camera
        // teleports between the two views.
        private static readonly Vector3 UndergroundCenter = new Vector3(80f, 0f, 0f);

        private void Awake()
        {
            EnsureGameManager();
            BuildGround();
            BuildNest();
            BuildResourceNodes();
            BuildSpawner();
            BuildUnderground();
            BuildEvents();
            SetupCameraAndLight();
            gameObject.AddComponent<SandboxHud>();
        }

        private static void BuildUnderground()
        {
            var underground = new GameObject("UndergroundView");
            underground.transform.position = UndergroundCenter;
            underground.AddComponent<UndergroundView>();
        }

        private static void BuildEvents()
        {
            new GameObject("Events").AddComponent<AntEmpire.Events.SpiderAttackEvent>();
        }

        private static void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>();
            }
        }

        private static void BuildGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3f, 1f, 3f); // 30×30 units
            ground.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(new Color(0.42f, 0.55f, 0.25f)); // grassy green
        }

        private static void BuildNest()
        {
            GameObject nest = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nest.name = "NestEntrance";
            nest.transform.position = Vector3.zero;
            nest.transform.localScale = new Vector3(1.6f, 0.5f, 1.6f); // flattened mound
            nest.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(new Color(0.45f, 0.3f, 0.15f)); // earth brown
            Object.Destroy(nest.GetComponent<Collider>());
            nest.AddComponent<NestEntrance>();

            // The queen sits on top of the mound: a dark sphere for now,
            // her logic (hatching workers) lives in QueenController.
            GameObject queen = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            queen.name = "Queen";
            queen.transform.position = new Vector3(0f, 0.35f, 0f);
            queen.transform.localScale = new Vector3(0.45f, 0.4f, 0.6f);
            queen.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(new Color(0.2f, 0.1f, 0.05f));
            Object.Destroy(queen.GetComponent<Collider>());
            queen.AddComponent<QueenController>();
        }

        private void BuildResourceNodes()
        {
            Material leafMaterial = MakeMaterial(new Color(0.15f, 0.65f, 0.2f));
            Material foodMaterial = MakeMaterial(new Color(0.9f, 0.75f, 0.2f));

            for (int i = 0; i < leafNodes; i++)
            {
                BuildNode(ResourceType.Leaves, PrimitiveType.Cube,
                    new Vector3(0.7f, 0.35f, 0.7f), leafMaterial, amount: 20);
            }
            for (int i = 0; i < foodNodes; i++)
            {
                BuildNode(ResourceType.Food, PrimitiveType.Sphere,
                    new Vector3(0.55f, 0.55f, 0.55f), foodMaterial, amount: 15);
            }
        }

        private void BuildNode(ResourceType type, PrimitiveType shape, Vector3 scale, Material material, double amount)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minNodeDistance, maxNodeDistance);
            Vector3 position = new Vector3(direction.x * distance, scale.y * 0.5f, direction.y * distance);

            GameObject nodeObject = GameObject.CreatePrimitive(shape);
            nodeObject.name = $"{type}Node";
            nodeObject.transform.position = position;
            nodeObject.transform.localScale = scale;
            nodeObject.GetComponent<Renderer>().sharedMaterial = material;
            Object.Destroy(nodeObject.GetComponent<Collider>());

            nodeObject.AddComponent<ResourceNode>().Configure(
                type, amount, regrowDelay: 25f,
                respawnCenter: Vector3.zero,
                respawnMinDistance: minNodeDistance,
                respawnMaxDistance: maxNodeDistance);
        }

        private static void BuildSpawner()
        {
            new GameObject("AntSpawner").AddComponent<AntSpawner>();
        }

        private static void SetupCameraAndLight()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
            }
            camera.transform.position = new Vector3(0f, 14f, -11f);
            camera.transform.LookAt(Vector3.zero);
            camera.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            var switcher = camera.gameObject.AddComponent<CameraViewSwitcher>();
            switcher.Configure(
                surfacePose: new Pose(camera.transform.position, camera.transform.rotation),
                undergroundPose: new Pose(
                    UndergroundCenter + new Vector3(0f, 3.6f, -14.5f),
                    Quaternion.LookRotation(Vector3.forward, Vector3.up)));

            if (Object.FindAnyObjectByType<Light>() == null)
            {
                var light = new GameObject("Sun").AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(55f, -30f, 0f);
                light.intensity = 1.1f;
            }
        }

        private static Material MakeMaterial(Color color)
        {
            return new Material(Shader.Find("Standard")) { color = color };
        }
    }
}
