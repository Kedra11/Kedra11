using UnityEngine;

namespace AntEmpire.Ants
{
    public enum AntRole
    {
        Worker,
        Soldier,
        Builder,
        Scout,
        RoyalGuard
    }

    /// <summary>
    /// Stable string ids for ant types, used in save files.
    /// Never rename once players have saves.
    /// </summary>
    public static class AntIds
    {
        public const string Worker = "worker";
        public const string Soldier = "soldier";
        public const string Scout = "scout";
    }

    /// <summary>
    /// Static configuration for one ant type. Configuration only — player
    /// progress (counts, evolution levels) lives in SaveData.
    ///
    /// Field initializers double as sensible defaults so the prototype can
    /// run with a ScriptableObject.CreateInstance fallback before any asset
    /// is authored in the editor.
    /// </summary>
    [CreateAssetMenu(menuName = "Ant Empire/Ant Type")]
    public class AntTypeData : ScriptableObject
    {
        public string id = AntIds.Worker;
        public string displayName = "Worker Ant";
        public AntRole role = AntRole.Worker;

        [Header("Movement")]
        [Tooltip("Units per second.")]
        public float moveSpeed = 2.0f;

        [Header("Gathering")]
        [Tooltip("How much the ant carries per trip.")]
        public int carryCapacity = 1;
        [Tooltip("Seconds spent gathering at a resource node.")]
        public float collectSeconds = 1.5f;
        [Tooltip("Seconds of rest before searching for work again.")]
        public float idleSeconds = 0.75f;

        // TODO (stage 5): EvolutionLevelData[] evolutionLevels — per-level
        // costs, stat multipliers and visual prefabs.
    }
}
