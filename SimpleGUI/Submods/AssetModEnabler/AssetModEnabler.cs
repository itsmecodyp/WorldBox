using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.Runtime.Internal.Transform;
using BepInEx;
using DG.Tweening.Plugins.Core.PathCore;
using HarmonyLib;
using Newtonsoft.Json;
using SimplerGUI.Menus;
using SimplerGUI.Submods.SimpleMessages;
using TMPro;
using tools.debug;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimplerGUI.Submods.AssetModEnabler
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class AsssetModEnabler_Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.asset_mod.enabler";
        public const string pluginName = "Asset Mod Loader Enabler";
        public const string pluginVersion = "0.0.0.1";

        public void Awake()
        {
            HarmonyPatchSetup();
		}		

		public void HarmonyPatchSetup()
        {
            Harmony harmony = new Harmony(pluginName);
            MethodInfo original;
            MethodInfo patch;

            original = AccessTools.Method(typeof(AssetModLoader), "loadTexture");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "loadTexture_prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            original = AccessTools.Method(typeof(AssetModLoader), "loadFileJson");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "loadFileJson_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            original = AccessTools.Method(typeof(MoodAsset), "getSprite");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "getSprite_prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            original = AccessTools.Method(typeof(AssetModLoader), "checkModFolder");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "checkModFolder_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
        }




        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
				if (Input.GetMouseButtonDown(0))
				{
					var asset = AssetManager.actor_library.get(selectedActorAsset.id);

					if (asset != null && MapBox.instance.getMouseTilePos() != null)
					{
						World.world.units.createNewUnit(asset.id, MapBox.instance.getMouseTilePos(), 0f);
					}
				}
			}
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                showSubMod = true;
            }
            mainWindowRect.height = 0f;
        }
        public static bool showHideTileTypeWindow;
        public static Rect tileTypeWindowRect = new Rect(0f, 1f, 1f, 1f);


        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

		public static bool showHideActorWindow;
		public static Rect actorWindowRect = new Rect(0f, 1f, 1f, 1f);
		public static bool showHideAssetEditWindow;
		public static Rect assetEditWindowRect = new Rect(0f, 1f, 1f, 1f);

		public static bool showHideRaceWindow;
		public static Rect raceWindowRect = new Rect(0f, 1f, 1f, 1f);
		public static bool showHideRaceEditWindow;
		public static Rect raceEditWindowRect = new Rect(0f, 1f, 1f, 1f);

		public static bool showHideBiomeWindow;
		public static Rect biomeWindowRect = new Rect(0f, 1f, 1f, 1f);
		public static bool showHideBiomeEditWindow;
		public static Rect biomeEditWindowRect = new Rect(0f, 1f, 1f, 1f);

		public Vector2 scrollPositionAssetEdit;
		public Vector2 scrollPositionActorSelect;
		public Vector2 scrollPositionActorEdit;
		public Vector2 scrollPositionRaceAssetEdit;
		public Vector2 scrollPositionBiomeAssetEdit;

		public Vector2 scrollPositionRaceAssetSelect;
		public Vector2 scrollPositionBiomeAssetSelect;

		BiomeAsset selectedBiomeAsset;
		ActorAsset selectedActorAsset;
		Race selectedRaceAsset;

		public bool showSubMod = false;

        public void OnGUI()
        {

            if (showSubMod)
            {
                if (GUI.Button(new Rect(Screen.width - 120, 100, 95, 20), "Assets"))
                {
                    showHideMainWindow = !showHideMainWindow;
                    //showHideTileTypeWindow = !showHideTileTypeWindow;
                }
                if (GUI.Button(new Rect(Screen.width - 25, 100, 25, 20), "x"))
                {
                    showHideMainWindow = false;
                    showSubMod = false;
                }
                if (showHideMainWindow)
                {
                    mainWindowRect = GUILayout.Window(410401, mainWindowRect, AssetModEnablerWindow, "Asset Stuff", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }
                if (showHideTileTypeWindow)
                {
                    tileTypeWindowRect = GUILayout.Window(410402, tileTypeWindowRect, TileTypeSetWindow, "TileType Stuff", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }

				//dreaded actor assets
				if (showHideActorWindow)
				{
					actorWindowRect = GUILayout.Window(410403, actorWindowRect, (id) => AssetSelectionWindow(410403,
					ref scrollPositionActorSelect,
					ref selectedActorAsset,
					AssetManager.actor_library.list,
					(selected) => AssetManager.actor_library.add(selected)),
					"Actor Selection",
					GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));

					assetEditWindowRect = GUILayout.Window(410404, assetEditWindowRect, (id) => AssetEditWindow(410404, ref scrollPositionAssetEdit, ref selectedActorAsset), "Actor Edit", GUILayout.MinWidth(200f));
					assetEditWindowRect.position = new Vector2(actorWindowRect.x + actorWindowRect.width, actorWindowRect.y);
				}

				if (showHideRaceWindow)
                {
					raceWindowRect = GUILayout.Window(410405, raceWindowRect, (id) => AssetSelectionWindow(410405,
						ref scrollPositionRaceAssetSelect,
						ref selectedRaceAsset,
						AssetManager.raceLibrary.list,
						(selected) => AssetManager.raceLibrary.add(selected)),
						"Race Selection",
						GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
					raceEditWindowRect = GUILayout.Window(410406, raceEditWindowRect, (id) => AssetEditWindow(410406, ref scrollPositionRaceAssetEdit, ref selectedRaceAsset), "Race Edit", GUILayout.MinWidth(200f));
					raceEditWindowRect.position = new Vector2(raceWindowRect.x + raceWindowRect.width, raceWindowRect.y);
				}

				if (showHideBiomeWindow)
				{
					biomeWindowRect = GUILayout.Window(410407, biomeWindowRect, (id) => AssetSelectionWindow(410407,
						ref scrollPositionBiomeAssetSelect, // scroll position for selection window
						ref selectedBiomeAsset, // selection reference, for assigning to
						AssetManager.biome_library.list, // list used in the action for displaying all assets
						(BiomeAsset selected) => AssetManager.biome_library.add(selected)), //action to add asset to library, refreshing
						"Biome Selection", // title of window
						GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f)); //guilayout options

					biomeEditWindowRect = GUILayout.Window(410408, biomeEditWindowRect, (id) => AssetEditWindow(410408, ref scrollPositionBiomeAssetEdit, ref selectedBiomeAsset), "Biome Edit", GUILayout.MinWidth(200f));
					biomeEditWindowRect.position = new Vector2(biomeWindowRect.x + biomeWindowRect.width, biomeWindowRect.y);
				}

			}
		}

		public string filePathToImport;

		public void AssetSelectionWindow<T>(int windowID, ref Vector2 scrollPosition, ref T selectedAsset, List<T> assetList, Action<T> actionOnAsset)
		{
			GuiMain.SetWindowInUse(windowID);
			if (GUILayout.Button("Clone") && selectedAsset != null)
			{
				actionOnAsset.Invoke(selectedAsset);
			}
			if (GUILayout.Button("Export"))
			{
				if (selectedAsset != null)
				{
					ExportIndividualAsset(selectedAsset);
				}
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Import"))
			{
                string typeToCheck = selectedAsset.GetType().ToString();
				string pathToCheck = Application.streamingAssetsPath + "/mods/Import";
				if (Directory.Exists(pathToCheck) == false)
				{
					Directory.CreateDirectory(pathToCheck);
                    Debug.Log("Created import folder");
                    return;
				}
				if (Directory.Exists(pathToCheck + "/" + typeToCheck) == false)
				{
                    Debug.Log("No assets of type " + typeToCheck + " to load!");
                    return;
				}
				string filePath = pathToCheck + "/" + typeToCheck + "/" + filePathToImport + ".json";
				if (File.Exists(filePath))
                {
					string readData = File.ReadAllText(filePath);
					// Get the type of the selectedAsset
					Type type = selectedAsset.GetType();
					// Deserialize the JSON data into the dynamically determined type
					var deserializedData = JsonConvert.DeserializeObject(readData, type);
					// Cast the deserialized object back to the type of selectedAsset
					object newAsset = Convert.ChangeType(deserializedData, type);
					// Use the deserialized data (now stored in selectedAsset)
					if (newAsset.GetType() == selectedAsset.GetType())
					{
						selectedAsset = (T)newAsset;
						assetList.Add(selectedAsset);
					}
				}
                else
                {
                    Debug.Log("File did not exist: " + filePath);
                }
			}
			filePathToImport = GUILayout.TextField(filePathToImport);
			GUILayout.EndHorizontal();
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxWidth(300f), GUILayout.Height(500f));
			foreach (T asset in assetList)
            {
                var idField = asset.GetType().GetField("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				object idValue = idField.GetValue(asset);
				if (GUILayout.Button(idValue.ToString()))
				{
					selectedAsset = asset;
				}
			}
			GUILayout.EndScrollView();
			GUI.DragWindow();
		}

		public void AssetEditWindow<T>(int windowID, ref Vector2 scrollPosition, ref T selectedAsset)
		{
			GuiMain.SetWindowInUse(windowID);
			if (selectedAsset != null)
			{
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(650f), GUILayout.Height(300f));
				DrawAndEditFieldsOf<T>(selectedAsset);
				GUILayout.EndScrollView();
			}
			GUI.DragWindow();
		}

		public void DrawAndEditFieldsOf<T>(T typeToEdit)
		{
			// Get all fields of the type T
			FieldInfo[] fields = typeof(T).GetFields();

			// Iterate through each field
			foreach (FieldInfo field in fields)
			{
				// Get the value of the field
				object fieldValueObject = field.GetValue(typeToEdit);
				if (fieldValueObject != null)
				{
					// Handle basic types, arrays, and lists
					if (field.FieldType == typeof(int))
					{
						GUILayout.Button(field.Name);
						GUILayout.BeginHorizontal();
						int fieldValue = (int)fieldValueObject;
						GUILayout.Button(fieldValue.ToString());
						fieldValue = (int)GUILayout.HorizontalSlider(fieldValue, 0, 1000);
						GUILayout.EndHorizontal();
						field.SetValue(typeToEdit, fieldValue); // Update the field value
					}
					else if (field.FieldType == typeof(float))
					{
						GUILayout.Button(field.Name);
						GUILayout.BeginHorizontal();
						float fieldValue = (float)fieldValueObject;
						GUILayout.Button(fieldValue.ToString());
						fieldValue = GUILayout.HorizontalSlider(fieldValue, 0, 1000);
						GUILayout.EndHorizontal();
						field.SetValue(typeToEdit, fieldValue); // Update the field value
					}
					else if (field.FieldType == typeof(string))
					{
						GUILayout.Button(field.Name);
						GUILayout.BeginHorizontal();
						string fieldValue = (string)fieldValueObject;
						GUILayout.Button(fieldValue);
						fieldValue = GUILayout.TextField(fieldValue);
						GUILayout.EndHorizontal();
						field.SetValue(typeToEdit, fieldValue); // Update the field value
					}
					else if (field.FieldType == typeof(bool))
					{
						GUILayout.Button(field.Name);
						GUILayout.BeginHorizontal();
						bool fieldValue = (bool)fieldValueObject;
						GUILayout.Button(fieldValue.ToString());
						fieldValue = GUILayout.Toggle(fieldValue, "");
						GUILayout.EndHorizontal();
						field.SetValue(typeToEdit, fieldValue); // Update the field value
					}
					else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
					{
						Type typeOfList = fieldValueObject.GetType().GetGenericArguments()[0];
						IList fieldValue = (IList)fieldValueObject;
						GUILayout.BeginHorizontal();
						GUILayout.Button(field.Name);
						if (GUILayout.Button("+"))
						{
							//in ActorAsset, string lists are the only type that exist, for now
							if (typeOfList == typeof(string))
							{
								fieldValue.Add("");
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();

						int c = 1;
						List<object> elementsToRemove = new List<object>();
						// iterate through each element of the list
						for (int j = 0; j < fieldValue.Count; j++)
						{
							object element = fieldValue[j];

							if (element != null && element.GetType() == typeOfList)
							{
								if (typeOfList == typeof(string))
								{
									string elementValue = (string)element;
									elementValue = GUILayout.TextField(elementValue);
									fieldValue[j] = elementValue;
									if (GUILayout.Button("-", GUILayout.Width(20)))
									{
										elementsToRemove.Add(element);
									}
									if (c % 3 == 0) // split buttons into horizontal rows of 3
									{
										GUILayout.EndHorizontal();
										GUILayout.BeginHorizontal();
									}
									c++;
								}
								// Add other type checks as needed
							}
						}

						// Remove marked elements
						foreach (var elementToRemove in elementsToRemove)
						{
							fieldValue.Remove(elementToRemove);
						}

						GUILayout.EndHorizontal();

						field.SetValue(typeToEdit, fieldValue);
						// Update the field value
					}
					else if (field.FieldType.IsArray)
					{
						Type elementType = field.FieldType.GetElementType();
						Array fieldValue = (Array)fieldValueObject;
						GUILayout.BeginHorizontal();
						GUILayout.Button(field.Name);
						if (GUILayout.Button("+"))
						{
							// Check if the element type is string
							if (elementType == typeof(string))
							{
								// Create a new array with one additional element
								Array newArray = Array.CreateInstance(elementType, fieldValue.Length + 1);
								// Copy existing elements to the new array
								Array.Copy(fieldValue, newArray, fieldValue.Length);
								// Set the last element to an empty string
								newArray.SetValue("", fieldValue.Length);
								// Update fieldValue with the new array
								fieldValue = newArray;
							}
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						int c = 1;
						List<object> elementsToRemove = new List<object>();
						// Iterate through each element of the array
						for (int j = 0; j < fieldValue.Length; j++)
						{
							object element = fieldValue.GetValue(j);

							if (element != null && element.GetType() == elementType)
							{
								if (elementType == typeof(string))
								{
									string elementValue = (string)element;
									elementValue = GUILayout.TextField(elementValue);
									fieldValue.SetValue(elementValue, j);
									if (GUILayout.Button("-", GUILayout.Width(20)))
									{
										elementsToRemove.Add(element);
									}
									if (c % 3 == 0) // Split buttons into vertical rows of 3
									{
										GUILayout.EndHorizontal();
										GUILayout.BeginHorizontal();
									}
									c++;
								}
								// Add other type checks as needed
							}
						}

						// Remove marked elements
						foreach (var elementToRemove in elementsToRemove)
						{
							// Find the index of the element to remove
							int index = Array.IndexOf(fieldValue, elementToRemove);
							// Create a new array with one less element
							Array newArray = Array.CreateInstance(elementType, fieldValue.Length - 1);
							// Copy elements before the removed element
							Array.Copy(fieldValue, 0, newArray, 0, index);
							// Copy elements after the removed element
							Array.Copy(fieldValue, index + 1, newArray, index, fieldValue.Length - index - 1);
							// Update fieldValue with the new array
							fieldValue = newArray;
						}

						GUILayout.EndHorizontal();

						field.SetValue(typeToEdit, fieldValue);
						// Update the field value
					}
					else if (field.FieldType == typeof(Dictionary<,>))
					{
						GUILayout.Button(field.Name);
						Type[] dictionaryTypes = field.FieldType.GetGenericArguments();
						Type keyType = dictionaryTypes[0];
						Type valueType = dictionaryTypes[1];

						// Assuming keys are always strings
						if (keyType == typeof(string))
						{
							if (valueType == typeof(float))
							{
								IDictionary fieldValue = (IDictionary)fieldValueObject;
								GUILayout.Button(field.Name);
								foreach (var key in fieldValue.Keys)
								{
									GUILayout.BeginHorizontal(); // Start of inner horizontal group
									GUILayout.Button(key.ToString());

									float value = (float)fieldValue[key];
									value = GUILayout.HorizontalSlider(value, 0, 1000);
									fieldValue[key] = value;

									GUILayout.EndHorizontal(); // End of inner horizontal group
								}

								// Add new key-value pair
								GUILayout.BeginHorizontal(); // Start of outer horizontal group
								/*
								if (GUILayout.Button("Add"))
								{
									if (!fieldValue.Contains("NewKey"))
									{
										if (valueType == typeof(int))
										{
											fieldValue.Add("NewKey", 0);
										}
										else if (valueType == typeof(float))
										{
											fieldValue.Add("NewKey", 0.0f);
										}
									}
								}
								*/
								GUILayout.EndHorizontal(); // End of outer horizontal group

								field.SetValue(typeToEdit, fieldValue);
							}
							if (valueType == typeof(string))
							{
								IDictionary fieldValue = (IDictionary)fieldValueObject;

								GUILayout.BeginHorizontal(); // Start of outer horizontal group
								GUILayout.Button(field.Name);
								GUILayout.EndHorizontal(); // End of outer horizontal group

								foreach (var key in fieldValue.Keys)
								{
									GUILayout.BeginHorizontal(); // Start of inner horizontal group
									GUILayout.Button(key.ToString());

									string value = (string)fieldValue[key];
									value = GUILayout.TextField(value);
									fieldValue[key] = value;

									GUILayout.EndHorizontal(); // End of inner horizontal group
								}

								// Add new key-value pair
								GUILayout.BeginHorizontal(); // Start of outer horizontal group
								/*
								if (GUILayout.Button("Add"))
								{
									if (!fieldValue.Contains("NewKey"))
									{
										if (valueType == typeof(int))
										{
											fieldValue.Add("NewKey", 0);
										}
										else if (valueType == typeof(float))
										{
											fieldValue.Add("NewKey", 0.0f);
										}
									}
								}
								*/
								GUILayout.EndHorizontal(); // End of outer horizontal group

								field.SetValue(typeToEdit, fieldValue);
							}
						}
					}
					else if (field.FieldType == typeof(BaseStats))
					{
						BaseStats baseStats = (BaseStats)fieldValueObject;
						GUILayout.Button(field.Name);
						foreach (string key in baseStats.stats_dict.Keys)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Button(key);
							float value = (float)baseStats[key];
							GUILayout.Button(value.ToString());
							value = GUILayout.HorizontalSlider(value, 0, 1000);
							baseStats[key] = value;

							GUILayout.EndHorizontal();
						}

						// Add new key-value pair
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Add"))
						{
							// You need to implement a method to add new key-value pairs to BaseStats
							// For example:
							// baseStats.Add("NewKey", 0);
						}
						GUILayout.EndHorizontal();

						field.SetValue(typeToEdit, baseStats);
					}
					else if (field.FieldType == typeof(KingdomStats))
					{
						KingdomStats kingdomStats = (KingdomStats)fieldValueObject;
						GUILayout.Button(field.Name);
						foreach (string key in kingdomStats.dict.Keys)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Button(key);
							//GUILayout.Label(key);

							float value = kingdomStats.dict[key].value;
							GUILayout.Button(value.ToString());
							value = GUILayout.HorizontalSlider(value, 0, 1000);
							kingdomStats.dict[key].value = value;
							GUILayout.EndHorizontal();
						}

						// Add new key-value pair
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Add"))
						{
							// You need to implement a method to add new key-value pairs to BaseStats
							// For example:
							// baseStats.Add("NewKey", 0);
						}
						GUILayout.EndHorizontal();

						field.SetValue(typeToEdit, kingdomStats);
					}
					else if (field.FieldType == typeof(BuildingPlacements))
					{
						BuildingPlacements fieldValue = (BuildingPlacements)fieldValueObject;

						GUILayout.BeginHorizontal();
						GUILayout.Button(field.Name);
						if (GUILayout.Button(fieldValue.ToString()))
						{
							if (fieldValue == BuildingPlacements.Center)
							{
								fieldValue = BuildingPlacements.Random;
							}
							else
							{
								fieldValue = BuildingPlacements.Center;
							}
						}
						GUILayout.EndHorizontal();
						field.SetValue(typeToEdit, fieldValue); // Update the field value
					}
					else
					{
						// Handle other types if needed
						GUILayout.BeginHorizontal();
						GUILayout.Button(field.Name);
						GUILayout.Button(fieldValueObject.ToString());
						GUILayout.EndHorizontal();
					}
				}
			}
		}

		public void ExportIndividualAsset<T>(T assetToExport)
		{
			var tempAsset = assetToExport;
			string typeOf = tempAsset.GetType().ToString();
			if (tempAsset != null)
			{
				string path12 = Application.streamingAssetsPath + "/mods";
				if (Directory.Exists(path12 + "/Export") == false)
				{
					Directory.CreateDirectory(path12 + "/Export");
				}
				if (Directory.Exists(path12 + "/Export/" + typeOf) == false)
				{
					Directory.CreateDirectory(path12 + "/Export/" + typeOf);
				}
				var idField = tempAsset.GetType().GetField("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				if (idField != null)
				{
					object idValue = idField.GetValue(tempAsset);
					// Ensure idValue is not null before using it
					if (idValue != null)
					{
						string dataToSave = JsonUtility.ToJson(tempAsset, true);
						File.WriteAllText(path12 + "/Export/" + typeOf + "/" + idValue.ToString() + ".json", dataToSave);
					}
					else
					{
						Debug.LogError("ID value is null.");
					}
				}
				else
				{
					Debug.LogError("The 'id' property was not found.");
				}
			}
			else
			{
				Debug.Log("tempAsset was null");
			}
		}

		/*
		WorldTile randomTile = MapBox.instance.tilesList.GetRandom();
		if(randomTile.Type.biome_id == "biome_grass")
		{
			MapBox.instance.units.createNewUnit("dragon", randomTile, 0);
		}
		*///blahblah dragon

		Texture2D TextureReadable(Texture2D texture)
		{
			// Create a new texture with the same properties to apply changes
			Texture2D readableTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);

			// Copy the pixels from the original texture to the new one
			Graphics.CopyTexture(texture, readableTexture);

			// Set the readable flag
			readableTexture.Apply(true);

			// Assign the new readable texture back to the original texture
			Graphics.CopyTexture(readableTexture, texture);
            return readableTexture;
		}

		public void AssetModEnablerWindow(int windowID)
        {
            if (GUILayout.Button("Load assets"))
            {
                AssetModLoader.load();
                AssetManager.months.post_init(); // reload months that got added
                List<string> moodList = new List<string>();
                foreach(MoodAsset mood in AssetManager.moods.list)
                {
                    moodList.Add(mood.id);
                }
                GuiMain.ActorInteraction.moods = moodList;
                AssetManager.powers.linkAssets();
                GuiMain.postAssetInitStuff();
            }
            if (GUILayout.Button("Load bonfire2"))
            {
                BuildingAsset b2 = AssetManager.buildings.get("bonfire2");
                if (b2 != null)
                {
                    AssetManager.buildings.loadSprites(b2);
                }
            }
			if (GUILayout.Button("Export ActorAnimations"))
			{
				foreach (ActorAsset aa in AssetManager.actor_library.list)
				{
					string assetID = aa.id;

					//setup folders
					string path12 = Application.streamingAssetsPath + "/mods";
					if (Directory.Exists(path12 + "/Export") == false)
					{
						Directory.CreateDirectory(path12 + "/Export");
					}
					if (Directory.Exists(path12 + "/Export/actors") == false)
					{
						Directory.CreateDirectory(path12 + "/Export/actors");
					}
					if (Directory.Exists(path12 + "/Export/actor_sprites") == false)
					{
						Directory.CreateDirectory(path12 + "/Export/actor_sprites");
					}

					string str = "";
					if (aa.unit)
					{
                        //wtf why is this like a puzzle being put together
						//str = getUnitTexturePath();
					}
					else
					{
						str = aa.texture_path;
						AnimationContainerUnit animationContainerUnit = ActorAnimationLoader.loadAnimationUnit("actors/" + str, aa);

                        foreach(KeyValuePair<string, Sprite> spritePair in animationContainerUnit.sprites)
                        {
							Texture2D itemBGTex = TextureReadable(spritePair.Value.texture);
							byte[] itemBGBytes = itemBGTex.EncodeToPNG();
							File.WriteAllBytes(path12 + "/Export/actor_sprites/" + spritePair.Key + ".png", itemBGBytes);
						}
					}

					string dataToSave = JsonUtility.ToJson(aa, true);
					File.WriteAllText(path12 + "/Export/actors/" + assetID + ".json", dataToSave);
				}
			}
			if (GUILayout.Button("Export bonfire"))
            {
                string assetID = "bonfire";
                //setup folders
                string path12 = Application.streamingAssetsPath + "/mods";
                if (Directory.Exists(path12 + "/BonfireExport") == false)
                {
                    Directory.CreateDirectory(path12 + "/BonfireExport");
                }
                if (Directory.Exists(path12 + "/BonfireExport/buildings") == false)
                {
                    Directory.CreateDirectory(path12 + "/BonfireExport/buildings");
                }
                if (Directory.Exists(path12 + "/BonfireExport/sprites") == false)
                {
                    Directory.CreateDirectory(path12 + "/BonfireExport/sprites");
                }
                Sprite[] spriteList2 = SpriteTextureLoader.getSpriteList("buildings/" + assetID);
                if (spriteList2 != null && spriteList2.Length > 0)
                {
                    foreach (Sprite sprite in spriteList2)
                    {
                        Texture2D itemBGTex = sprite.texture;
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/BonfireExport/sprites/" + assetID + "_" + sprite.name + ".png", itemBGBytes);
                    }
                }

                string dataToSave = JsonUtility.ToJson(AssetManager.buildings.get(assetID), true);
                File.WriteAllText(path12 + "/BonfireExport/buildings/" + assetID + ".json", dataToSave);

            }
            if(GUILayout.Button("Load debug map"))
            {
                DebugMap.makeDebugMap();
            }
            if (GUILayout.Button("Export ALL Buildings"))
            {
                string assetID = "";

                //setup folders
                string path12 = Application.streamingAssetsPath + "/mods";
                foreach (BuildingAsset ba in AssetManager.buildings.list)
                {
                    assetID = ba.id;
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/buildings") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/buildings");
                    }
                    if (Directory.Exists(path12 + "/Export/building_sprites") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/building_sprites");
                    }
                    /*
                    Sprite[] spriteList2 = SpriteTextureLoader.getSpriteList("buildings/" + assetID);
                    if (spriteList2 != null && spriteList2.Length > 0)
                    {
                        foreach (Sprite sprite in spriteList2)
                        {
                            Texture2D itemBGTex = sprite.texture;
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/building_sprites/" + assetID + "_" + sprite.name + ".png", itemBGBytes);
                        }
                    }

                    string dataToSave = JsonUtility.ToJson(ba, true);
                    File.WriteAllText(path12 + "/Export/buildings/" + assetID + ".json", dataToSave);
                    */

                }
                StartCoroutine(ExtractSprites());
            }
            //export traits for reuse
            if (GUILayout.Button("Export ALL Traits"))
            {
                foreach (ActorTrait ta in AssetManager.traits.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/traits") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/traits");
                    }
                    if (Directory.Exists(path12 + "/Export/trait_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/trait_icon");
                    }
                    Sprite[] spriteList2 = SpriteTextureLoader.getSpriteList("buildings/" + assetID);
                    if (spriteList2 != null && spriteList2.Length > 0)
                    {
                        foreach (Sprite sprite in spriteList2)
                        {
                            Texture2D itemBGTex = sprite.texture;
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/trait_icon/" + sprite.name + ".png", itemBGBytes);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ta.path_icon) == false)
                        {
                            Sprite sprite = SpriteTextureLoader.getSprite(ta.path_icon);
                            if (sprite != null)
                            {
                                Texture2D itemBGTex = duplicateTexture(sprite.texture);//new Texture2D(sprite.texture.width, sprite.texture.height);
                                //itemBGTex.LoadRawTextureData(sprite.texture.GetRawTextureData());
                                byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                                File.WriteAllBytes(path12 + "/Export/trait_icon/" + assetID + "_" + sprite.name + ".png", itemBGBytes);
                            }
                        }
                    }

                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/traits/" + assetID + ".json", dataToSave);

                }
            }
            if (GUILayout.Button("Export ALL Resources"))
            {
                foreach (ResourceAsset ta in AssetManager.resources.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/resources") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/resources");
                    }
                    if (Directory.Exists(path12 + "/Export/resources_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/resources_icon");
                    }
                    Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/" + ta.path_icon);
                    if (sprite != null)
                    {

                        Texture2D itemBGTex = duplicateTexture(sprite.texture);//sprite.texture;
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/Export/resources_icon/" + sprite.name + ".png", itemBGBytes);
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/resources/" + assetID + ".json", dataToSave);

                }
            }
            if (GUILayout.Button("Export ALL Eras"))
            {
                foreach (EraAsset ta in AssetManager.era_library.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/eras") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/eras");
                    }
                    if (Directory.Exists(path12 + "/Export/era_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/era_icon");
                    }
                    Sprite sprite = SpriteTextureLoader.getSprite(ta.path_icon);
                    if (sprite != null)
                    {

                        Texture2D itemBGTex = sprite.texture;
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/Export/era_icon/" + sprite.name + ".png", itemBGBytes);
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/eras/" + assetID + ".json", dataToSave);

                }
            }
            if (GUILayout.Button("Export ALL Months"))
            {
                foreach (MonthAsset ta in AssetManager.months.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/months") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/months");
                    }
                  
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/months/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL Moods"))
            {
                foreach (MoodAsset ta in AssetManager.moods.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/moods") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/moods");
                    }
                    if (Directory.Exists(path12 + "/Export/mood_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/mood_icon");
                    }
                    Sprite sprite = (Sprite)Resources.Load("ui/Icons/" + ta.icon, typeof(Sprite));
                    if (sprite != null)
                    {
                        Texture2D itemBGTex = duplicateTexture(sprite.texture);
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/Export/mood_icon/" + sprite.name + ".png", itemBGBytes);
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/moods/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL TileTypes"))
            {
                foreach (TileType ta in AssetManager.tiles.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/tiles") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/tiles");
                    }
                    if (Directory.Exists(path12 + "/Export/tile_icons") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/tile_icons");
                    }
                    Sprite[] array = Resources.LoadAll<Sprite>("tiles/" + ta.id);
                    if (array != null && array.Length > 0)
                    {
                        foreach(Sprite sprite in array)
                        {
                            Texture2D itemBGTex = duplicateTexture(sprite.texture);
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/tile_icons/" + sprite.name + ".png", itemBGBytes);
                        }
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/tiles/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL TopTileTypes"))
            {
                foreach (TopTileType ta in AssetManager.topTiles.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/topTiles") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/topTiles");
                    }
                    if (Directory.Exists(path12 + "/Export/topTile_icons") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/topTile_icons");
                    }
                    Sprite[] array = Resources.LoadAll<Sprite>("topTiles/" + ta.id);
                    if (array != null && array.Length > 0)
                    {
                        foreach (Sprite sprite in array)
                        {
                            Texture2D itemBGTex = duplicateTexture(sprite.texture);
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/topTile_icons/" + sprite.name + ".png", itemBGBytes);
                        }
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/topTiles/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL Biomes"))
            {
                foreach (BiomeAsset ta in AssetManager.biome_library.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/biomes") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/biomes");
                    }
                    //biomes dont have sprites?
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/biomes/" + assetID + ".json", dataToSave);
                }
            }
			if (GUILayout.Button("Export ALL ActorAsset"))
			{
				foreach (ActorAsset aa in AssetManager.actor_library.list)
				{
					string assetID = aa.id;

					//setup folders
					string path12 = Application.streamingAssetsPath + "/mods";
					if (Directory.Exists(path12 + "/Export") == false)
					{
						Directory.CreateDirectory(path12 + "/Export");
					}
					if (Directory.Exists(path12 + "/Export/actors") == false)
					{
						Directory.CreateDirectory(path12 + "/Export/actors");
					}				
					string dataToSave = JsonUtility.ToJson(aa, true);
					File.WriteAllText(path12 + "/Export/actors/" + assetID + ".json", dataToSave);
				}
			}
			if (GUILayout.Button("Export ALL Race"))
			{
				foreach (Race ra in AssetManager.raceLibrary.list)
				{
					string assetID = ra.id;

					//setup folders
					string path12 = Application.streamingAssetsPath + "/mods";
					if (Directory.Exists(path12 + "/Export") == false)
					{
						Directory.CreateDirectory(path12 + "/Export");
					}
					if (Directory.Exists(path12 + "/Export/races") == false)
					{
						Directory.CreateDirectory(path12 + "/Export/races");
					}
					string dataToSave = JsonUtility.ToJson(ra, true);
					File.WriteAllText(path12 + "/Export/races/" + assetID + ".json", dataToSave);
				}
			}

			if (GUILayout.Button("actor window"))
            {
                showHideActorWindow = !showHideActorWindow;
            }
			if (GUILayout.Button("race window"))
			{
				showHideRaceWindow = !showHideRaceWindow;
			}
			if (GUILayout.Button("biome window"))
			{
				showHideBiomeWindow = !showHideBiomeWindow;
			}
			GUI.DragWindow();
        }
        public void TileTypeSetWindow(int windowID)
        {
            if(selectedTileTypeMap != null)
            {
                if(selectedTileTypeMap.mainsheetFull != null)
                {
                    //?
                    GUILayout.Box(selectedTileTypeMap.mainsheetFull);
                }
                /*
                for (int y = 0; y < selectedTileTypeMap.height; y++)
                {
                    for (int x = 0; x < selectedTileTypeMap.width; x++)
                    {
                        
                        //GUI.Button(new Rect(x, y, 5, 5), x.ToString() + "," + y.ToString());

                        //GUILayout.Button(x.ToString() + "," + y.ToString());
                    }
                }
                */
            }
            GUILayout.Button("t");
            //make background the full sheet/image
            GUI.DragWindow();
        }


        IEnumerator ExtractSprites()
        {

            string assetID = "";

            //setup folders
            string path12 = Application.streamingAssetsPath + "/mods";
            yield return new WaitForEndOfFrame(); // Wait for end of frame to ensure all rendering is done

            foreach (Building building in MapBox.instance.buildings)
            {
                SpriteRenderer renderer = building.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Sprite sprite = renderer.sprite;
                    if (sprite != null)
                    {
                        Texture2D itemBGTex = sprite.texture;
                        if (itemBGTex != null)
                        {
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            try
                            {
                                string filePath = System.IO.Path.Combine(path12, "Export", "building_sprites", $"{building.name + "_" + assetID}_{sprite.name}.png");
                                File.WriteAllBytes(filePath, itemBGBytes);
                                Debug.Log($"Sprite exported successfully: {filePath}");
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error exporting sprite: {e.Message}");
                            }
                        }
                        else
                        {
                            Debug.Log("Texture is null for sprite: " + sprite.name);
                        }
                    }
                    else
                    {
                        Debug.Log("Sprite is null for building: " + building.name);
                    }
                }
                else
                {
                    Debug.Log("SpriteRenderer is missing for building: " + building.name);
                }

                // Saving JSON data here as before
                string dataToSave = JsonUtility.ToJson(assetID, true);
                File.WriteAllText(path12 + "/Export/buildings/" + building.name + "_" + assetID + ".json", dataToSave);
            }
            /*
            Debug.Log("Amount of buildings on map: " + MapBox.instance.buildings.Count);
            foreach (Building building in MapBox.instance.buildings)
            {
                SpriteRenderer renderer = building.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Sprite sprite = renderer.sprite;
                    if (sprite != null)
                    {
                        Texture2D itemBGTex = sprite.texture;
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/Export/building_sprites/" + assetID + "_" + sprite.name + ".png", itemBGBytes);
                    }
                    else
                    {
                        Debug.Log("Sprite is null for building: " + building.name);
                    }
                }
                else
                {
                    Debug.Log("SpriteRenderer is missing for building: " + building.name);
                }

                string dataToSave = JsonUtility.ToJson(assetID, true);
                File.WriteAllText(path12 + "/Export/buildings/" + assetID + ".json", dataToSave);
            }
            */
        }

        public TileTypeMap selectedTileTypeMap;

        public class TileTypeMap
        {
            //name of tilemap/spritesheet/theme/etc
            public string id;
            public Texture2D mainsheetFull;
            //dimension of sprites on sheet
            public int width = 8;
            public int height = 8;
            //map of sub-sprites/textures, accessable like spriteMap[0,1] for first from bottom left
            public Dictionary<Vector2, Sprite> spriteMap;
            public Dictionary<Vector2, Texture2D> texture2dMap;

        }
        //split image into many 8x8 images

        public static bool checkModFolder_Prefix(string pPath, string pType)
        {
            AssetModLoader.log("CMF pType: " + pType);
            List<string> files = AssetModLoader.getFiles(pPath);
            string[] array = pPath.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
            AssetModLoader.log("");
            AssetModLoader.log("# CHECKING PATH... " + array[array.Length - 1]);
            AssetModLoader.log("FILES: " + files.Count.ToString());
            AssetModLoader.log("");
            List<Sprite> multiSpritesForAsset = null;
            string assetName = null;
            if (pType == "buildings" || pType == "tiles" || pType == "topTiles")
            {
                multiSpritesForAsset = new List<Sprite>();
            }
            foreach (string text in files)
            {
                AssetModLoader.log(text);
                if (text.Contains("json"))
                {
                    if(assetName == null)
                    {
                        string[] array2 = pPath.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
                        string text2 = array[array.Length - 1];
                        AssetModLoader.log("# FOUND NAME: " + text2);
                        assetName = text2.Replace(".json", "");
                    }
                    AssetModLoader.loadFileJson(text, pType);
                }
                if (text.Contains("png"))
                {
                    //check if image is inside folder for multi-sprite assets
                    if(pType == "buildings" && multiSpritesForAsset != null)
                    {
                        loadTextureBuilding(text, multiSpritesForAsset);
                    }
                    else if (pType == "tiles" || pType == "topTiles" && multiSpritesForAsset != null)
                    {
                        loadTextureTiles(text, multiSpritesForAsset);
                    }
                    else
                    {
                        AssetModLoader.loadTexture(text);
                    }
                }
            }
            if(pType == "buildings" && multiSpritesForAsset != null && multiSpritesForAsset.Count > 0)
            {
                Sprite[] spriteArray = multiSpritesForAsset.ToArray();
                Debug.Log("building sprite count: " + spriteArray.Length);
                SpriteTextureLoader.cached_sprite_list.Add("buildings/" + assetName, spriteArray);
                Debug.Log("added sprites to cached_sprite_list");
                BuildingAsset buildingAsset = AssetManager.buildings.get(assetName);
                if(buildingAsset != null)
                {
                    Debug.Log("asset found, loading sprites");
                    //buildingAsset.spritePath should probably equal the building name/id
                    AssetManager.buildings.loadSprites(buildingAsset);
                }
            }
            if (pType == "tiles")
            {
                TileType asset = AssetManager.tiles.get(assetName);
                if (asset != null)
                {
                    loadSpritesForTile(asset, multiSpritesForAsset);
                }

                GodPower powerForTile = AssetManager.powers.clone(assetName, "tileDeepOcean");
                powerForTile.tileType = assetName;
                powerForTile.name = assetName;
                AssetManager.powers.add(powerForTile);
                //icon needs changed and reloaded, GodPower.getIconSprite

            }
            if (pType == "topTiles")
            {
                TopTileType asset = AssetManager.topTiles.get(assetName);
                if (asset != null)
                {
                    loadSpritesForTile(asset, multiSpritesForAsset);
                }

                GodPower powerForTile = AssetManager.powers.clone(assetName, "tileDeepOcean");
                powerForTile.topTileType = assetName;
                powerForTile.name = assetName;
                AssetManager.powers.add(powerForTile);
            }
            return false;
        }

        private static void loadTextureTiles(string pPath, List<Sprite> multiSpritesForAsset)
        {
            string[] array = pPath.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD TEXTURE TILE: " + text);
            byte[] array2 = File.ReadAllBytes(pPath);
            //string text2 = "@wb_" + text;
            text = text.Replace(".png", "");
            AssetModLoader.log("ADDING TEXTURE TILE... " + text);
            //SpriteTextureLoader.addSprite(text, array2);

            //addSprite edit
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.filterMode = FilterMode.Point;
            if (texture2D.LoadImage(array2))
            {
                Rect rect = new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height);
                Vector2 vector = new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(texture2D, rect, vector, 1f);
                sprite.name = text;
                //SpriteTextureLoader.cached_sprites.Add(pPathID, sprite);
                multiSpritesForAsset.Add(sprite);
            }
        }

        //copy paste with smol edit. lil ugly, just like you
        private static void loadTextureBuilding(string pPath, List<Sprite> listToAddTo)
        {
            string[] array = pPath.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD TEXTURE BUILDING: " + text);
            byte[] array2 = File.ReadAllBytes(pPath);
            //string text2 = "@wb_" + text;
            text = text.Replace(".png", "");
            AssetModLoader.log("ADDING TEXTURE BUILDING... " + text);
            //SpriteTextureLoader.addSprite(text, array2);

            //addSprite edit
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.filterMode = FilterMode.Point;
            if (texture2D.LoadImage(array2))
            {
                Rect rect = new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height);
                Vector2 vector = new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(texture2D, rect, vector, 1f);
                sprite.name = text;
                //SpriteTextureLoader.cached_sprites.Add(pPathID, sprite);
                listToAddTo.Add(sprite);
            }
        }

        //copy paste and edit
        public static void loadSpritesForTile(TileType pType, List<Sprite> listToAddTo)
        {
            Sprite[] array = listToAddTo.ToArray();//Resources.LoadAll<Sprite>("tiles/" + pType.id);
            if (array == null || array.Length == 0)
            {
                return;
            }
            pType.sprites = new TileSprites();
            foreach (Sprite sprite in array)
            {
                pType.sprites.addVariation(sprite);
            }
        }

        public static void loadSpritesForTile(TopTileType pType, List<Sprite> listToAddTo)
        {
            Sprite[] array = listToAddTo.ToArray();//Resources.LoadAll<Sprite>("tiles/" + pType.id);
            if (array == null || array.Length == 0)
            {
                return;
            }
            pType.sprites = new TileSprites();
            foreach (Sprite sprite in array)
            {
                pType.sprites.addVariation(sprite);
            }
        }

        //remove @wb_ prefix from textures..
        //replace @ in filename with /, allows nested path identical to what game might expect ex: civ/icons/minimap_boat_small
        public static bool loadTexture_prefix(string pPath)
        {
            string[] array = pPath.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD TEXTURE: " + text);
            byte[] array2 = File.ReadAllBytes(pPath);
            text = text.Replace('@', '/');
            text = text.Replace(".png", "");
            AssetModLoader.log("(p)ADDING TEXTURE... " + text);
            SpriteTextureLoader.addSprite(text, array2);
            return false;
        }

        public static bool getSprite_prefix(ref Sprite __result, MoodAsset __instance)
        {
            if (__instance.sprite == null)
            {
                __instance.sprite = (Sprite)Resources.Load("ui/Icons/" + __instance.icon, typeof(Sprite));
            }
            //above is original, below is in case above isnt found, default to normal AML behaviour
            if (__instance.sprite == null)
            {
                __instance.sprite = SpriteTextureLoader.getSprite(__instance.icon);
            }
            __result = __instance.sprite;
            return false;
        }

        //load new asset types
        //prefix to just go ahead and replace with a clean switch statement
        public static void loadFileJson_Prefix(string pPath, string pType)
        {
            string[] array = pPath.Split(new char[] { System.IO.Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD ASSET: " + text);
            string fileText = File.ReadAllText(pPath);
            if (pType != null)
            {
                switch (pType)
                {
                    case "tiles":
                        LoadAssetTileType(fileText);
                        break;
                    case "topTiles":
                        LoadAssetTopTileType(fileText);
                        break;
                    case "months":
                        //months need MANY patches to make work: formatDate, getDate, getCurrentMonth, getCurrentYear, etc
                        //hard coded for 12 months and "inflected" names which will null if not found
                        //not worth it for now, revisit later
                        LoadAssetMonth(fileText);
                        break;
                    case "moods":
                        LoadAssetMood(fileText);
                        break;
                    case "eras":
                        LoadAssetEra(fileText);
                        break;
                    case "resources":
                        LoadAssetResource(fileText);
                        break;
                    case "traits":
                        LoadAssetTrait(fileText);
                        break;
                    case "powers":
                        AssetModLoader.loadAssetPowers(fileText);
                        break;
                    case "buildings":
                        //building textures are not supported yet
                        AssetModLoader.loadAssetBuilding(fileText);
                        break;
                    default:
                        break;
                }
            }
        }

        public static void LoadAssetTileType(string tileData)
        {
            TileType tileAsset = JsonUtility.FromJson<TileType>(tileData);
            AssetManager.tiles.add(tileAsset);
        }

        public static void LoadAssetTopTileType(string tileData)
        {
            TopTileType tileAsset = JsonUtility.FromJson<TopTileType>(tileData);
            AssetManager.topTiles.add(tileAsset);
        }

        public static void LoadAssetTrait(string traitData)
        {
            ActorTrait traitAsset = JsonUtility.FromJson<ActorTrait>(traitData);
            AssetManager.traits.add(traitAsset);
        }

        public static void LoadAssetEra(string eraData)
        {
            EraAsset eraAsset = JsonUtility.FromJson<EraAsset>(eraData);
            AssetManager.era_library.add(eraAsset);
        }

        public static void LoadAssetResource(string resourceData)
        {
            ResourceAsset resourceAsset = JsonUtility.FromJson<ResourceAsset>(resourceData);
            AssetManager.resources.add(resourceAsset);
        }
        public static void LoadAssetMonth(string monthData)
        {
            MonthAsset monthAsset = JsonUtility.FromJson<MonthAsset>(monthData);
            AssetManager.months.add(monthAsset);
        }
        public static void LoadAssetMood(string moodData)
        {
            MoodAsset moodAsset = JsonUtility.FromJson<MoodAsset>(moodData);
            AssetManager.moods.add(moodAsset);
        }
        Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

    }

}