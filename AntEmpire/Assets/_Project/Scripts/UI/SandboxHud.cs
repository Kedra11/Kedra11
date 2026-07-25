using AntEmpire.Ants;
using AntEmpire.Colony;
using AntEmpire.Core;
using AntEmpire.Economy;
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
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;

        private void Start()
        {
            _queen = FindAnyObjectByType<QueenController>();
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

            GUILayout.BeginArea(new Rect(20, 15, Screen.width * 0.42f, Screen.height - 30));

            // -- Resources ------------------------------------------------------
            GUILayout.Label(
                $"Food: {resources.Get(ResourceType.Food):0} / {rooms.FoodCapacity:0}", _labelStyle);
            GUILayout.Label($"Leaves: {resources.Get(ResourceType.Leaves):0}", _labelStyle);
            GUILayout.Label($"Soil: {resources.Get(ResourceType.Soil):0}", _labelStyle);
            GUILayout.Label($"DNA: {resources.Get(ResourceType.DNA):0}", _labelStyle);
            GUILayout.Space(8);
            GUILayout.Label($"Workers: {population} / {rooms.PopulationCap}", _labelStyle);

            // -- Job assignment -------------------------------------------------
            WorkforceService workforce = game.Workforce;
            foreach (ResourceType type in WorkforceService.AssignableResources)
            {
                DrawJobRow(workforce, type, population);
            }
            GUILayout.Label($"Auto (nearest): {workforce.FreeWorkers(population)}", _labelStyle);

            // -- Queen status ---------------------------------------------------
            if (_queen != null)
            {
                string queenLine = string.IsNullOrEmpty(_queen.PausedReason)
                    ? $"Queen: next larva {Mathf.RoundToInt(_queen.BirthProgress * 100f)}%"
                    : $"Queen: {_queen.PausedReason}";
                GUILayout.Label(queenLine, _labelStyle);
            }

            GUILayout.Space(10);

            // -- Room upgrade buttons ------------------------------------------
            foreach (RoomTypeData config in rooms.Configs)
            {
                DrawRoomButton(rooms, config);
            }

            GUILayout.EndArea();
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
            string cost = FormatCost(food, leaves, soil);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = rooms.CanUpgrade(config.id);
            if (GUILayout.Button($"{action} {config.displayName} (Lv {level})  —  {cost}", _buttonStyle))
            {
                rooms.TryUpgrade(config.id);
            }
            GUI.enabled = previousEnabled;
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

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(fontSize * 0.85f),
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 8, 8)
            };
        }
    }
}
