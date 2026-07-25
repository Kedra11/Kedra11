using UnityEngine;

namespace AntEmpire.Combat
{
    /// <summary>
    /// Primitive spider with combat feedback so the fight is visible:
    /// - a floating HP bar (green → red) above the body;
    /// - the body flashes red and jitters while ants are biting it.
    /// All visual parts sit under a "Body" child so the jitter never touches
    /// the logic transform. Colliders stripped, as with all placeholder art.
    /// </summary>
    public class PlaceholderSpiderVisual : MonoBehaviour
    {
        private static readonly Color BodyColor = new Color(0.14f, 0.1f, 0.12f);

        private EnemyController _enemy;
        private Transform _body;
        private Material _bodyMaterial;
        private Transform _hpBarRoot;
        private Transform _hpFill;
        private Material _hpFillMaterial;

        private float _lastHealth = -1f;
        private float _flashTimer;

        private void Awake()
        {
            _body = new GameObject("Body").transform;
            _body.SetParent(transform, worldPositionStays: false);

            _bodyMaterial = new Material(Shader.Find("Standard")) { color = BodyColor };
            Material eyeMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.8f, 0.15f, 0.1f) };

            // Abdomen (rear) and head (front, +Z).
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.45f, -0.25f), new Vector3(0.8f, 0.65f, 0.9f), _bodyMaterial);
            AddPart(PrimitiveType.Sphere, new Vector3(0f, 0.42f, 0.35f), new Vector3(0.5f, 0.45f, 0.5f), _bodyMaterial);

            // Glowing red eyes.
            AddPart(PrimitiveType.Sphere, new Vector3(-0.1f, 0.5f, 0.56f), Vector3.one * 0.09f, eyeMaterial);
            AddPart(PrimitiveType.Sphere, new Vector3(0.1f, 0.5f, 0.56f), Vector3.one * 0.09f, eyeMaterial);

            // Eight legs: four per side, fanned out and tilted down.
            float[] legAngles = { -55f, -20f, 15f, 50f };
            foreach (float angle in legAngles)
            {
                AddLeg(angle, left: true);
                AddLeg(angle, left: false);
            }

            BuildHpBar();
        }

        private void Start()
        {
            _enemy = GetComponent<EnemyController>();
            if (_enemy != null)
            {
                _lastHealth = _enemy.Health;
            }
        }

        private void Update()
        {
            if (_enemy == null)
            {
                return;
            }

            // Bite feedback: flash + jitter whenever health dropped this frame.
            if (_enemy.Health < _lastHealth)
            {
                _flashTimer = 0.25f;
            }
            _lastHealth = _enemy.Health;

            _flashTimer = Mathf.Max(0f, _flashTimer - Time.deltaTime);
            float flash = _flashTimer / 0.25f;
            _bodyMaterial.color = Color.Lerp(BodyColor, new Color(0.85f, 0.15f, 0.1f), flash * 0.8f);
            _body.localPosition = flash > 0f
                ? new Vector3(Random.Range(-0.05f, 0.05f), 0f, Random.Range(-0.05f, 0.05f))
                : Vector3.zero;

            // HP bar: fill scales with health, colour goes green -> red,
            // and the bar always faces the camera.
            float fraction = _enemy.MaxHealth > 0f ? _enemy.Health / _enemy.MaxHealth : 0f;
            const float barWidth = 1.3f;
            _hpFill.localScale = new Vector3(barWidth * fraction, 0.16f, 1f);
            _hpFill.localPosition = new Vector3(-barWidth * (1f - fraction) * 0.5f, 0f, -0.01f);
            _hpFillMaterial.color = Color.Lerp(new Color(0.85f, 0.2f, 0.1f), new Color(0.25f, 0.8f, 0.2f), fraction);

            if (Camera.main != null)
            {
                _hpBarRoot.rotation = Camera.main.transform.rotation;
            }
        }

        private void BuildHpBar()
        {
            _hpBarRoot = new GameObject("HpBar").transform;
            _hpBarRoot.SetParent(transform, worldPositionStays: false);
            _hpBarRoot.localPosition = new Vector3(0f, 1.5f, 0f);

            Transform background = MakeQuad("Background", _hpBarRoot);
            background.localScale = new Vector3(1.4f, 0.22f, 1f);
            background.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Unlit/Color")) { color = new Color(0.1f, 0.1f, 0.1f, 1f) };

            _hpFill = MakeQuad("Fill", _hpBarRoot);
            _hpFillMaterial = new Material(Shader.Find("Unlit/Color")) { color = Color.green };
            _hpFill.GetComponent<Renderer>().sharedMaterial = _hpFillMaterial;
        }

        private static Transform MakeQuad(string name, Transform parent)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, worldPositionStays: false);
            Destroy(quad.GetComponent<Collider>());
            return quad.transform;
        }

        private void AddLeg(float yawDegrees, bool left)
        {
            float side = left ? -1f : 1f;
            var leg = AddPart(
                PrimitiveType.Cube,
                new Vector3(side * 0.45f, 0.35f, 0f),
                new Vector3(0.07f, 0.07f, 0.9f),
                _bodyMaterial);
            leg.localRotation = Quaternion.Euler(0f, side * (90f + yawDegrees), side * -28f);
        }

        private Transform AddPart(PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.transform.SetParent(_body, worldPositionStays: false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Destroy(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
        }
    }
}
