using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// Builds a tiny primitive "ant" with a visible shape per evolution
    /// level (CLAUDE.md, design doc: 5 visual stages):
    ///   1 — small brown ant
    ///   2 — bigger
    ///   3 — + mandibles
    ///   4 — + armor plates
    ///   5 — big golden glowing elite
    /// Colliders are stripped — ants don't need physics on mobile.
    ///
    /// Replace by assigning a real prefab on AntSpawner later.
    /// </summary>
    public class PlaceholderAntVisual : MonoBehaviour
    {
        [SerializeField] private Color bodyColor = new Color(0.35f, 0.2f, 0.08f);

        private int _builtLevel = -1;

        private void Awake()
        {
            if (transform.childCount == 0)
            {
                ApplyLevel(1);
            }
        }

        /// <summary>Rebuild the body for the given evolution level (no-op if unchanged).</summary>
        public void ApplyLevel(int level)
        {
            if (level == _builtLevel)
            {
                return;
            }
            _builtLevel = level;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            Build(level);
        }

        private void Build(int level)
        {
            // Each level is ~15% bigger; level 5 turns golden and glows.
            float s = 1f + 0.15f * (level - 1);
            Color color = level >= 5 ? new Color(0.85f, 0.65f, 0.2f) : bodyColor;

            Material bodyMaterial = new Material(Shader.Find("Standard")) { color = color };
            if (level >= 5)
            {
                bodyMaterial.EnableKeyword("_EMISSION");
                bodyMaterial.SetColor("_EmissionColor", color * 0.5f);
            }

            // Abdomen, thorax, head (facing +Z), legs hint.
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.12f, -0.16f) * s, new Vector3(0.22f, 0.16f, 0.26f) * s, bodyMaterial);
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0.02f) * s, new Vector3(0.16f, 0.14f, 0.16f) * s, bodyMaterial);
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.13f, 0.16f) * s, new Vector3(0.14f, 0.13f, 0.14f) * s, bodyMaterial);
            AddPart(PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f) * s, new Vector3(0.3f, 0.03f, 0.2f) * s, bodyMaterial);

            if (level >= 3)
            {
                // Mandibles: two small jaws in front of the head.
                Material jawMaterial = new Material(Shader.Find("Standard"))
                {
                    color = Color.Lerp(color, Color.black, 0.35f)
                };
                AddPart(PrimitiveType.Cube, new Vector3(-0.045f, 0.12f, 0.25f) * s, new Vector3(0.03f, 0.03f, 0.12f) * s, jawMaterial);
                AddPart(PrimitiveType.Cube, new Vector3(0.045f, 0.12f, 0.25f) * s, new Vector3(0.03f, 0.03f, 0.12f) * s, jawMaterial);
            }

            if (level >= 4)
            {
                // Armor plates on the back.
                Material armorMaterial = new Material(Shader.Find("Standard"))
                {
                    color = new Color(0.55f, 0.55f, 0.6f)
                };
                AddPart(PrimitiveType.Cube, new Vector3(0f, 0.21f, -0.16f) * s, new Vector3(0.2f, 0.04f, 0.24f) * s, armorMaterial);
                AddPart(PrimitiveType.Cube, new Vector3(0f, 0.2f, 0.02f) * s, new Vector3(0.15f, 0.04f, 0.14f) * s, armorMaterial);
            }
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
