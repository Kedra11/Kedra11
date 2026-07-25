using AntEmpire.Core;
using AntEmpire.Economy;
using AntEmpire.Rooms;
using UnityEngine;

namespace AntEmpire.Colony
{
    /// <summary>
    /// The "ant farm" panel: a side cutaway of the nest, built from
    /// primitives far away from the surface meadow (the camera teleports
    /// between the two views).
    ///
    /// It is a live visualization, not a separate simulation:
    /// - the Queen Chamber grows with its room level, the queen sits inside;
    /// - the Food Storage chamber appears once built, its food pile scales
    ///   with the actual Food stock;
    /// - the Larva Nursery appears once built, its central egg swells with
    ///   the queen's real birth progress.
    ///
    /// Cheap by design: a handful of transform updates per frame.
    /// </summary>
    public class UndergroundView : MonoBehaviour
    {
        private static readonly Vector3 QueenChamberBaseScale = new Vector3(3.4f, 2.3f, 1.1f);
        private static readonly Vector3 SideChamberBaseScale = new Vector3(2.6f, 1.9f, 1.1f);

        private Transform _queenChamber;
        private Transform _foodChamber;
        private Transform _nurseryChamber;
        private Transform _foodPile;
        private Transform _growingEgg;
        private GameObject _foodGroup;
        private GameObject _nurseryGroup;
        private GameObject _tunnelGroup;
        private QueenController _queen;

        private void Start()
        {
            _queen = FindAnyObjectByType<QueenController>();
            Build();
        }

        private void Update()
        {
            GameManager game = GameManager.Instance;
            if (game == null)
            {
                return;
            }

            RoomService rooms = game.RoomService;

            // Queen chamber is always there (the queen lives in it); it grows
            // with each Queen Chamber level, capped so high levels don't
            // balloon over the whole cutaway.
            float queenGrowth = Mathf.Min(1f + 0.06f * rooms.GetLevel(RoomIds.QueenChamber), 1.35f);
            _queenChamber.localScale = QueenChamberBaseScale * queenGrowth;

            // Side chambers exist only once their room is built.
            int storageLevel = rooms.GetLevel(RoomIds.FoodStorage);
            _foodGroup.SetActive(storageLevel > 0);
            if (storageLevel > 0)
            {
                float growth = Mathf.Min(1f + 0.05f * storageLevel, 1.3f);
                _foodChamber.localScale = SideChamberBaseScale * growth;
                float fraction = (float)(game.ResourceManager.Get(ResourceType.Food) / rooms.FoodCapacity);
                _foodPile.localScale = Vector3.one * Mathf.Lerp(0.25f, 1.2f, Mathf.Clamp01(fraction));
            }

            int nurseryLevel = rooms.GetLevel(RoomIds.LarvaNursery);
            _nurseryGroup.SetActive(nurseryLevel > 0);
            if (nurseryLevel > 0)
            {
                float growth = Mathf.Min(1f + 0.05f * nurseryLevel, 1.3f);
                _nurseryChamber.localScale = SideChamberBaseScale * growth;
                float progress = _queen != null ? _queen.BirthProgress : 0f;
                _growingEgg.localScale = Vector3.one * Mathf.Lerp(0.15f, 0.55f, progress);
            }

            _tunnelGroup.SetActive(rooms.GetLevel(RoomIds.TunnelHub) > 0);
        }

        // ---- Construction -----------------------------------------------------

