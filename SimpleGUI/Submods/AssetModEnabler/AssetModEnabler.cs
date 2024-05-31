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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SimplerGUI.Menus;
using SimplerGUI.Submods.SimpleMessages;
using TMPro;
using tools.debug;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
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
                        Debug.Log("Creating unit with id: " + asset.id);
						Actor newActor = World.world.units.createNewUnit(asset.id, MapBox.instance.getMouseTilePos(), 0f);
                        if(newActor != null && newActor.isAlive())
                        {
                            Debug.Log("Created actor!");
                        }
					}
                    if(asset == null)
                    {
                        Debug.Log("cant try to create unit, asset was null! id: " + selectedActorAsset.id);
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

        EraAsset selectedEraAsset;
        public Vector2 scrollPositionEraSelect;
        public Vector2 scrollPositionEraEdit;
        public static bool showHideEraWindow;
        public static Rect eraWindowRect = new Rect(0f, 1f, 1f, 1f);
        public static Rect eraEditWindowRect = new Rect(0f, 1f, 1f, 1f);

        NameGeneratorAsset selectedNameGeneratorAsset;
        public Vector2 scrollPositionNameGeneratorSelect;
        public Vector2 scrollPositionNameGeneratorEdit;
        public static bool showHideNameGeneratorWindow;
        public static Rect NameGeneratorWindowRect = new Rect(0f, 1f, 1f, 1f);
        public static Rect NameGeneratorEditWindowRect = new Rect(0f, 1f, 1f, 1f);

        ResourceAsset selectedResourceAsset;
        public Vector2 scrollPositionResourceAssetSelect;
        public Vector2 scrollPositionResourceAssetEdit;
        public static bool showHideResourceAssetWindow;
        public static Rect ResourceAssetWindowRect = new Rect(0f, 1f, 1f, 1f);
        public static Rect ResourceAssetEditWindowRect = new Rect(0f, 1f, 1f, 1f);

        ActorTrait selectedActorTraitAsset;
        public Vector2 scrollPositionActorTraitAssetSelect;
        public Vector2 scrollPositionActorTraitAssetEdit;
        public static bool showHideActorTraitAssetWindow;
        public static Rect ActorTraitAssetWindowRect = new Rect(0f, 1f, 1f, 1f);
        public static Rect ActorTraitAssetEditWindowRect = new Rect(0f, 1f, 1f, 1f);

		CloudAsset selectedCloudAsset;
		public Vector2 scrollPositionCloudAssetSelect;
		public Vector2 scrollPositionCloudAssetEdit;
		public static bool showHideCloudAssetWindow;
		public static Rect CloudAssetWindowRect = new Rect(0f, 1f, 1f, 1f);
		public static Rect CloudAssetEditWindowRect = new Rect(0f, 1f, 1f, 1f);


		public bool showSubMod = true;

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
                    AssetManager.actor_library.dict,
					(selected) => AssetManager.actor_library.clone(selected.id + "_clone", selected.id)),
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
                        AssetManager.raceLibrary.dict,
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
                        AssetManager.biome_library.dict,
                        (BiomeAsset selected) => AssetManager.biome_library.add(selected)), //action to add asset to library, refreshing
						"Biome Selection", // title of window
						GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f)); //guilayout options

					biomeEditWindowRect = GUILayout.Window(410408, biomeEditWindowRect, (id) => AssetEditWindow(410408, ref scrollPositionBiomeAssetEdit, ref selectedBiomeAsset), "Biome Edit", GUILayout.MinWidth(200f));
					biomeEditWindowRect.position = new Vector2(biomeWindowRect.x + biomeWindowRect.width, biomeWindowRect.y);
				}

                if (showHideEraWindow)
                {
                    eraWindowRect = GUILayout.Window(410409, eraWindowRect, (id) => AssetSelectionWindow(410409,
                        ref scrollPositionEraSelect, // scroll position for selection window
                        ref selectedEraAsset, // selection reference, for assigning to
                        AssetManager.era_library.list, // list used in the action for displaying all assets
                        AssetManager.era_library.dict,
                        (EraAsset selected) => AssetManager.era_library.clone(selected.id + "_clone", selected.id)), //action to add asset to library, refreshing
                        "Era Selection", // title of window
                        GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f)); //guilayout options

                    eraEditWindowRect = GUILayout.Window(410410, eraEditWindowRect, (id) => AssetEditWindow(410410, ref scrollPositionEraEdit, ref selectedEraAsset), "Era Edit", GUILayout.MinWidth(200f));
                    eraEditWindowRect.position = new Vector2(eraWindowRect.x + eraWindowRect.width, eraWindowRect.y);
                }

                if (showHideNameGeneratorWindow)
                {
                    NameGeneratorWindowRect = GUILayout.Window(410411, NameGeneratorWindowRect, (id) => AssetSelectionWindow(410411,
                        ref scrollPositionNameGeneratorSelect, // scroll position for selection window
                        ref selectedNameGeneratorAsset, // selection reference, for assigning to
                        AssetManager.nameGenerator.list, // list used in the action for displaying all assets
                        AssetManager.nameGenerator.dict,
                        (NameGeneratorAsset selected) => AssetManager.nameGenerator.clone(selected.id + "_clone", selected.id)), //action to add asset to library, refreshing
                        "NameGenerator Selection", // title of window
                        GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f)); //guilayout options

                    NameGeneratorEditWindowRect = GUILayout.Window(410412, NameGeneratorEditWindowRect, (id) => AssetEditWindow(410412, ref scrollPositionNameGeneratorEdit, ref selectedNameGeneratorAsset), "NameGenerator Edit", GUILayout.MinWidth(200f));
                    NameGeneratorEditWindowRect.position = new Vector2(NameGeneratorWindowRect.x + NameGeneratorWindowRect.width, NameGeneratorWindowRect.y);
                }

                if (showHideResourceAssetWindow)
                {
                    ResourceAssetWindowRect = GUILayout.Window(410423, ResourceAssetWindowRect, (id) => AssetSelectionWindow(410423,
                        ref scrollPositionResourceAssetSelect, // scroll position for selection window
                        ref selectedResourceAsset, // selection reference, for assigning to
                        AssetManager.resources.list, // list used in the action for displaying all assets
                        AssetManager.resources.dict,
                        (ResourceAsset selected) => AssetManager.resources.clone(selected.id + "_clone", selected.id)), //action to add asset to library, refreshing
                        "ResourceAsset Selection", // title of window
                        GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f)); //guilayout options

                    ResourceAssetEditWindowRect = GUILayout.Window(410424, ResourceAssetEditWindowRect, (id) => AssetEditWindow(410424, ref scrollPositionResourceAssetEdit, ref selectedResourceAsset), "ResourceAsset Edit", GUILayout.MinWidth(200f));
                    ResourceAssetEditWindowRect.position = new Vector2(ResourceAssetWindowRect.x + ResourceAssetWindowRect.width, ResourceAssetWindowRect.y);
                }

                if (showHideActorTraitAssetWindow)
                {
                    ActorTraitAssetWindowRect = GUILayout.Window(410425, ActorTraitAssetWindowRect, (id) => AssetSelectionWindow(410425,
                        ref scrollPositionActorTraitAssetSelect, // scroll position for selection window
                        ref selectedActorTraitAsset, // selection reference, for assigning to
                        AssetManager.traits.list, // list used in the action for displaying all assets
                        AssetManager.traits.dict,
                        (ActorTrait selected) => AssetManager.traits.clone(selected.id + "_clone", selected.id)), //action to add asset to library, refreshing
                        "ActorTrait Selection", // title of window
                        GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f)); //guilayout options

                    ActorTraitAssetEditWindowRect = GUILayout.Window(410426, ActorTraitAssetEditWindowRect, (id) => AssetEditWindow(410426, ref scrollPositionActorTraitAssetEdit, ref selectedActorTraitAsset), "ActorTrait Edit", GUILayout.MinWidth(200f));
                    ActorTraitAssetEditWindowRect.position = new Vector2(ActorTraitAssetWindowRect.x + ActorTraitAssetWindowRect.width, ActorTraitAssetWindowRect.y);
                }

				if (showHideCloudAssetWindow)
				{
					CloudAssetWindowRect = GUILayout.Window(410426, CloudAssetWindowRect, (id) => AssetSelectionWindow(410426,
						ref scrollPositionCloudAssetSelect, // scroll position for selection window
						ref selectedCloudAsset, // selection reference, for assigning to
						AssetManager.clouds.list, // list used in the action for displaying all assets
						AssetManager.clouds.dict,
						(CloudAsset selected) => AssetManager.clouds.clone(selected.id + "_clone", selected.id)), //action to add asset to library, refreshing
						"CloudAsset Selection", // title of window
						GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f)); //guilayout options

					CloudAssetEditWindowRect = GUILayout.Window(410427, CloudAssetEditWindowRect, (id) => AssetEditWindow(410427, ref scrollPositionCloudAssetEdit, ref selectedCloudAsset), "CloudAsset Edit", GUILayout.MinWidth(200f));
					CloudAssetEditWindowRect.position = new Vector2(CloudAssetWindowRect.x + CloudAssetWindowRect.width, CloudAssetWindowRect.y);
				}
			}
        }

		public string filePathToImport;

		public void AssetSelectionWindow<T>(int windowID, ref Vector2 scrollPosition, ref T selectedAsset, List<T> assetList, Dictionary<string, T> assetDict, Action<T> actionOnAsset)
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
                ImportIndividualAsset(ref selectedAsset, assetList, assetDict, selectedAsset.GetType().ToString(), filePathToImport);
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
            if (tempAsset != null)
            {
                string path = Application.streamingAssetsPath + "/mods/Export";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var idField = tempAsset.GetType().GetField("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (idField != null)
                {
                    object idValue = idField.GetValue(tempAsset);
                    if (idValue != null)
                    {
                        string typeOf = tempAsset.GetType().Name; // Get type name
                        string dataToSave = JsonConvert.SerializeObject(tempAsset, Formatting.Indented, new JsonSerializerSettings
                        {
                            ContractResolver = new IncludeNonSerializedContractResolver(),
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore // Add this line
                        });

                        File.WriteAllText(System.IO.Path.Combine(path, idValue.ToString() + "." + typeOf + ".json"), dataToSave);
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
        public void ImportIndividualAsset<T>(ref T selectedAsset, List<T> assetList, Dictionary<string, T> assetDict, string folderName, string fileName, bool isForFolderImport = false)
        {
            string typeToCheck = selectedAsset.GetType().ToString();
            string pathToCheck = System.IO.Path.Combine(Application.streamingAssetsPath, "mods", "Import");

            if (!Directory.Exists(pathToCheck))
            {
                Directory.CreateDirectory(pathToCheck);
                Debug.Log("Created import folder");
                return;
            }

            string filePath;
            if (isForFolderImport)
            {
                filePath = System.IO.Path.Combine(pathToCheck, folderName, fileName + ".json");
            }
            else
            {
                filePath = System.IO.Path.Combine(pathToCheck, typeToCheck, System.IO.Path.GetFileNameWithoutExtension(fileName) + ".json");
            }

            if (File.Exists(filePath))
            {
                string readData = File.ReadAllText(filePath);
                // Get the type of the selectedAsset
                Type type = selectedAsset.GetType();
                // Deserialize the JSON data into the dynamically determined type
                var deserializedData = JsonConvert.DeserializeObject<JObject>(readData);
                // Exclude color properties from the deserialized JSON object
                ExcludeColorProperties(deserializedData);
                // Convert the modified JSON object back to the original asset type
                selectedAsset = deserializedData.ToObject<T>();
                // Use the deserialized data (now stored in selectedAsset)
                if (selectedAsset.GetType() == type)
                {
                    var idField1 = selectedAsset.GetType().GetField("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    object idValue1 = idField1.GetValue(selectedAsset);
                    for (int i = 0; i < assetList.Count; i++)
                    {
                        T asset = assetList[i];
                        var idField2 = asset.GetType().GetField("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        object idValue2 = idField2.GetValue(asset);

                        if ((string)idValue1 == (string)idValue2)
                        {
                            assetList.Remove(asset);
                            assetDict[(string)idValue1] = selectedAsset;
                            Debug.Log("attempted to remove/overwrite existing asset with same id: " + (string)idValue1);
                        }
                    }
                    assetList.Add(selectedAsset);
                    if (assetDict.ContainsKey((string)idValue1)){
                    }
                    else
                    {
                        assetDict.Add((string)idValue1, selectedAsset);
                    }
                }
            }
            else
            {
                Debug.Log("File did not exist: " + filePath);
            }
        }

        // Method to exclude color properties from a JObject
        private void ExcludeColorProperties(JObject jsonObject)
        {
            var colorProperties = jsonObject.DescendantsAndSelf()
                .OfType<JProperty>()
                .Where(p => p.Value.Type == JTokenType.Object && p.Value["linear"] != null);

            foreach (var colorProperty in colorProperties.ToList())
            {
                colorProperty.Value["linear"].Parent.Remove();
            }
        }
        public void ImportAssetsFromFolder(string folderName)
        {
            string importFolderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "mods", "Import", folderName);
            if (Directory.Exists(importFolderPath))
            {
                string[] files = Directory.GetFiles(importFolderPath, "*.json");
                Debug.Log("Loading mod folder: " + folderName + ", fileCount: " + files.Length.ToString());

                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    string[] parts = fileName.Split('.');
                    string typeName = parts[1]; // Assuming type name is the second part

                    Type type = FindType(typeName);
                    if (type != null)
                    {
                        Debug.Log("Type found: " + typeName);

                        // Create a default instance of the type
                        var instance = Activator.CreateInstance(type);
                        // Call ImportIndividualAsset with the reference to the default instance
                        Debug.Log("Loading asset of type: " + type.ToString());
                        switch (typeName)
                        {
                            case "ResourceAsset":
                                ResourceAsset instanceAsResource = (ResourceAsset)instance;
                                ImportIndividualAsset<ResourceAsset>(ref instanceAsResource, AssetManager.resources.list, AssetManager.resources.dict, folderName, fileName, true);
                                break;
                            case "ActorTrait":
                                ActorTrait instanceAsTrait = (ActorTrait)instance;
                                ImportIndividualAsset<ActorTrait>(ref instanceAsTrait, AssetManager.traits.list, AssetManager.traits.dict, folderName, fileName, true);
                                break;
                            case "BiomeAsset":
                                BiomeAsset instanceAsBiome = (BiomeAsset)instance;
                                ImportIndividualAsset<BiomeAsset>(ref instanceAsBiome, AssetManager.biome_library.list, AssetManager.biome_library.dict, folderName, fileName, true);
                                break;
                            case "ActorAsset":
                                ActorAsset instanceAsActor = (ActorAsset)instance;
                                ImportIndividualAsset<ActorAsset>(ref instanceAsActor, AssetManager.actor_library.list, AssetManager.actor_library.dict, folderName, fileName, true);
                                break;
                            case "Race":
                                Race instanceAsRace = (Race)instance;
                                ImportIndividualAsset<Race>(ref instanceAsRace, AssetManager.raceLibrary.list, AssetManager.raceLibrary.dict, folderName, fileName, true);
                                break;
                            case "EraAsset":
                                EraAsset instanceAsEra = (EraAsset)instance;
                                ImportIndividualAsset<EraAsset>(ref instanceAsEra, AssetManager.era_library.list, AssetManager.era_library.dict, folderName, fileName, true);
                                break;
                            case "NameGeneratorAsset":
                                NameGeneratorAsset instanceAsNameGen = (NameGeneratorAsset)instance;
                                ImportIndividualAsset<NameGeneratorAsset>(ref instanceAsNameGen, AssetManager.nameGenerator.list, AssetManager.nameGenerator.dict, folderName, fileName, true);
                                break;
							case "CloudAsset":
								CloudAsset instanceAsCloud = (CloudAsset)instance;
								ImportIndividualAsset<CloudAsset>(ref instanceAsCloud, AssetManager.clouds.list, AssetManager.clouds.dict, folderName, fileName, true);
								break;
							default:
                                Debug.LogWarning("Type not handled: " + typeName);
                                break;
                        }
                        Debug.Log("Imported asset from mod folder");
                    }
                    else
                    {
                        Debug.LogWarning("Type not found: " + typeName);
                    }
                }
            }
            else
            {
                Debug.Log("Folder does not exist: " + importFolderPath);
            }
        }
        public class IncludeNonSerializedContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);

                foreach (var property in properties)
                {
                    var field = type.GetField(property.UnderlyingName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (field != null)
                    {
                        if (Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                        {
                            Debug.Log($"Processing field: {field.Name} marked with NonSerialized attribute");
                            property.Ignored = false; // DONT Ignore fields marked with NonSerialized attribute
                        }
                        if (field.FieldType == typeof(Color) || field.FieldType == typeof(Color32))
                        {
                            Debug.Log($"Processing field: {field.Name} of type Color or Color32");
                            property.Ignored = true; // Ignore fields of type Color or Color32
                        }
                        if (IsDelegateType(field.FieldType))
                        {
                            Debug.Log($"Processing field: {field.Name} of custom delegate type");
                            property.Ignored = true; // Ignore fields of custom delegate type
                        }
                    }
                }

                return properties;
            }
        }

        public static bool IsDelegateType(Type fieldType)
        {
            return fieldType.IsSubclassOf(typeof(Delegate)) || fieldType == typeof(Delegate);
        }


        private Type FindType(string typeName)
        {
            // Get all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Iterate through each assembly to find the type
            foreach (var assembly in assemblies)
            {
                // Check if the type exists in the assembly
                var type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
                if (type != null)
                    return type;
            }

            // Type not found in any assembly
            return null;
        }

        public class ColorConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Color color = (Color)value;
                writer.WriteStartObject();
                writer.WritePropertyName("r");
                writer.WriteValue(color.r);
                writer.WritePropertyName("g");
                writer.WriteValue(color.g);
                writer.WritePropertyName("b");
                writer.WriteValue(color.b);
                writer.WritePropertyName("a");
                writer.WriteValue(color.a);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                float r = obj["r"].Value<float>();
                float g = obj["g"].Value<float>();
                float b = obj["b"].Value<float>();
                float a = obj["a"].Value<float>();
                return new Color(r, g, b, a);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Color);
            }
        }

        public class Color32Converter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Color32 color = (Color32)value;
                writer.WriteStartObject();
                writer.WritePropertyName("r");
                writer.WriteValue(color.r);
                writer.WritePropertyName("g");
                writer.WriteValue(color.g);
                writer.WritePropertyName("b");
                writer.WriteValue(color.b);
                writer.WritePropertyName("a");
                writer.WriteValue(color.a);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                byte r = obj["r"].Value<byte>();
                byte g = obj["g"].Value<byte>();
                byte b = obj["b"].Value<byte>();
                byte a = obj["a"].Value<byte>();
                return new Color32(r, g, b, a);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Color32);
            }
        }

        public class SpriteConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Sprite sprite = (Sprite)value;
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(sprite?.name); // Write sprite name or null if sprite is null
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null; // Return null if the JSON value is null
                }

                JObject obj = JObject.Load(reader);
                string name = obj["name"]?.Value<string>(); // Get the name or null if it doesn't exist
                if (string.IsNullOrEmpty(name))
                {
                    return null; // Return null if the name is null or empty
                }

                // Recreate the sprite from the name or other relevant data
                return Resources.Load<Sprite>(name);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Sprite);
            }
        }

        public class ExcludeColorConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                if(objectType == typeof(UnityEngine.Color) || objectType == typeof(UnityEngine.Color32))
                {
                    return false;
                }
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // Load JObject from the reader
                JObject jsonObject = JObject.Load(reader);

                // Remove color properties from the JObject
                RemoveColorProperties(jsonObject);

                // Deserialize the JObject to the target objectType
                return jsonObject.ToObject(objectType);
            }

            private void RemoveColorProperties(JObject jsonObject)
            {
                foreach (JProperty property in jsonObject.Properties().ToList())
                {
                    JToken token = property.Value;

                    if (token != null)
                    {
                        if (token.Type == JTokenType.Object)
                        {
                            RemoveColorProperties((JObject)token); // Recursively remove color properties
                        }
                        else if (token.Type == JTokenType.Array)
                        {
                            foreach (JToken item in token)
                            {
                                if (item.Type == JTokenType.Object)
                                {
                                    RemoveColorProperties((JObject)item); // Recursively remove color properties
                                }
                            }
                        }
                        else if (token.Type == JTokenType.String || token.Type == JTokenType.Integer)
                        {
                            // Check if the property name contains "color" or "Color"
                            if (property.Name.ToLower().Contains("color"))
                            {
                                property.Remove(); // Remove the color property
                            }
                        }
                    }
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // Skip writing color fields
            }
        }

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
			#region OldAssetStuff
            /*
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
            */
			#endregion
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Load"))
			{
				ImportAssetsFromFolder(modFolderToLoad);
			}
            GUILayout.Label("Import/");
            GUILayout.Space(-45f + (3f * modFolderToLoad.Length));
			modFolderToLoad = GUILayout.TextField(modFolderToLoad);
            GUILayout.EndHorizontal();
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
            if (GUILayout.Button("era window"))
            {
                showHideEraWindow = !showHideEraWindow;
            }
            if (GUILayout.Button("namegen window"))
            {
                showHideNameGeneratorWindow = !showHideNameGeneratorWindow;
            }
            if (GUILayout.Button("resources window"))
            {
                showHideResourceAssetWindow = !showHideResourceAssetWindow;
            }
            if (GUILayout.Button("traits window"))
            {
                showHideActorTraitAssetWindow = !showHideActorTraitAssetWindow;
            }
			if (GUILayout.Button("clouds window"))
			{
				showHideCloudAssetWindow = !showHideCloudAssetWindow;
			}
			GUI.DragWindow();
        }

        public string modFolderToLoad = "";

        public void TileTypeSetWindow(int windowID)
        {
            if(selectedTileTypeMap != null)
            {
                if(selectedTileTypeMap.mainsheetFull != null)
                {
                    //?
                    GUILayout.Box(selectedTileTypeMap.mainsheetFull);
                }
              
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