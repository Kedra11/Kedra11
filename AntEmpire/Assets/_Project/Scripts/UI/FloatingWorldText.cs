using UnityEngine;

namespace AntEmpire.UI
{
    /// <summary>
    /// A short-lived floating reward label in world space ("+10 DNA"):
    /// rises, fades, always faces the camera. Spawn via the static helper.
    /// </summary>
    public class FloatingWorldText : MonoBehaviour
    {
        private const float Duration = 1.6f;
        private const float RiseSpeed = 1.2f;

        private TextMesh _text;
        private float _age;

        public static void Spawn(Vector3 position, string message, Color color)
        {
            var textObject = new GameObject("FloatingText");
            textObject.transform.position = position;

            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = message;
            textMesh.characterSize = 0.22f;
            textMesh.fontSize = 48;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = color;
            // Unity 6 has no built-in Arial; LegacyRuntime is the bundled font.
            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textMesh.GetComponent<MeshRenderer>().sharedMaterial = textMesh.font.material;

            textObject.AddComponent<FloatingWorldText>()._text = textMesh;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position += Vector3.up * (RiseSpeed * Time.deltaTime);

            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            Color color = _text.color;
            color.a = Mathf.Clamp01(1f - _age / Duration);
            _text.color = color;

            if (_age >= Duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
