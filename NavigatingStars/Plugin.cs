using BepInEx;
using LethalCompany.Mods;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace NavigatingStars
{
    [BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.nilaier.navigatingstars";
        public const string PLUGIN_NAME = "Navigating Stars";
        public const string PLUGIN_VERSION = "0.0.4";

        private Harmony harmony = new Harmony(PLUGIN_GUID);

        private void Awake()
        {
            this.harmony.PatchAll(typeof(Plugin));
            PluginConfig.BindConfig(Config);
            this.Logger.LogInfo($"Plugin {PLUGIN_NAME} v{PLUGIN_VERSION} is loaded!");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string[] excludedScenes = { "InitScene", "InitSceneLaunchOptions", "InitSceneLANMode", "ColdOpen1", "MainMenu", "SampleSceneRelay", "CompanyBuilding" };

            if (Array.IndexOf(excludedScenes, scene.name) != -1)
            {
                return;
            }

            EntranceTeleport mainEntrance = RoundManager.FindMainEntranceScript(true);

            if (mainEntrance != null)
            {
                PlayerControllerB localPlayer = PlayerUtils.GetPlayerControllerB();

                if (localPlayer != null && !localPlayer.gameObject.TryGetComponent(out PathDrawer pathDrawer))
                {
                    localPlayer.gameObject.AddComponent<PathDrawer>();
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void PlayerUpdate(PlayerControllerB __instance)
        {
            if (__instance == PlayerUtils.GetPlayerControllerB() && !__instance.gameObject.TryGetComponent(out PathDrawer pathDrawer))
            {
                __instance.gameObject.AddComponent<PathDrawer>();
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "ResetPlayersLoadedValueClientRpc")]
        [HarmonyPostfix]
        public static void ClearPathDrawersOnNewMap()
        {
            PlayerControllerB localPlayer = PlayerUtils.GetPlayerControllerB();

            if (localPlayer != null && localPlayer.gameObject.TryGetComponent(out PathDrawer pathDrawer))
            {
                pathDrawer.ClearLine();
            }
        }
    }

    public static class PlayerUtils
    {
        private static PlayerControllerB? localPlayerController;

        public static PlayerControllerB? GetPlayerControllerB()
        {
            if (localPlayerController == null)
            {
                if (GameNetworkManager.Instance != null)
                {
                    localPlayerController = GameNetworkManager.Instance.localPlayerController;
                    Debug.Log($"[{Plugin.PLUGIN_NAME}] GetPlayerControllerB returned: {localPlayerController?.name ?? "null"}");
                }
                else
                {
                    Debug.LogError($"[{Plugin.PLUGIN_NAME}] GameNetworkManager.Instance is null.");
                }
            }
            return localPlayerController;
        }
    }
}