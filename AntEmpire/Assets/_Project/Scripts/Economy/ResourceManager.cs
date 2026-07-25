using System;
using System.Collections.Generic;
using AntEmpire.SaveSystem;

namespace AntEmpire.Economy
{
    /// <summary>
    /// The single entry point for all resource changes in the game.
    ///
    /// Responsibilities:
    /// - owns the ResourceWallet (in-memory balances)
    /// - syncs balances with SaveData on load and on every change
    /// - exposes events so the UI can react without polling
    ///
    /// Rules:
    /// - Gameplay code (ants, rooms, events) calls Add/TrySpend here.
    /// - UI only listens to ResourceChanged; it never computes economy.
    /// - Nothing else writes resource values into SaveData.
    /// </summary>
    public class ResourceManager
    {
        private readonly ResourceWallet _wallet = new ResourceWallet();
        private readonly SaveData _saveData;
        private readonly Dictionary<ResourceType, double> _capacities = new Dictionary<ResourceType, double>();

        /// <summary>Fired after any balance change: (type, newAmount). Subscribe from UI.</summary>
        public event Action<ResourceType, double> ResourceChanged;

        public ResourceManager(SaveData saveData)
        {
            _saveData = saveData;

            // Restore balances from the save file into the wallet.
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _wallet.Set(type, _saveData.GetResource(ResourceIds.FromType(type)));
            }

            // From now on, mirror every wallet change back into SaveData and
            // forward it to listeners. (Subscribed after the initial restore
            // so loading does not spam events.)
            _wallet.BalanceChanged += OnWalletChanged;
        }

        public double Get(ResourceType type) => _wallet.Get(type);

        public bool CanAfford(ResourceType type, double amount) => _wallet.CanAfford(type, amount);

        /// <summary>Check a multi-resource cost (e.g. a room that needs Leaves + Soil).</summary>
        public bool CanAfford(params (ResourceType type, double amount)[] cost)
        {
            foreach (var (type, amount) in cost)
            {
                if (!_wallet.CanAfford(type, amount))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Storage limit for a resource (default: unlimited). Driven by
        /// room effects — e.g. Food Storage level sets the Food capacity.</summary>
        public double GetCapacity(ResourceType type)
        {
            return _capacities.TryGetValue(type, out double cap) ? cap : double.PositiveInfinity;
        }

        public void SetCapacity(ResourceType type, double capacity)
        {
            _capacities[type] = capacity;
            if (_wallet.Get(type) > capacity)
            {
                _wallet.Set(type, capacity);
            }
        }

        /// <summary>Add resources, clamped to the storage capacity. Overflow is lost —
        /// that's the player's cue to upgrade Food Storage.</summary>
        public void Add(ResourceType type, double amount)
        {
            double space = GetCapacity(type) - _wallet.Get(type);
            if (space <= 0)
            {
                return;
            }
            _wallet.Add(type, amount < space ? amount : space);
        }

        public ResourceTransactionResult TrySpend(ResourceType type, double amount)
        {
            return _wallet.Spend(type, amount);
        }

        /// <summary>Spend a multi-resource cost atomically: all or nothing.</summary>
        public ResourceTransactionResult TrySpend(params (ResourceType type, double amount)[] cost)
        {
            if (!CanAfford(cost))
            {
                return ResourceTransactionResult.NotEnoughResources;
            }

            foreach (var (type, amount) in cost)
            {
                _wallet.Spend(type, amount);
            }
            return ResourceTransactionResult.Success;
        }

        private void OnWalletChanged(ResourceType type, double newAmount)
        {
            _saveData.SetResource(ResourceIds.FromType(type), newAmount);
            ResourceChanged?.Invoke(type, newAmount);
        }
    }
}
