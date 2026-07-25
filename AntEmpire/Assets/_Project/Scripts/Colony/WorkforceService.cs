using System;
using AntEmpire.Economy;
using AntEmpire.SaveSystem;

namespace AntEmpire.Colony
{
    /// <summary>
    /// Player-controlled job assignment: how many workers are dedicated to
    /// each resource. Workers not assigned to anything gather on "auto"
    /// (nearest node of any type).
    ///
    /// Pure C#; targets persist in SaveData. AntSpawner listens to
    /// JobsChanged and re-assigns the live ants.
    /// </summary>
    public class WorkforceService
    {
        /// <summary>Resources the player can dedicate workers to.</summary>
        public static readonly ResourceType[] AssignableResources =
        {
            ResourceType.Food,
            ResourceType.Leaves
        };

        private readonly SaveManager _saveManager;

        /// <summary>Fired after any assignment change.</summary>
        public event Action JobsChanged;

        public WorkforceService(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public int GetTarget(ResourceType type)
        {
            return _saveManager.Data.GetWorkerJobTarget(ResourceIds.FromType(type));
        }

        public int TotalAssigned
        {
            get
            {
                int total = 0;
                foreach (ResourceType type in AssignableResources)
                {
                    total += GetTarget(type);
                }
                return total;
            }
        }

        /// <summary>Workers left on auto-gathering given the current population.</summary>
        public int FreeWorkers(int totalWorkers)
        {
            int free = totalWorkers - TotalAssigned;
            return free < 0 ? 0 : free;
        }

        /// <summary>Change an assignment by ±delta. Fails silently if it would go
        /// below zero or exceed the worker population.</summary>
        public bool TryAdjust(ResourceType type, int delta, int totalWorkers)
        {
            int next = GetTarget(type) + delta;
            if (next < 0)
            {
                return false;
            }
            if (delta > 0 && TotalAssigned + delta > totalWorkers)
            {
                return false;
            }

            _saveManager.Data.SetWorkerJobTarget(ResourceIds.FromType(type), next);
            _saveManager.Save();
            JobsChanged?.Invoke();
            return true;
        }
    }
}
