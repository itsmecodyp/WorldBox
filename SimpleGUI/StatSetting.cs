using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;
namespace SimpleGUI
{
    class GuiStatSetting
    {
        public void StatSettingWindowUpdate()
        {
			if (GuiMain.showWindowMinimizeButtons.Value)
			{
				string buttontext = "S";
				if (GuiMain.showHideStatSettingConfig.Value)
				{
					buttontext = "-";
				}
				if (GUI.Button(new Rect(StatSettingWindowRect.x + StatSettingWindowRect.width - 25f, StatSettingWindowRect.y - 25, 25, 25), buttontext))
				{
					GuiMain.showHideStatSettingConfig.Value = !GuiMain.showHideStatSettingConfig.Value;
				}
			}
		
			//
			if (GuiMain.showHideStatSettingConfig.Value)
            {
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
			if (lastSelected == null || Config.selectedUnit != null && Config.selectedUnit != lastSelected)
			{
				lastSelected = Config.selectedUnit;
			}
			GUI.backgroundColor = Color.grey;
			if (Config.selectedUnit != null)
			{
				ActorStatus data = Reflection.GetField(lastSelected.GetType(), lastSelected, "data") as ActorStatus;
				ActorStats stats = Reflection.GetField(lastSelected.GetType(), lastSelected, "stats") as ActorStats;
				BaseStats curStats = Reflection.GetField(lastSelected.GetType(), lastSelected, "curStats") as BaseStats;

				GUILayout.Button(data.firstName);
				bool flag2 = false; //GUILayout.Button("Set to current stats");
				if (flag2)
				{
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
				GUILayout.BeginHorizontal();
				bool flag3 = GUILayout.Button("Health: ");
				if (flag3)
				{
					lastSelected.restoreHealth(curStats.health);
				}
				targetHealth = Convert.ToInt32(GUILayout.TextField(targetHealth.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("attackDamage: ");
				targetAttackDamage = Convert.ToInt32(GUILayout.TextField(targetAttackDamage.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("attackRate: ");
				targetAttackRate = float.Parse(GUILayout.TextField(targetAttackRate.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("speed: ");
				targetSpeed = float.Parse(GUILayout.TextField(targetSpeed.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("armor: ");
				targetArmor = float.Parse(GUILayout.TextField(targetArmor.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("range: ");
				targetRange = float.Parse(GUILayout.TextField(targetRange.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("AreaOfEffect: ");
				targetAreaOfEffect = float.Parse(GUILayout.TextField(targetAreaOfEffect.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Accuracy: ");
				targetAccuracy = float.Parse(GUILayout.TextField(targetAccuracy.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Dodge: ");
				targetDodge = float.Parse(GUILayout.TextField(targetDodge.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Personality_aggression: ");
				targetPersonality_aggression = float.Parse(GUILayout.TextField(targetPersonality_aggression.ToString()));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Button("targetPersonality_administration: ");
				targetPersonality_administration = float.Parse(GUILayout.TextField(targetPersonality_administration.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Personality_diplomatic: ");
				targetPersonality_diplomatic = float.Parse(GUILayout.TextField(targetPersonality_diplomatic.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Personality_rationality: ");
				targetPersonality_rationality = float.Parse(GUILayout.TextField(targetPersonality_rationality.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Diplomacy: ");
				targetDiplomacy = Convert.ToInt32(GUILayout.TextField(targetDiplomacy.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Warfare : ");
				targetWarfare = Convert.ToInt32(GUILayout.TextField(targetWarfare.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Stewardship: ");
				targetStewardship = Convert.ToInt32(GUILayout.TextField(targetStewardship.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Intelligence: ");
				targetIntelligence = Convert.ToInt32(GUILayout.TextField(targetIntelligence.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Army: ");
				targetArmy = Convert.ToInt32(GUILayout.TextField(targetArmy.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Cities: ");
				targetCities = Convert.ToInt32(GUILayout.TextField(targetCities.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Zones: ");
				targetZones = Convert.ToInt32(GUILayout.TextField(targetZones.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Bonus_towers: ");
				targetBonus_towers = Convert.ToInt32(GUILayout.TextField(targetBonus_towers.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("s_crit_chance: ");
				targetS_crit_chance = float.Parse(GUILayout.TextField(targetS_crit_chance.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Projectiles: ");
				targetProjectiles = Convert.ToInt32(GUILayout.TextField(targetProjectiles.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Crit: ");
				targetCrit = float.Parse(GUILayout.TextField(targetCrit.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("DamageCritMod: ");
				targetDamageCritMod = float.Parse(GUILayout.TextField(targetDamageCritMod.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Size: ");
				targetSize = Convert.ToInt32(GUILayout.TextField(targetSize.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Knockback: ");
				targetKnockback = Convert.ToInt32(GUILayout.TextField(targetKnockback.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Loyalty_traits: ");
				targetLoyalty_traits = Convert.ToInt32(GUILayout.TextField(targetLoyalty_traits.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Loyalty_mood: ");
				targetLoyalty_mood = Convert.ToInt32(GUILayout.TextField(targetLoyalty_mood.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Opinion: ");
				targetOpinion = Convert.ToInt32(GUILayout.TextField(targetOpinion.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("KnockbackReduction: ");
				targetKnockbackReduction = float.Parse(GUILayout.TextField(targetKnockbackReduction.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Scale: ");
				targetScale = float.Parse(GUILayout.TextField(targetScale.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Mod_supply_timer: ");
				targetMod_supply_timer = float.Parse(GUILayout.TextField(targetMod_supply_timer.ToString()));
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				GUILayout.Button("Inherit: ");
				targetInherit = (float)Convert.ToInt32(GUILayout.TextField(targetInherit.ToString()));
				GUILayout.EndHorizontal();
				if (!lastSelected.haveTrait("stats" + data.firstName))
				{
					GUI.backgroundColor = Color.red;
				}
				else
				{
					GUI.backgroundColor = Color.green;
				}
				if (GUILayout.Button("Add stats to target"))
				{
					ActorTrait actorTrait = new ActorTrait();
					actorTrait.id = "stats" + data.firstName;
					actorTrait.icon = "iconVermin";
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
					actorTrait.baseStats.zones = targetZones;
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
					*/
					AssetManager.traits.add(actorTrait);
					if (lastSelected.haveTrait(actorTrait.id))
					{
						lastSelected.removeTrait(actorTrait.id);
					}
					lastSelected.addTrait(actorTrait.id);
					lastSelected.restoreHealth(curStats.health);
					bool statsDirty = (bool)Reflection.GetField(lastSelected.GetType(), lastSelected, "statsDirty");
					statsDirty = true;
				}
				GUI.backgroundColor = Color.grey;
				GUILayout.BeginHorizontal();
				bool flag6 = GUILayout.Button("Add stats to trait: ");
				if (flag6)
				{
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
				bool flag7 = GUILayout.Button("Make leader");
				if (flag7)
				{
					lastSelected.city.leader = lastSelected;
				}
				bool flag8 = GUILayout.Button("Make king");
				if (flag8)
				{
					lastSelected.kingdom.king = lastSelected;
				}
				bool flag9 = GUILayout.Button("Set city to stats");
				if (flag9)
				{
					ActorTrait actorTrait3 = new ActorTrait();
					actorTrait3.baseStats.health = targetHealth;
					actorTrait3.baseStats.damage = targetAttackDamage;
					actorTrait3.baseStats.speed = targetSpeed;
					actorTrait3.baseStats.attackSpeed = targetAttackRate;
					actorTrait3.baseStats.armor = (int)targetArmor;
					actorTrait3.baseStats.range = targetRange;
					actorTrait3.baseStats.areaOfEffect = targetAreaOfEffect;
					actorTrait3.inherit = 0f;
					foreach (Actor actor in lastSelected.city.units)
					{
						ActorStatus cityActorData = Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;

						actorTrait3.id = "stats" + cityActorData.firstName;
						AssetManager.traits.add(actorTrait3);
						actor.addTrait(actorTrait3.id);
					}
				}
				GUILayout.BeginHorizontal();
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
				*/
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				// unnecessary when traits have size now
				/*
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
				*/
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.Button("Need inspected unit");
				
			}
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
}
