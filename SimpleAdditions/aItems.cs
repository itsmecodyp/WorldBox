using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.UI;
using DG.Tweening;

namespace SimpleAdditions {
	class aItems {

		public static Dictionary<string, Sprite> newWeaponsIcons = new Dictionary<string, Sprite>();
		// each item needs added in simplegui.itemGeneration.simpleAdditionWeapons and the switch statement there
		public static void AddItems()
		{
			ItemAsset blueSword1 = new ItemAsset() {
				id = "blueSword1",
				materials = new List<String>() { "blueSword1" },
				slash = "punch",
				baseStats = new BaseStats(),
				attackType = WeaponType.Melee
			};
			AssetManager.items.add(blueSword1);
			//ActorAnimationLoader.addSprite("w_" + "sword" + "_" + blueSword1.id,
				//LoadSprite(Properties.Resources.bluesword1, 8, 8, 0.8f, 0.15f));
			newWeaponsIcons.Add(blueSword1.materials[0], LoadSprite(Properties.Resources.bluesword1, 0, 0, 0f, 0f));

			ItemAsset blueSword2 = new ItemAsset() {
				id = "blueSword2",
				materials = new List<String>() { "blueSword2" },
				slash = "punch",
				baseStats = new BaseStats(),
				attackType = WeaponType.Melee
			};
			AssetManager.items.add(blueSword2);
			//ActorAnimationLoader.addSprite("w_" + "sword" + "_" + blueSword2.id,
				//LoadSprite(Properties.Resources.bluesword2, 8, 8, 0.8f, 0.15f));
			newWeaponsIcons.Add(blueSword2.materials[0], LoadSprite(Properties.Resources.bluesword2, 16, 16, 0f, 0f));

			ItemAsset blueSword3 = new ItemAsset() {
				id = "blueSword3",
				materials = new List<String>() { "blueSword3" },
				slash = "punch",
				baseStats = new BaseStats(),
				attackType = WeaponType.Melee
				//tech_needed = "weapon_axe"
			};
			AssetManager.items.add(blueSword3);
			//ActorAnimationLoader.addSprite("w_" + "sword" + "_" + blueSword3.id,
				//LoadSprite(Properties.Resources.bluesword3, 8, 8, 0.8f, 0.15f));
			newWeaponsIcons.Add(blueSword3.materials[0], LoadSprite(Properties.Resources.bluesword3, 16, 16, 0f, 0f));

		}


		public static Sprite LoadSprite(byte[] bytes, int resizeX = 0, int resizeY = 0, float offsetx = 0f, float offsety = 0.5f)
		{
			byte[] data = bytes;
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.anisoLevel = 0;
			texture2D.LoadImage(data);
			texture2D.filterMode = FilterMode.Point;
			if(resizeX != 0) {
				TextureScale.Point(texture2D, resizeX, resizeY);
			}
			return Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(offsetx, offsety), 1f);
		}
	}

	// crabzilla spams NRE here
	[HarmonyPatch(typeof(EquipmentButton))]
	class EquipmentButton_load {
		[HarmonyPatch("load", MethodType.Normal)]
		public static bool Prefix(ActorEquipmentSlot pSlot, EquipmentButton __instance)
		{
			if(pSlot != null && aItems.newWeaponsIcons.ContainsKey(pSlot.data.material)) {
				__instance.GetComponent<Image>().sprite = aItems.newWeaponsIcons[pSlot.data.material];
				return false;
			}
			else {
				return true;
			}
			
		}
		/*
		[HarmonyPatch("showHoverTooltip", MethodType.Normal)]
		public static bool Prefix(EquipmentButton __instance)
		{
			if(!Config.tooltipsActive) {
				return false;
			}
			ActorEquipmentSlot slot = Reflection.GetField(__instance.GetType(), __instance, "slot") as ActorEquipmentSlot;
			if(Items.newWeaponsIcons.ContainsKey(slot.data.material)) {
				Debug.Log("log:");
				Tooltip.info_equipment_slot = slot;
				Tooltip.instance.show(__instance.gameObject, "equipment", null, null);
				__instance.transform.localScale = new Vector3(1f, 1f, 1f);
				__instance.transform.CallMethod("DOKill", new object[] { false });
				__instance.transform.CallMethod("DOScale", new object[] { 0.8f, 0.1f }).CallMethod("SetEase", new object[] { Ease.InBack });
				return false;
			}
			else {
				return true;
			}

		}
		*/
	}
	
}
