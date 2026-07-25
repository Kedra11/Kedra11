using UnityEngine;

namespace AntEmpire.Rooms
{
    /// <summary>
    /// Stable string ids for rooms, used in save files. Never rename.
    /// </summary>
    public static class RoomIds
    {
        public const string QueenChamber = "queen_chamber";
        public const string FoodStorage = "food_storage";
        public const string LarvaNursery = "larva_nursery";
    }

    /// <summary>
    /// Static configuration for one room type. Level 0 means "not built yet";
    /// the first upgrade builds the room. Player-facing effects of each room
    /// are computed in RoomService from the current level.
    ///
    /// Field initializers double as runtime defaults so the prototype can run
    /// before any assets are authored in the editor.
    /// </summary>
    [CreateAssetMenu(menuName = "Ant Empire/Room Type")]
    public class RoomTypeData : ScriptableObject
    {
        public string id;
        public string displayName;

        [Header("Upgrade cost (level 0 -> 1), grows each level")]
        public int baseFoodCost;
        public int baseLeavesCost;
        public int baseSoilCost;
        [Tooltip("Each level multiplies costs by this factor.")]
        public float costMultiplierPerLevel = 1.6f;

        public int maxLevel = 10;

        /// <summary>Cost to go from <paramref name="currentLevel"/> to the next one.</summary>
        public (int food, int leaves, int soil) CostForNextLevel(int currentLevel)
        {
            float factor = Mathf.Pow(costMultiplierPerLevel, currentLevel);
            return (
                Mathf.CeilToInt(baseFoodCost * factor),
                Mathf.CeilToInt(baseLeavesCost * factor),
                Mathf.CeilToInt(baseSoilCost * factor)
            );
        }

        public static RoomTypeData CreateRuntime(
            string id, string displayName,
            int foodCost, int leavesCost, int soilCost,
            float multiplier = 1.6f, int maxLevel = 10)
        {
            var data = CreateInstance<RoomTypeData>();
            data.id = id;
            data.displayName = displayName;
            data.baseFoodCost = foodCost;
            data.baseLeavesCost = leavesCost;
            data.baseSoilCost = soilCost;
            data.costMultiplierPerLevel = multiplier;
            data.maxLevel = maxLevel;
            return data;
        }

        /// <summary>The three MVP rooms with balanced default costs — used until
        /// hand-authored assets replace them.</summary>
        public static RoomTypeData[] CreateDefaultSet()
        {
            return new[]
            {
                CreateRuntime(RoomIds.QueenChamber, "Queen Chamber", foodCost: 5, leavesCost: 15, soilCost: 0, multiplier: 1.7f),
                CreateRuntime(RoomIds.FoodStorage, "Food Storage", foodCost: 0, leavesCost: 12, soilCost: 0, multiplier: 1.6f),
                CreateRuntime(RoomIds.LarvaNursery, "Larva Nursery", foodCost: 5, leavesCost: 10, soilCost: 0, multiplier: 1.7f)
            };
        }
    }
}
