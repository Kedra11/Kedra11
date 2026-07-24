using System;
using System.Collections.Generic;

namespace AntEmpire.Economy
{
    /// <summary>
    /// In-memory balances for all resources. Pure C#, no Unity dependency,
    /// so it is trivially unit-testable.
    ///
    /// The wallet does not know about the save file — ResourceManager syncs
    /// it with SaveData. The wallet also does not know about storage caps;
    /// ResourceManager applies caps before calling Add.
    /// </summary>
    public class ResourceWallet
    {
        private readonly Dictionary<ResourceType, double> _amounts = new Dictionary<ResourceType, double>();

        /// <summary>Fired after any balance change: (type, newAmount).</summary>
        public event Action<ResourceType, double> BalanceChanged;

        public double Get(ResourceType type)
        {
            return _amounts.TryGetValue(type, out double amount) ? amount : 0.0;
        }

        public void Set(ResourceType type, double amount)
        {
            _amounts[type] = amount < 0 ? 0 : amount;
            BalanceChanged?.Invoke(type, _amounts[type]);
        }

        public void Add(ResourceType type, double amount)
        {
            if (amount <= 0)
            {
                return;
            }
            Set(type, Get(type) + amount);
        }

        public bool CanAfford(ResourceType type, double amount)
        {
            return Get(type) >= amount;
        }

        /// <summary>Try to remove resources. Fails atomically — balance is untouched on failure.</summary>
        public ResourceTransactionResult Spend(ResourceType type, double amount)
        {
            if (amount < 0)
            {
                return ResourceTransactionResult.InvalidAmount;
            }
            if (!CanAfford(type, amount))
            {
                return ResourceTransactionResult.NotEnoughResources;
            }

            Set(type, Get(type) - amount);
            return ResourceTransactionResult.Success;
        }
    }
}
