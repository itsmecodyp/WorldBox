using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace GeneralPurposeOnlineAddon
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]

    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.online.addon2";
        public const string pluginName = "Online Addon2"; // Stat send test
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

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(GeneralPurposeOnlineServer.NetworkClient), "ClientSendMessage");
            patch = AccessTools.Method(typeof(Main), "ClientSendMessage_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(GeneralPurposeOnlineServer.NetworkClient), "ClientSendReply");
            patch = AccessTools.Method(typeof(Main), "ClientSendReply_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
        }


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                showHideHistoryWindow = !showHideHistoryWindow;
            }
        }

        public static bool showHideHistoryWindow;
        public static Rect historyWindowRect = new Rect(0f, 1f, 1f, 1f);


        public void OnGUI()
        {
            if (showHideHistoryWindow)
            {
                historyWindowRect = GUILayout.Window(305001, historyWindowRect, new GUI.WindowFunction(HistoryWindow), "Command History", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
        }
        public void HistoryWindow(int windowID)
        {
            if (GUILayout.Button("Reset"))
            {
                commandHistory.Clear();
            }
            foreach (string command in commandHistory.Keys)
            {
                GUILayout.Label(command + ", " + commandHistory[command]);
            }
            GUI.DragWindow();
        }
        // probably want a dictionary of two lists instead..
        public static Dictionary<string, string> commandHistory = new Dictionary<string, string>();

        public static void ClientSendReply_Postfix(string input)
        {
            if (!commandHistory.ContainsKey(input))
            {
                commandHistory.Add(input, "");             
            }
        }
        public static void ClientSendMessage_Postfix(string input)
        {
            if (!commandHistory.ContainsKey(input))
            {
                commandHistory.Add(input, "");            
            }

        }
        public static void ParseCommand_Postfix(string command, string param, int connectionId, string identifier = null) // another mod could patch this method
        {                                                                 // cannot use character : (semicolon), its used to find the identifier (username)
            try
            {
                // adding the commands to history
                if (identifier != null)
                {
                    command = identifier + ": " + command;
                }
                if (!commandHistory.ContainsKey(command))
                {
                    commandHistory.Add(command, param);             
                }
            }
            catch (Exception e)
            {
                Debug.Log("Command error: " + e.ToString());
            }
        }
    }
    /*
            stats command, neat trick i wanted to try

                           leftClickCount = Config.AddSetting("Stats", "Click count total", 0, "Total amount of times left clicked");


       public static ConfigEntry<int> leftClickCount
       {
           get; set;
       }

                if (command == "stats" && GeneralPurposeOnlineServer.OnlineMain.instance.isClient)
               {
                   GeneralPurposeOnlineServer.OnlineMain.instance.clientReference.ClientSendReply("Sending stats... \n" +
                             "Map name - " + MapBox.instance.mapStats.name + "\n" +
                             "Population - " + MapBox.instance.mapStats.population + "\n" +
                             "Time - " + MapBox.instance.mapStats.worldTime + "\n"
                             );
               }

                * */
}
