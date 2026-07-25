using AntEmpire.Economy;

namespace AntEmpire.Ants
{
    /// <summary>
    /// The ant's "backpack": what it is carrying right now. Pure C#.
    ///
    /// Capacity comes from AntTypeData (× evolution multipliers later);
    /// the collector itself just holds and hands over the load.
    /// </summary>
    public class WorkerAntCollector
    {
        public ResourceType CarriedType { get; private set; }
        public double CarriedAmount { get; private set; }
        public bool IsCarrying => CarriedAmount > 0;

        /// <summary>Put gathered resources into the backpack (replaces any previous load).</summary>
        public void Take(ResourceType type, double amount)
        {
            if (amount <= 0)
            {
                return;
            }
            CarriedType = type;
            CarriedAmount = amount;
        }

        /// <summary>Empty the backpack into the colony economy.</summary>
        public void DepositInto(ResourceManager resourceManager)
        {
            if (!IsCarrying)
            {
                return;
            }
            resourceManager.Add(CarriedType, CarriedAmount);
            CarriedAmount = 0;
        }

        // TODO (Colony Memory): count completed trips per route here so
        // frequently used routes give a speed bonus and draw a visible trail.
    }
}
