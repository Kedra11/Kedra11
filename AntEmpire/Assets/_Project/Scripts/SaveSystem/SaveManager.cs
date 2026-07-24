using System;
using System.IO;
using UnityEngine;

namespace AntEmpire.SaveSystem
{
    /// <summary>
    /// Plain C# service that owns loading and saving player progress as JSON
    /// under Application.persistentDataPath.
    ///
    /// GameManager creates one instance at boot and passes it to the systems
    /// that need it. No other class should read or write the save file.
    ///
    /// Corruption handling: a save that fails to parse is moved aside
    /// (.corrupted) instead of being deleted, and a fresh save is created,
    /// so the player's broken file can still be inspected or recovered.
    /// </summary>
    public class SaveManager
    {
        private const string SaveFileName = "ant_empire_save.json";

        private readonly string _savePath;

        /// <summary>The live save. Systems mutate this object, then call Save().</summary>
        public SaveData Data { get; private set; }

        /// <summary>True if this launch created a brand-new save (first run or corrupted file).</summary>
        public bool IsNewGame { get; private set; }

        public SaveManager()
        {
            _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        /// <summary>Load the save file, or create a new save on first launch / corruption.</summary>
        public void LoadOrCreate()
        {
            if (!File.Exists(_savePath))
            {
                CreateNewSave();
                return;
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                if (loaded == null)
                {
                    throw new InvalidDataException("Save file parsed to null.");
                }

                Data = loaded;
                IsNewGame = false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Save file is corrupted ({ex.Message}). Backing it up and starting fresh.");
                BackupCorruptedFile();
                CreateNewSave();
            }
        }

        /// <summary>Create a fresh save with the starting state of a new colony.</summary>
        public void CreateNewSave()
        {
            Data = new SaveData
            {
                lastSaveUnixTime = NowUnix(),
                queenLevel = 1,
                currentDay = 1,
                tutorialCompleted = false
            };

            // A new colony starts with a little food so the first worker can hatch.
            Data.SetResource(AntEmpire.Economy.ResourceIds.Food, 10);

            IsNewGame = true;
            Save();
        }

        /// <summary>Write the current state to disk and stamp lastSaveUnixTime.</summary>
        public void Save()
        {
            if (Data == null)
            {
                Debug.LogError("[SaveManager] Save() called before LoadOrCreate().");
                return;
            }

            Data.lastSaveUnixTime = NowUnix();

            try
            {
                string json = JsonUtility.ToJson(Data, prettyPrint: false);

                // Write to a temp file first so a crash mid-write never
                // destroys the previous good save.
                string tmpPath = _savePath + ".tmp";
                File.WriteAllText(tmpPath, json);
                if (File.Exists(_savePath))
                {
                    File.Delete(_savePath);
                }
                File.Move(tmpPath, _savePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to save: {ex.Message}");
            }
        }

        /// <summary>Called by GameManager on a timer and on app pause/quit.</summary>
        public void AutoSave()
        {
            Save();
        }

        public static long NowUnix()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void BackupCorruptedFile()
        {
            try
            {
                string backupPath = _savePath + ".corrupted";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                File.Move(_savePath, backupPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Could not back up corrupted save: {ex.Message}");
            }
        }
    }
}
