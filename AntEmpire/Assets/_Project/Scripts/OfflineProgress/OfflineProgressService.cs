using System;
using AntEmpire.Ants;
using AntEmpire.Colony;
using AntEmpire.Economy;
using AntEmpire.Rooms;
using AntEmpire.SaveSystem;
using UnityEngine;

namespace AntEmpire.OfflineProgress
{
    /// <summary>What the colony gathered while the app was closed.</summary>
    public class OfflineReward
    {
        public long Seconds;
        public double Food;
        public double Leaves;

        public bool HasAnything => Food >= 1 || Leaves >= 1;
    }

    /// <summary>
    /// Computes offline earnings at boot, after SaveManager.LoadOrCreate()
    /// and before gameplay starts. Pure C#.
    ///
    /// Model: each worker produces resources at a rate derived from its
    /// evolution stats (carry × speed), at reduced offline efficiency —
    /// the colony works slower without the player watching, which also
    /// keeps active play the better strategy. Job assignments are honored:
    /// dedicated workers earn their resource, auto workers split 50/50.
    /// Food is capped by the Food Storage's free space.
    /// </summary>
    public class OfflineProgressService
    {
        private const double PerWorkerBaseRate = 0.10; // resources/sec at carry 1, speed x1
        private const double OfflineEfficiency = 0.6;
        private const long MaxOfflineSeconds = 8 * 60 * 60; // cap at 8 hours

        public OfflineReward Calculate(
            SaveData saveData,
            EvolutionService evolution,
            WorkforceService workforce,
            RoomService rooms,
            ResourceManager resources,
            bool isNewGame)
        {
            if (isNewGame)
            {
                return null;
            }

            long elapsed = SaveManager.NowUnix() - saveData.lastSaveUnixTime;
            if (elapsed < 60)
            {
                return null; // ignore blinks and clock jitter
            }
            elapsed = Math.Min(elapsed, MaxOfflineSeconds);

            int workers = saveData.GetAntCount(AntIds.Worker);
            if (workers <= 0)
            {
                return null;
            }

            EvolutionLevelData stats = evolution.Current;
            double ratePerWorker =
                PerWorkerBaseRate * stats.carryCapacity * stats.speedMultiplier * OfflineEfficiency;

            int onFood = Mathf.Min(workforce.GetTarget(ResourceType.Food), workers);
            int onLeaves = Mathf.Min(workforce.GetTarget(ResourceType.Leaves), workers - onFood);
            double autoWorkers = workers - onFood - onLeaves;

            double foodRate = ratePerWorker * (onFood + autoWorkers * 0.5);
            double leavesRate = ratePerWorker * (onLeaves + autoWorkers * 0.5);

            // Food is limited by the free space in the Food Storage —
            // the design-doc reason to keep upgrading it.
            double freeFoodSpace = Math.Max(0, rooms.FoodCapacity - resources.Get(ResourceType.Food));

            var reward = new OfflineReward
            {
                Seconds = elapsed,
                Food = Math.Min(foodRate * elapsed, freeFoodSpace),
                Leaves = leavesRate * elapsed
            };
            return reward.HasAnything ? reward : null;
        }
    }
}