        private void Build()
        {
            Color dirt = new Color(0.42f, 0.30f, 0.18f);
            Color cavity = new Color(0.24f, 0.16f, 0.09f);
            Color grass = new Color(0.42f, 0.55f, 0.25f);
            Color queenColor = new Color(0.2f, 0.1f, 0.05f);
            Color foodColor = new Color(0.9f, 0.75f, 0.2f);
            Color eggColor = new Color(0.93f, 0.9f, 0.78f);

            // Dirt wall (the "glass" side of the farm) and the grass line on top.
            AddPart(PrimitiveType.Cube, new Vector3(0f, 0.5f, 1.2f), new Vector3(22f, 13f, 0.6f), dirt);
            AddPart(PrimitiveType.Cube, new Vector3(0f, 7.05f, 1.2f), new Vector3(22f, 0.5f, 0.8f), grass);

            // Entrance shaft and tunnels, dug into the wall.
            AddPart(PrimitiveType.Cube, new Vector3(0f, 5.9f, 0.4f), new Vector3(0.9f, 2.6f, 0.7f), cavity);
            AddPart(PrimitiveType.Cube, new Vector3(0f, 3.4f, 0.4f), new Vector3(0.9f, 3.0f, 0.7f), cavity);
            AddPart(PrimitiveType.Cube, new Vector3(-2.7f, 3.8f, 0.4f), new Vector3(4.4f, 0.8f, 0.7f), cavity);
            AddPart(PrimitiveType.Cube, new Vector3(2.7f, 3.8f, 0.4f), new Vector3(4.4f, 0.8f, 0.7f), cavity);

            // Queen chamber (always present) with the queen inside.
            _queenChamber = AddPart(PrimitiveType.Sphere, new Vector3(0f, 1.6f, 0.4f), QueenChamberBaseScale, cavity);
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 1.25f, 0.3f), new Vector3(0.85f, 0.6f, 1.1f), queenColor);

            // Food storage chamber (left) with its food pile.
            _foodGroup = new GameObject("FoodStorageChamber");
            _foodGroup.transform.SetParent(transform, worldPositionStays: false);
            _foodChamber = AddPart(PrimitiveType.Sphere, new Vector3(-5.4f, 3.8f, 0.4f), SideChamberBaseScale, cavity, _foodGroup.transform);
            _foodPile = AddPart(PrimitiveType.Sphere, new Vector3(-5.4f, 3.45f, 0.3f), Vector3.one * 0.6f, foodColor, _foodGroup.transform);

            // Larva nursery chamber (right) with two resting eggs and the
            // growing one in the middle.
            _nurseryGroup = new GameObject("LarvaNurseryChamber");
            _nurseryGroup.transform.SetParent(transform, worldPositionStays: false);
            _nurseryChamber = AddPart(PrimitiveType.Sphere, new Vector3(5.4f, 3.8f, 0.4f), SideChamberBaseScale, cavity, _nurseryGroup.transform);
            AddPart(PrimitiveType.Sphere, new Vector3(4.8f, 3.5f, 0.3f), Vector3.one * 0.3f, eggColor, _nurseryGroup.transform);
            AddPart(PrimitiveType.Sphere, new Vector3(6.0f, 3.5f, 0.3f), Vector3.one * 0.3f, eggColor, _nurseryGroup.transform);
            _growingEgg = AddPart(PrimitiveType.Sphere, new Vector3(5.4f, 3.55f, 0.3f), Vector3.one * 0.15f, eggColor, _nurseryGroup.transform);

            // Tunnel Hub: extra dug corridors below the queen chamber,
            // visible once the room is built.
            _tunnelGroup = new GameObject("TunnelHub");
            _tunnelGroup.transform.SetParent(transform, worldPositionStays: false);
            AddPart(PrimitiveType.Cube, new Vector3(-3f, 0.6f, 0.4f), new Vector3(5.5f, 0.7f, 0.7f), cavity, _tunnelGroup.transform);
            AddPart(PrimitiveType.Cube, new Vector3(3f, 0.6f, 0.4f), new Vector3(5.5f, 0.7f, 0.7f), cavity, _tunnelGroup.transform);
            AddPart(PrimitiveType.Sphere, new Vector3(-6f, 0.6f, 0.4f), new Vector3(1.6f, 1.2f, 1f), cavity, _tunnelGroup.transform);
            AddPart(PrimitiveType.Sphere, new Vector3(6f, 0.6f, 0.4f), new Vector3(1.6f, 1.2f, 1f), cavity, _tunnelGroup.transform);

            // The cutaway faces away from the sun, so give it its own light.
            var lightObject = new GameObject("UndergroundLight");
            lightObject.transform.SetParent(transform, worldPositionStays: false);
            lightObject.transform.localPosition = new Vector3(0f, 3.5f, -6f);
            var pointLight = lightObject.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.range = 30f;
            pointLight.intensity = 1.6f;
        }

        private Transform AddPart(PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, Transform parent = null)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.transform.SetParent(parent != null ? parent : transform, worldPositionStays: false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Destroy(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Standard")) { color = color };
            return part.transform;
        }
    }
}
