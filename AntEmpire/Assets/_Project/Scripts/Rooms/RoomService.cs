using System;
using AntEmpire.Economy;
using AntEmpire.SaveSystem;
using UnityEngine;

namespace AntEmpire.Rooms
{
    /// <summary>
    /// Owns room levels and their gameplay effects.
    ///
    /// - Levels are stored in SaveData (level 0 = not built).
    /// - Upgrades spend resources atomically through ResourceManager.
    /// - Effect formulas (population cap, food capacity, birth speed) live
    ///   here so no MonoBehaviour hardcodes balance numbers.
    ///
    /// UI listens to RoomUpgraded; GameManager re-applies effects on it.
    /// </summary>
    public class RoomService
    {
        private readonly SaveData _saveData;
        private readonly ResourceManager _resources;
        private readonly SaveManager _saveManager;
        private readonly RoomTypeData[] _configs;

        /// <summary>Fired after a successful upgrade: (roomId, newLevel).</summary>
        public event Action<string, int> RoomUpgraded;

        public RoomService(SaveManager saveManager, ResourceManager resources, RoomTypeData[] configs)
        {
            _saveManager = saveManager;
            _saveData = saveManager.Data;
            _resources = resources;
            _configs = configs ?? RoomTypeData.CreateDefaultSet();
        }

        public RoomTypeData[] Configs => _configs;

        public RoomTypeData GetConfig(string roomId)
        {
            foreach (RoomTypeData config in _configs)
            {
                if (config.id == roomId)
                {
                    return config;
                }
            }
            return null;
        }

        public int GetLevel(string roomId) => _saveData.GetRoomLevel(roomId);

        public bool IsMaxLevel(string roomId)
        {
            RoomTypeData config = GetConfig(roomId);
            return config == null || GetLevel(roomId) >= config.maxLevel;
        }

        public (int food, int leaves, int soil) GetUpgradeCost(string roomId)
        {
            RoomTypeData config = GetConfig(roomId);
            return config == null ? (0, 0, 0) : config.CostForNextLevel(GetLevel(roomId));
        }

        public bool CanUpgrade(string roomId)
        {
            if (IsMaxLevel(roomId))
            {
                return false;
            }
            var (food, leaves, soil) = GetUpgradeCost(roomId);
            return _resources.CanAfford(
                (ResourceType.Food, food),
                (ResourceType.Leaves, leaves),
                (ResourceType.Soil, soil));
        }

        public bool TryUpgrade(string roomId)
        {
            if (IsMaxLevel(roomId))
            {
                return false;
            }

            var (food, leaves, soil) = GetUpgradeCost(roomId);
            ResourceTransactionResult result = _resources.TrySpend(
                (ResourceType.Food, food),
                (ResourceType.Leaves, leaves),
                (ResourceType.Soil, soil));

            if (result != ResourceTransactionResult.Success)
            {
                return false;
            }

            int newLevel = GetLevel(roomId) + 1;
            _saveData.SetRoomLevel(roomId, newLevel);
            _saveManager.Save();
            RoomUpgraded?.Invoke(roomId, newLevel);
            return true;
        }

        // ---- Room effects (single source of balance truth) --------------------

        /// <summary>Max ants the colony can support. Base 3, +2 per Queen Chamber level.</summary>
        public int PopulationCap => 3 + 2 * GetLevel(RoomIds.QueenChamber);

        /// <summary>Max Food the colony can store. Base 50, +75 per Food Storage level.</summary>
        public double FoodCapacity => 50 + 75 * GetLevel(RoomIds.FoodStorage);

        /// <summary>Seconds between new larvae. 0 while the Nursery is not built.
        /// Level 1 = 12s, each level 15% faster, floor 4s.</summary>
        public float BirthIntervalSeconds
        {
            get
            {
                int level = GetLevel(RoomIds.LarvaNursery);
                if (level <= 0)
                {
                    return 0f;
                }
                return Mathf.Max(4f, 12f * Mathf.Pow(0.85f, level - 1));
            }
        }

        /// <summary>Food cost of hatching one worker.</summary>
        public int HatchFoodCost => 5;
    }
}
