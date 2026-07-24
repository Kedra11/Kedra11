using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// Builds a tiny primitive "ant" (three body segments + legs hint) so the
    /// prototype is playable before any real 3D models exist (CLAUDE.md art
    /// direction, placeholder stage). Colliders are stripped — ants don't
    /// need physics and colliders would cost on mobile.
    ///
    /// Replace by assigning a real prefab on AntSpawner later.
    /// </summary>
    public class PlaceholderAntVisual : MonoBehaviour
    {
        [SerializeField] private Color bodyColor = new Color(0.35f, 0.2f, 0.08f);

        private void Awake()
        {
            if (transform.childCount > 0)
            {
                return; // Real art already attached.
            }

            Material bodyMaterial = new Material(Shader.Find("Standard")) { color = bodyColor };

            // Abdomen — rear, biggest segment.
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.12f, -0.16f), new Vector3(0.22f, 0.16f, 0.26f), bodyMaterial);
            // Thorax — middle.
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0.02f), new Vector3(0.16f, 0.14f, 0.16f), bodyMaterial);
            // Head — front, facing +Z (the walking direction).
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.13f, 0.16f), new Vector3(0.14f, 0.13f, 0.14f), bodyMaterial);
            // Legs hint — a flattened box under the thorax.
            AddPart(PrimitiveType.Cube, new Vector3(0f, 0.05f, 0.0f), new Vector3(0.3f, 0.03f, 0.2f), bodyMaterial);
        }

        private void AddPart(PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = type.ToString();
            part.transform.SetParent(transform, worldPositionStays: false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;

            Destroy(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
