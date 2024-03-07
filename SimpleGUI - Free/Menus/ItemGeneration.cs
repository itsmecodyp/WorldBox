using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGUI.Menus
{
	class GuiItemGeneration
	{
		public void itemGenerationWindowUpdate()
		{
			if (SimpleSettings.showHideItemGenerationConfig)
			{
				itemGenerationWindowRect = GUILayout.Window(1005, itemGenerationWindowRect, ItemGenerationWindow, "Items", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
			}
			if(selectedTypeString != "" && itemSelection) {
				itemGenerationEquipmentWindow1 = GUILayout.Window(95466, itemGenerationEquipmentWindow1, ItemEquipSelectionWindow, "Items1", GUILayout.MinWidth(600f), GUILayout.ExpandWidth(false));
				itemGenerationEquipmentWindow1.position = new Vector2(itemGenerationWindowRect.x + itemGenerationWindowRect.width, (itemGenerationWindowRect.y));
			}
		}

		public static void ItemGenerationWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
			if(itemSelection) {
				GUI.backgroundColor = Color.green;
			}
			else {
				GUI.backgroundColor = Color.red;
			}
			if(GUILayout.Button("Item selection")) {
				itemSelection = !itemSelection;
			}
			GUI.backgroundColor = ori;

			if(itemSelection) {
				string[] enumNames = Enum.GetNames(typeof(EquipmentType));
				foreach(string equipmentTypeName in enumNames) {
					if(selectedTypeString == equipmentTypeName) {
						GUI.backgroundColor = Color.green;
					}
					else {
						GUI.backgroundColor = Color.red;
					}
					if(GUILayout.Button(equipmentTypeName)) {
						// open sub menu for selecting specific item in this type
						selectedTypeString = equipmentTypeName;
					}
					GUI.backgroundColor = ori;
				}
			}

			if((lastSelectedActor == null && Config.selectedUnit != null) || (lastSelectedActor != Config.selectedUnit && Config.selectedUnit != null)) {
				lastSelectedActor = Config.selectedUnit;
			}
			if(lastSelectedActor == null) {
				GUILayout.Button("Inspect a unit to continue");
			}
			else {
				if(GUILayout.Button("Give items to inspected actor")) {
					foreach(string itemID in selectedItems) {
						ItemAsset asset = AssetManager.items.get(itemID);
						string materialForItem = selectedItemMaterials[itemID];
						//check if kingdom isnt null, then add more data about item using more param
						ItemData pData = ItemGenerator.generateItem(asset, materialForItem, World.world.mapStats.year);
						if(selectedItemModifiers.TryGetValue(itemID, out var modifier)) {
							pData.modifiers.Clear();
							foreach(string mod in modifier) {
								pData.modifiers.Add(mod);
							}
						}
						if(lastSelectedActor.equipment == null) {
							Debug.LogError("Actor equipment is null, creating so items can be given");
							lastSelectedActor.equipment = new ActorEquipment();

						}
						lastSelectedActor.equipment.getSlot(asset.equipmentType).setItem(pData);
						//add modifiers
					}
					lastSelectedActor.setStatsDirty();
				}
				GUI.backgroundColor = ori;

				GUILayout.BeginHorizontal();
				GUILayout.EndHorizontal();
			}
			GUI.DragWindow();
		}

		public static Vector2 scrollPosition;

		public static List<string> notRealItems = new List<string> { "base", "hands", "jaws", "claws", "snowball", "fire_hands", "bite", "rocks" };

		public static void ItemEquipSelectionWindow(int windowID)
		{
			ori = GUI.backgroundColor;
			GuiMain.SetWindowInUse(windowID);
			scrollPosition = GUILayout.BeginScrollView(
		  scrollPosition, GUILayout.Height(itemGenerationWindowRect.height - 31.5f));
			foreach(ItemAsset item in AssetManager.items.list) {
				// i could use actual equipmentType type, idk if the speed matters
				// if it does, could also just cache once instead of looping every frame
				if(notRealItems.Contains(item.id) == false && item.id[0] != '_' && item.equipmentType.ToString() == selectedTypeString) {
					GUILayout.BeginHorizontal();
					if(selectedItems.Contains(item.id)) {
						GUI.backgroundColor = Color.green;
					}
					else {
						GUI.backgroundColor = Color.red;
					}
					GUILayout.Label(item.id);
					GUI.backgroundColor = ori;
					foreach(string materialForEquipment in item.materials) {
						string equipString = item.equipmentType.ToString();
						if(item.materials.Contains(materialForEquipment)) {
							if(selectedItemMaterials.ContainsKey(item.id) && selectedItemMaterials[item.id] == materialForEquipment) {
								GUI.backgroundColor = Color.green;
							}
							else {
								GUI.backgroundColor = Color.red;
							}
							if(GUILayout.Button(materialForEquipment)) {
								//does item list already have item?
								if(selectedItems.Contains(item.id)) {
									if(selectedItemMaterials[item.id] == materialForEquipment) {
										//click matches exact item, remove
										selectedItems.Remove(item.id);
										selectedTypes.Remove(equipString);
										selectedItemMaterials.Remove(item.id);
									}
									else {
										//material is different, item is same, update item
										selectedItemMaterials[item.id] = materialForEquipment;
									}
								}
								//items list did not contain item
								else {
									//do we have another item of same type already?
									if(selectedTypes.ContainsKey(equipString)) {
										//if yes, remove that item
										selectedItems.Remove(selectedTypes[equipString]);
										selectedItemMaterials.Remove(item.id);
										//then add the new id
										selectedItems.Add(item.id);
										selectedItemMaterials.Add(item.id, materialForEquipment);
										//and update the other list
										selectedTypes[equipString] = item.id;
									}
									else {
										//no item of same type, add item fresh
										selectedItems.Add(item.id);
										selectedTypes.Add(equipString, item.id);
										selectedItemMaterials.Add(item.id, materialForEquipment);
									}
								}
							}
							GUI.backgroundColor = ori;
						}
						GUI.backgroundColor = ori;

					}
					GUILayout.EndHorizontal();
				}
			}
			if(lastSelectedItemID != "") {
				foreach(ItemAsset modifier in AssetManager.items_modifiers.dict.Values) {
					GUI.backgroundColor = Color.red;
					if(selectedItemModifiers.TryGetValue(lastSelectedItemID, out var itemModifier)) {
						if(itemModifier.Contains(modifier.id)) {
							GUI.backgroundColor = Color.green;
						}
					}
					if(GUILayout.Button("Modifier: " + modifier.id)) {
						if(selectedItemModifiers.TryGetValue(lastSelectedItemID, out var existingList)) {
							if(existingList.Contains(modifier.id)) {
								existingList.Remove(modifier.id);
							}
							else {
								existingList.Add(modifier.id);
							}
						}
						else {
							selectedItemModifiers.Add(lastSelectedItemID, new List<string> { modifier.id });
						}
					}
					GUI.backgroundColor = ori;
				}
			}
			GUILayout.EndScrollView();
			GUI.DragWindow();
		}

		public static Color ori;

		public static List<string> selectedItems = new List<string>();
		public static Dictionary<string, string> selectedTypes = new Dictionary<string, string>();

		//sorta bad, but whatever works for now
		public static Dictionary<string, string> selectedItemMaterials = new Dictionary<string, string>();
		//extra bad, but whatever works for now
		public static Dictionary<string, List<string>> selectedItemModifiers = new Dictionary<string, List<string>>();


		public static string selectedTypeString = "";
		public static string lastSelectedItemID = "";

		public static Actor lastSelectedActor;
		//public static bool showHideItemGeneration;
		public static bool itemSelection;
		//public static bool showEquipWindow1;
		//public static bool showEquipWindow2;
		public static Rect itemGenerationWindowRect;

		public Rect itemGenerationEquipmentWindow1;
		public Rect itemGenerationEquipmentWindow2;
	}

}
