using BepInEx;
using LethalCompany.Mods;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace NavigatingStars
{
    [BepInPlugin("NavigatingStars", "NavigatingStars", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony("NavigatingStars");

        private void Awake()
        {
            // Patch all methods in the Plugin class using Harmony
            this.harmony.PatchAll(typeof(Plugin));

            // Bind the plugin configuration to the BepInEx configuration
            PluginConfig.BindConfig(Config);

            // Log a message to indicate that the plugin is loaded
            this.Logger.LogInfo((object)"Plugin NavigatingStars is loaded!");

            // Subscribe to the sceneLoaded event to perform actions when a scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Define an array of excluded scene names
            string[] excludedScenes = { "InitScene", "InitSceneLaunchOptions", "MainMenu", "SampleSceneRelay", "InitSceneLANMode", "CompanyBuilding" };

            // Check if the loaded scene is in the excluded scenes array
            if (Array.IndexOf(excludedScenes, scene.name) != -1)
            {
                // If the scene is excluded, return and do not execute further code
                return;
            }

            // Find the main entrance script using the RoundManager
            EntranceTeleport mainEntrance = RoundManager.FindMainEntranceScript(true);

            if (mainEntrance != null)
            {
                // Iterate through all player controllers in the scene
                foreach (PlayerControllerB player in UnityEngine.Object.FindObjectsOfType<PlayerControllerB>())
                {
                    // Check if the player controller does not have a PathDrawer component
                    if (((Component)player).GetComponent<PathDrawer>() == null)
                    {
                        // Add a PathDrawer component to the player controller's game object
                        ((Component)player).gameObject.AddComponent<PathDrawer>();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void PlayerUpdate(PlayerControllerB __instance)
        {
            // Check if the player controller does not have a PathDrawer component
            if (((Component)__instance).GetComponent<PathDrawer>() == null)
            {
                // Add a PathDrawer component to the player controller's game object
                ((Component)__instance).gameObject.AddComponent<PathDrawer>();
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "ResetPlayersLoadedValueClientRpc")]
        [HarmonyPostfix]
        public static void LoadNewMap()
        {
            // Iterate through all PathDrawer components in the scene
            foreach (PathDrawer pathDrawer in UnityEngine.Object.FindObjectsOfType<PathDrawer>())
            {
                // Clear the line drawn by each PathDrawer component
                pathDrawer.ClearLine();
            }
        }
    }
}