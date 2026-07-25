using UnityEngine;

namespace AntEmpire.Events
{
    /// <summary>
    /// Random weather event from the design doc: rain slows everything on
    /// the surface (ants, spiders) and dims the world for its duration.
    ///
    /// Movement code multiplies by the static SpeedFactor, so the event
    /// needs no references to the creatures it affects.
    /// </summary>
    public class RainEvent : MonoBehaviour
    {
        /// <summary>Global surface speed multiplier (1 = dry weather).</summary>
        public static float SpeedFactor { get; private set; } = 1f;
        public static bool IsRaining { get; private set; }

        [Header("Timing (seconds)")]
        [SerializeField] private float minInterval = 150f;
        [SerializeField] private float maxInterval = 300f;
        [SerializeField] private float rainDuration = 25f;
        [SerializeField] private float rainSpeedFactor = 0.55f;

        private float _timer;
        private float _rainLeft;
        private Light _sun;
        private float _sunIntensity;
        private Color _skyColor;

        private void Start()
        {
            _timer = Random.Range(minInterval * 0.5f, maxInterval * 0.5f); // first rain sooner
            _sun = FindAnyObjectByType<Light>();
            if (_sun != null)
            {
                _sunIntensity = _sun.intensity;
            }
            if (Camera.main != null)
            {
                _skyColor = Camera.main.backgroundColor;
            }
        }

        private void Update()
        {
            if (IsRaining)
            {
                _rainLeft -= Time.deltaTime;
                if (_rainLeft <= 0f)
                {
                    StopRain();
                }
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                StartRain();
            }
        }

        private void StartRain()
        {
            IsRaining = true;
            SpeedFactor = rainSpeedFactor;
            _rainLeft = rainDuration;

            if (_sun != null)
            {
                _sun.intensity = _sunIntensity * 0.55f;
            }
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = new Color(0.42f, 0.5f, 0.58f); // overcast
            }
        }

        private void StopRain()
        {
            IsRaining = false;
            SpeedFactor = 1f;
            _timer = Random.Range(minInterval, maxInterval);

            if (_sun != null)
            {
                _sun.intensity = _sunIntensity;
            }
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = _skyColor;
            }
        }

        private void OnDestroy()
        {
            SpeedFactor = 1f;
            IsRaining = false;
        }
    }
}
