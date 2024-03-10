using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalCompany.Mods
{
    public class PluginConfig
    {
        // Configuration entry for the toggle key used to draw the line
        public static ConfigEntry<Key> ToggleKey;

        // Configuration entry for the width of the drawn line
        public static ConfigEntry<float> LineWidth;

        // Configuration entry for the color of the drawn line
        public static ConfigEntry<Color> LineColor;

        // Configuration entry for the distance threshold to auto-clear the line when the player is near the main entrance
        public static ConfigEntry<float> AutoClearDistance;

        // Configuration entry to determine whether the line should be redrawn dynamically based on the player's position
        public static ConfigEntry<bool> DynamicLineRedraw;

        /// <summary>
        /// Binds the configuration entries to the specified ConfigFile.
        /// </summary>
        /// <param name="config">The ConfigFile to bind the configuration entries to.</param>
        public static void BindConfig(ConfigFile config)
        {
            // Bind the ToggleKey configuration entry
            // Default value: Key.T
            // Description: The key to press for drawing the line
            ToggleKey = config.Bind("General", "ToggleKey", Key.T, "The key to press for drawing the line.");

            // Bind the LineWidth configuration entry
            // Default value: 0.1f
            // Description: The width of the drawn line
            LineWidth = config.Bind("General", "LineWidth", 0.1f, "The width of the drawn line.");

            // Bind the LineColor configuration entry
            // Default value: Color.green
            // Description: The color of the drawn line
            LineColor = config.Bind("General", "LineColor", Color.green, "The color of the drawn line.");

            // Bind the AutoClearDistance configuration entry
            // Default value: 5f
            // Description: The distance threshold for auto-clearing the line when the player is near the main entrance
            AutoClearDistance = config.Bind("General", "AutoClearDistance", 5f, "The distance threshold for auto-clearing the line when the player is near the main entrance.");

            // Bind the DynamicLineRedraw configuration entry
            // Default value: false
            // Description: Determines whether the line should be redrawn dynamically based on the player's position
            DynamicLineRedraw = config.Bind("General", "DynamicLineRedraw", false, "Determines whether the line should be redrawn dynamically based on the player's position.");
        }
    }
}