using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
//using System.Drawing;
using System.IO;
using HarmonyLib;

namespace SimpleAdditions
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]

    public class Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.simple.additions";
        public const string pluginName = "SimpleAdditions";
        public const string pluginVersion = "0.0.0.0";

        public void Awake()
		{
            Harmony harmony;
            harmony = new Harmony(pluginName);
            harmony.PatchAll();
        }

        public void Update()
		{
            if(hasAddedTraits == false && AssetManager.traits != null) {
                Traits.AddTraits();
                UnityEngine.Debug.LogError("Added traits");
                hasAddedTraits = true;
			}
            if(hasAddedItems == false && AssetManager.items != null) {
                Items.AddItems();
                UnityEngine.Debug.LogError("Added items");
                hasAddedItems = true;
            }

        }
        public bool hasAddedTraits;
        public bool hasAddedItems;

    }
}
