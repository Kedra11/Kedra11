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
    /// Dev-only HUD for the prototype sandbox, drawn with IMGUI so it needs
    /// zero scene setup: resource readout, population, queen status and
    /// room upgrade buttons.
    ///
    /// The real game will replace this with a proper HUD (MainHUD /
    /// ResourcePanel / RoomPanel) — per CLAUDE.md, UI listens to economy
    /// events and never computes economy. Room costs shown here come from
    /// RoomService, never calculated in UI.
    /// </summary>
    public class SandboxHud : MonoBehaviour
    {
        private QueenController _queen;
        private CameraViewSwitcher _viewSwitcher;
        private SpiderAttackEvent _spiderEvent;
        private AntSpawner _spawner;
        private GUIStyle _labelStyle;
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

            ResourceManager resources = game.ResourceManager;
            RoomService rooms = game.RoomService;
            int population = game.SaveManager.Data.GetAntCount(AntIds.Worker);

            GUILayout.BeginArea(new Rect(20, 15, Screen.width * 0.48f, Screen.height - 30));

            // -- Event banner ---------------------------------------------------
            if (RainEvent.IsRaining)
            {
                GUILayout.Label("Rain — everyone moves slower…", _alertStyle);
                GUILayout.Space(6);
            }
            if (_spiderEvent != null)
            {
                if (_spiderEvent.ActiveSpider != null)
                {
                    GUILayout.Label(
                        $"SPIDER ATTACK!  HP {_spiderEvent.ActiveSpider.Health:0}/{_spiderEvent.ActiveSpider.MaxHealth:0}" +
                        "  — workers near it bite it!",
                        _alertStyle);
                    GUILayout.Space(6);
                }
                else if (Time.time < _spiderEvent.ResultVisibleUntil)
                {
                    GUILayout.Label(_spiderEvent.ResultMessage, _alertStyle);
                    GUILayout.Space(6);
                }
            }

            // -- View toggle ----------------------------------------------------
            if (_viewSwitcher != null)
            {
                string viewLabel = _viewSwitcher.IsUnderground
                    ? "Go to Surface"
                    : "Go Underground (ant farm)";
                if (GUILayout.Button(viewLabel, _buttonStyle))
                {
                    _viewSwitcher.Toggle();
                }
                GUILayout.Space(8);
            }

            // -- Resources ------------------------------------------------------
            GUILayout.Label(
                $"Food: {resources.Get(ResourceType.Food):0} / {rooms.FoodCapacity:0}", _labelStyle);
            GUILayout.Label($"Leaves: {resources.Get(ResourceType.Leaves):0}", _labelStyle);
            GUILayout.Label($"Soil: {resources.Get(ResourceType.Soil):0}", _labelStyle);
            GUILayout.Label($"DNA: {resources.Get(ResourceType.DNA):0}", _labelStyle);
            GUILayout.Space(8);
            int totalPopulation = _spawner != null ? _spawner.TotalPopulation : population;
            int populationCap = rooms.PopulationCap + game.Queen.BonusPopulationCap;
            int soldiers = game.SaveManager.Data.GetAntCount(AntIds.Soldier);
            int scouts = game.SaveManager.Data.GetAntCount(AntIds.Scout);
            GUILayout.Label(
                $"Population: {totalPopulation} / {populationCap}  (workers {population}, soldiers {soldiers}, scouts {scouts})",
                _labelStyle);

            // -- Job assignment (workers only) ----------------------------------
            WorkforceService workforce = game.Workforce;
            foreach (ResourceType type in WorkforceService.AssignableResources)
            {
                DrawJobRow(workforce, type, population);
            }
            GUILayout.Label($"Auto (nearest): {workforce.FreeWorkers(population)}", _labelStyle);

            // -- Hatch specialists ----------------------------------------------
            if (_spawner != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Hatch Soldier — 10 Food", _buttonStyle))
                {
                    _spawner.TryHatchSoldier();
                }
                if (GUILayout.Button("Hatch Scout — 15 Food", _buttonStyle))
                {
                    _spawner.TryHatchScout();
                }
                GUILayout.EndHorizontal();
            }

            // -- Queen status ---------------------------------------------------
            if (_queen != null)
            {
                string queenLine = string.IsNullOrEmpty(_queen.PausedReason)
                    ? $"Queen: next larva {Mathf.RoundToInt(_queen.BirthProgress * 100f)}%"
                    : $"Queen: {_queen.PausedReason}";
                GUILayout.Label(queenLine, _labelStyle);
            }

            // -- Queen evolution ------------------------------------------------
            QueenService queenService = game.Queen;
            GUILayout.Space(6);
            GUILayout.Label($"Queen: Lv {queenService.Level} / {QueenService.MaxLevel}", _labelStyle);
            if (queenService.Level < QueenService.MaxLevel)
            {
                var (queenFood, queenDna) = queenService.UpgradeCost;
                bool guiWasEnabled = GUI.enabled;
                GUI.enabled = queenService.CanUpgrade();
                if (GUILayout.Button($"Upgrade Queen → Lv {queenService.Level + 1}:  {queenFood} Food  {queenDna} DNA", _buttonStyle))
                {
                    queenService.TryUpgrade();
                }
                GUI.enabled = guiWasEnabled;
            }

            // -- Evolution ------------------------------------------------------
            EvolutionService evolution = game.Evolution;
            GUILayout.Space(6);
            GUILayout.Label($"Worker Evolution: Lv {evolution.GetLevel()} / {evolution.MaxLevel}", _labelStyle);
            EvolutionLevelData next = evolution.Next;
            if (next != null)
            {
                bool wasEnabled = GUI.enabled;
                GUI.enabled = evolution.CanEvolve();
                if (GUILayout.Button($"Evolve → Lv {next.level}:  {FormatCost(next.foodCost, 0, 0)} {next.dnaCost} DNA", _buttonStyle))
                {
                    evolution.TryEvolve();
                }
                GUI.enabled = wasEnabled;
            }

            GUILayout.Space(10);

            // -- Room upgrade buttons ------------------------------------------
            foreach (RoomTypeData config in rooms.Configs)
            {
                DrawRoomButton(rooms, config);
            }

            GUILayout.EndArea();

            DrawOfflinePopup(game); // drawn last so it sits on top
        }

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

        private static string FormatDuration(long seconds)
        {
            if (seconds >= 3600)
            {
                return $"{seconds / 3600}h {seconds % 3600 / 60}m";
            }
            return seconds >= 60 ? $"{seconds / 60}m" : $"{seconds}s";
        }

        private void DrawJobRow(WorkforceService workforce, ResourceType type, int population)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"  On {type}: {workforce.GetTarget(type)}", _labelStyle, GUILayout.ExpandWidth(false));
            GUILayout.Space(10);

            float buttonSize = _labelStyle.fontSize * 1.6f;
            if (GUILayout.Button("-", _buttonStyle, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
            {
                workforce.TryAdjust(type, -1, population);
            }
            if (GUILayout.Button("+", _buttonStyle, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
            {
                workforce.TryAdjust(type, +1, population);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawRoomButton(RoomService rooms, RoomTypeData config)
        {
            int level = rooms.GetLevel(config.id);

            if (rooms.IsMaxLevel(config.id))
            {
                GUILayout.Label($"{config.displayName}  Lv {level} (MAX)", _labelStyle);
                return;
            }

            var (food, leaves, soil) = rooms.GetUpgradeCost(config.id);
            string action = level == 0 ? "Build" : "Upgrade";

            // Room name on its own line, the button carries only action + cost,
            // so the price is always fully visible.
            GUILayout.Label($"{config.displayName}  Lv {level}", _labelStyle);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = rooms.CanUpgrade(config.id);
            if (GUILayout.Button($"{action} → Lv {level + 1}:  {FormatCost(food, leaves, soil)}", _buttonStyle))
            {
                rooms.TryUpgrade(config.id);
            }
            GUI.enabled = previousEnabled;
            GUILayout.Space(6);
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

            int fontSize = Mathf.RoundToInt(Screen.height * 0.028f);
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            };
            _labelStyle.normal.textColor = Color.white;

            _alertStyle = new GUIStyle(_labelStyle) { wordWrap = true };
            _alertStyle.normal.textColor = new Color(1f, 0.45f, 0.2f);

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(fontSize * 0.85f),
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 8, 8),
                wordWrap = true // never clip the upgrade cost
            };
        }
    }
}
