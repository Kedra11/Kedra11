using System;
using AntEmpire.Economy;
using AntEmpire.SaveSystem;

namespace AntEmpire.Ants
{
    /// <summary>
    /// Worker evolution: the retention core of the game. Owns the current
    /// evolution level (persisted in SaveData), the upgrade transaction and
    /// access to per-level stats. AntSpawner listens to EvolutionChanged and
    /// re-skins/re-stats the live ants.
    /// </summary>
    public class EvolutionService
    {
        private readonly SaveManager _saveManager;
        private readonly ResourceManager _resources;
        private readonly EvolutionLevelData[] _levels;

        /// <summary>Fired after a successful evolution with the new level.</summary>
        public event Action<int> EvolutionChanged;

        public EvolutionService(SaveManager saveManager, ResourceManager resources, EvolutionLevelData[] levels)
        {
            _saveManager = saveManager;
            _resources = resources;
            _levels = levels ?? EvolutionLevelData.CreateDefaultSet();
        }

        public int MaxLevel => _levels[_levels.Length - 1].level;

        public int GetLevel() => _saveManager.Data.GetAntEvolutionLevel(AntIds.Worker);

        /// <summary>Stats config for the colony's current level.</summary>
        public EvolutionLevelData Current => ForLevel(GetLevel());

        /// <summary>Config for the next level, or null at max.</summary>
        public EvolutionLevelData Next
        {
            get
            {
                int level = GetLevel();
                return level >= MaxLevel ? null : ForLevel(level + 1);
            }
        }

        public bool CanEvolve()
        {
            EvolutionLevelData next = Next;
            return next != null && _resources.CanAfford(
                (ResourceType.Food, next.foodCost),
                (ResourceType.DNA, next.dnaCost));
        }

        public bool TryEvolve()
        {
            EvolutionLevelData next = Next;
            if (next == null)
            {
                return false;
            }

            ResourceTransactionResult result = _resources.TrySpend(
                (ResourceType.Food, next.foodCost),
                (ResourceType.DNA, next.dnaCost));
            if (result != ResourceTransactionResult.Success)
            {
                return false;
            }

            _saveManager.Data.SetAntEvolutionLevel(AntIds.Worker, next.level);
            _saveManager.Save();
            EvolutionChanged?.Invoke(next.level);
            return true;
        }

        private EvolutionLevelData ForLevel(int level)
        {
            foreach (EvolutionLevelData data in _levels)
            {
                if (data.level == level)
                {
                    return data;
                }
            }
            return _levels[0];
        }
    }
}
