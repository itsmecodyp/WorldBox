using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Proyecto26;
using SimplerGUI.Menus;
using SimplerGUI.Submods;
using SimplerGUI.Submods.SimpleGamba;
using SimplerGUI.Submods.SimpleMessages;
using SimpleJSON;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SimplerGUI {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    class GuiMain : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.simple.gui";
        public const string pluginName = "SimplerGUI";
        public const string pluginVersion = "0.1.8.3";

        //wtf is this??
        public static int timesLocalizedRan = 0;
        public static void loadLocalizedText_Postfix(string pLocaleID)
        {
            timesLocalizedRan++;
            Debug.Log("localizedText postfix ran " + timesLocalizedRan.ToString() + " times");
            string language = LocalizedTextManager.instance.language;
            Dictionary<string, string> localizedText = LocalizedTextManager.instance.localizedText;
            if (language == "en")
            {
                // text tips
                if (localizedText != null)
                {
                    localizedText.Add("Styderr makes awesome maps, check them out!", "Styderr makes awesome maps, check them out!");
                    localizedText.Add("Nothing to see here guys - KJYhere", "Nothing to see here guys - KJYhere");
                    localizedText.Add("Call up Rajit at 1(800)-911-SCAM   - Ramlord", "Call up Rajit at 1(800)-911-SCAM   - Ramlord");
                    localizedText.Add("10/10 would recommend - boopahead08", "10/10 would recommend - boopahead08");
                    localizedText.Add("Kosovo je srbija!", "Kosovo je srbija!");
                    localizedText.Add("This mod is sponsored by Raid: Shadow Legends - Slime", "This mod is sponsored by Raid: Shadow Legends - Slime");
                    localizedText.Add("The four nations lived in harmony, until the orc nation attacked", "The four nations lived in harmony, until the orc nation attacked");
                    localizedText.Add("Now with raytracing!", "Now with raytracing!");
                    localizedText.Add("Modificating and customizating the game...", "Modificating and customizating the game...");
                    localizedText.Add("Tiempo con Juan Diego makes amazing worldbox videos! - Juanchiz", "Tiempo con Juan Diego makes amazing worldbox videos! - Juanchiz");
                    localizedText.Add("null", "null");
                }
            }
            else if (language == "es") // Just an example
            {
                Debug.Log("Using language: Spanish");
            }
            else
            {
                Debug.Log("English/Spanish not in use");
            }
            //localizedText.Add("en", "Lays Eggs");
        }

        public void Awake()
        {
            SettingSetup();
            HarmonyPatchSetup();
            InvokeRepeating("SaveMenuPositions", 10, 3);
            InvokeRepeating("ConstantWarCheck", 10, 10);
        }

        //why havent i done this sooner?
        //postfix after InitLibraries.initLibs
        public static void postAssetInitStuff()
        {
            listOfActorAssetIDs = new List<string>();
            //init texture reassignment list
            foreach (ActorAsset actorAsset in AssetManager.actor_library.list)
            {
                if(ActorInteraction.textureSelectionBlacklist.Contains(actorAsset.id) == false)
                {
                    ActorInteraction.textureSelectionList.Add(actorAsset.id);
                }
                listOfActorAssetIDs.Add(actorAsset.id);
            }
            listOfRaceIDs = new List<string>();
            foreach (Race raceAsset in AssetManager.raceLibrary.list)
            {
                listOfRaceIDs.Add(raceAsset.id);
            }
            listOfTileTypes = new List<string>();
            listOfTopTileTypes = new List<string>();
            foreach (TileType tileAsset in AssetManager.tiles.list)
            {
                listOfTileTypes.Add(tileAsset.id);
            }
            foreach (TopTileType tileAsset in AssetManager.topTiles.list)
            {
                listOfTopTileTypes.Add(tileAsset.id);
            }
            //SimpleMessages json stuff
            Messages.SaveCurrent();
            Messages.LoadStartersFromJson();
            Messages.LoadCustomsFromJson();

            //MapGenTemplate customTemplate = new MapGenTemplate();
            //customTemplate.id = "custom";
            //AssetManager.map_gen_templates.add(customTemplate);

            //GuiOther.actorMinimumSpawns.Add("chicken", 10);
            Debug.Log("SimplerGUI post-asset stuff finished");
        }

        public bool hasInitAssets;
        public SimpleCultists cultistsManager = new SimpleCultists();
        public Messages messageManager = new Messages();

        public static bool useDebugHotkeys = false;

        //easy access for selection menus
        public static List<string> listOfActorAssetIDs = new List<string>();
        public static List<string> listOfRaceIDs = new List<string>();
        public static List<string> listOfTileTypes = new List<string>();
        public static List<string> listOfTopTileTypes = new List<string>();

        public static bool tempForDrag;
        public void Update()
        {
            //init cultist stuff
			if(global::Config.gameLoaded) {
                if(hasInitAssets == false) {
                    //cultistsManager.init(); //oops

                    //remove official H hotkey and replace with our own
                    HotkeyAsset hideUI = AssetManager.hotkey_library.get("hide_ui");
                    hideUI.default_key_1 = KeyCode.None;
                    hasInitAssets = true;
                }
			}
            //try preventing camera movement during menu usage
           if(windowInUse != -1)
            {
                tempForDrag = true;
            }
            else
            {
                tempForDrag = false;
            }
            //adding our own H ui toggle
            if(Input.GetKeyDown(KeyCode.H) && GuiOther.hideGameGUIHotkeyEnabled) {
                GuiOther.toggleBar();
            }
            if(Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftControl)){

            }
            Other.OtherControlsUpdate();
            if (useDebugHotkeys && Input.GetKeyDown(KeyCode.L)) {
				Actor closest = Toolbox.getClosestActor(MapBox.instance.units.getSimpleList(), MapBox.instance.getMouseTilePos());
                SimpleCultists.cultsDict.Add(closest, new List<Actor>());
                closest.ai.setJob("cultLeader");

                //ScrollWindow.get("settings");
                //GuiStatSetting.updateAllElements_Postfix();
			}
           
            if(SimpleSettings.showHideConstructionConfig) {
                if(Construction != null) {
                    Construction.constructionControl();
                }
            }
            if(SimpleSettings.showHideWorldOptionsConfig) {
                if(World.activeFill == null) {
                    World.fillIterationPosition = 0;
                }
                else {
                    float start = Time.realtimeSinceStartup;
                    World.tileChangingUpdate();
                    World.lastBenchedTime = World.BenchEndTime(start);
                }
                World.worldUpdate();
            }
			if(GuiOther.kingdomGetsCapitalName) {
                foreach(Kingdom kingdom in MapBox.instance.kingdoms.list) {
                    if(kingdom.capital != null && kingdom.name != kingdom.capital.name) {
                        kingdom.data.name = kingdom.capital.name;
					}
				}
			}
            if(SimpleSettings.showHideActorInteractConfig) {
                ActorInteraction.actorDragSelectionUpdate(); // advertise everything not just patreon
            }

            if (SimpleSettings.showHideActorControlConfig)
            {
                ActorControl.actorControlUpdate();
            }
        }

        public void OnGUI()
        {
            // swapped to gui.button at Key's request
            if (GUI.Button(new Rect(Screen.width - 120, 0, 120, 20), "SimplerGUI"))
            {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Main);
                if (SimpleSettings.showHideMainWindowConfig == false) {
                    // close all windows if main window is closed, fast exit for the many menus
                    SimpleSettings.CloseAllWindows();
                }
            }
            if(SimpleSettings.showHideMainWindowConfig == true) {
                updateWindows();
            }

            //this doesnt seem to work, but tried a tooltip method in ActorControl window
            var mousePosition = Input.mousePosition;

            float x = mousePosition.x;
            float y = Screen.height - mousePosition.y;
            float width = 400;
            float height = 200;
            var rect = new Rect(x, y, width, height);

            GUI.Label(rect, GUI.tooltip);
        }

        public void ConstantWarCheck()
        {
            // probably unnecessary with new ages in 0.15+
            if(Diplomacy.EnableConstantWar) {
                if(MapBox.instance.kingdoms.list_civs.Count >= 2) {
                    bool isThereWar = false;
                    foreach(Kingdom kingdom in MapBox.instance.kingdoms.list_civs) {
                        foreach(Kingdom otherKingdom in MapBox.instance.kingdoms.list_civs) {
                            if(otherKingdom != kingdom) {
                                bool isEnemy2 = kingdom.getEnemiesKingdoms().Contains(otherKingdom);
                                if(isEnemy2) {
                                    isThereWar = true;
                                    break;
                                }
                            }
                        }
                        if(isThereWar == true) {
                            break;
                        }
                    }
                    if(isThereWar == false) {
                        Kingdom kingdom1 = MapBox.instance.kingdoms.list_civs.GetRandom();
                        Kingdom kingdom2 = null;
                        while(kingdom2 == null || kingdom2 == kingdom1) {
                            kingdom2 = MapBox.instance.kingdoms.list_civs.GetRandom();
                        }
                        //0.14 version
                        //MapBox.instance.kingdoms.diplomacyManager.startWar(kingdom1, kingdom2, true);

                        MapBox.instance.diplomacy.startWar(kingdom1, kingdom2, WarTypeLibrary.normal, false);
                        // why not just log using startwar??
                        WorldLog.logNewWar(kingdom1, kingdom2);
                        //UnityEngine.Debug.Log("Constant war: war not found, starting one between: " + kingdom1.name + " and " + kingdom2.name);
                    }
                }
            }
        }


        public void HarmonyPatchSetup()
        {
            Harmony harmony = new Harmony(pluginName);
            MethodInfo original;
            MethodInfo patch;

            harmony.PatchAll();

            original = AccessTools.Method(typeof(CityPlaceFinder), "check");
            patch = AccessTools.Method(typeof(GuiOther), "check_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(CityZoneGrowth), "checkGrowBorder");
            patch = AccessTools.Method(typeof(GuiOther), "checkGrowBorder_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(TooltipLibrary), "showActor");
            patch = AccessTools.Method(typeof(ActorInteraction), "showActor_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(CrabArm), "damageWorld");
            patch = AccessTools.Method(typeof(ActorControlMain), "damageWorld_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Giantzilla), "followCamera");
            patch = AccessTools.Method(typeof(ActorControlMain), "followCamera_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(Actor), "checkEnemyTargets");
            patch = AccessTools.Method(typeof(ActorControlMain), "checkEnemyTargets_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(ActorBase), "checkAnimationContainer");
            patch = AccessTools.Method(typeof(ActorInteraction), "checkAnimationContainer_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(InitLibraries), "initLibs");
            patch = AccessTools.Method(typeof(GuiMain), "postAssetInitStuff");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorBase), "checkSpriteHead");
            patch = AccessTools.Method(typeof(ActorInteraction), "checkSpriteHead_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            /*
            original = AccessTools.Method(typeof(ActorBase), "nextJobActor");
            patch = AccessTools.Method(typeof(SimpleCultists), "nextJobActor_postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(PowerLibrary), "drawDivineLight");
            patch = AccessTools.Method(typeof(GuiTraits), "drawDivineLight_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), "checkEmptyClick");
            patch = AccessTools.Method(typeof(GuiMain), "checkEmptyClick_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorBase), "generatePersonality");
            patch = AccessTools.Method(typeof(GuiPatreon), "generatePersonality_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(LoadingScreen), "OnEnable");
            patch = AccessTools.Method(typeof(GuiPatreon), "OnEnable_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(LocalizedTextManager), "loadLocalizedText");
            patch = AccessTools.Method(typeof(GuiMain), "loadLocalizedText_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            /* disable cloud patch? broken in 0.15+
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(CloudController), "spawn");
            patch = AccessTools.Method(typeof(GUIWorld), "spawn_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(QualityChanger), "update");
            patch = AccessTools.Method(typeof(GuiOther), "update_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(TraitButton), "load");
            patch = AccessTools.Method(typeof(GuiTraits), "load_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
			Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            //example patch extending actorsay functionality
            original = AccessTools.Method(typeof(Submods.SimpleMessages.Messages), "ActorSay");
            patch = AccessTools.Method(typeof(Submods.SimpleCultists), "ActorSay_postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(BuildingActions), "spawnResource");
            patch = AccessTools.Method(typeof(GUIWorld), "spawnResource_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), "updateControls");
            patch = AccessTools.Method(typeof(GuiMain), "updateControls_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), "isActionHappening");
            patch = AccessTools.Method(typeof(GuiMain), "isActionHappening_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Building), "startDestroyBuilding");
            patch = AccessTools.Method(typeof(GUIConstruction), "startDestroyBuilding_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MoveCamera), "updateMouseCameraDrag");
            patch = AccessTools.Method(typeof(GuiOther), "updateMouseCameraDrag_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            /*
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MapBox), "spawnAndLoadUnit");
            patch = AccessTools.Method(typeof(GuiMain), "spawnAndLoadUnit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(SaveWorldButton), "saveWorld");
            patch = AccessTools.Method(typeof(GuiMain), "saveWorld_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Harmony), "PatchAll");
            patch = AccessTools.Method(typeof(GUIConstruction), "addBuilding_Prefix");
            //harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Building), "startRemove");
            patch = AccessTools.Method(typeof(GuiOther), "startRemove_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            //original = AccessTools.Method(typeof(ActorEquipmentSlot), "setItem");
            //patch = AccessTools.Method(typeof(GuiItemGeneration), "setItem_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), "addExperience");
            patch = AccessTools.Method(typeof(GuiMain), "addExperience_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorBase), "addTrait");
            patch = AccessTools.Method(typeof(GuiMain), "addTrait_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorAnimationLoader), "getItem");
            patch = AccessTools.Method(typeof(GuiMain), "getItem_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(LocalizedTextManager), "getText");
            patch = AccessTools.Method(typeof(GuiMain), "getText_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), "createNewUnit");
            patch = AccessTools.Method(typeof(GUIWorld), "createNewUnit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), "spawnNewUnit");
            patch = AccessTools.Method(typeof(GUIWorld), "spawnNewUnit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), "destroyObject");
            patch = AccessTools.Method(typeof(GUIWorld), "destroyObject_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapAction), "terraformTile");
            patch = AccessTools.Method(typeof(GuiOther), "terraformTile_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapAction), "applyTileDamage");
            patch = AccessTools.Method(typeof(GuiOther), "applyTileDamage_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(WorldTile), "setBurned");
            patch = AccessTools.Method(typeof(GuiOther), "setBurned_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(Heat), "addTile");
            patch = AccessTools.Method(typeof(GuiOther), "addTile_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(WorldTile), "setFireData");
            patch = AccessTools.Method(typeof(GuiOther), "setFireData_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            /*
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Building), "setSpriteRuin");
            patch = AccessTools.Method(typeof(GuiOther), "setSpriteRuin_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(ActorBase), "updateDeadBlackAnimation");
            patch = AccessTools.Method(typeof(GuiOther), "updateDeadBlackAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(City), "addZone");
            patch = AccessTools.Method(typeof(GuiOther), "addZone_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(City), "removeZone");
            patch = AccessTools.Method(typeof(GuiOther), "removeZone_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(CityBehCheckFarms), "checkZone");
            patch = AccessTools.Method(typeof(GuiOther), "checkZone_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(City), "getLimitOfBuildingsType");
            patch = AccessTools.Method(typeof(GuiOther), "getLimitOfBuildingsType_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(BaseSimObject), "updateStats");
            patch = AccessTools.Method(typeof(GuiStatSetting), "updateStats_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Docks), "docksAtBoatLimit");
            patch = AccessTools.Method(typeof(GuiOther), "docksAtBoatLimit_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(PowerButtonSelector), "isBottomBarShowing");
            patch = AccessTools.Method(typeof(GuiOther), "isBottomBarShowing_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), "finalizeActor");
            patch = AccessTools.Method(typeof(GUIWorld), "finalizeActor_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), "killHimself");
            patch = AccessTools.Method(typeof(GUIWorld), "killHimself_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), "killHimself");
            patch = AccessTools.Method(typeof(ActorControlMain), "killHimself_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), "newKillAction");
            patch = AccessTools.Method(typeof(ActorControlMain), "newKillActionPostfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(BattleKeeperManager), "unitKilled");
            patch = AccessTools.Method(typeof(ActorControlMain), "unitKilled_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), "createNewUnit");
            patch = AccessTools.Method(typeof(GUIWorld), "createNewUnit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(PowerLibrary), "spawnUnit");
            patch = AccessTools.Method(typeof(GUIWorld), "spawnUnit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), "checkClickTouchInspect");
            patch = AccessTools.Method(typeof(ActorControlMain), "checkClickTouchInspect_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), "checkEmptyClick");
            patch = AccessTools.Method(typeof(ActorControlMain), "checkEmptyClick_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
           
			/* tired of messing with this
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Kingdom), "createColors");
            patch = AccessTools.Method(typeof(GuiOther), "createColors_Postfix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

		}

		public static bool getItem_Prefix(string pID)
        {
            ActorAnimationLoader.dictItems.TryGetValue(pID, out Sprite sprite);
            if(sprite == null) {
                return false; // prevent error from item gen with null textures
            }

            return true;
        }


        public static bool getText_Prefix(string pKey, Text text, ref string __result)
        {
            if (pKey == null)
            {
                __result = "_placeholder?";
                return false; // prevent error from random localized texts
            }
            else
            {
                return true;
            }
        }

        public static bool addTrait_Prefix(string pTrait, bool pRemoveOpposites, ActorBase __instance)
        {
            ActorData data = __instance.data; //Reflection.GetField(__instance.GetType(), __instance, "data") as ActorStatus;
            if(__instance.hasTrait(pTrait) && Other.allowMultipleSameTrait == false) {
                return false;
            }

            if(AssetManager.traits.get(pTrait) == null) {
                return false;
            }
            if(pRemoveOpposites) {
                __instance.removeOppositeTraits(pTrait);
            }
            if(__instance.hasOppositeTrait(pTrait)) {
                return false;
            }
            __instance.data.traits.Add(pTrait);
            __instance.setStatsDirty();
            return false;
        }

        // quick fix for string replacing
        public void addTextToLocalized(string stringToReplace, string stringReplacement)
        {
            string language = LocalizedTextManager.instance.language; //Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
            Dictionary<string, string> localizedText = LocalizedTextManager.instance.localizedText; //Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;

            if(language == "en") {
                localizedText.Add(stringToReplace, stringReplacement);
            }
            Debug.Log("Added: '" + stringToReplace + "' to localized text library with translation: '" + stringReplacement + "'.");

        }

        public static void SetWindowInUse(int windowID)
        {
            Event current = Event.current;
            bool inUse = current.type == EventType.MouseDown || current.type == EventType.MouseUp || current.type == EventType.MouseDrag || current.type == EventType.MouseMove;
            if(inUse) {
                windowInUse = windowID;
            }
        }

        public void mainWindow(int windowID)
        {
            SetWindowInUse(windowID);
            if(b == true) {
                SimpleSettings.CloseAllWindows();
                // display ban status
                GUILayout.Label("BANNED.");
                // prevent clouds from spawning
                GUIWorld.disableClouds = true;
                MapBox.instance.clearWorld();
                // simulate "blue screen" by making ingame always ocean
                return;
            }
            // if (!runOnce)
            {
                // Patreon.showHidePatreon = true;
                /*
                if (autoPositionSomeWindows.Value)
                {
                    runOnce = true;
                    Timescale.showHideTimescaleWindow = true;
                    ItemGen.showHideItemGeneration = true;
                    Diplomacy.showHideDiplomacy = true;
                    World.showHideWorldOptions = true;
                    Traits.showHideTraitsWindow = true;
                }
                */
            }
            if(GUILayout.Button("Timescale")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Timescale);
            }
            if (GUILayout.Button("Items")) {
                if(AssetManager.items.list != null) {
                    //ItemGen.allWeapons = new List<string>();
                    foreach(ItemAsset item in AssetManager.items.list) {
                       // if(ItemGen.allOtherEquipment.Contains(item.id) == false && item.id.StartsWith("_") == false && item.id != "base") {
                            //ItemGen.allWeapons.Add(item.id);
                        //}
                    }
                }
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Items);
                if (SimpleSettings.showHideItemGenerationConfig == false) {
                    GuiItemGeneration.itemSelection = false;
				}
            }
            if(GUILayout.Button("Traits")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Traits);
            }
            if (GUILayout.Button("Diplomacy")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Diplomacy);
            }
            if (SimpleSettings.showHideDiplomacyConfig == false) {
                GuiDiplomacy.showCultureSelectionWindow = false;
            }
            if (GUILayout.Button("World")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.World);
            }
            if (GUILayout.Button("Construction")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Construction);
            }
            if (GUILayout.Button("Actor Interaction")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Interaction);
            }
            if (SimpleSettings.showHideActorInteractConfig == false) {
                ActorInteraction.showJobWindow = false;
                ActorInteraction.showTaskWindow = false;
            }
            if (GUILayout.Button("Actor Control"))
            {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Control);
            }
            if (GUILayout.Button("Stats")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.StatSetting);
            }
            if (GUILayout.Button("Other")) {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Other);
            }
            /* settings menu
            if (GUILayout.Button("Settings"))
            {
                SimpleSettings.ToggleMenu(SimpleSettings.MenuType.Settings);
            }
            */
            //if (GUILayout.Button("Patreon")){showHidePatreonConfig.Value = !showHidePatreonConfig.Value; }
            GUI.DragWindow();
        }

        public static Rect messageWindowRect = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 100, 10, 10);

        
        public void updateWindows()
        {
            //set inUse to "inactive", let other menus update if they are active
            SetWindowInUse(-1);
            Color originalcol = GUI.backgroundColor;
            if(SimpleSettings.showHideMainWindowConfig) {
                GUI.contentColor = Color.white;
                mainWindowRect = GUILayout.Window(1001, mainWindowRect, mainWindow, "Main", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
            /*
            if((myuser != null && myuser.UID != "null") && hasAskedToOptIn.Value == false) {
                GUI.backgroundColor = Color.yellow;
                disclaimerWindowRect = GUILayout.Window(184071, disclaimerWindowRect, new GUI.WindowFunction(DisclaimerWindow), "Disclaimer", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
                GUI.backgroundColor = originalcol;
            }
            */
            if(SimpleSettings.showHideTimescaleWindowConfig) {
                Timescale.timescaleWindowUpdate();
            }
            if(SimpleSettings.showHideItemGenerationConfig) {
                ItemGen.itemGenerationWindowUpdate(); 
            }
            if(SimpleSettings.showHideDiplomacyConfig) {
                Diplomacy.diplomacyWindowUpdate();
            }
            if(SimpleSettings.showHideOtherConfig) {
                Other.otherWindowUpdate();
            }
            if(SimpleSettings.showHideStatSettingConfig) {
                StatSetting.StatSettingWindowUpdate();
            }
            if(SimpleSettings.showHideWorldOptionsConfig) {
                World.worldOptionsWindowUpdate();
            }
            if(SimpleSettings.showHideConstructionConfig) {
                Construction.constructionWindowUpdate();
            }
            if(SimpleSettings.showHideActorInteractConfig) {
                ActorInteraction.actorInteractionWindowUpdate(); // advertise everything not just patreon
            }
            if(SimpleSettings.showHidePatreonConfig) {
                Patreon.patreonWindowUpdate(); // advertise everything not just patreon
            }
            if (SimpleSettings.showHideActorControlConfig)
            {
                ActorControl.actorControlWindowUpdate();
            }
            if(Traits != null) {
                Traits.traitWindowUpdate();
                World.fillToolIterations = SimpleSettings.fillToolIterations.Value;
                World.timerBetweenFill = SimpleSettings.timerBetweenFill.Value;
                World.fillTileCount = SimpleSettings.fillTileCount.Value;
            }
        }

        public void test1()
        {
            Debug.Log("tttt");
        }

        public void runGodPower(string inputPower, WorldTile inputTile)
        {
            if(AssetManager.powers.get(inputPower) == null) {
                Debug.Log("runGodPower: power was null");
                return;
            }
            GodPower power = AssetManager.powers.get(inputPower);
            //world.Clicked(new Vector2Int(inputTile.x, inputTile.y), 1, power, null);
        }

        public void BenchEnd(string message, float prevTime) // maxims benchmark
        {
            float time = Time.realtimeSinceStartup - prevTime;
            Debug.Log(message + " took " + time);
        }

        public Rect StringIntoRect(string input)
        {
            string setup = input;
            // default values
            float x = 0f;
            float y = 0f;
            float width = 0f;
            float height = 0f;
            if(setup.Contains("(")) {
                setup = input.Replace("(", "");
            }
            if(setup.Contains(")")) {
                setup = input.Replace(")", "");
            }
            string[] mainWindowArray = setup.Split(',');
            for(int i = 0; i < mainWindowArray.Length; i++)
            {
                var s = mainWindowArray[i];
                if(s.Contains(":")) {
                    string[] pair = s.Split(':'); // split into identifier and value
                    if(pair[0].Contains("x")) {
                        x = float.Parse(pair[1]);
                    }
                    else if(pair[0].Contains("y")) {
                        y = float.Parse(pair[1]);
                    }
                    else if(pair[0].Contains("width")) {
                        width = float.Parse(pair[1]);
                    }
                    else if(pair[0].Contains("height")) {
                        height = float.Parse(pair[1]);
                    }
                }
            }
            return new Rect(x, y, width, height);
        }

        public void SettingSetup()
        {
#pragma warning disable CS0618 // Type or member is obsolete

            SimpleSettings.fillToolIterations = Config.AddSetting("Fill", "Fill tool maximum tiles", 250, "Number of tiles each fill attempts to replace in total");
            SimpleSettings.fillTileCount = Config.AddSetting("Fill", "Fill tile count per spread", 50, "Amount of tiles changed every time the fill spreads");
            SimpleSettings.timerBetweenFill = Config.AddSetting("Fill", "Fill tool timer", 0.1f, "Time between possible spreads");
            SimpleSettings.maxTimeToWait = Config.AddSetting("Fill", "Max time to wait", 1f, "If a spread tick takes longer than this filling will stop");
            SimpleSettings.fillByLines = Config.AddSetting("Fill", "Fill by mode", "random", "Which fill mode is used. Valid inputs: first, last, random");

            // detect bloated steam_api file, impossible to have on legit copy
            var fileInfo2 = new FileInfo(Application.dataPath + "/Plugins/x86_64/steam_api64.dll");
            if(fileInfo2.Length > 265000) {
                // removed for now
                //GuiPatreon.birthdays.Add("Adin", new DateTime(1, 9, 11));
            }
            SimpleSettings.zoneAlpha = Config.AddSetting("Other", "Zone Alpha", 0.3f, "Transparency of kingdom zones, for easier visibility");
            SimpleSettings.farmsNewRange = Config.AddSetting("Other", "Windmill farms range", 27f, "Distance from windmills farms can be created");

            /*
            hasOptedInToStats = Config.AddSetting("Other", "Has opted into stats", false, "Whether your game will send game and map stats");
            hasAskedToOptIn = Config.AddSetting("Other", "Has asked to opt in", false, "Whether you've been asked about submitting stats");
            */
        }

        public List<string> bl = new List<string>
        {
        };

        public static bool b;

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private string identifyOwnerRegex()
        {
            string fileName = Assembly.GetExecutingAssembly().GetName().Name;
            string filePath = Directory.GetCurrentDirectory() + "/BepInEx/plugins/" + fileName + ".dll";
            string readContents = File.ReadAllText(filePath);
            Match bytes = Regex.Match(readContents, "<%%<(.*)>%%>");
            return bytes.Groups[1].Value;
        }

        public static void saveWorld_Postfix()
        {
            foreach(Actor actor in MapBox.instance.units) {
                ActorData data = actor.data; //Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;
                if(data.traits.Contains("stats" + data.name)) {
                    actor.removeTrait("stats" + data.name);
                }
            }
        }

        /*old patch to remove custom traits that have been forgotten at this point
        public static bool spawnAndLoadUnit_Prefix(string pStatsID, ActorData pSaveData, WorldTile pTile)
        {
            for(int i = 0; i < pSaveData.traits.Count; i++) {
                if(pSaveData.traits[i].Contains("stats") ||
                    pSaveData.traits[i].Contains("customTrait") ||
                    pSaveData.traits[i].Contains("lays_eggs") ||
                    pSaveData.traits[i].Contains("ghost") ||
                    pSaveData.traits[i].Contains("giant2") ||
                    pSaveData.traits[i].Contains("flying") ||
                    pSaveData.traits[i].Contains("assassin")
                    ) {
                    pSaveData.traits.Remove(pSaveData.traits[i]);
                }
            }
            return true;
        }
        */

        // click-through fix
        public static void isActionHappening_Postfix(ref bool __result)
        {
            if(windowInUse != -1) {
                __result = true; // "menu in use" is the action happening
            }
        }

        public static bool updateControls_Prefix()
        {
            if(windowInUse != -1) {
                return false; // cancel all control input if a window is in use
            }
            return true;
        }

        public static bool checkEmptyClick_Prefix()
        {
            if(windowInUse != -1) {
                return false; // cancel empty click usage when windows in use
            }
            return true;
        }

        public static bool addExperience_Prefix(int pValue, Actor __instance)
        {
            if(Other.disableLevelCap) {
                //ActorStats stats = __instance.stats; //Reflection.GetField(__instance.GetType(), __instance, "stats") as ActorStats;
                ActorData data = __instance.data; //Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;
                if(__instance.asset.canLevelUp) {
					if(__instance.data.alive) {
                        int expToLevelup = __instance.getExpToLevelup();
                        data.experience += pValue;
                        bool readyToLevelUp = data.experience >= expToLevelup;
                        if(readyToLevelUp) {
                            data.experience = 0;
                            data.level++;
                            __instance.setStatsDirty();
                            __instance.event_full_heal = true;
                        }
                    }
                }
                return false;
            }

            return true;
        }

        //misc
        public Camera mainCamera => Camera.main;
        public float rotationRate = 2f;
        public List<LineRenderer> buildingLings = new List<LineRenderer>();
        // vars
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);
        // Menus
        public static GuiTimescale Timescale = new GuiTimescale();
        public static GuiItemGeneration ItemGen = new GuiItemGeneration();
        public static GuiTraits Traits = new GuiTraits();
        public static GuiOther Other = new GuiOther();
        public static GuiDiplomacy Diplomacy = new GuiDiplomacy();
        public static GUIWorld World = new GUIWorld();
        public static GUIConstruction Construction = new GUIConstruction();
        public static GuiPatreon Patreon = new GuiPatreon();
        public static GuiStatSetting StatSetting = new GuiStatSetting();
        public static ActorInteraction ActorInteraction = new ActorInteraction();
        public static ActorControlMain ActorControl = new ActorControlMain();

        public static int windowInUse = -1;
        //public bool showHideDisclaimerWindow;
        public static Rect disclaimerWindowRect = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 100, 10, 10);
        int attempts;
        string response = "";
        //public float lastMapTime;
        public bool sentOneTimeStats;

    }
}






