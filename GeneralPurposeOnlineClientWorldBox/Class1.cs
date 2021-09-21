using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace GeneralPurposeOnlineClientWorldBox
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]

    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.online.addon";
        public const string pluginName = "Online Addon";
        public const string pluginVersion = "0.0.0.1";

        public void Awake()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(GeneralPurposeOnlineServer.OnlineMain), "ParseCommand");
            patch = AccessTools.Method(typeof(Main), "ParseCommand_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

        }
        public static void ParseCommand_Postfix(string command, string param, int connectionId /*sender*/) // another mod could patch this method
        {
            try
            {
                if (command == "test")
                {
                    MapBox.instance.spawnFlash(MapBox.instance.tilesList.GetRandom(), 10);
                    Debug.Log("Test command added from harmony patch!");
                }

            }
            catch (Exception e)
            {
                Debug.Log("Command error: " + e.ToString());
            }
            Debug.Log("finished patched: " + command + ", param: " + param);
        }
    }
}
