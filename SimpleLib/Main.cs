using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using UnityEngine;


namespace SimpleLib
{
    // A collection of features
    // Might be helpful for other modders
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.simple.lib";
        public const string pluginName = "SimpleLib";
        public const string pluginVersion = "0.0.0.3";

        public void Awake()
		{

		}

        bool hasLoadedAssets;

        public void Update()
        {
			if(Input.GetKeyDown(KeyCode.N)) {

			}
        }

        public void OnGUI()
        {

        } 

     
    }


}
