using AntEmpire.Economy;
using AntEmpire.Rooms;
using AntEmpire.SaveSystem;
using UnityEngine;

namespace AntEmpire.Core
{
    /// <summary>
    /// Entry point of the game. Lives on a single GameObject in the Boot
    /// scene, survives scene loads, and wires the core services together.
    ///
    /// Boot order:
    ///   1. SaveManager.LoadOrCreate()  — restore player progress
    ///   2. ResourceManager             — economy, restored from the save
    ///   3. (later) OfflineProgressService — compute away-time rewards
    ///   4. (later) load MainMenu / gameplay scene
    ///
    /// Systems reach services through GameManager.Instance. MonoBehaviours
    /// stay thin: logic lives in the plain C# services, this class only
    /// owns their lifetime and the autosave timer.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Seconds between automatic saves.")]
        [SerializeField] private float autoSaveIntervalSeconds = 30f;

        public SaveManager SaveManager { get; private set; }
        public ResourceManager ResourceManager { get; private set; }
        public RoomService RoomService { get; private set; }

        private float _autoSaveTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            SaveManager = new SaveManager();
            SaveManager.LoadOrCreate();

            ResourceManager = new ResourceManager(SaveManager.Data);

            // Room configs: runtime defaults for now; swap for authored
            // ScriptableObject assets once they exist in the project.
            RoomService = new RoomService(SaveManager, ResourceManager, RoomTypeData.CreateDefaultSet());
            RoomService.RoomUpgraded += (_, _) => ApplyRoomEffects();
            ApplyRoomEffects();

            // TODO: OfflineProgressService — calculate offline rewards here,
            //       before gameplay starts, and show OfflineRewardPopup.
            // TODO: SceneLoader — load MainMenu after services are ready.

            Debug.Log(SaveManager.IsNewGame
                ? "[GameManager] New colony founded."
                : "[GameManager] Colony loaded from save.");
        }

        /// <summary>Push current room levels into the systems they affect.</summary>
        private void ApplyRoomEffects()
        {
            ResourceManager.SetCapacity(ResourceType.Food, RoomService.FoodCapacity);
        }

        private void Update()
        {
            // Cheap timer instead of a coroutine — one float add per frame.
            _autoSaveTimer += Time.unscaledDeltaTime;
            if (_autoSaveTimer >= autoSaveIntervalSeconds)
            {
                _autoSaveTimer = 0f;
                SaveManager.AutoSave();
            }
        }

        // Mobile lifecycle: save whenever the app goes to background,
        // because on Android/iOS OnApplicationQuit is not guaranteed.
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveManager?.AutoSave();
            }
        }

        private void OnApplicationQuit()
        {
            SaveManager?.AutoSave();
        }
    }
}
