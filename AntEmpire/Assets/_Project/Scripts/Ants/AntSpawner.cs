using AntEmpire.Colony;
using AntEmpire.Core;
using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// Spawns worker ants around the nest entrance and keeps the worker
    /// count in sync with SaveData.
    ///
    /// - On scene start it restores the saved worker population (or grants
    ///   the starting workers on a brand-new game).
    /// - HatchWorker() is the single entry point for adding NEW ants — the
    ///   future Larva Nursery / queen production will call it.
    /// </summary>
    public class AntSpawner : MonoBehaviour
    {
        [SerializeField] private AntTypeData workerType;
        [Tooltip("Optional art prefab. When empty, a placeholder primitive ant is built.")]
        [SerializeField] private GameObject workerPrefab;
        [SerializeField] private int newGameWorkerCount = 3;
        [SerializeField] private float spawnRadius = 1.5f;

        public int AliveWorkers { get; private set; }

        private void Start()
        {
            if (workerType == null)
            {
                workerType = ScriptableObject.CreateInstance<AntTypeData>();
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("[AntSpawner] No GameManager in the scene. Add one (or use PrototypeSandbox).");
                enabled = false;
                return;
            }

            var save = GameManager.Instance.SaveManager;
            int savedCount = save.Data.GetAntCount(AntIds.Worker);

            if (savedCount <= 0 && save.IsNewGame)
            {
                savedCount = newGameWorkerCount;
                save.Data.SetAntCount(AntIds.Worker, savedCount);
                save.Save();
            }

            for (int i = 0; i < savedCount; i++)
            {
                SpawnWorkerInstance();
            }
        }

        /// <summary>Add a brand-new worker to the colony (updates the save).</summary>
        public AntController HatchWorker()
        {
            AntController ant = SpawnWorkerInstance();

            var save = GameManager.Instance.SaveManager;
            save.Data.SetAntCount(AntIds.Worker, save.Data.GetAntCount(AntIds.Worker) + 1);
            save.Save();

            return ant;
        }

        /// <summary>Instantiate one ant near the nest without touching the save.</summary>
        private AntController SpawnWorkerInstance()
        {
            Vector3 center = NestEntrance.Instance != null
                ? NestEntrance.Instance.Position
                : transform.position;

            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(0.5f, spawnRadius);
            Vector3 position = center + new Vector3(offset.x, 0f, offset.y);

            GameObject antObject;
            if (workerPrefab != null)
            {
                antObject = Instantiate(workerPrefab, position, Quaternion.identity);
            }
            else
            {
                antObject = new GameObject("WorkerAnt");
                antObject.transform.position = position;
                antObject.AddComponent<PlaceholderAntVisual>();
            }

            var controller = antObject.GetComponent<AntController>();
            if (controller == null)
            {
                controller = antObject.AddComponent<AntController>();
            }
            controller.Initialize(workerType);

            AliveWorkers++;
            return controller;
        }
    }
}
