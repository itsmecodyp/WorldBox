using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleGUI
{
	class GuiItemGeneration
	{
		public void itemGenerationWindowUpdate()
		{
			if (GuiMain.showWindowMinimizeButtons.Value)
			{
				string buttontext = "I";
				if (GuiMain.showHideItemGenerationConfig.Value)
				{
					buttontext = "-";
				}
				if (GUI.Button(new Rect(itemGenerationWindowRect.x + itemGenerationWindowRect.width - 25f, itemGenerationWindowRect.y - 25, 25, 25), buttontext))
				{
					GuiMain.showHideItemGenerationConfig.Value = !GuiMain.showHideItemGenerationConfig.Value;
				}
			}
			if (lastSelectedActor == null || (lastSelectedActor != null && lastSelectedActor != Config.selectedUnit))
			{
				lastSelectedActor = Config.selectedUnit;
			}
			if (GuiMain.showHideItemGenerationConfig.Value)
			{
				itemGenerationWindowRect = GUILayout.Window(1005, itemGenerationWindowRect, new GUI.WindowFunction(ItemGenerationWindow), "Items", new GUILayoutOption[]
				{
				GUILayout.MaxWidth(300f),
				GUILayout.MinWidth(200f)
				});
			}
		}

		public static void ItemGenerationWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
			bool flag = lastSelectedActor == null;
			if (flag)
			{
				GUILayout.Button("Inspect a unit to continue", new GUILayoutOption[0]);
			}
			else
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				itemGenerationQualityString = GUILayout.TextField(itemGenerationQualityString, new GUILayoutOption[0]);
				bool flag2 = GUILayout.Button("Quality", new GUILayoutOption[0]);
				if (flag2)
				{
					bool flag3 = itemGenerationQualityString == ItemQuality.Junk.ToString();
					if (flag3)
					{
						itemGenerationQualityString = ItemQuality.Normal.ToString();
						itemGenerationQuality = ItemQuality.Normal;
						return;
					}
					bool flag4 = itemGenerationQualityString == ItemQuality.Normal.ToString();
					if (flag4)
					{
						itemGenerationQualityString = ItemQuality.Rare.ToString();
						itemGenerationQuality = ItemQuality.Rare;
						return;
					}
					bool flag5 = itemGenerationQualityString == ItemQuality.Rare.ToString();
					if (flag5)
					{
						itemGenerationQualityString = ItemQuality.Epic.ToString();
						itemGenerationQuality = ItemQuality.Epic;
						return;
					}
					bool flag6 = itemGenerationQualityString == ItemQuality.Epic.ToString();
					if (flag6)
					{
						itemGenerationQualityString = ItemQuality.Legendary.ToString();
						itemGenerationQuality = ItemQuality.Legendary;
						return;
					}
					bool flag7 = itemGenerationQualityString == ItemQuality.Legendary.ToString();
					if (flag7)
					{
						itemGenerationQualityString = ItemQuality.Junk.ToString();
						itemGenerationQuality = ItemQuality.Junk;
						return;
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				itemGenerationMaterial = GUILayout.TextField(itemGenerationMaterial, new GUILayoutOption[0]);
				bool flag8 = GUILayout.Button("Material", new GUILayoutOption[0]);
				if (flag8)
				{
					bool flag9 = itemMaterialPos > AssetManager.items_material_armor.list.Count - 2;
					if (flag9)
					{
						itemMaterialPos = 0;
						return;
					}
					bool flag10 = itemMaterialPos >= 0;
					if (flag10)
					{
						itemMaterialPos++;
					}
					int num = itemMaterialPos;
					itemGenerationMaterial = AssetManager.items_material_armor.list[itemMaterialPos].id;
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				itemGenerationPrefix = GUILayout.TextField(itemGenerationPrefix);
				if (GUILayout.Button("Prefix"))
				{
					bool flag14 = itemPrefixPos >= 0;
					if (flag14)
					{
						itemPrefixPos++;
					}
					bool flag15 = itemPrefixPos > AssetManager.items_prefix.list.Count;
					if (flag15)
					{
						itemPrefixPos = 0;
					}
					int num3 = itemPrefixPos;
					itemGenerationPrefix = AssetManager.items_prefix.list[itemPrefixPos].id;
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				itemGenerationSuffix = GUILayout.TextField(itemGenerationSuffix);
				if (GUILayout.Button("Suffix"))
				{
					bool flag14 = itemSuffixPos >= 0;
					if (flag14)
					{
						itemSuffixPos++;
					}
					bool flag15 = itemSuffixPos > AssetManager.items_suffix.list.Count;
					if (flag15)
					{
						itemSuffixPos = 0;
					}
					int num3 = itemSuffixPos;
					itemGenerationSuffix = AssetManager.items_suffix.list[itemSuffixPos].id;
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				itemGenerationSlot = GUILayout.TextField(itemGenerationSlot, new GUILayoutOption[0]);
				bool flag11 = GUILayout.Button("Slot", new GUILayoutOption[0]);
				if (flag11)
				{
					string a = itemGenerationSlot;
					bool flag12 = a == "weapon";
					if (flag12)
					{
						itemGenerationSlot = "helmet";
						return;
					}
					bool flag13 = a == "helmet";
					if (flag13)
					{
						itemGenerationSlot = "armor";
						return;
					}
					bool flag14 = a == "armor";
					if (flag14)
					{
						itemGenerationSlot = "boots";
						return;
					}
					bool flag15 = a == "boots";
					if (flag15)
					{
						itemGenerationSlot = "ring";
						return;
					}
					bool flag16 = a == "ring";
					if (flag16)
					{
						itemGenerationSlot = "amulet";
						return;
					}
					bool flag17 = a == "amulet";
					if (flag17)
					{
						itemGenerationSlot = "weapon";
						return;
					}
				}
				GUILayout.EndHorizontal();
				bool flag18 = itemGenerationSlot == "weapon" && GUILayout.Button("Weapon type: " + itemGenerationWeaponType, new GUILayoutOption[0]);
				if (flag18)
				{
					switch (itemGenerationWeaponType)
					{
						case "sword":
							itemGenerationWeaponType = "bow";
							break;
						case "bow":
							itemGenerationWeaponType = "hammer";
							break;
						case "hammer":
							itemGenerationWeaponType = "spear";
							break;
						case "spear":
							itemGenerationWeaponType = "axe";
							break;
						case "axe":
							itemGenerationWeaponType = "jaws";
							break;
						case "jaws":
							itemGenerationWeaponType = "alien_blaster";
							break;
						case "alien_blaster":
							itemGenerationWeaponType = "rocks";
							break;
						case "rocks":
							itemGenerationWeaponType = "shotgun";
							break;
						case "shotgun":
							itemGenerationWeaponType = "machinegun";
							tempSavedString = itemGenerationMaterial;
							itemGenerationMaterial = "base";
							break;
						case "machinegun":
							itemGenerationWeaponType = "claws";
							itemGenerationMaterial = "base";
							break;
						case "claws":
							itemGenerationWeaponType = "snowball";
							itemGenerationMaterial = "base";
							break;
						case "snowball":
							itemGenerationWeaponType = "flame_sword";
							itemGenerationMaterial = "base";
							break;
						case "flame_sword":
							itemGenerationWeaponType = "evil_staff";
							itemGenerationMaterial = "base";
							break;
						case "evil_staff":
							itemGenerationWeaponType = "white_staff";
							itemGenerationMaterial = "base";
							break;
						case "white_staff":
							itemGenerationWeaponType = "necromancer_staff";
							itemGenerationMaterial = "base";
							break;
						case "necromancer_staff":
							//SimpleAdditions compat
							if(AssetManager.items.get("blueSword1") != null) {
								itemGenerationWeaponType = "blueSword1";
								itemGenerationMaterial = itemGenerationWeaponType;
							}
							else {
								itemGenerationWeaponType = "sword";
								itemGenerationMaterial = tempSavedString;
								tempSavedString = "";
							}
							break;
						case "blueSword1":
							itemGenerationWeaponType = "blueSword2";
							itemGenerationMaterial = itemGenerationWeaponType;
							break;
						case "blueSword2":
							itemGenerationWeaponType = "blueSword3";
							itemGenerationMaterial = itemGenerationWeaponType;
							break;
						case "blueSword3":
							itemGenerationWeaponType = "sword"; // last valid case
							itemGenerationMaterial = tempSavedString;
							tempSavedString = "";
							break;
						default:
							itemGenerationWeaponType = "sword";
							tempSavedString = "";
							break;
					}

				}
				if(lastSelectedActor != null && GUILayout.Button("single item", new GUILayoutOption[0]))
				{
					if (itemGenerationSlot == "weapon")
					{
						ItemAsset weapon;
						if(simpleAdditionItems.Contains(itemGenerationWeaponType)) {
							weapon = AssetManager.items.get("sword");
						}
						else {
							weapon = AssetManager.items.get(itemGenerationWeaponType);
						}
						weapon.quality = itemGenerationQuality;
						bool flag26 = useRandomBaseStats;
						if (flag26)
						{
							weapon.baseStats = randomBaseStats(randomStatsMax);
						}
						manualGeneration = true;
						ItemGenerator.generateItem(weapon, itemGenerationMaterial, lastSelectedActor.equipment.weapon, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					}
					if (itemGenerationSlot == "amulet")
					{
						ItemAsset amulet = AssetManager.items.get(itemGenerationSlot);
						amulet.quality = itemGenerationQuality;
						bool flag28 = useRandomBaseStats;
						if (flag28)
						{
							amulet.baseStats = randomBaseStats(randomStatsMax);
						}
						manualGeneration = true;
						ItemGenerator.generateItem(amulet, itemGenerationMaterial, lastSelectedActor.equipment.amulet, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					}
					if(itemGenerationSlot == "armor")
					{
						ItemAsset armor = AssetManager.items.get(itemGenerationSlot);
						armor.quality = itemGenerationQuality;
						bool flag30 = useRandomBaseStats;
						if (flag30)
						{
							armor.baseStats = randomBaseStats(randomStatsMax);
						}
						manualGeneration = true;

						ItemGenerator.generateItem(armor, itemGenerationMaterial, lastSelectedActor.equipment.armor, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					}
					if (itemGenerationSlot == "boots")
					{
						ItemAsset boots = AssetManager.items.get(itemGenerationSlot);
						boots.quality = itemGenerationQuality;
						bool flag32 = useRandomBaseStats;
						if (flag32)
						{
							boots.baseStats = randomBaseStats(randomStatsMax);
						}
						manualGeneration = true;

						ItemGenerator.generateItem(boots, itemGenerationMaterial, lastSelectedActor.equipment.boots, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					}
					if (itemGenerationSlot == "helmet")
					{
						ItemAsset helmet = AssetManager.items.get(itemGenerationSlot);
						helmet.quality = itemGenerationQuality;
						bool flag34 = useRandomBaseStats;
						if (flag34)
						{
							helmet.baseStats = randomBaseStats(randomStatsMax);
						}
						manualGeneration = true;

						ItemGenerator.generateItem(helmet, itemGenerationMaterial, lastSelectedActor.equipment.helmet, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					}
					if (itemGenerationSlot == "ring")
					{
						ItemAsset ring = AssetManager.items.get(itemGenerationSlot);
						ring.quality = itemGenerationQuality;
						bool flag36 = useRandomBaseStats;
						if (flag36)
						{
							ring.baseStats = randomBaseStats(randomStatsMax);
						}
						manualGeneration = true;

						ItemGenerator.generateItem(ring, itemGenerationMaterial, lastSelectedActor.equipment.ring, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					}
					setActorStatsDirty(lastSelectedActor);

				}
				bool flag38 = lastSelectedActor != null && GUILayout.Button("whole set", new GUILayoutOption[0]);
				if (flag38)
				{
					ItemAsset weapon2 = AssetManager.items.get(itemGenerationWeaponType);
					weapon2.quality = itemGenerationQuality;
					ItemAsset ring2 = AssetManager.items.get("ring");
					ring2.quality = itemGenerationQuality;
					ItemAsset amulet2 = AssetManager.items.get("amulet");
					amulet2.quality = itemGenerationQuality;
					ItemAsset armor2 = AssetManager.items.get("armor");
					armor2.quality = itemGenerationQuality;
					ItemAsset boots2 = AssetManager.items.get("boots");
					boots2.quality = itemGenerationQuality;
					ItemAsset helmet2 = AssetManager.items.get("helmet");
					helmet2.quality = itemGenerationQuality;
					bool flag39 = useRandomBaseStats;
					if (flag39)
					{
						weapon2.baseStats = randomBaseStats(randomStatsMax);
						ring2.baseStats = randomBaseStats(randomStatsMax);
						amulet2.baseStats = randomBaseStats(randomStatsMax);
						boots2.baseStats = randomBaseStats(randomStatsMax);
						armor2.baseStats = randomBaseStats(randomStatsMax);
						helmet2.baseStats = randomBaseStats(randomStatsMax);
					}
					manualGeneration = true;

					ItemGenerator.generateItem(weapon2, itemGenerationMaterial, lastSelectedActor.equipment.weapon, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					manualGeneration = true;

					ItemGenerator.generateItem(ring2, itemGenerationMaterial, lastSelectedActor.equipment.ring, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					manualGeneration = true;

					ItemGenerator.generateItem(amulet2, itemGenerationMaterial, lastSelectedActor.equipment.amulet, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					manualGeneration = true;

					ItemGenerator.generateItem(armor2, itemGenerationMaterial, lastSelectedActor.equipment.armor, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					manualGeneration = true;

					ItemGenerator.generateItem(boots2, itemGenerationMaterial, lastSelectedActor.equipment.boots, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					manualGeneration = true;

					ItemGenerator.generateItem(helmet2, itemGenerationMaterial, lastSelectedActor.equipment.helmet, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
					setActorStatsDirty(lastSelectedActor);
				}
				bool flag41 = GUILayout.Button("Set city to armor set", new GUILayoutOption[0]);
				if (flag41)
				{
					foreach (Actor actor in lastSelectedActor.city.units)
					{
						ItemAsset weapon3 = AssetManager.items.get(itemGenerationWeaponType);
						weapon3.quality = itemGenerationQuality;
						ItemAsset ring3 = AssetManager.items.get("ring");
						ring3.quality = itemGenerationQuality;
						ItemAsset amulet3 = AssetManager.items.get("amulet");
						amulet3.quality = itemGenerationQuality;
						ItemAsset armor3 = AssetManager.items.get("armor");
						armor3.quality = itemGenerationQuality;
						ItemAsset boots3 = AssetManager.items.get("boots");
						boots3.quality = itemGenerationQuality;
						ItemAsset helmet3 = AssetManager.items.get("helmet");
						helmet3.quality = itemGenerationQuality;
						bool flag42 = useRandomBaseStats;
						if (flag42)
						{
							weapon3.baseStats = randomBaseStats(randomStatsMax);
							ring3.baseStats = randomBaseStats(randomStatsMax);
							amulet3.baseStats = randomBaseStats(randomStatsMax);
							boots3.baseStats = randomBaseStats(randomStatsMax);
							armor3.baseStats = randomBaseStats(randomStatsMax);
							helmet3.baseStats = randomBaseStats(randomStatsMax);
						}
						manualGeneration = true;

						ItemGenerator.generateItem(weapon3, itemGenerationMaterial, actor.equipment.weapon, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
						manualGeneration = true;
						ItemGenerator.generateItem(armor3, itemGenerationMaterial, actor.equipment.armor, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
						manualGeneration = true;
						ItemGenerator.generateItem(boots3, itemGenerationMaterial, actor.equipment.boots, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
						manualGeneration = true;
						ItemGenerator.generateItem(helmet3, itemGenerationMaterial, actor.equipment.helmet, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
						manualGeneration = true;
						ItemGenerator.generateItem(amulet3, itemGenerationMaterial, actor.equipment.amulet, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
						manualGeneration = true;
						ItemGenerator.generateItem(ring3, itemGenerationMaterial, actor.equipment.ring, MapBox.instance.mapStats.year, lastSelectedActor.kingdom.name, "a mod", 1);
						setActorStatsDirty(actor);
					}
				}
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				if (GUILayout.Button("Toggle random stats on item"))
				{
					useRandomBaseStats = !useRandomBaseStats;
				}
				if (useRandomBaseStats)
				{
					int newNumber;
					randomStatsMaxString = GUILayout.TextField(randomStatsMaxString, new GUILayoutOption[0]);
					try
					{
						newNumber = Convert.ToInt32(randomStatsMaxString);
					}
					catch (System.Exception exception)
					{
						Debug.Log(exception.GetType() + ", resetting stats input");
						newNumber = 0;
						randomStatsMaxString = "0";
					}
					randomStatsMax = newNumber;
				}
				GUILayout.EndHorizontal();
			}
			GUI.DragWindow();
		}

		public static void setActorStatsDirty(Actor target)
		{
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
									| BindingFlags.Static;
			FieldInfo field = typeof(Actor).GetField("statsDirty", bindFlags);
			field.SetValue(target, true);
		}

		public static BaseStats randomBaseStats(int maxRange)
		{
			return new BaseStats
			{
				accuracy = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				areaOfEffect = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				armor = UnityEngine.Random.Range(1, maxRange),
				attackSpeed = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				crit = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				damage = UnityEngine.Random.Range(1, maxRange),
				damageCritMod = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				diplomacy = UnityEngine.Random.Range(1, maxRange),
				dodge = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				health = UnityEngine.Random.Range(1, maxRange),
				knockback = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				knockbackReduction = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				range = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				size = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				speed = UnityEngine.Random.Range(-(float)maxRange, (float)maxRange),
				targets = UnityEngine.Random.Range(1, maxRange)
			};
		}

		public static bool setItem_Prefix(ItemData pData, ActorEquipmentSlot __instance)
		{
			if (manualGeneration)
			{
				pData.prefix = itemGenerationPrefix;
				pData.suffix = itemGenerationSuffix;
				__instance.data = pData;
				manualGeneration = false;
				return false;
			}
			return true;
		}

		public static bool manualGeneration; public static string itemGenerationWeaponType = "sword";
		public static string itemGenerationPrefix = "perfect";
		public static string itemGenerationSuffix = "terror";
		public static string itemGenerationMaterial = "adamantine";
		public static string itemGenerationSlot = "weapon";
		public static string itemGenerationQualityString = ItemQuality.Legendary.ToString();
		public static ItemQuality itemGenerationQuality = ItemQuality.Legendary;
		public static int itemSuffixPos;
		public static int itemPrefixPos;
		public static int itemMaterialPos;
		public static int itemSlotPos;
		public static int randomStatsMax = 10;
		public static string randomStatsMaxString = "10";
		public static bool useRandomBaseStats;
		public static Actor lastSelectedActor;
		public bool showHideItemGeneration;
		public Rect itemGenerationWindowRect; public static string tempSavedString;

		public static List<string> simpleAdditionItems = new List<string>() { "blueSword1", "blueSword2", "blueSword3" };
	}
}
