using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Proyecto26;
using SimpleJSON;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SimpleGUI {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    class GuiMain : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.simple.gui";
        public const string pluginName = "SimpleGUI";
        public const string pluginVersion = "0.1.6.3";

        public static ManualLogSource logger;

        public void Awake()
        {
            SettingSetup();
            resetMenuOpenSettingsForBug();
            LoadMenuPositions();
            HarmonyPatchSetup();
            InvokeRepeating("SaveMenuPositions", 10, 3);
            InvokeRepeating("ConstantWarCheck", 10, 10);
            InvokeRepeating("DiscordStuff", 10, 10);
            logger = Logger;
        }

        public void Update()
        {
			if(Input.GetKeyDown(KeyCode.L)) {
                //ScrollWindow.get("settings");
                //GuiStatSetting.updateAllElements_Postfix();
			}
            if(showHideConstructionConfig.Value) {
                if(Construction != null) {
                    Construction.constructionControl();
                }
            }
            if(showHideWorldOptionsConfig.Value) {
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
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 0, 120, 30));
            if(GUILayout.Button("SimpleGUI")) {
                showHideMainWindowConfig.Value = !showHideMainWindowConfig.Value;
                if(showHideMainWindowConfig.Value == false) {
                    CloseAllWindows();
                }
            }
            GUILayout.EndArea();
            if(showHideMainWindowConfig.Value == true) {
                updateWindows();
            }
        }

        public void CloseAllWindows()
        {
            showHideTimescaleWindowConfig.Value = false;
            showHideFastCitiesConfig.Value = false;
            showHideItemGenerationConfig.Value = false;
            showHideTraitsWindowConfig.Value = false;
            showHideDiplomacyConfig.Value = false;
            showHideWorldOptionsConfig.Value = false;
            showHideConstructionConfig.Value = false;
            showHideOtherConfig.Value = false;
            showHideStatSettingConfig.Value = false;
            showHidePatreonConfig.Value = false;
        }

        /*
        public void DisclaimerWindow(int windowID)
        {
            GUILayout.Button("Hello!");  //+ discordUser.Replace("_", "#") + "!");
            GUILayout.Button(pluginName + " creator Cody#1695 here.");
            GUILayout.Label("I'd like to collect some stats every now and then. You can change your decision later using the config file and request your information or a deletion at any time.");
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            if(GUILayout.Button("Accept")) {
                hasOptedInToStats.Value = true;
                hasAskedToOptIn.Value = true;
            }
            GUI.backgroundColor = Color.red;
            if(GUILayout.Button("Decline")) {
                hasOptedInToStats.Value = false;
                hasAskedToOptIn.Value = true;
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
        */

        public void CheckMessage() // starts after 1 time stats sent
        {
            if(myuser.UID != null) {
                //StartCoroutine(GetUserMessage(myuser.UID));
            }
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

            original = AccessTools.Method(typeof(TooltipLibrary), "showActor");
            patch = AccessTools.Method(typeof(ActorInteraction), "showActor_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


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
            patch = AccessTools.Method(typeof(GuiTraits), "loadLocalizedText_Postfix");
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
            harmony.Patch(original, null, new HarmonyMethod(patch));
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

            original = AccessTools.Method(typeof(ActorManager), "destroyActor");
            patch = AccessTools.Method(typeof(GUIWorld), "destroyActor_Prefix");
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


            /* tired of messing with this
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Kingdom), "createColors");
            patch = AccessTools.Method(typeof(GuiOther), "createColors_Postfix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            harmony.PatchAll();
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
            if(pKey == null) {
                __result = "_placeholder?";
                return false; // prevent error from random localized texts
            }
            if(LocalizedTextManager.instance.localizedText.ContainsKey(pKey)) {
                return true;
            }

            __result = "_placeholder?";
            return false; // prevent error from random localized texts
        }

        public static bool addTrait_Prefix(string pTrait, ActorBase __instance)
        {
            ActorData data = __instance.data; //Reflection.GetField(__instance.GetType(), __instance, "data") as ActorStatus;
            if(__instance.hasTrait(pTrait) && Other.allowMultipleSameTrait == false) {
                return false;
            }

            ActorTrait trait = AssetManager.traits.get(pTrait);
            if(trait != null) {
                if(trait.oppositeArr == null) {
                    //return false;
                }
                else {
                    for(int i = 0; i < trait.oppositeArr.Length; i++) {
                        string pTrait2 = trait.oppositeArr[i];
                        if(__instance.hasTrait(pTrait2)) {
                            return false;
                        }
                    }
                }
                data.traits.Add(pTrait);
                __instance.setStatsDirty();
            }
            if(pTrait == "madness") {
                foreach(BaseActionActor baseActionActor in __instance.callbacks_added_madness) {
                    baseActionActor((Actor)__instance);
                }
            }
            return false;
        }


        public void SaveMenuPositions()
        {
            Construction.ConstructionWindowRect.height = 0f;
            mainWindowRect.height = 0f;
            GuiDiplomacy.diplomacyWindowRect.height = 0f;
            GuiItemGeneration.itemGenerationWindowRect.height = 0f;
            StatSetting.StatSettingWindowRect.height = 0f;
            ActorInteraction.actorInteractionWindowRect.height = 0f;
            ActorInteraction.actorJobListWindowRect.height = 0f;
            ActorInteraction.actorJobListWindowRect.height = 0f;
            ItemGen.itemGenerationEquipmentWindow1.height = 0f;
            ItemGen.itemGenerationEquipmentWindow2.height = 0f;
            if(!loadMenuSettingsOnStartup.Value) {
                return;
            }
            mainWindowRectConfig.Value = mainWindowRect.ToString();
            timescaleWindowRectConfig.Value = Timescale.timescaleWindowRect.ToString();
            fastCitiesWindowRectConfig.Value = FastCities.fastCitiesWindowRect.ToString();
            itemGenWindowRectConfig.Value = GuiItemGeneration.itemGenerationWindowRect.ToString();
            traitsWindowRectConfig.Value = Traits.traitWindowRect.ToString();
            diplomacyWindowRectConfig.Value = GuiDiplomacy.diplomacyWindowRect.ToString();
            worldWindowRectConfig.Value = World.worldOptionsRect.ToString();
            constructionWindowRectConfig.Value = Construction.ConstructionWindowRect.ToString();
            otherWindowRectConfig.Value = Other.otherWindowRect.ToString();
            //statSettingWindowRectConfig.Value = StatSetting.StatSettingWindowRect.ToString();
        }

        public void LoadMenuPositions()
        {
            if(!loadMenuSettingsOnStartup.Value) {
                return;
            }
            if(mainWindowRectConfig.Value != "null") //  && mainWindowRectConfig.Value != new Rect(0f, 1f, 1f, 1f).ToString()
            {
                mainWindowRect = StringIntoRect(mainWindowRectConfig.Value);
            }
            if(timescaleWindowRectConfig.Value != "null") {
                Timescale.timescaleWindowRect = StringIntoRect(timescaleWindowRectConfig.Value);
            }
            if(fastCitiesWindowRectConfig.Value != "null") {
                FastCities.fastCitiesWindowRect = StringIntoRect(fastCitiesWindowRectConfig.Value);
            }
            if(itemGenWindowRectConfig.Value != "null") {
                GuiItemGeneration.itemGenerationWindowRect = StringIntoRect(itemGenWindowRectConfig.Value);
            }
            if(traitsWindowRectConfig.Value != "null") {
                Traits.traitWindowRect = StringIntoRect(traitsWindowRectConfig.Value);
            }
            if(diplomacyWindowRectConfig.Value != "null") {
                GuiDiplomacy.diplomacyWindowRect = StringIntoRect(diplomacyWindowRectConfig.Value);
            }
            if(worldWindowRectConfig.Value != "null") {
                World.worldOptionsRect = StringIntoRect(worldWindowRectConfig.Value);
            }
            if(constructionWindowRectConfig.Value != "null") {
                Construction.ConstructionWindowRect = StringIntoRect(constructionWindowRectConfig.Value);
            }
            if(otherWindowRectConfig.Value != "null") {
                Other.otherWindowRect = StringIntoRect(otherWindowRectConfig.Value);
            }
            if(statSettingWindowRectConfig.Value != "null") {
               // StatSetting.StatSettingWindowRect = StringIntoRect(statSettingWindowRectConfig.Value);
            }          
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
                // ban problematic users
                // keep all menus closed so mod is unusable
                showHideTimescaleWindowConfig.Value = false;
                showHideTraitsWindowConfig.Value = false;
                showHideDiplomacyConfig.Value = false;
                showHideWorldOptionsConfig.Value = false;
                showHideConstructionConfig.Value = false;
                showHideStatSettingConfig.Value = false;
                showHideOtherConfig.Value = false;
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
                    FastCities.showHideFastCities = true;
                    ItemGen.showHideItemGeneration = true;
                    Diplomacy.showHideDiplomacy = true;
                    World.showHideWorldOptions = true;
                    Traits.showHideTraitsWindow = true;
                }
                */
            }
            if(GUILayout.Button("Timescale")) {
                showHideTimescaleWindowConfig.Value = !showHideTimescaleWindowConfig.Value;
            }
            /* fastCities is a debug option now
            if(GUILayout.Button("Fast cities")) {
                showHideFastCitiesConfig.Value = !showHideFastCitiesConfig.Value;
            }
            */
            if(GUILayout.Button("Items")) {
                if(AssetManager.items.list != null) {
                    
                    //ItemGen.allWeapons = new List<string>();
                    foreach(ItemAsset item in AssetManager.items.list) {
                       // if(ItemGen.allOtherEquipment.Contains(item.id) == false && item.id.StartsWith("_") == false && item.id != "base") {
                            //ItemGen.allWeapons.Add(item.id);
                        //}
                    }
                    
                }
				showHideItemGenerationConfig.Value = !showHideItemGenerationConfig.Value;
                if(showHideItemGenerationConfig.Value == false) {
                    GuiItemGeneration.itemSelection = false;
				}
            }
            if(GUILayout.Button("Traits")) {
                showHideTraitsWindowConfig.Value = !showHideTraitsWindowConfig.Value;
            }
            if(GUILayout.Button("Diplomacy")) {
                showHideDiplomacyConfig.Value = !showHideDiplomacyConfig.Value;
            }
            if(showHideDiplomacyConfig.Value == false) {
                GuiDiplomacy.showCultureSelectionWindow = false;
            }
            if (GUILayout.Button("World")) {
                showHideWorldOptionsConfig.Value = !showHideWorldOptionsConfig.Value;
            }
            if(GUILayout.Button("Construction")) {
                showHideConstructionConfig.Value = !showHideConstructionConfig.Value;
            }
            if(GUILayout.Button("Actor Interaction")) {
                showHideActorInteractConfig.Value = !showHideActorInteractConfig.Value;
            }
            if(showHideActorInteractConfig.Value == false) {
                ActorInteraction.showJobWindow = false;
                ActorInteraction.showTaskWindow = false;
            }
            if(GUILayout.Button("Stats")) {
                showHideStatSettingConfig.Value = !showHideStatSettingConfig.Value;
            }
            if(GUILayout.Button("Other")) {
                showHideOtherConfig.Value = !showHideOtherConfig.Value;
            }
            //if (GUILayout.Button("Patreon")){showHidePatreonConfig.Value = !showHidePatreonConfig.Value; }
            GUI.DragWindow();
        }

        public static Rect messageWindowRect = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 100, 10, 10);


        public void updateWindows()
        {
            Color originalcol = GUI.backgroundColor;
            if(showHideMainWindowConfig.Value) {
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
            if(receivedMessage != null) {
                messageWindowRect = GUILayout.Window(186075, messageWindowRect, showModMessageWindow, "Message", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
            if(showHideTimescaleWindowConfig.Value) {
                Timescale.timescaleWindowUpdate();
            }
            if(showHideFastCitiesConfig.Value) {
                FastCities.fastCitiesWindowUpdate();
            }
            if(showHideItemGenerationConfig.Value) {
                ItemGen.itemGenerationWindowUpdate(); 
            }
            if(showHideDiplomacyConfig.Value) {
                Diplomacy.diplomacyWindowUpdate();
            }
            if(showHideOtherConfig.Value) {
                Other.otherWindowUpdate();
            }
            if(showHideStatSettingConfig.Value) {
                StatSetting.StatSettingWindowUpdate();
            }
            if(showHideWorldOptionsConfig.Value) {
                World.worldOptionsUpdate();
            }
            if(showHideConstructionConfig.Value) {
                Construction.constructionWindowUpdate();
            }
            if(Traits != null) {
                Traits.traitWindowUpdate();
                World.fillToolIterations = fillToolIterations.Value;
                World.timerBetweenFill = timerBetweenFill.Value;
                World.fillTileCount = fillTileCount.Value;
            }
            if(showHideActorInteractConfig.Value) {
                ActorInteraction.actorInteractionWindowUpdate(); // advertise everything not just patreon
            }
            if(showHidePatreonConfig.Value) {
                Patreon.patreonWindowUpdate(); // advertise everything not just patreon
            }
            SetWindowInUse(-1);
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

            fillToolIterations = Config.AddSetting("Fill", "Fill tool maximum tiles", 250, "Number of tiles each fill attempts to replace in total");
            fillTileCount = Config.AddSetting("Fill", "Fill tile count per spread", 50, "Amount of tiles changed every time the fill spreads");
            timerBetweenFill = Config.AddSetting("Fill", "Fill tool timer", 0.1f, "Time between possible spreads");
            maxTimeToWait = Config.AddSetting("Fill", "Max time to wait", 1f, "If a spread tick takes longer than this filling will stop");
            fillByLines = Config.AddSetting("Fill", "Fill by mode", "random", "Which fill mode is used. Valid inputs: first, last, random");

            showHideMainWindowConfig = Config.AddSetting("Menus", "Main Menu Toggle", false, "Whether Main menu was last displayed or open");
            showHideTimescaleWindowConfig = Config.AddSetting("Menus", "Timescale Menu Toggle", false, "Whether Timescale menu was last displayed or open");
            showHideFastCitiesConfig = Config.AddSetting("Menus", "Fast Cities Menu Toggle", false, "Whether Fast Cities menu was last displayed or open");
            showHideItemGenerationConfig = Config.AddSetting("Menus", "Item Menu Toggle", false, "Whether Items menu was last displayed or open");
            showHideTraitsWindowConfig = Config.AddSetting("Menus", "Traits Menu Toggle", false, "Whether Traits menu was last displayed or open");
            showHideDiplomacyConfig = Config.AddSetting("Menus", "Diplomacy Menu Toggle", false, "Whether Diplomacy menu was last displayed or open");
            showHideWorldOptionsConfig = Config.AddSetting("Menus", "World Menu Toggle", false, "Whether world menu was last displayed or open");
            showHideConstructionConfig = Config.AddSetting("Menus", "Construction Menu Toggle", false, "Whether Construction menu was last displayed or open");
            showHideOtherConfig = Config.AddSetting("Menus", "Other Menu Toggle", false, "Whether Other menu was last displayed or open");
            showHideStatSettingConfig = Config.AddSetting("Menus", "Stats Menu Toggle", false, "Whether Stats menu was last displayed or open");
            showHidePatreonConfig = Config.AddSetting("Menus", "Patreon Menu Toggle", false, "Whether Patreon menu was last displayed or open");
            showHideActorInteractConfig = Config.AddSetting("Menus", "Actor Interaction Menu Toggle", false, "Whether Actor Interaction menu was last displayed or open");


            showPatreonWindow = Config.AddSetting("Menus", "Show Patreon Advertisement", false, "Display my advertisement for Patreon and Discord");

            mainWindowRectConfig = Config.AddSetting("Pos", "Main Menu Position", "null", "Last saved position for window");
            timescaleWindowRectConfig = Config.AddSetting("Pos", "Timescale Menu Position", "null", "Last saved position for window");
            fastCitiesWindowRectConfig = Config.AddSetting("Pos", "Fastcities Menu Position", "null", "Last saved position for window");
            itemGenWindowRectConfig = Config.AddSetting("Pos", "ItemGen Menu Position", "null", "Last saved position for window");
            traitsWindowRectConfig = Config.AddSetting("Pos", "Traits Menu Position", "null", "Last saved position for window");
            diplomacyWindowRectConfig = Config.AddSetting("Pos", "Diplomacy Menu Position", "null", "Last saved position for window");
            worldWindowRectConfig = Config.AddSetting("Pos", "World Menu Position", "null", "Last saved position for window");
            constructionWindowRectConfig = Config.AddSetting("Pos", "Construction Menu Position", "null", "Last saved position for window");
            otherWindowRectConfig = Config.AddSetting("Pos", "Other Menu Position", "null", "Last saved position for window");
            statSettingWindowRectConfig = Config.AddSetting("Pos", "Stats Menu Position", "null", "Last saved position for window");

            loadMenuSettingsOnStartup = Config.AddSetting("Menus", "Save/load menu settings", false, "Whether the menus automatically save and load their settings");
            // leaving this one on false ^ when it's enabled people have to reset their config to make the mod work every time they play

            showWindowMinimizeButtons = Config.AddSetting("Menus", "Show minimize buttons", false, "Whether the menus have an extra button above them to quickly open and close them");
            // detect bloated steam_api file, impossible to have on legit copy
            var fileInfo2 = new FileInfo(Application.dataPath + "/Plugins/x86_64/steam_api64.dll");
            if(fileInfo2.Length > 265000) {
                // removed for now
                //GuiPatreon.birthdays.Add("Adin", new DateTime(1, 9, 11));
            }
            disableMinimap = Config.AddSetting("Other", "Disable Minimap", false, "Whether the zoomed out, low resolution mode is disabled");
            disableMouseDrag = Config.AddSetting("Other", "Disable Mouse camera drag", false, "Whether the the click+drag camera movement behaviour is disabled");
            zoneAlpha = Config.AddSetting("Other", "Zone Alpha", 0.3f, "Transparency of kingdom zones, for easier visibility");
            farmsNewRange = Config.AddSetting("Other", "Windmill farms range", 27f, "Distance from windmills farms can be created");

            /*
            hasOptedInToStats = Config.AddSetting("Other", "Has opted into stats", false, "Whether your game will send game and map stats");
            hasAskedToOptIn = Config.AddSetting("Other", "Has asked to opt in", false, "Whether you've been asked about submitting stats");
            */
        }

        public void resetMenuOpenSettingsForBug()
        {
            if(loadMenuSettingsOnStartup.Value == false) // close all the menus on startup if this is disabled
            {
                CloseAllWindows();
            }

        }

        public List<string> bl = new List<string>
        {
            "274102697307930635",
            "693297828243439697",
        };

        public static bool b;

        public void DiscordStuff()
        {
            // use discord to ID users and do stuff
            if(myuser.discordID == "null") {
                if(attempts < 4) {
                    attempts++;
                    Discord.Discord discord = DiscordTracker.discord;
                    //Steamworks.SteamClient.SteamId.v
                    // discord.GetActivityManager().
                    // discord.GetUserManager().GetCurrentUser().
                    if(discord != null) {
                        var userManager = discord.GetUserManager();
                        var user = userManager.GetCurrentUser();
                        myuser.discordUsername = user.Username + "_" + user.Discriminator;
                        myuser.discordID = user.Id.ToString();
                    }
                }
            }

            if(SteamClient.IsValid) {
                myuser.steamUsername = SteamClient.Name;
                myuser.steamID = SteamClient.SteamId.Value.ToString();

                //over 9,000 logged users named Goldberg? over 1,500 named noob?
                //caused by popular steam emulators used for piracy
                string lowered = myuser.steamUsername.ToLower();
                if(lowered == "goldberg" || lowered == "noob") {
                    // revisit later after getting the file verification figured out
                    GuiTraits.addTraitsToAssets();
                }
            }

            //check ban list
            if(myuser.discordID != "null") {
				if(bl.Contains(myuser.discordID)) {
                    b = true;
                }
			}

            if(myuser.steamID != "null") {
                if(bl.Contains(myuser.steamID)) {
                    b = true;
                }
            }


            if(myuser.discordID != "null" || myuser.steamUsername != "null") {
                if(sentOneTimeStats == false) {
                    myuser.VersionSimpleGUI = pluginVersion;
                    if(Application.version != null)
                        myuser.VersionWorldBox = Application.version;
                    GameStatsData gameStatsData = MapBox.instance.gameStats.data; //Reflection.GetField(MapBox.instance.gameStats.GetType(), MapBox.instance.gameStats, "data") as GameStatsData;
                    TimeSpan timePlayed = TimeSpan.FromSeconds(gameStatsData.gameTime);
                    myuser.GameTimeTotal = timePlayed.Days + " days, " + timePlayed.Hours + " hours, " + timePlayed.Minutes + " minutes"; //gameStatsData.gameTime.ToString();
                    myuser.GameLaunches = gameStatsData.gameLaunches.ToString();
                    myuser.lastLaunchTime = DateTime.Now.ToUniversalTime().Ticks.ToString();
                    // mods, modcount
                    string path = Directory.GetCurrentDirectory() + "\\BepInEx//plugins//";
                    FileInfo[] fileArray = new DirectoryInfo(path).GetFiles();
                    for(int i = 0; i < fileArray.Length; i++) {
                        if(fileArray[i].Extension.Contains("dll"))
                            myuser.Mods.Add(fileArray[i].Name);
                    }

                    path = Directory.GetCurrentDirectory() + "\\worldbox_Data//StreamingAssets//Mods//";
                    if(Directory.Exists(path)) {
                        FileInfo[] fileArray2 = new DirectoryInfo(path).GetFiles();
                        for(int i = 0; i < fileArray2.Length; i++) {
                            if(fileArray2[i].Extension.Contains("dll"))
                                myuser.Mods.Add(fileArray2[i].Name);
                        }
                    }

                    path = Directory.GetCurrentDirectory() + "\\Mods//";
                    if(Directory.Exists(path)) {
                        FileInfo[] fileArray3 = new DirectoryInfo(path).GetFiles();
                        for(int i = 0; i < fileArray3.Length; i++) {
                            if(fileArray3[i].Extension.Contains("mod"))
                                myuser.Mods.Add(fileArray3[i].Name);
                        }

                        DirectoryInfo[] directoryArray = new DirectoryInfo(path).GetDirectories();
                        for(int i = 0; i < directoryArray.Length; i++) {
                            if(directoryArray[i].Name.Contains("Example") == false) {
                                myuser.Mods.Add(directoryArray[i].Name);
                            }
                        }
                    }

                    myuser.ModCount = myuser.Mods.Count.ToString();
                    // finally, submit
                    var vURL = "https://simplegui-default-rtdb.firebaseio.com/users/" + myuser.UID + "/.json";
                    RestClient.Put(vURL, myuser);
                    //StartCoroutine(GetUserMessage(discordUser)); // no plans for messaging anytime soon
                    sentOneTimeStats = true;
                }

            }
            //mapstats, regularly updated
            /* not anymore, never checked them, waste of data
            MapStats currentMapStats = MapBox.instance.mapStats;
            if (true) // log only worlds people have spent some time in
            {
                var vURLMapData = "https://simplegui-default-rtdb.firebaseio.com//maps.json";
                RestClient.Delete(vURLMapData);
                lastMapTime = Time.realtimeSinceStartup;
            }
            */
            // check user message
            StartCoroutine(GetUserMessage(myuser.UID));

            /* removed option to opt out, which is when this would be called
            else {
                if(sentOneTimeStats == false) {
                    ModUserOptedOut newUser = new ModUserOptedOut();
                    var vURL = "https://simplegui-default-rtdb.firebaseio.com//users/" + myuser.UID + "/.json";
                    RestClient.Put(vURL, newUser);
                    sentOneTimeStats = true;
                }
            }
            */




        }

        string lastMessageReceived = "";
        string myUserName => myuser.discordUsername != "null" ? myuser.discordUsername : myuser.steamUsername;
        public IEnumerator GetUserMessage(string username)
        {
            string url = "https://simplegui-default-rtdb.firebaseio.com/messages/" + username + "/.json";
            // "https://simplegui-default-rtdb.firebaseio.com/messages/user/.json";
            UnityWebRequest dataRequest = UnityWebRequest.Get(url);
            yield return dataRequest.SendWebRequest();
            JSONNode data = JSON.Parse(dataRequest.downloadHandler.text);
            if(data != null) {
                string messageText = data[0];
                if(lastMessageReceived == messageText) {

                }
                else {
                    lastMessageReceived = messageText;
                    if(messageText.Contains("#response")) {
                        responseAskedFor = true;
                        messageText = messageText.Replace("#response", "");
                    }
                    if(messageText.Contains("#command~")) {
                        string[] split = messageText.Split(new[] { "~" }, StringSplitOptions.None);
                        string message = split[1];
                        string command;
                        if(message.Contains('(')) {
                            if(message.Contains(')')) {
                                var param = message.Split('(', ')')[1];
                                command = message.Replace("(" + param + ")", "");
                                ParseCommand(command, param);
                            }
                        }
                        else {
                            command = message;
                            ParseCommand(command, "");
                        }
                        messageText = messageText.Replace("#command~" + split[1], "");
                    }
                    if(!messageText.Contains("#hidden")) {
                        Logger.Log(LogLevel.Message, "Message for " + myUserName + ": " + messageText);
                        //UnityEngine.Debug.Log("Message for " + username + ": " + messageText);
                        receivedMessage = messageText;
                    }
                }

            }
        }

        public void ParseCommand(string command, string param)
        {
            if(command == "setSeed" && param != "") {
                int.TryParse(param, out int newSeed);
            }
            if(command == "url" && param.StartsWith("http") && param.Contains("youtube")) {
                Application.OpenURL(param);//Process.Start("cmd.exe", "/c start " + param);
            }
            string url = "https://simplegui-default-rtdb.firebaseio.com/messages/" + myuser.UID + "/.json";
            RestClient.Delete(url);
            receivedMessage = null;
        }

        public void showModMessageWindow(int windowID)
        {
            if(responseAskedFor) {
                response = GUILayout.TextField(response);
                if(GUILayout.Button("Submit response")) {
                    ModResponse message = new ModResponse
                    {
                        message = receivedMessage,
                        response = response
                    };
                    string url = "https://simplegui-default-rtdb.firebaseio.com/responses/" + myuser.UID + "/.json";
                    RestClient.Put(url, message);
                    url = "https://simplegui-default-rtdb.firebaseio.com/messages/" + myuser.UID + "/.json";
                    RestClient.Delete(url);
                    receivedMessage = null;
                }
            }
            GUILayout.Label(receivedMessage);
            if(GUILayout.Button("Dismiss")) {
                string url = "https://simplegui-default-rtdb.firebaseio.com/messages/" + myuser.UID + "/.json";
                RestClient.Delete(url);
                receivedMessage = null;
            }
            GUI.DragWindow();
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

        public static ModUser myuser = new ModUser();
        #region variableDeclaration
        //misc
        public Camera mainCamera => Camera.main;
        public bool animationTest;
        public float xRot;
        public float yRot;
        public float zRot;
        public float rotationRate = 2f;
        public List<LineRenderer> buildingLings = new List<LineRenderer>();
        public bool actorTest;
        public bool lockCameraZ;
        public Quaternion rotationDefault;
        public bool clearActiveLines;
        // vars
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);
        // Menus
        public static GuiTimescale Timescale = new GuiTimescale();
        public static GuiFastCities FastCities = new GuiFastCities();
        public static GuiItemGeneration ItemGen = new GuiItemGeneration();
        public static GuiTraits Traits = new GuiTraits();
        public static GuiOther Other = new GuiOther();
        public static GuiDiplomacy Diplomacy = new GuiDiplomacy();
        public static GUIWorld World = new GUIWorld();
        public static GUIConstruction Construction = new GUIConstruction();
        public static GuiPatreon Patreon = new GuiPatreon();
        public static GuiStatSetting StatSetting = new GuiStatSetting();
        public static ActorInteraction ActorInteraction = new ActorInteraction();

        // Config
        public static ConfigEntry<float> farmsNewRange {
            get; set;
        }
        public static ConfigEntry<int> fillToolIterations {
            get; set;
        }
        public static ConfigEntry<float> timerBetweenFill {
            get; set;
        }
        public static ConfigEntry<float> maxTimeToWait {
            get; set;
        }
        public static ConfigEntry<int> fillTileCount {
            get; set;
        }
        public static ConfigEntry<string> fillByLines {
            get; set;
        }
        public static ConfigEntry<bool> autoPositionSomeWindows {
            get; set;
        }
        public static ConfigEntry<bool> showPatreonWindow {
            get; set;
        }
        public static ConfigEntry<bool> showHideMainWindowConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideTimescaleWindowConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideFastCitiesConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideItemGenerationConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideTraitsWindowConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideDiplomacyConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideWorldOptionsConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideConstructionConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideOtherConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHidePatreonConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideActorInteractConfig {
            get; set;
        }
        public static ConfigEntry<bool> showHideStatSettingConfig {
            get; set;
        }
        public static ConfigEntry<bool> showWindowMinimizeButtons {
            get; set;
        }
        public static ConfigEntry<bool> disableMinimap {
            get; set;
        }
        public static ConfigEntry<bool> disableMouseDrag {
            get; set;
        }
        public static ConfigEntry<string> mainWindowRectConfig {
            get; set;
        } // string because Rect isnt supported 
        public static ConfigEntry<string> timescaleWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> fastCitiesWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> itemGenWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> traitsWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> diplomacyWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> worldWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> constructionWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> otherWindowRectConfig {
            get; set;
        }
        public static ConfigEntry<string> statSettingWindowRectConfig {
            get; set;
        }

        public static ConfigEntry<bool> loadMenuSettingsOnStartup {
            get; set;
        }
        public static ConfigEntry<float> zoneAlpha {
            get; set;
        }
        #endregion
        public static int windowInUse = -1;
        public bool showHideDisclaimerWindow;
        public static Rect disclaimerWindowRect = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 100, 10, 10);
        int attempts;
        string response = "";
        public float lastMapTime;
        public bool sentOneTimeStats;
        public string receivedMessage;
        bool responseAskedFor;

        [Serializable]
        public class ModUser {
            public string UID = SystemInfo.deviceUniqueIdentifier;
            public string VersionWorldBox = "null";
            public string VersionSimpleGUI = "null";
            public string GameTimeTotal = "null";
            public string GameLaunches = "null";
            public string lastLaunchTime = "null";
            public string ModCount = "null";
            public List<string> Mods = new List<string>();
            public string discordUsername = "null";
            public string discordID = "null";
            public string steamUsername = "null";
            public string steamID = "null";
        }

        [Serializable]
        public class ModUserOptedOut {
            public bool OptedInToStats;
        }

        [Serializable]
        public class ModMessage {
            public string message = "";
        }

        [Serializable]
        public class ModResponse {
            public string message = "";
            public string response = "";
        }
    }

    /////////////////////////////////////////////////

}






