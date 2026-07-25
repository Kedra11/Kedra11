using AntEmpire.Ants;
using AntEmpire.Colony;
using AntEmpire.Core;
using AntEmpire.Economy;
using AntEmpire.Events;
using AntEmpire.Rooms;
using UnityEngine;

namespace AntEmpire.UI
{
    /// <summary>
    /// Dev HUD for the prototype sandbox (IMGUI, zero scene setup).
    ///
    /// Layout, after playtest feedback ("the menu covers half the screen and
    /// the bottom is cut off"):
    /// - a compact two-line stat bar at the top left;
    /// - event banners at the top right;
    /// - everything interactive lives in a collapsible, scrollable Menu
    ///   panel so the meadow stays visible and nothing is clipped.
    ///
    /// Per CLAUDE.md the UI never computes economy — all values and costs
    /// come from the services.
    /// </summary>
    public class SandboxHud : MonoBehaviour
    {
        private QueenController _queen;
        private CameraViewSwitcher _viewSwitcher;
        private SpiderAttackEvent _spiderEvent;
        private AntSpawner _spawner;

        private bool _menuOpen;
        private Vector2 _menuScroll;

        private GUIStyle _labelStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _alertStyle;
        private GUIStyle _buttonStyle;

        private void Start()
        {
            _queen = FindAnyObjectByType<QueenController>();
            _viewSwitcher = FindAnyObjectByType<CameraViewSwitcher>();
            _spiderEvent = FindAnyObjectByType<SpiderAttackEvent>();
            _spawner = FindAnyObjectByType<AntSpawner>();
        }

        private void OnGUI()
        {
            GameManager game = GameManager.Instance;
            if (game == null)
            {
                return;
            }

            EnsureStyles();
            DrawTopBar(game);
            DrawBanners();
            if (_menuOpen)
            {
                DrawMenu(game);
            }
            DrawOfflinePopup(game);
        }

        // ---- Top bar ----------------------------------------------------------

        private void DrawTopBar(GameManager game)
        {
            ResourceManager resources = game.ResourceManager;
            RoomService rooms = game.RoomService;
            int workers = game.SaveManager.Data.GetAntCount(AntIds.Worker);
            int soldiers = game.SaveManager.Data.GetAntCount(AntIds.Soldier);
            int scouts = game.SaveManager.Data.GetAntCount(AntIds.Scout);
            int total = workers + soldiers + scouts;
            int cap = rooms.PopulationCap + game.Queen.BonusPopulationCap;

            GUILayout.BeginArea(new Rect(15, 10, Screen.width * 0.62f, Screen.height * 0.24f));

            GUILayout.Label(
                $"Food {resources.Get(ResourceType.Food):0}/{rooms.FoodCapacity:0}    " +
                $"Leaves {resources.Get(ResourceType.Leaves):0}    " +
                $"Soil {resources.Get(ResourceType.Soil):0}    " +
                $"DNA {resources.Get(ResourceType.DNA):0}",
                _smallStyle);
            GUILayout.Label(
                $"Pop {total}/{cap} (W{workers} S{soldiers} C{scouts})    " +
                $"Queen Lv{game.Queen.Level}    Evo Lv{game.Evolution.GetLevel()}",
                _smallStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_menuOpen ? "Menu ▲" : "Menu ▼", _buttonStyle, GUILayout.ExpandWidth(false)))
            {
                _menuOpen = !_menuOpen;
            }
            if (_viewSwitcher != null &&
                GUILayout.Button(_viewSwitcher.IsUnderground ? "Surface" : "Ant farm", _buttonStyle, GUILayout.ExpandWidth(false)))
            {
                _viewSwitcher.Toggle();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        // ---- Event banners (top right) ---------------------------------------

        private void DrawBanners()
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.64f, 10, Screen.width * 0.34f, Screen.height * 0.3f));

