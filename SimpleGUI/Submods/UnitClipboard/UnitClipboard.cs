using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BodySnatchers;
using SimplerGUI.Menus;
using UnityEngine;

namespace SimplerGUI.Submods.UnitClipboard {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class UnitClipboard_Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.unit.clipboard";
        public const string pluginName = "Unit Clipboard";
        public const string pluginVersion = "0.0.0.3";
        public bool t;
        public void Awake()
        {
            HarmonyPatchSetup();
        }

        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.C))) {
                //CopyUnit(ClosestActorToTile(MapBox.instance.getMouseTilePos(), 3f));
            }
            if(Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.V))) {
                //PasteUnit(MapBox.instance.getMouseTilePos(), selectedUnitToPaste);
            }
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T)) {
                showHideMainWindow = !showHideMainWindow;
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                showSubMod = true;
            }
            mainWindowRect.height = 0f;
        }

        public static Actor PasteUnit(WorldTile targetTile, UnitData unitData)
        {
            WorldTile tTile = targetTile;
            if (tTile != null)
            {
                if (unitData == null)
                {
                    Debug.Log("Unit data on pasted unit was null, returning");
                    return null;
                }
                Actor pastedUnit = MapBox.instance.units.createNewUnit(unitData.statsID, tTile);
                if (pastedUnit != null)
                {
                    if (pastedUnit.data.traits != null && pastedUnit.data.traits.Count >= 1)
                    {
                        pastedUnit.data.traits.Clear();
                        //pastedUnit.resetTraits(); // removed?
                    }
                    if (unitData.equipment != null)
                    {
                        pastedUnit.equipment = unitData.equipment;
                    }
                    if (unitData.data != null)
                    {
                        //pastedUnit.data = unitData.status;
                        //pastedUnit.data.traits = unitData.data.traits;
                        pastedUnit.data.created_time = unitData.data.created_time;
                        pastedUnit.data.culture = unitData.data.culture;
                        pastedUnit.data.children = unitData.data.children;
                        pastedUnit.data.diplomacy = unitData.data.diplomacy;
                        pastedUnit.data.experience = unitData.data.experience;
                        // pastedUnit.data.favorite = unitData.status.favorite;
                        pastedUnit.data.favoriteFood = unitData.data.favoriteFood;
                        //do roman thing instead
                        pastedUnit.data.name = unitData.data.name; // + " " + "Pasted"
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
                        pastedUnit.data.culture = unitData.data.culture;
                        pastedUnit.data.inventory = unitData.data.inventory;
                        pastedUnit.data.items = unitData.data.items;
                        pastedUnit.data.clan = unitData.data.clan;
                        if (string.IsNullOrEmpty(unitData.data.clan) == false)
                        {
                            Clan clan = MapBox.instance.clans.get(unitData.data.clan);
                            if (clan != null)
                            {
                                clan.addUnit(pastedUnit);
                            }
                            else
                            {
                                Clan recreatedClan = MapBox.instance.clans.newClan(pastedUnit);
                                recreatedClan.data.name = unitData.data.clan;
                                recreatedClan.addUnit(pastedUnit);
                            }
                        }
                        //Debug.Log("made it to custom data");

                        //original actor death is causing pasted actor deaths
                        //pastedUnit.setData(unitData.customData);
                        //i only wanted custom data
                        if(pastedUnit.base_data != null)
                        {
                            pastedUnit.base_data.custom_data_bool = unitData.customData.custom_data_bool;
                            pastedUnit.base_data.custom_data_flags = unitData.customData.custom_data_flags;
                            pastedUnit.base_data.custom_data_float = unitData.customData.custom_data_float;
                            pastedUnit.base_data.custom_data_int = unitData.customData.custom_data_int;
                            pastedUnit.base_data.custom_data_string = unitData.customData.custom_data_string;
                        }
                      

                        //loading previous "prepared" item data. this allows transfer between worlds
                        if(pastedUnit.data.items != null)
                        {
                            pastedUnit.equipment.load(pastedUnit.data.items);
                        }
                        //Debug.Log("made it to custom data2");
                    }
                    foreach (string trait in unitData.traits)
                    {
                        pastedUnit.addTrait(trait);
                    }
                    pastedUnit.setStatsDirty();

                    pastedUnit.restoreHealth(10 ^ 9); //lazy
                    ActorInteraction.lastSelected = pastedUnit;
                    if (unitData.data != null) Debug.Log("Pasted " + unitData.data.name);
                    return pastedUnit;
                }
            }
            return null;
        }

        public static List<string> addedTraits = new List<string>(); // lazy
        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);
        public bool showSubMod = true;

        public void OnGUI()
        {
            if (showSubMod)
            {
                if (GUI.Button(new Rect(Screen.width - 120, 60, 95, 20), "Clipboard"))
                {
                    showHideMainWindow = !showHideMainWindow;
                }
                if (GUI.Button(new Rect(Screen.width - 25, 60, 25, 20), "x"))
                {
                    showHideMainWindow = false;
                    showSubMod = false;
                }
                if (showHideMainWindow)
                {
                    mainWindowRect = GUILayout.Window(500701, mainWindowRect, UnitClipboardWindow, "Unit Clipboard", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }
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
            Color ori = GUI.backgroundColor;
            if (unitClipboardDict.Count >= 1) {
                for(int i = 0; i < unitClipboardDict.Count(); i++) {
                    if(unitClipboardDict.ContainsKey(i.ToString())) {
                        UnitData data = unitClipboardDict[i.ToString()];
                        if(data.isForResize == false)
                        {
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(data.data.name))
                            {
                                selectedUnitToPaste = data;
                            }
                            if (GUILayout.Button("x", new GUILayoutOption[] { GUILayout.Width(20f), GUILayout.MaxWidth(20f) }))
                            {
                                unitClipboardDict.Remove(i.ToString());
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.BeginHorizontal();
                if (BodySnatchers.ControlledActor.GetActor() != null && BodySnatchers.Main.squad != null)
                {
                    if (GUILayout.Button("Copy squad"))
                    {
                        //need a special clipboard method for squads
                        UnitClipboard_Main.CopySquad(BodySnatchers.Main.squad);
                    }
                }
                if (UnitClipboard_Main.squadListForPaste != null)
                {
                    GUI.backgroundColor = Color.red;
                    if (isPasting)
                    {
                        GUI.backgroundColor = Color.yellow;
                    }
                    if (GUILayout.Button("Paste squad"))
                    {
                        isPasting = !isPasting;
                    }
                    GUI.backgroundColor = ori;
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Clear clipboard")) {
                    unitClipboardDict.Clear();
                    unitClipboardDictNum = 0;
                }
            }
            GUI.DragWindow();
        }

        public bool isPasting;

        public static bool wasControlled;
        public static List<string> squadListForPaste;
        public static string leaderID;
        public static int maxSize;
        public static int x;
        public static int y;
        public static FormationType type;


        public static void CopySquad (Squad targetSquad)
        {
            if(targetSquad != null) {
                x = targetSquad.lineX;
                y = targetSquad.lineY;
                maxSize = targetSquad.maxSize;
                type = targetSquad.formation;
                squadListForPaste = new List<string>();
                if (targetSquad.leader != null && targetSquad.leader.isAlive())
                {
                    CopyUnit(targetSquad.leader);
                    //adds previous (the one we just copied) unit to unique paste list
                    leaderID = (unitClipboardDictNum - 1).ToString();
                    if (BodySnatchers.ControlledActor.GetActor() != null && BodySnatchers.ControlledActor.GetActor() == targetSquad.leader)
                    {
                        wasControlled = true;
                    }
                }
                foreach (Actor squadActor in targetSquad.squad)
                {
                    if(squadActor != null && squadActor.isAlive()) {
                        CopyUnit(squadActor);
                        squadListForPaste.Add((unitClipboardDictNum - 1).ToString());
                    }
                }
            }
        }

        public static void PasteSquad(WorldTile target)
        {
            if (squadListForPaste != null)
            {
                Actor leader = null;
                if (leaderID != null) // it better not be null or the squad cant be created..
                {
                    Actor pastedUnit = PasteUnit(target, unitClipboardDict[leaderID]);
                    unitClipboardDict.Remove(leaderID);
                    leaderID = null;
                    leader = pastedUnit;
                    BodySnatchers.ControlledActor.SetActor(leader);
                }
                Squad newSquad = new BodySnatchers.Squad(leader, maxSize, x, y, type, FormationAction.Follow);
                newSquad.SetLeader(leader);
                foreach (string id in squadListForPaste)
                {
                    Actor pastedSquadUnit = PasteUnit(target, unitClipboardDict[id]);
                    newSquad.HireActor(pastedSquadUnit, false);
                    unitClipboardDict.Remove(id);
                }
                squadListForPaste = null;
            }
        }

        public static void CopyUnit(Actor targetActor, bool isForResize = false)
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
                newSavedUnit.dictInt = unitClipboardDictNum;
                unitClipboardDict.Add(unitClipboardDictNum.ToString(), newSavedUnit);
                newSavedUnit.oldPos = targetActor.currentTile.pos;
                if(actorPositionsOnMap.ContainsKey(targetActor.currentTile.pos) == false)
                {
                    actorPositionsOnMap.Add(targetActor.currentTile.pos, unitClipboardDictNum);
                }
                newSavedUnit.isForResize = isForResize;
                unitClipboardDictNum++;
                selectedUnitToPaste = newSavedUnit;
                ActorInteraction.lastSelected = targetActor;
                Debug.Log("Copied " + targetActor.data.name);
            }

        }

        public static Dictionary<Vector2Int, int> actorPositionsOnMap = new Dictionary<Vector2Int, int>();

        public static int unitClipboardDictNum;

        public static UnitData selectedUnitToPaste;

        public class UnitData {
            public string statsID = "";
            public List<string> traits = new List<string>();
            public ActorEquipment equipment;
            public ActorData data;
            public BaseObjectData customData;
            public int dictInt;
            public bool isForResize;
            public Vector2Int oldPos;
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