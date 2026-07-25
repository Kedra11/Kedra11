using System.Collections.Generic;
using AntEmpire.Colony;
using AntEmpire.Core;
using AntEmpire.Economy;
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

        private readonly List<AntController> _ants = new List<AntController>();

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

            GameManager.Instance.Workforce.JobsChanged += ReassignJobs;
            ReassignJobs();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Workforce.JobsChanged -= ReassignJobs;
            }
        }

        /// <summary>Distribute player-set job targets over the live ants:
        /// the first N go to the first assigned resource, and so on; the
        /// remainder gathers on auto (nearest node of any type).</summary>
        private void ReassignJobs()
        {
            WorkforceService workforce = GameManager.Instance.Workforce;
            int antIndex = 0;

            foreach (ResourceType type in WorkforceService.AssignableResources)
            {
                int target = workforce.GetTarget(type);
                for (int n = 0; n < target && antIndex < _ants.Count; n++, antIndex++)
                {
                    _ants[antIndex].AssignedResource = type;
                }
            }

            for (; antIndex < _ants.Count; antIndex++)
            {
                _ants[antIndex].AssignedResource = null;
            }
        }

        /// <summary>Add a brand-new worker to the colony (updates the save).</summary>
        public AntController HatchWorker()
        {
            AntController ant = SpawnWorkerInstance();

            var save = GameManager.Instance.SaveManager;
            save.Data.SetAntCount(AntIds.Worker, save.Data.GetAntCount(AntIds.Worker) + 1);
            save.Save();

            ReassignJobs();
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

            _ants.Add(controller);
            AliveWorkers++;
            return controller;
        }
    }
}