            if (RainEvent.IsRaining)
            {
                GUILayout.Label("Rain — everyone slows down…", _alertStyle);
            }

            if (_spiderEvent != null)
            {
                if (_spiderEvent.ActiveSpider != null)
                {
                    GUILayout.Label(
                        $"SPIDER ATTACK!  HP {_spiderEvent.ActiveSpider.Health:0}/{_spiderEvent.ActiveSpider.MaxHealth:0}",
                        _alertStyle);
                }
                else if (Time.time < _spiderEvent.ResultVisibleUntil)
                {
                    GUILayout.Label(_spiderEvent.ResultMessage, _alertStyle);
                }
            }

            GUILayout.EndArea();
        }

        // ---- Collapsible menu -------------------------------------------------

        private void DrawMenu(GameManager game)
        {
            float top = Screen.height * 0.25f;
            Rect rect = new Rect(15, top, Screen.width * 0.42f, Screen.height - top - 15);
            GUI.Box(rect, GUIContent.none);
            GUILayout.BeginArea(rect);
            _menuScroll = GUILayout.BeginScrollView(_menuScroll);

            DrawJobsSection(game);
            DrawHatchSection(game);
            DrawQueenSection(game);
            DrawEvolutionSection(game);
            DrawRoomsSection(game);

            GUILayout.Space(10);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawJobsSection(GameManager game)
        {
            int workers = game.SaveManager.Data.GetAntCount(AntIds.Worker);
            WorkforceService workforce = game.Workforce;

            GUILayout.Label("— Worker jobs —", _labelStyle);
            foreach (ResourceType type in WorkforceService.AssignableResources)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"On {type}: {workforce.GetTarget(type)}", _smallStyle, GUILayout.ExpandWidth(false));
                float size = _smallStyle.fontSize * 1.9f;
                if (GUILayout.Button("-", _buttonStyle, GUILayout.Width(size), GUILayout.Height(size)))
                {
                    workforce.TryAdjust(type, -1, workers);
                }
                if (GUILayout.Button("+", _buttonStyle, GUILayout.Width(size), GUILayout.Height(size)))
                {
                    workforce.TryAdjust(type, +1, workers);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label($"Auto (nearest): {workforce.FreeWorkers(workers)}", _smallStyle);

            if (_queen != null && !string.IsNullOrEmpty(_queen.PausedReason))
            {
                GUILayout.Label($"Queen: {_queen.PausedReason}", _smallStyle);
            }
            GUILayout.Space(8);
        }

        private void DrawHatchSection(GameManager game)
        {
            if (_spawner == null)
            {
                return;
            }

            GUILayout.Label("— Hatch specialists —", _labelStyle);
            if (GUILayout.Button("Hatch Soldier — 10 Food", _buttonStyle))
            {
                _spawner.TryHatchSoldier();
            }
            if (GUILayout.Button("Hatch Scout — 15 Food", _buttonStyle))
            {
                _spawner.TryHatchScout();
            }
            GUILayout.Space(8);
        }

        private void DrawQueenSection(GameManager game)
        {
            QueenService queen = game.Queen;
            GUILayout.Label($"— Queen: Lv {queen.Level}/{QueenService.MaxLevel} —", _labelStyle);
            if (queen.Level < QueenService.MaxLevel)
            {
                var (food, dna) = queen.UpgradeCost;
                bool wasEnabled = GUI.enabled;
                GUI.enabled = queen.CanUpgrade();
                if (GUILayout.Button($"Upgrade → Lv {queen.Level + 1}:  {food} Food  {dna} DNA", _buttonStyle))
                {
                    queen.TryUpgrade();
                }
                GUI.enabled = wasEnabled;
            }
            GUILayout.Space(8);
        }

        private void DrawEvolutionSection(GameManager game)
        {
            EvolutionService evolution = game.Evolution;
            GUILayout.Label($"— Worker Evolution: Lv {evolution.GetLevel()}/{evolution.MaxLevel} —", _labelStyle);
            EvolutionLevelData next = evolution.Next;
            if (next != null)
            {
                bool wasEnabled = GUI.enabled;
                GUI.enabled = evolution.CanEvolve();
                if (GUILayout.Button($"Evolve → Lv {next.level}:  {next.foodCost} Food  {next.dnaCost} DNA", _buttonStyle))
                {
                    evolution.TryEvolve();
                }
                GUI.enabled = wasEnabled;
            }
            GUILayout.Space(8);
        }

        private void DrawRoomsSection(GameManager game)
        {
            RoomService rooms = game.RoomService;
            GUILayout.Label("— Rooms —", _labelStyle);
            foreach (RoomTypeData config in rooms.Configs)
            {
                int level = rooms.GetLevel(config.id);
                if (rooms.IsMaxLevel(config.id))
                {
                    GUILayout.Label($"{config.displayName}  Lv {level} (MAX)", _smallStyle);
                    continue;
                }

                var (food, leaves, soil) = rooms.GetUpgradeCost(config.id);
                string action = level == 0 ? "Build" : $"Lv {level} → {level + 1}";

                bool wasEnabled = GUI.enabled;
                GUI.enabled = rooms.CanUpgrade(config.id);
                if (GUILayout.Button($"{config.displayName} ({action}):  {FormatCost(food, leaves, soil)}", _buttonStyle))
                {
                    rooms.TryUpgrade(config.id);
                }
                GUI.enabled = wasEnabled;
            }
        }

        // ---- Offline popup ----------------------------------------------------

        private void DrawOfflinePopup(GameManager game)
        {
            var reward = game.PendingOfflineReward;
            if (reward == null)
            {
                return;
            }

            float width = Screen.width * 0.5f;
            float height = Screen.height * 0.45f;
            Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

            GUI.Box(rect, GUIContent.none);
            GUI.Box(rect, GUIContent.none); // double for a denser backdrop
            GUILayout.BeginArea(rect);
            GUILayout.Space(16);
            GUILayout.Label("While you were away…", _labelStyle);
            GUILayout.Label($"({FormatDuration(reward.Seconds)})", _labelStyle);
            GUILayout.Space(10);
            if (reward.Food >= 1)
            {
                GUILayout.Label($"+{reward.Food:0} Food", _labelStyle);
            }
            if (reward.Leaves >= 1)
            {
                GUILayout.Label($"+{reward.Leaves:0} Leaves", _labelStyle);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Collect!", _buttonStyle))
            {
                game.ClaimOfflineReward();
            }
            GUILayout.Space(16);
            GUILayout.EndArea();
        }

        // ---- Helpers ----------------------------------------------------------

        private static string FormatDuration(long seconds)
        {
            if (seconds >= 3600)
            {
                return $"{seconds / 3600}h {seconds % 3600 / 60}m";
            }
            return seconds >= 60 ? $"{seconds / 60}m" : $"{seconds}s";
        }

        private static string FormatCost(int food, int leaves, int soil)
        {
            string result = "";
            if (food > 0) result += $"{food} Food  ";
            if (leaves > 0) result += $"{leaves} Leaves  ";
            if (soil > 0) result += $"{soil} Soil";
            return result.TrimEnd();
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            int fontSize = Mathf.Max(12, Mathf.RoundToInt(Screen.height * 0.021f));

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            };
            _labelStyle.normal.textColor = Color.white;

            _smallStyle = new GUIStyle(_labelStyle)
            {
                fontSize = Mathf.RoundToInt(fontSize * 0.9f),
                fontStyle = FontStyle.Normal
            };

            _alertStyle = new GUIStyle(_labelStyle) { wordWrap = true };
            _alertStyle.normal.textColor = new Color(1f, 0.45f, 0.2f);

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(fontSize * 0.85f),
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 8, 6, 6),
                wordWrap = true
            };
        }
    }
}
