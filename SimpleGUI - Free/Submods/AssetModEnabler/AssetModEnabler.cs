using System.Collections.Generic;
using System.Linq;
using BepInEx;
using SimpleGUI.Menus;
using UnityEngine;

namespace SimpleGUI.Submods.AssetModEnabler
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class AsssetModEnabler_Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.asset_mod.enabler";
        public const string pluginName = "Asset Mod Loader Enabler";
        public const string pluginVersion = "0.0.0.1";

        public void Awake()
        {
            HarmonyPatchSetup();
        }

        public void HarmonyPatchSetup()
        {
            /*
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;
            */
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {

            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
            {
                showHideMainWindow = !showHideMainWindow;
            }
            mainWindowRect.height = 0f;
        }

        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public void OnGUI()
        {
            /*
            if (GUI.Button(new Rect(Screen.width - 120, 60, 120, 20), "Clipboard"))
            {
                showHideMainWindow = !showHideMainWindow;
            }
            if (showHideMainWindow)
            {
                mainWindowRect = GUILayout.Window(410101, mainWindowRect, AssetModEnablerWindow, "Unit Clipboard", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
            */
        }




        public void AssetModEnablerWindow(int windowID)
        {
            GUI.DragWindow();
        }

    }

}