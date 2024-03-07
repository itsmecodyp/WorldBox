using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SimplerGUI.Menus
{
	class GuiStatSetting {
		public void StatSettingWindowUpdate()
		{
			if(SimpleSettings.showHideStatSettingConfig) {
				StatSettingWindowRect = GUILayout.Window(50050, StatSettingWindowRect, StatSettingWindow, "Stats", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
			}
		}

        public Vector2 scrollPositionStats;

        public void StatSettingWindow(int windowID)
		{
            GuiMain.SetWindowInUse(windowID);
			if(lastSelected == null || Config.selectedUnit != null && Config.selectedUnit != lastSelected) {
				lastSelected = Config.selectedUnit;
				GUILayout.Button("Inspect an actor");
			}
            if (lastSelected != null) {
				GUILayout.Button("Name: " + lastSelected.data.name);
                scrollPositionStats = GUILayout.BeginScrollView(
 // maybe modify height later
 scrollPositionStats, GUILayout.Width(300f), GUILayout.Height(200f));
                if (statNames.Count > 1) {
					foreach(string statNameInList in statNames.Keys) {
						//removing the c before stat names
						GUILayout.BeginHorizontal();
						string statNameWithoutPrefix = statNameInList.Remove(0, 1);
                        // display stat name
                        GUILayout.Button(statNameWithoutPrefix);
                        statsToAdd[statNameWithoutPrefix] = (float)Convert.ToDouble(GUILayout.TextField(statsToAdd[statNameWithoutPrefix].ToString(CultureInfo.CurrentCulture)));
                        //GUILayout.Button(statNameWithoutPrefix + ": " + statsToAdd[statNameWithoutPrefix]);
                        //statsToAdd[statNameWithoutPrefix] = GUILayout.HorizontalSlider(statsToAdd[statNameWithoutPrefix], 0, 10000);
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
                if (GUILayout.Button("Apply stats")) {
					foreach(string statNameInList in statNames.Keys) {
						string statNameWithoutPrefix = statNameInList.Remove(0, 1);
						float statValue = statsToAdd[statNameWithoutPrefix];
						lastSelected.data.set("c" + statNameWithoutPrefix, statValue);

					}
					lastSelected.data.set("customStats", true);
					lastSelected.data.set("hasStatTrait", false);
					lastSelected.setStatsDirty();
				}
			}
			GUI.DragWindow();
		}

		//c prefix for custom, probably unnecessary
		public static Dictionary<string, float> statNames = new Dictionary<string, float>
		{
						{"cspeed", 0},
						{"chealth", 0},
						{"cdamage", 0},
                        {"carmor", 0},
                        {"cdodge", 0},
                        {"caccuracy", 0},
                        {"cattack_speed", 0},
						{"cknockback", 0},
                        {"cknockback_reduction", 0},
                        {"ctargets", 0},
                        {"cprojectiles", 0},
                        {"carea_of_effect", 0},
						{"csize", 0},
						{"crange", 0},
						{"cscale", 0},
						{"cmod_supply_timer", 0},
						{"cfertility", 0},
						{"cmax_age", 0},
						{"cmax_children", 0},
						{"cdiplomacy", 0},
                        {"cwarfare", 0},
                        {"cstewardship", 0},
						{"cintelligence", 0},
						{"ccities", 0},
                        {"carmy", 0},
                        {"ccritical_chance", 0},
                        {"ccritical_damage_multiplier", 0},
                        {"cpersonality_aggression", 0},
                        {"cpersonality_administration", 0},
                        {"cpersonality_diplomatic", 0},
                        {"cpersonality_rationality", 0},
                        {"cbonus_towers", 0},
                        {"czone_range", 0},
                        {"cloyalty_traits", 0},
                        {"cloyalty_mood", 0},
                        {"copinion", 0},
                        {"cclan_members", 0},
						{"cstatus_chance", 0},
                        {"cdamage_range", 0}
        };

		public BaseStats statsToAdd = new BaseStats();
		public Rect StatSettingWindowRect;
		public static Actor lastSelected;

		//after stats are updated add any saved custom stats on top
		//probably bypasses any stat limiting from another mod, sorry dej
		public static void updateStats_Postfix(BaseSimObject __instance)
		{
			if(__instance.isActor() && __instance.a != null) {
				if(__instance.a.data.custom_data_bool == null) { // this is null until used OR created manually like this
					__instance.a.data.custom_data_bool = new CustomDataContainer<bool>();

				}
				if(__instance.a.data.custom_data_bool.dict.ContainsKey("customStats")) { 
					// detect bool which signals rest of logic
					__instance.a.data.get("hasStatTrait", out bool hasTrait);
					// check if hasStatTrait is false, means actor hasnt received trait or needs refresh
					if(hasTrait == false) {
						ActorTrait freshTrait = new ActorTrait
						{
							id = __instance.a.name + "Stats"
						};
						BaseStats statsToAdd = new BaseStats();
						foreach(string statName in statNames.Keys) {
							__instance.a.data.get(statName, out float value);
							string statWithoutCPrefix = statName.Remove(0, 1);
							if(value != 0) {
								statsToAdd[statWithoutCPrefix] = value;
							}
						}
						freshTrait.base_stats = statsToAdd;
						freshTrait.path_icon = __instance.a.asset.icon;
						freshTrait.can_be_given = false;
						freshTrait.inherit = 0;
						AssetManager.traits.add(freshTrait);
						if(__instance.a.hasTrait(__instance.a.name + "Stats")) {
							__instance.a.setStatsDirty(); // doing this from updateStats (the method that runs when statsDirty is true) is BAD
							// but hopefully it refreshes the new stats on actor
						}
						else {
							__instance.a.addTrait(__instance.a.name + "Stats");
							__instance.a.data.set("hasStatTrait", true);
						}
						lastSelected.data.set("hasStatTrait", true);
					}
				}
			}
		}
	}
}
