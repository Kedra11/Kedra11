using AntEmpire.Ants;
using AntEmpire.Core;
using AntEmpire.Economy;
using AntEmpire.Rooms;
using UnityEngine;

namespace AntEmpire.Colony
{
    /// <summary>
    /// The queen: heart of the colony. Once the Larva Nursery is built she
    /// periodically hatches new workers, spending Food, until the population
    /// cap (Queen Chamber effect) is reached.
    ///
    /// All balance numbers come from RoomService — nothing hardcoded here.
    /// </summary>
    public class QueenController : MonoBehaviour
    {
        private AntSpawner _spawner;
        private float _timer;

        /// <summary>0..1 progress towards the next larva, for UI. 0 while paused.</summary>
        public float BirthProgress { get; private set; }

        /// <summary>Why hatching is paused right now, for UI ("" = hatching).</summary>
        public string PausedReason { get; private set; } = "";

        private void Start()
        {
            _spawner = FindAnyObjectByType<AntSpawner>();
            if (_spawner == null)
            {
                Debug.LogError("[QueenController] No AntSpawner in the scene.");
                enabled = false;
            }
        }

        private void Update()
        {
            GameManager game = GameManager.Instance;
            if (game == null)
            {
                return;
            }

            RoomService rooms = game.RoomService;

            float interval = rooms.BirthIntervalSeconds;
            if (interval <= 0f)
            {
                Pause("Build the Larva Nursery to hatch new workers");
                return;
            }

            int population = game.SaveManager.Data.GetAntCount(AntIds.Worker);
            if (population >= rooms.PopulationCap)
            {
                Pause("Population is full — upgrade the Queen Chamber");
                return;
            }

            if (game.ResourceManager.Get(ResourceType.Food) < rooms.HatchFoodCost)
            {
                Pause($"Need {rooms.HatchFoodCost} Food for the next larva");
                return;
            }

            PausedReason = "";
            _timer += Time.deltaTime;
            BirthProgress = Mathf.Clamp01(_timer / interval);

            if (_timer >= interval)
            {
                _timer = 0f;
                if (game.ResourceManager.TrySpend(ResourceType.Food, rooms.HatchFoodCost)
                    == ResourceTransactionResult.Success)
                {
                    _spawner.HatchWorker();
                }
            }
        }

        private void Pause(string reason)
        {
            PausedReason = reason;
            BirthProgress = 0f;
            _timer = 0f;
        }
    }
}
