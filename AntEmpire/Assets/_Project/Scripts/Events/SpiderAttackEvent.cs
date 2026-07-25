using AntEmpire.Ants;
using AntEmpire.Colony;
using AntEmpire.Combat;
using AntEmpire.Core;
using AntEmpire.Economy;
using AntEmpire.UI;
using UnityEngine;

namespace AntEmpire.Events
{
    /// <summary>
    /// The first random event (MVP): a spider periodically raids the nest.
    /// Defeat it (workers near the nest bite it) to earn DNA — the resource
    /// that fuels evolution. If it survives its raid it steals Food.
    ///
    /// One active spider at a time; the next raid is scheduled with a
    /// random delay after the previous one ends.
    /// </summary>
    public class SpiderAttackEvent : MonoBehaviour
    {
        [Header("Timing (seconds)")]
        [SerializeField] private float firstDelay = 45f;
        [SerializeField] private float minInterval = 70f;
        [SerializeField] private float maxInterval = 140f;

        [Header("Balance — spider scales with the colony")]
        [SerializeField] private float baseHealth = 20f;
        [SerializeField] private float healthPerWorker = 3f;
        [SerializeField] private int baseDnaReward = 10;
        [SerializeField] private float spawnDistance = 14f;

        private int _currentReward;

        private AntSpawner _spawner;
        private float _timer;

        public EnemyController ActiveSpider { get; private set; }

        /// <summary>Banner text for the HUD after an event ends ("" = nothing to show).</summary>
        public string ResultMessage { get; private set; } = "";
        public float ResultVisibleUntil { get; private set; }

        private void Start()
        {
            _spawner = FindAnyObjectByType<AntSpawner>();
            _timer = firstDelay;
        }

        private void Update()
        {
            if (ActiveSpider != null || GameManager.Instance == null)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                SpawnSpider();
            }
        }

        private void SpawnSpider()
        {
            Vector3 nest = NestEntrance.Instance != null ? NestEntrance.Instance.Position : Vector3.zero;
            Vector2 direction = Random.insideUnitCircle.normalized;
            Vector3 spawnPosition = nest + new Vector3(direction.x, 0f, direction.y) * spawnDistance;

            var spiderObject = new GameObject("Spider");
            spiderObject.transform.position = spawnPosition;
            spiderObject.AddComponent<PlaceholderSpiderVisual>();

            // Bigger colonies face tougher spiders — and earn more DNA.
            // Soldiers count extra: they hit three times harder than workers.
            int workers = _spawner != null ? _spawner.Ants.Count : 3;
            int soldiers = _spawner != null ? _spawner.Soldiers.Count : 0;
            float health = baseHealth + healthPerWorker * (workers + soldiers * 3);
            _currentReward = baseDnaReward + workers / 2 + soldiers;

            ActiveSpider = spiderObject.AddComponent<EnemyController>();
            ActiveSpider.Init(GameManager.Instance.ResourceManager, _spawner, nest, health);
            ActiveSpider.Finished += OnSpiderFinished;
        }

        private void OnSpiderFinished(EnemyController spider, bool defeated)
        {
            ActiveSpider = null;
            _timer = Random.Range(minInterval, maxInterval);

            if (defeated)
            {
                GameManager.Instance.ResourceManager.Add(ResourceType.DNA, _currentReward);
                ResultMessage = $"Spider defeated!  +{_currentReward} DNA";
                FloatingWorldText.Spawn(
                    spider.transform.position + Vector3.up * 1.4f,
                    $"+{_currentReward} DNA",
                    new Color(0.75f, 0.45f, 1f));
            }
            else
            {
                ResultMessage = spider.StolenFood > 0
                    ? $"The spider stole {spider.StolenFood:0} Food!"
                    : "The spider gave up and fled.";
            }
            ResultVisibleUntil = Time.time + 6f;
        }
    }
}
