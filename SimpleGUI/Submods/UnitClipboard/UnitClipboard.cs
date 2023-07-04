using System.Collections.Generic;
using System.Linq;
using BepInEx;
using SimpleGUI.Menus;
using UnityEngine;

namespace SimpleGUI.Submods.UnitClipboard {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class UnitClipboard_Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.unit.clipboard";
        public const string pluginName = "Unit Clipboard";
        public const string pluginVersion = "0.0.0.3";

        public void Awake()
        {
            HarmonyPatchSetup();
        }

        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.C))) {
                CopyUnit(ClosestActorToTile(MapBox.instance.getMouseTilePos(), 3f));
            }
            if(Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.V))) {
                PasteUnit(MapBox.instance.getMouseTilePos(), selectedUnitToPaste);
            }
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T)) {
                showHideMainWindow = !showHideMainWindow;
            }
            mainWindowRect.height = 0f;
        }

        public static void PasteUnit(WorldTile targetTile, UnitData unitData)
        {
            if(targetTile != null && unitData != null) {
                Actor pastedUnit = MapBox.instance.units.createNewUnit(unitData.statsID, targetTile);
                if(pastedUnit != null) {
                    if(pastedUnit.data.traits != null && pastedUnit.data.traits.Count >= 1) {
                        pastedUnit.data.traits.Clear();
                        //pastedUnit.resetTraits(); // removed?
                    }
                    if(unitData.equipment != null) {
                        pastedUnit.equipment = unitData.equipment;
                    }
                    if(unitData.data != null) {
                        //pastedUnit.data = unitData.status;
                        //pastedUnit.data.traits = unitData.data.traits;
                        pastedUnit.data.created_time = unitData.data.created_time;
                        pastedUnit.data.culture = unitData.data.culture;
                        pastedUnit.data.children = unitData.data.children;
                        pastedUnit.data.diplomacy = unitData.data.diplomacy;
                        pastedUnit.data.experience = unitData.data.experience;
                        // pastedUnit.data.favorite = unitData.status.favorite;
                        pastedUnit.data.favoriteFood = unitData.data.favoriteFood;
                        pastedUnit.data.name = unitData.data.name + " " + "Pasted";
                        pastedUnit.data.gender = unitData.data.gender;
                        pastedUnit.data.head = unitData.data.head;
                        pastedUnit.data.homeBuildingID = unitData.data.homeBuildingID;
                        pastedUnit.data.hunger = unitData.data.hunger;
                        pastedUnit.data.intelligence = unitData.data.intelligence;
                        pastedUnit.data.kills = unitData.data.kills;
                        pastedUnit.data.level = unitData.data.level;
                        pastedUnit.data.mood = unitData.data.mood;
                        pastedUnit.data.profession = unitData.data.profession;
                        pastedUnit.data.skin_set = unitData.data.skin_set;
                        pastedUnit.data.skin = unitData.data.skin;
                        pastedUnit.data.asset_id = unitData.data.asset_id;
                        pastedUnit.data.stewardship = unitData.data.stewardship;
                        pastedUnit.data.warfare = unitData.data.warfare;
                        Debug.Log("made it to custom data");
                        pastedUnit.data.culture = unitData.data.culture;
                        pastedUnit.data.inventory = unitData.data.inventory;
                        pastedUnit.data.items = unitData.data.items;
                        pastedUnit.data.clan = unitData.data.clan;
                        if(string.IsNullOrEmpty(unitData.data.clan) == false) {
                            Clan clan = MapBox.instance.clans.get(unitData.data.clan);
                            if(clan != null) {
                                clan.addUnit(pastedUnit);
                            }
							else {
                                Clan recreatedClan = MapBox.instance.clans.newClan(pastedUnit);
                                recreatedClan.data.name = unitData.data.clan;
                                recreatedClan.addUnit(pastedUnit);
                            }
                        }
                       
                        //original actor death is causing pasted actor deaths
                        //pastedUnit.setData(unitData.customData);
                        //i only wanted custom data
                        pastedUnit.base_data.custom_data_bool = unitData.customData.custom_data_bool;
                        pastedUnit.base_data.custom_data_flags = unitData.customData.custom_data_flags;
                        pastedUnit.base_data.custom_data_float = unitData.customData.custom_data_float;
                        pastedUnit.base_data.custom_data_int = unitData.customData.custom_data_int;
                        pastedUnit.base_data.custom_data_string = unitData.customData.custom_data_string;

                        //loading previous "prepared" item data. this allows transfer between worlds
                        pastedUnit.equipment.load(pastedUnit.data.items);
                        Debug.Log("made it to custom data2");
                    }
                    foreach(string trait in unitData.traits) {
                        pastedUnit.addTrait(trait);
                    }
                    pastedUnit.setStatsDirty();
                   
                    pastedUnit.restoreHealth(10 ^ 9); //lazy
                    ActorInteraction.lastSelected = pastedUnit;
                    if (unitData.data != null) Debug.Log("Pasted " + unitData.data.name);
                }
            }
               
        }
        public static List<string> addedTraits = new List<string>(); // lazy
        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width - 120, 60, 120, 20), "Clipboard"))
            {
                showHideMainWindow = !showHideMainWindow;
            }
            if(showHideMainWindow) {
                mainWindowRect = GUILayout.Window(500701, mainWindowRect, UnitClipboardWindow, "Unit Clipboard", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
        }

        public static Actor ClosestActorToTile(WorldTile pTarget, float range, Actor excludingActor = null)
        {
            Actor returnActor = null;
            foreach(Actor actorToCheck in MapBox.instance.units) {
                float actorDistanceFromTile = Toolbox.Dist(actorToCheck.currentPosition.x, actorToCheck.currentPosition.y, pTarget.pos.x, pTarget.pos.y);
                if(actorDistanceFromTile < range && actorToCheck != excludingActor) {
                    range = actorDistanceFromTile;
                    returnActor = actorToCheck;
                }
            }
            return returnActor;
        }


        public static Dictionary<string, UnitData> unitClipboardDict = new Dictionary<string, UnitData>(); // id, data

        public void UnitClipboardWindow(int windowID)
        {
            if(unitClipboardDict.Count >= 1) {
                for(int i = 0; i < unitClipboardDict.Count(); i++) {
                    if(unitClipboardDict.ContainsKey(i.ToString())) {
                        GUILayout.BeginHorizontal();
                        if(GUILayout.Button(unitClipboardDict[i.ToString()].data.name)) {
                            selectedUnitToPaste = unitClipboardDict[i.ToString()];
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                if(GUILayout.Button("Clear clipboard")) {
                    unitClipboardDict.Clear();
                    unitClipboardDictNum = 0;
                }
            }
            GUI.DragWindow();
        }        

        private void CopyUnit(Actor targetActor)
        {
            if(targetActor != null) {
                ActorData data = targetActor.data;
                //vanilla game does this to prepare item data specifically, then loads it from data.items
                targetActor.prepareForSave();
                UnitData newSavedUnit = new UnitData();
                foreach(string trait in data.traits) {
                    newSavedUnit.traits.Add(trait);
                }
                newSavedUnit.statsID = targetActor.asset.id;
                newSavedUnit.equipment = targetActor.equipment;

                ActorData data0 = new ActorData
                {
                    traits = data.traits,
                    created_time = data.created_time,
                    culture = data.culture,
                    children = data.children,
                    diplomacy = data.diplomacy,
                    experience = data.experience,
                    // data0.favorite = data.favorite;
                    favoriteFood = data.favoriteFood,
                    name = data.name,
                    gender = data.gender,
                    head = data.head,
                    homeBuildingID = data.homeBuildingID,
                    hunger = data.hunger,
                    intelligence = data.intelligence,
                    kills = data.kills,
                    level = data.level,
                    mood = data.mood,
                    profession = data.profession,
                    skin_set = data.skin_set,
                    skin = data.skin,
                    asset_id = data.asset_id,
                    stewardship = data.stewardship,
                    warfare = data.warfare,
                    inventory = data.inventory,
                    clan = data.clan
                };
                newSavedUnit.customData = targetActor.data;
                newSavedUnit.data = data0;
                unitClipboardDict.Add(unitClipboardDictNum.ToString(), newSavedUnit);
                unitClipboardDictNum++;
                selectedUnitToPaste = newSavedUnit;
                ActorInteraction.lastSelected = targetActor;
                Debug.Log("Copied " + targetActor.data.name);
            }

        }

        public static int unitClipboardDictNum;

        public static UnitData selectedUnitToPaste;

        public class UnitData {
            public string statsID = "";
            public List<string> traits = new List<string>();
            public ActorEquipment equipment;
            public ActorData data;
            public BaseObjectData customData;
        }

        public void HarmonyPatchSetup()
        {
            /*
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;
            */
        }
    }

}