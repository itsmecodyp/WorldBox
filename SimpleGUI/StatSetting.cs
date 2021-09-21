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
				targetAttackRate = (float)Convert.ToInt32(GUILayout.TextField(targetAttackRate.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("speed: ");
				targetSpeed = (float)Convert.ToInt32(GUILayout.TextField(targetSpeed.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("armor: ");
				targetArmor = (float)Convert.ToInt32(GUILayout.TextField(targetArmor.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("range: ");
				targetRange = (float)Convert.ToInt32(GUILayout.TextField(targetRange.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("AreaOfEffect: ");
				targetAreaOfEffect = (float)Convert.ToInt32(GUILayout.TextField(targetAreaOfEffect.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Accuracy: ");
				targetAccuracy = (float)Convert.ToInt32(GUILayout.TextField(targetAccuracy.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Dodge: ");
				targetDodge = (float)Convert.ToInt32(GUILayout.TextField(targetDodge.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Button("Targets: ");
				targetTargets = (float)Convert.ToInt32(GUILayout.TextField(targetTargets.ToString()));
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
		public static Actor lastSelected;
		public static string traitReplacing;
	}
}
