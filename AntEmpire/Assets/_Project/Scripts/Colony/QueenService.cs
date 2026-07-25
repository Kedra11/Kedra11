using System;
using AntEmpire.Economy;
using AntEmpire.SaveSystem;
using UnityEngine;

namespace AntEmpire.Colony
{
    /// <summary>
    /// The queen's own evolution (design doc: queen star levels 1..5).
    /// Each level adds population room and speeds up births; the queen's
    /// visual grows with her. Level persists in SaveData.queenLevel.
    /// </summary>
    public class QueenService
    {
        public const int MaxLevel = 5;

        private readonly SaveManager _saveManager;
        private readonly ResourceManager _resources;

        /// <summary>Fired after a successful upgrade with the new level.</summary>
        public event Action<int> QueenUpgraded;

        public QueenService(SaveManager saveManager, ResourceManager resources)
        {
            _saveManager = saveManager;
            _resources = resources;
        }

        public int Level => Mathf.Max(1, _saveManager.Data.queenLevel);

        /// <summary>+2 population per level above 1 (stacks with Queen Chamber).</summary>
        public int BonusPopulationCap => 2 * (Level - 1);

        /// <summary>Births are 8% faster per level above 1.</summary>
        public float BirthSpeedFactor => 1f + 0.08f * (Level - 1);

        public (int food, int dna) UpgradeCost
        {
            get
            {
                float factor = Mathf.Pow(2.2f, Level - 1);
                return (Mathf.CeilToInt(200 * factor), Mathf.CeilToInt(20 * Mathf.Pow(2f, Level - 1)));
            }
        }

        public bool CanUpgrade()
        {
            if (Level >= MaxLevel)
            {
                return false;
            }
            var (food, dna) = UpgradeCost;
            return _resources.CanAfford((ResourceType.Food, food), (ResourceType.DNA, dna));
        }

        public bool TryUpgrade()
        {
            if (Level >= MaxLevel)
            {
                return false;
            }

            var (food, dna) = UpgradeCost;
            if (_resources.TrySpend((ResourceType.Food, food), (ResourceType.DNA, dna))
                != ResourceTransactionResult.Success)
            {
                return false;
            }

            _saveManager.Data.queenLevel = Level + 1;
            _saveManager.Save();
            QueenUpgraded?.Invoke(_saveManager.Data.queenLevel);
            return true;
        }
    }
}
