using System;
using System.Collections.Generic;

namespace AntEmpire.SaveSystem
{
    /// <summary>
    /// The full serializable snapshot of player progress.
    ///
    /// Unity's JsonUtility cannot serialize Dictionary, so every map is stored
    /// as a list of key/value entries. Use the helper methods (GetResource,
    /// SetResource, etc.) instead of touching the lists directly.
    ///
    /// This is the ONLY place player progress lives. ScriptableObjects are for
    /// static configuration, never for save data.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;

        /// <summary>Unix time (seconds, UTC) of the last successful save.
        /// Used by OfflineProgressService to compute offline rewards.</summary>
        public long lastSaveUnixTime;

        public List<ResourceEntry> resources = new List<ResourceEntry>();
        public List<IntEntry> antCounts = new List<IntEntry>();
        public List<IntEntry> antEvolutionLevels = new List<IntEntry>();
        public List<IntEntry> roomLevels = new List<IntEntry>();

        public int queenLevel = 1;
        public int currentDay;
        public bool tutorialCompleted;

        [Serializable]
        public class ResourceEntry
        {
            public string id;
            public double amount;
        }

        [Serializable]
        public class IntEntry
        {
            public string id;
            public int value;
        }

        // ---- Resource helpers -------------------------------------------------

        public double GetResource(string id)
        {
            var entry = resources.Find(e => e.id == id);
            return entry != null ? entry.amount : 0.0;
        }

        public void SetResource(string id, double amount)
        {
            var entry = resources.Find(e => e.id == id);
            if (entry == null)
            {
                resources.Add(new ResourceEntry { id = id, amount = amount });
            }
            else
            {
                entry.amount = amount;
            }
        }

        // ---- Generic int-map helpers -----------------------------------------

        public int GetInt(List<IntEntry> list, string id, int fallback = 0)
        {
            var entry = list.Find(e => e.id == id);
            return entry != null ? entry.value : fallback;
        }

        public void SetInt(List<IntEntry> list, string id, int value)
        {
            var entry = list.Find(e => e.id == id);
            if (entry == null)
            {
                list.Add(new IntEntry { id = id, value = value });
            }
            else
            {
                entry.value = value;
            }
        }

        public int GetAntCount(string antId) => GetInt(antCounts, antId);
        public void SetAntCount(string antId, int count) => SetInt(antCounts, antId, count);

        public int GetAntEvolutionLevel(string antId) => GetInt(antEvolutionLevels, antId, 1);
        public void SetAntEvolutionLevel(string antId, int level) => SetInt(antEvolutionLevels, antId, level);

        public int GetRoomLevel(string roomId) => GetInt(roomLevels, roomId);
        public void SetRoomLevel(string roomId, int level) => SetInt(roomLevels, roomId, level);
    }
}
