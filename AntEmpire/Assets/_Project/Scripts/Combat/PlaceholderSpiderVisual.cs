using UnityEngine;

namespace AntEmpire.Combat
{
    /// <summary>
    /// Primitive spider: dark round body, small head, eight stick legs.
    /// Noticeably bigger than the ants so the threat reads instantly.
    /// Colliders stripped, same as all placeholder art.
    /// </summary>
    public class PlaceholderSpiderVisual : MonoBehaviour
    {
        private void Awake()
        {
            if (transform.childCount > 0)
            {
                return;
            }

            Color bodyColor = new Color(0.14f, 0.1f, 0.12f);
            Material bodyMaterial = new Material(Shader.Find("Standard")) { color = bodyColor };
            Material eyeMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.8f, 0.15f, 0.1f) };

            // Abdomen (rear) and head (front, +Z).
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.45f, -0.25f), new Vector3(0.8f, 0.65f, 0.9f), bodyMaterial);
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.42f, 0.35f), new Vector3(0.5f, 0.45f, 0.5f), bodyMaterial);

            // Glowing red eyes.
            AddPart(PrimitiveType.Sphere, new Vector3(-0.1f, 0.5f, 0.56f), Vector3.one * 0.09f, eyeMaterial);
            AddPart(PrimitiveType.Sphere, new Vector3(0.1f, 0.5f, 0.56f), Vector3.one * 0.09f, eyeMaterial);

            // Eight legs: four per side, fanned out and tilted down.
            float[] legAngles = { -55f, -20f, 15f, 50f };
            foreach (float angle in legAngles)
            {
                AddLeg(angle, left: true, bodyMaterial);
                AddLeg(angle, left: false, bodyMaterial);
            }
        }

        private void AddLeg(float yawDegrees, bool left, Material material)
        {
            float side = left ? -1f : 1f;
            var leg = AddPart(
                PrimitiveType.Cube,
                new Vector3(side * 0.45f, 0.35f, 0f),
                new Vector3(0.07f, 0.07f, 0.9f),
                material);
            leg.localRotation = Quaternion.Euler(0f, side * (90f + yawDegrees), side * -28f);
        }

        private Transform AddPart(PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.transform.SetParent(transform, worldPositionStays: false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Destroy(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
        }
    }
}
