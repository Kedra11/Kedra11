using System.Collections.Generic;
using AntEmpire.Colony;
using AntEmpire.Core;
using AntEmpire.Economy;
using AntEmpire.UI;
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
        private readonly List<SoldierController> _soldiers = new List<SoldierController>();
        private readonly List<ScoutController> _scouts = new List<ScoutController>();

        public int AliveWorkers { get; private set; }

        /// <summary>Live ants — used by combat (spider bites) and reassignment.</summary>
        public IReadOnlyList<AntController> Ants => _ants;
        public IReadOnlyList<SoldierController> Soldiers => _soldiers;
        public IReadOnlyList<ScoutController> Scouts => _scouts;

        /// <summary>All colony members counted against the population cap.</summary>
        public int TotalPopulation
        {
            get
            {
                var data = GameManager.Instance.SaveManager.Data;
                return data.GetAntCount(AntIds.Worker)
                     + data.GetAntCount(AntIds.Soldier)
                     + data.GetAntCount(AntIds.Scout);
            }
        }

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
            for (int i = 0; i < save.Data.GetAntCount(AntIds.Soldier); i++)
            {
                SpawnSoldierInstance();
            }
            for (int i = 0; i < save.Data.GetAntCount(AntIds.Scout); i++)
            {
                SpawnScoutInstance();
            }

            GameManager.Instance.Workforce.JobsChanged += ReassignJobs;
            ReassignJobs();

            GameManager.Instance.Evolution.EvolutionChanged += OnEvolutionChanged;
            OnEvolutionChanged(GameManager.Instance.Evolution.GetLevel());
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Workforce.JobsChanged -= ReassignJobs;
                GameManager.Instance.Evolution.EvolutionChanged -= OnEvolutionChanged;
            }
        }

        private void OnEvolutionChanged(int _)
        {
            EvolutionLevelData current = GameManager.Instance.Evolution.Current;
            foreach (AntController ant in _ants)
            {
                ant.ApplyEvolution(current);
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

        /// <summary>Hatch a soldier (10 Food): patrols the nest and shields
        /// workers in fights. Fails when the cap is reached or Food is short.</summary>
        public bool TryHatchSoldier()
        {
            return TryHatchRole(AntIds.Soldier, foodCost: 10, SpawnSoldierInstance);
        }

        /// <summary>Hatch a scout (15 Food): roams far and discovers rich finds.</summary>
        public bool TryHatchScout()
        {
            return TryHatchRole(AntIds.Scout, foodCost: 15, SpawnScoutInstance);
        }

        private bool TryHatchRole(string antId, int foodCost, System.Action spawn)
        {
            GameManager game = GameManager.Instance;
            int cap = game.RoomService.PopulationCap + game.Queen.BonusPopulationCap;
            if (TotalPopulation >= cap)
            {
                return false;
            }
            if (game.ResourceManager.TrySpend(ResourceType.Food, foodCost)
                != ResourceTransactionResult.Success)
            {
                return false;
            }

            spawn();
            var save = game.SaveManager;
            save.Data.SetAntCount(antId, save.Data.GetAntCount(antId) + 1);
            save.Save();
            return true;
        }

        public void KillSoldier(SoldierController soldier)
        {
            if (soldier == null || !_soldiers.Remove(soldier))
            {
                return;
            }

            var save = GameManager.Instance.SaveManager;
            int count = save.Data.GetAntCount(AntIds.Soldier) - 1;
            save.Data.SetAntCount(AntIds.Soldier, count < 0 ? 0 : count);
            save.Save();

            FloatingWorldText.Spawn(
                soldier.transform.position + Vector3.up * 0.6f,
                "-1 soldier", new Color(0.9f, 0.25f, 0.2f));
            Destroy(soldier.gameObject);
        }

        /// <summary>Kill a worker (enemy attack): remove it from the colony,
        /// update the save and show a small death marker.</summary>
        public void KillWorker(AntController ant)
        {
            if (ant == null || !_ants.Remove(ant))
            {
                return;
            }

            AliveWorkers--;
            var save = GameManager.Instance.SaveManager;
            int count = save.Data.GetAntCount(AntIds.Worker) - 1;
            save.Data.SetAntCount(AntIds.Worker, count < 0 ? 0 : count);
            save.Save();

            FloatingWorldText.Spawn(
                ant.transform.position + Vector3.up * 0.6f,
                "-1 ant", new Color(0.9f, 0.25f, 0.2f));
            Destroy(ant.gameObject);

            ReassignJobs();
        }

        private void SpawnSoldierInstance()
        {
            GameObject soldierObject = SpawnAntObject("SoldierAnt");
            soldierObject.GetComponent<PlaceholderAntVisual>()
                .SetAppearance(new Color(0.5f, 0.12f, 0.08f), extraScale: 1.3f);
            _soldiers.Add(soldierObject.AddComponent<SoldierController>());
        }

        private void SpawnScoutInstance()
        {
            GameObject scoutObject = SpawnAntObject("ScoutAnt");
            scoutObject.GetComponent<PlaceholderAntVisual>()
                .SetAppearance(new Color(0.72f, 0.68f, 0.35f), extraScale: 0.9f);
            _scouts.Add(scoutObject.AddComponent<ScoutController>());
        }

        private GameObject SpawnAntObject(string name)
        {
            Vector3 center = NestEntrance.Instance != null
                ? NestEntrance.Instance.Position
                : transform.position;
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(0.5f, spawnRadius);

            var antObject = new GameObject(name);
            antObject.transform.position = center + new Vector3(offset.x, 0f, offset.y);
            antObject.AddComponent<PlaceholderAntVisual>();
            return antObject;
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
            if (GameManager.Instance != null)
            {
                controller.ApplyEvolution(GameManager.Instance.Evolution.Current);
            }

            _ants.Add(controller);
            AliveWorkers++;
            return controller;
        }
    }
}
