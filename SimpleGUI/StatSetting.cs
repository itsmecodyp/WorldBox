using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGUI
{
	class GuiStatSetting {
		public void StatSettingWindowUpdate()
		{
			if(GuiMain.showWindowMinimizeButtons.Value) {
				string buttontext = "S";
				if(GuiMain.showHideStatSettingConfig.Value) {
					buttontext = "-";
				}
				if(GUI.Button(new Rect(StatSettingWindowRect.x + StatSettingWindowRect.width - 25f, StatSettingWindowRect.y - 25, 25, 25), buttontext)) {
					GuiMain.showHideStatSettingConfig.Value = !GuiMain.showHideStatSettingConfig.Value;
				}
			}

			if(GuiMain.showHideStatSettingConfig.Value) {
				StatSettingWindowRect = GUILayout.Window(50050, StatSettingWindowRect, new GUI.WindowFunction(StatSettingWindow), "Stats", new GUILayoutOption[]
				{
					GUILayout.MaxWidth(300f),
					GUILayout.MinWidth(200f)
				});
			}
		}

		public void StatSettingWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
			if(lastSelected == null || Config.selectedUnit != null && Config.selectedUnit != lastSelected) {
				lastSelected = Config.selectedUnit;
			}
			if(lastSelected != null) {
				GUILayout.Button("Name: " + lastSelected.data.name);
				if(statNames.Count > 1) {
					foreach(string statNameInList in statNames.Keys) {
						string statNameWithoutPrefix = statNameInList.Remove(0, 1);
						GUILayout.BeginHorizontal();
						GUILayout.Button(statNameWithoutPrefix); // display stat name
						// why is there no Convert.ToFloat?
						statsToAdd[statNameWithoutPrefix] = (float)Convert.ToDouble(GUILayout.TextField(statsToAdd[statNameWithoutPrefix].ToString()));
						GUILayout.EndHorizontal();
					}
				}
				if(GUILayout.Button("Apply stats")) {
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
		public static Dictionary<string, float> statNames = new Dictionary<string, float>() {
						{"cspeed", 0},
						{"chealth", 0},
						{"cdamage", 0},
						{"cattack_speed", 0},
						{"cknockback", 0},
						{"ctargets", 0},
						{"carea_of_effect", 0},
						{"csize", 0},
						{"crange", 0},
						{"ccritical_damage_multiplier", 0},
						{"cscale", 0},
						{"cmod_supply_timer", 0},
						{"cfertility", 0},
						{"cmax_age", 0},
						{"cmax_children", 0},
						{"cdiplomacy", 0},
						{"cstewardship", 0},
						{"cintelligence", 0},
						{"cloyalty_traits", 0},
						{"cdamage_range", 0},
						{"ccities", 0},
						{"cpersonality_rationality", 0},
						{"ccritical_chance", 0},
						{"carmy", 0},
		};

		public BaseStats statsToAdd = new BaseStats();
		public Rect StatSettingWindowRect;
		public static Actor lastSelected;
		public static Vector2 scrollPosition;

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
					__instance.a.data.get("hasStatTrait", out bool hasTrait, false);
					// check if hasStatTrait is false, means actor hasnt received trait or needs refresh
					if(hasTrait == false) {
						ActorTrait freshTrait = new ActorTrait();
						freshTrait.id = __instance.a.name + "Stats";
						BaseStats statsToAdd = new BaseStats();
						foreach(string statName in GuiStatSetting.statNames.Keys) {
							__instance.a.data.get(statName, out float value, 0f);
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

	

		/* remove and remake post 0.15, new custom data on actors is perfect
	class GuiStatSetting {
		public void StatSettingWindowUpdate()
		{
			if(GuiMain.showWindowMinimizeButtons.Value) {
				string buttontext = "S";
				if(GuiMain.showHideStatSettingConfig.Value) {
					buttontext = "-";
				}
				if(GUI.Button(new Rect(StatSettingWindowRect.x + StatSettingWindowRect.width - 25f, StatSettingWindowRect.y - 25, 25, 25), buttontext)) {
					GuiMain.showHideStatSettingConfig.Value = !GuiMain.showHideStatSettingConfig.Value;
				}
			}

			//
			if(GuiMain.showHideStatSettingConfig.Value) {
				StatSettingWindowRect = GUILayout.Window(50050, StatSettingWindowRect, new GUI.WindowFunction(StatSettingWindow), "Stats", new GUILayoutOption[]
				{
				GUILayout.MaxWidth(300f),
				GUILayout.MinWidth(200f)
				});
			}
		}

		public static Vector2 scrollPosition;

		public void StatSettingWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
			if(lastSelected == null || Config.selectedUnit != null && Config.selectedUnit != lastSelected) {
				lastSelected = Config.selectedUnit;
			}
			GUI.backgroundColor = Color.grey;
			scrollPosition = GUILayout.BeginScrollView(
		  scrollPosition, GUILayout.Width(225), GUILayout.Height(250));
			if(Config.selectedUnit != null) {
				ActorStatus data = Reflection.GetField(lastSelected.GetType(), lastSelected, "data") as ActorStatus;
				ActorStats stats = Reflection.GetField(lastSelected.GetType(), lastSelected, "stats") as ActorStats;
				BaseStats curStats = Reflection.GetField(lastSelected.GetType(), lastSelected, "curStats") as BaseStats;

				GUILayout.Button(data.firstName, GUILayout.Width(200));
				bool flag2 = false; //GUILayout.Button("Set to current stats");
				if(flag2) {
					targetHealth = curStats.health;
					targetAreaOfEffect = curStats.areaOfEffect;
					targetArmor = (float)curStats.armor;
					targetSpeed = curStats.speed;
					targetAttackRate = curStats.attackSpeed;
					targetAttackDamage = curStats.damage;
					targetHealth = curStats.health;
					targetRange = curStats.range;
					targetTargets = (float)curStats.targets;
					targetDodge = curStats.dodge;
					targetAccuracy = curStats.accuracy;
				}
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				bool flag3 = GUILayout.Button("Health: ");
				if(flag3) {
					lastSelected.restoreHealth(curStats.health);
				}
				targetHealth = Convert.ToInt32(GUILayout.TextField(targetHealth.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("attackDamage: ");
				targetAttackDamage = Convert.ToInt32(GUILayout.TextField(targetAttackDamage.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("attackRate: ");
				targetAttackRate = float.Parse(GUILayout.TextField(targetAttackRate.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("speed: ");
				targetSpeed = float.Parse(GUILayout.TextField(targetSpeed.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("armor: ");
				targetArmor = float.Parse(GUILayout.TextField(targetArmor.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("range: ");
				targetRange = float.Parse(GUILayout.TextField(targetRange.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("AreaOfEffect: ");
				targetAreaOfEffect = float.Parse(GUILayout.TextField(targetAreaOfEffect.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Accuracy: ");
				targetAccuracy = float.Parse(GUILayout.TextField(targetAccuracy.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Dodge: ");
				targetDodge = float.Parse(GUILayout.TextField(targetDodge.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("pAggression: ");
				targetPersonality_aggression = float.Parse(GUILayout.TextField(targetPersonality_aggression.ToString()));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("pAdministration: ");
				targetPersonality_administration = float.Parse(GUILayout.TextField(targetPersonality_administration.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("pDiplomatic: ");
				targetPersonality_diplomatic = float.Parse(GUILayout.TextField(targetPersonality_diplomatic.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("pRationality: ");
				targetPersonality_rationality = float.Parse(GUILayout.TextField(targetPersonality_rationality.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Diplomacy: ");
				targetDiplomacy = Convert.ToInt32(GUILayout.TextField(targetDiplomacy.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Warfare: ");
				targetWarfare = Convert.ToInt32(GUILayout.TextField(targetWarfare.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Stewardship: ");
				targetStewardship = Convert.ToInt32(GUILayout.TextField(targetStewardship.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Intelligence: ");
				targetIntelligence = Convert.ToInt32(GUILayout.TextField(targetIntelligence.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Army: ");
				targetArmy = Convert.ToInt32(GUILayout.TextField(targetArmy.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Cities: ");
				targetCities = Convert.ToInt32(GUILayout.TextField(targetCities.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Zones: ");
				targetZones = Convert.ToInt32(GUILayout.TextField(targetZones.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Bonus_towers: ");
				targetBonus_towers = Convert.ToInt32(GUILayout.TextField(targetBonus_towers.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("s_crit_chance: ");
				targetS_crit_chance = float.Parse(GUILayout.TextField(targetS_crit_chance.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Projectiles: ");
				targetProjectiles = Convert.ToInt32(GUILayout.TextField(targetProjectiles.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Crit: ");
				targetCrit = float.Parse(GUILayout.TextField(targetCrit.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("DamageCritMod: ");
				targetDamageCritMod = float.Parse(GUILayout.TextField(targetDamageCritMod.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Size: ");
				targetSize = Convert.ToInt32(GUILayout.TextField(targetSize.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Knockback: ");
				targetKnockback = Convert.ToInt32(GUILayout.TextField(targetKnockback.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Loyalty_traits: ");
				targetLoyalty_traits = Convert.ToInt32(GUILayout.TextField(targetLoyalty_traits.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Loyalty_mood: ");
				targetLoyalty_mood = Convert.ToInt32(GUILayout.TextField(targetLoyalty_mood.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Opinion: ");
				targetOpinion = Convert.ToInt32(GUILayout.TextField(targetOpinion.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("KnockbackReduction: ");
				targetKnockbackReduction = float.Parse(GUILayout.TextField(targetKnockbackReduction.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Scale: ");
				targetScale = float.Parse(GUILayout.TextField(targetScale.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Mod_supply_timer: ");
				targetMod_supply_timer = float.Parse(GUILayout.TextField(targetMod_supply_timer.ToString()));
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Button("Inherit: ");
				targetInherit = (float)Convert.ToInt32(GUILayout.TextField(targetInherit.ToString()));
				GUILayout.EndHorizontal();
				if(!lastSelected.haveTrait("stats" + data.firstName)) {
					GUI.backgroundColor = Color.red;
				}
				else {
					GUI.backgroundColor = Color.green;
				}
				if(GUILayout.Button("Add stats to target", GUILayout.Width(200))) {
					AssetManager.actor_library.get("human").
					ActorTrait actorTrait = new ActorTrait();
					actorTrait.id = "stats" + data.firstName;
					actorTrait.path_icon = "iconVermin";
					actorTrait.baseStats.health = targetHealth;
					actorTrait.baseStats.damage = targetAttackDamage;
					actorTrait.baseStats.speed = targetSpeed;
					actorTrait.baseStats.attackSpeed = targetAttackRate;
					actorTrait.baseStats.armor = (int)targetArmor;
					actorTrait.baseStats.range = targetRange;
					actorTrait.baseStats.areaOfEffect = targetAreaOfEffect;
					actorTrait.baseStats.accuracy = targetAccuracy;
					actorTrait.baseStats.dodge = targetDodge;
					actorTrait.baseStats.targets = (int)targetTargets;

					actorTrait.baseStats.personality_aggression = targetPersonality_aggression;
					actorTrait.baseStats.personality_administration = targetPersonality_administration;
					actorTrait.baseStats.personality_diplomatic = targetPersonality_diplomatic;
					actorTrait.baseStats.personality_rationality = targetPersonality_rationality;
					actorTrait.baseStats.diplomacy = targetDiplomacy;
					actorTrait.baseStats.warfare = targetWarfare;
					actorTrait.baseStats.stewardship = targetStewardship;
					actorTrait.baseStats.intelligence = targetIntelligence;
					actorTrait.baseStats.army = targetArmy;
					actorTrait.baseStats.cities = targetCities;
					actorTrait.baseStats.zone_range = targetZones;
					actorTrait.baseStats.bonus_towers = targetBonus_towers;
					actorTrait.baseStats.s_crit_chance = targetS_crit_chance;
					actorTrait.baseStats.projectiles = targetProjectiles;
					actorTrait.baseStats.crit = targetCrit;
					actorTrait.baseStats.damageCritMod = targetDamageCritMod;
					actorTrait.baseStats.size = targetSize;
					actorTrait.baseStats.knockback = targetKnockback;
					actorTrait.baseStats.loyalty_traits = targetLoyalty_traits;
					actorTrait.baseStats.loyalty_mood = targetLoyalty_mood;
					actorTrait.baseStats.opinion = targetOpinion;
					actorTrait.baseStats.knockbackReduction = targetKnockbackReduction;
					actorTrait.baseStats.scale = targetScale;
					actorTrait.baseStats.mod_supply_timer = targetMod_supply_timer;


					actorTrait.inherit = 0f;
					/*actionTest(null, MapBox.instance.tilesList.GetRandom()
					bool flag5 = traitSprite != null;
					if (flag5)
					{
						actorTrait.icon = "custom";
					}

					AssetManager.traits.add(actorTrait);
					if(lastSelected.haveTrait(actorTrait.id)) {
						lastSelected.removeTrait(actorTrait.id);
					}
					lastSelected.addTrait(actorTrait.id);
					lastSelected.restoreHealth(curStats.health);
					bool statsDirty = (bool)Reflection.GetField(lastSelected.GetType(), lastSelected, "statsDirty");
					statsDirty = true;
				}
				GUI.backgroundColor = Color.grey;
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				bool flag6 = GUILayout.Button("Add stats to trait: ");
				if(flag6) {
					ActorTrait actorTrait2 = AssetManager.traits.get(traitReplacing);
					actorTrait2.baseStats.health = targetHealth;
					actorTrait2.baseStats.damage = targetAttackDamage;
					actorTrait2.baseStats.speed = targetSpeed;
					actorTrait2.baseStats.attackSpeed = targetAttackRate;
					actorTrait2.baseStats.armor = (int)targetArmor;
					actorTrait2.baseStats.range = targetRange;
					actorTrait2.baseStats.areaOfEffect = targetAreaOfEffect;
					actorTrait2.baseStats.accuracy = targetAccuracy;
					actorTrait2.baseStats.dodge = targetDodge;
					actorTrait2.baseStats.targets = (int)targetTargets;
					actorTrait2.inherit = targetInherit;

					AssetManager.traits.add(actorTrait2);
				}
				traitReplacing = GUILayout.TextField(traitReplacing);
				GUILayout.EndHorizontal();
				bool flag7 = GUILayout.Button("Make leader", GUILayout.Width(200));
				if(flag7) {
					lastSelected.city.leader = lastSelected;
				}
				bool flag8 = GUILayout.Button("Make king", GUILayout.Width(200));
				if(flag8) {
					lastSelected.kingdom.king = lastSelected;
				}
				bool flag9 = GUILayout.Button("Set city to stats", GUILayout.Width(200));
				if(flag9) {
					ActorTrait actorTrait3 = new ActorTrait();
					actorTrait3.baseStats.health = targetHealth;
					actorTrait3.baseStats.damage = targetAttackDamage;
					actorTrait3.baseStats.speed = targetSpeed;
					actorTrait3.baseStats.attackSpeed = targetAttackRate;
					actorTrait3.baseStats.armor = (int)targetArmor;
					actorTrait3.baseStats.range = targetRange;
					actorTrait3.baseStats.areaOfEffect = targetAreaOfEffect;
					actorTrait3.inherit = 0f;
					foreach(Actor actor in lastSelected.city.units) {
						ActorStatus cityActorData = Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;

						actorTrait3.id = "stats" + cityActorData.firstName;
						AssetManager.traits.add(actorTrait3);
						actor.addTrait(actorTrait3.id);
					}
				}
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				/*
				bool flag10 = GUILayout.Button("head size-");
				if (flag10)
				{
					lastSelected.head.transform.localScale /= 1.5f;
				}
				bool flag11 = GUILayout.Button("head size+");
				if (flag11)
				{
					lastSelected.head.transform.localScale *= 1.5f;
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				// unnecessary when traits have size now
				bool flag12 = GUILayout.Button("body size-");
				if (flag12)
				{
					lastSelected.transform.localScale /= 1.5f;
				}
				bool flag13 = GUILayout.Button("body size+");
				if (flag13)
				{
					lastSelected.transform.localScale *= 1.5f;
				}
				GUILayout.EndHorizontal();
			}
			else {
				GUILayout.Button("Need inspected unit", GUILayout.Width(200));

			}
			GUILayout.EndScrollView();
			GUI.DragWindow();
		}

		public Rect StatSettingWindowRect;
		public static float targetSpeed;
		public static int targetAttackDamage;
		public static float targetAttackRate;
		public static int targetHealth;
		public static int targetLevel;
		public static float targetInherit;
		public static float targetAccuracy;
		public static float targetDodge;
		public static float targetTargets;
		public static float targetArmor;
		public static float targetRange;
		public static float targetAreaOfEffect;

		public float targetPersonality_aggression;
		public float targetPersonality_administration;
		public float targetPersonality_diplomatic;
		public float targetPersonality_rationality;
		public int targetDiplomacy;
		public int targetWarfare;
		public int targetStewardship;
		public int targetIntelligence;
		public int targetArmy;
		public int targetCities;
		public int targetZones;
		public int targetBonus_towers;
		public float targetS_crit_chance;
		public int targetProjectiles;
		public float targetCrit;
		public float targetDamageCritMod;
		public float targetSize;
		public float targetKnockback;
		public int targetLoyalty_traits;
		public int targetLoyalty_mood;
		public int targetOpinion;
		public float targetKnockbackReduction;

		public float targetScale;
		public float targetMod_supply_timer;

		public static Actor lastSelected;
		public static string traitReplacing;
	}
	*/
				}
