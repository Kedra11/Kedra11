using AntEmpire.Core;
using AntEmpire.Economy;
using UnityEngine;

namespace AntEmpire.UI
{
    /// <summary>
    /// Dev-only resource readout for the prototype sandbox, drawn with IMGUI
    /// so it needs zero scene setup. The real game will use a proper HUD
    /// (MainHUD / ResourcePanel) driven by ResourceManager events — per
    /// CLAUDE.md, UI listens to economy events and never computes economy.
    ///
    /// It still subscribes to ResourceChanged (instead of polling Get() in
    /// OnGUI) to establish the event-driven pattern early.
    /// </summary>
    public class SandboxHud : MonoBehaviour
    {
        private double _food;
        private double _leaves;
        private double _soil;
        private double _dna;

        private GUIStyle _style;

        private void Start()
        {
            var resources = GameManager.Instance.ResourceManager;
            resources.ResourceChanged += OnResourceChanged;

            _food = resources.Get(ResourceType.Food);
            _leaves = resources.Get(ResourceType.Leaves);
            _soil = resources.Get(ResourceType.Soil);
            _dna = resources.Get(ResourceType.DNA);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResourceManager.ResourceChanged -= OnResourceChanged;
            }
        }

        private void OnResourceChanged(ResourceType type, double amount)
        {
            switch (type)
            {
                case ResourceType.Food: _food = amount; break;
                case ResourceType.Leaves: _leaves = amount; break;
                case ResourceType.Soil: _soil = amount; break;
                case ResourceType.DNA: _dna = amount; break;
            }
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.RoundToInt(Screen.height * 0.03f),
                    fontStyle = FontStyle.Bold
                };
                _style.normal.textColor = Color.white;
            }

            // Plain ASCII labels — IMGUI's default font has no emoji glyphs.
            GUI.Label(new Rect(20, 15, 600, 40), $"Food: {_food:0}", _style);
            GUI.Label(new Rect(20, 15 + _style.fontSize * 1.5f, 600, 40), $"Leaves: {_leaves:0}", _style);
            GUI.Label(new Rect(20, 15 + _style.fontSize * 3.0f, 600, 40), $"Soil: {_soil:0}", _style);
            GUI.Label(new Rect(20, 15 + _style.fontSize * 4.5f, 600, 40), $"DNA: {_dna:0}", _style);
        }
    }
}
