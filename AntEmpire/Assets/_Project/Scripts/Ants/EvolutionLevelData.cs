using UnityEngine;

namespace AntEmpire.Ants
{
    /// <summary>
    /// Static configuration of one worker evolution level. Costs are what it
    /// takes to REACH this level from the previous one. Player's current
    /// level lives in SaveData, never here.
    /// </summary>
    [CreateAssetMenu(menuName = "Ant Empire/Evolution Level")]
    public class EvolutionLevelData : ScriptableObject
    {
        public int level = 1;

        [Header("Cost to reach this level")]
        public int foodCost;
        public int dnaCost;

        [Header("Worker stats at this level")]
        public float speedMultiplier = 1f;
        public int carryCapacity = 1;

        public static EvolutionLevelData Create(int level, int foodCost, int dnaCost, float speed, int carry)
        {
            var data = CreateInstance<EvolutionLevelData>();
            data.level = level;
            data.foodCost = foodCost;
            data.dnaCost = dnaCost;
            data.speedMultiplier = speed;
            data.carryCapacity = carry;
            return data;
        }

        /// <summary>The 5 visual stages from the design doc, with default balance —
        /// used until hand-authored assets replace them.</summary>
        public static EvolutionLevelData[] CreateDefaultSet()
        {
            return new[]
            {
                Create(level: 1, foodCost: 0, dnaCost: 0, speed: 1.00f, carry: 1),
                Create(level: 2, foodCost: 50, dnaCost: 5, speed: 1.15f, carry: 2),
                Create(level: 3, foodCost: 150, dnaCost: 15, speed: 1.30f, carry: 3),
                Create(level: 4, foodCost: 400, dnaCost: 35, speed: 1.45f, carry: 4),
                Create(level: 5, foodCost: 1000, dnaCost: 80, speed: 1.60f, carry: 6)
            };
        }
    }
}
