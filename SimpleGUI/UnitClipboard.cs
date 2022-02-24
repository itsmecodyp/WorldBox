using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using static UnityEngine.UI.Image;
using UnityEngine;
using System.Reflection;
using static UnitClipboard.UnitClipboard_Main;
using SimpleGUI;

namespace UnitClipboard {
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
                Actor pastedUnit = MapBox.instance.createNewUnit(unitData.statsID, targetTile, null, 0f, null);
                if(pastedUnit.data.traits != null && pastedUnit.data.traits.Count >= 1) {
                    pastedUnit.resetTraits();
                }
                if(unitData.equipment != null) {
                    pastedUnit.equipment = unitData.equipment;
                }
                if(unitData.status != null) {
                    //pastedUnit.data = unitData.status;
                    pastedUnit.data.traits = unitData.status.traits;
                    pastedUnit.data.age = unitData.status.age;
                    pastedUnit.data.bornTime = unitData.status.bornTime;
                    pastedUnit.data.culture = unitData.status.culture;
                    pastedUnit.data.children = unitData.status.children;
                    pastedUnit.data.diplomacy = unitData.status.diplomacy;
                    pastedUnit.data.experience = unitData.status.experience;
                    // pastedUnit.data.favorite = unitData.status.favorite;
                    pastedUnit.data.favoriteFood = unitData.status.favoriteFood;
                    pastedUnit.data.firstName = unitData.status.firstName + " " + "Pasted";
                    pastedUnit.data.gender = unitData.status.gender;
                    pastedUnit.data.head = unitData.status.head;
                    pastedUnit.data.homeBuildingID = unitData.status.homeBuildingID;
                    pastedUnit.data.hunger = unitData.status.hunger;
                    pastedUnit.data.intelligence = unitData.status.intelligence;
                    pastedUnit.data.kills = unitData.status.kills;
                    pastedUnit.data.level = unitData.status.level;
                    pastedUnit.data.mood = unitData.status.mood;
                    pastedUnit.data.profession = unitData.status.profession;
                    pastedUnit.data.skin_set = unitData.status.skin_set;
                    pastedUnit.data.skin = unitData.status.skin;
                    pastedUnit.data.special_graphics = unitData.status.special_graphics;
                    pastedUnit.data.statsID = unitData.status.statsID;
                    pastedUnit.data.special_graphics = unitData.status.special_graphics;
                    pastedUnit.data.stewardship = unitData.status.stewardship;
                    pastedUnit.data.warfare = unitData.status.warfare;
                }
                ActorTrait pasted = new ActorTrait();
                
                pasted.id = "pasted " + unitData.status.firstName; // might need to change to be unique
                if(addedTraits.Contains(pasted.id)) {
                    pastedUnit.addTrait(pasted.id); // refresh stats
                    pastedUnit.removeTrait(pasted.id); // remove because unnecessary
                }
                else {
                    AssetManager.traits.add(pasted);
                    addedTraits.Add(pasted.id);
                    pastedUnit.addTrait(pasted.id); // refresh stats
                    pastedUnit.removeTrait(pasted.id); // remove because unnecessary
                }
                foreach(string trait in unitData.traits) {
                    pastedUnit.addTrait(trait);
                }
                pastedUnit.restoreHealth(10 ^ 9); //lazy
                Debug.Log("Pasted " + unitData.status.firstName);
            }
        }
        public static List<string> addedTraits = new List<string>(); // lazy
        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 100, 120, 30));
            if(GUILayout.Button("UnitClipboard")) // "WorldBox3D"
            {
                showHideMainWindow = !showHideMainWindow;
            }
            GUILayout.EndArea();
            if(showHideMainWindow) {
                mainWindowRect = GUILayout.Window(500401, mainWindowRect, new GUI.WindowFunction(UnitClipboardWindow), "Unit Clipboard", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
        }

        public static Actor ClosestActorToTile(WorldTile pTarget, float range)
        {
            Actor returnActor = null;
            foreach(Actor actorToCheck in MapBox.instance.units) {
                float actorDistanceFromTile = Toolbox.Dist(actorToCheck.currentPosition.x, actorToCheck.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                if(actorDistanceFromTile < range) {
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
					if(unitClipboardDict.ContainsKey(i.ToString())){
                        GUILayout.BeginHorizontal();
                        if(GUILayout.Button(unitClipboardDict[i.ToString()].status.firstName)) {
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
                ActorStatus data = targetActor.data;
                BaseStats curStats = Reflection.GetField(targetActor.GetType(), targetActor, "curStats") as BaseStats;

                UnitData newSavedUnit = new UnitData();
                foreach(string trait in data.traits) {
                    newSavedUnit.traits.Add(trait);
                }
                newSavedUnit.statsID = data.statsID;
                newSavedUnit.equipment = targetActor.equipment;

                ActorStatus data0 = new ActorStatus();
                data0.traits = data.traits;
                data0.age = data.age;
                data0.bornTime = data.bornTime;
                data0.culture = data.culture;
                data0.children = data.children;
                data0.diplomacy = data.diplomacy;
                data0.experience = data.experience;
                // data0.favorite = data.favorite;
                data0.favoriteFood = data.favoriteFood;
                data0.firstName = data.firstName;
                data0.gender = data.gender;
                data0.head = data.head;
                data0.homeBuildingID = data.homeBuildingID;
                data0.hunger = data.hunger;
                data0.intelligence = data.intelligence;
                data0.kills = data.kills;
                data0.level = data.level;
                data0.mood = data.mood;
                data0.profession = data.profession;
                data0.skin_set = data.skin_set;
                data0.skin = data.skin;
                data0.special_graphics = data.special_graphics;
                data0.statsID = data.statsID;
                data0.special_graphics = data.special_graphics;
                data0.stewardship = data.stewardship;
                data0.warfare = data.warfare;

                newSavedUnit.status = data0;
                unitClipboardDict.Add(unitClipboardDictNum.ToString(), newSavedUnit);
                unitClipboardDictNum++;
                selectedUnitToPaste = newSavedUnit;
                Debug.Log("Copied " + targetActor.data.firstName);
            }

        }

        public static int unitClipboardDictNum = 0;

        public static UnitData selectedUnitToPaste;

        public class UnitData {
            public string statsID = "";
            public List<string> traits = new List<string>();
            public ActorEquipment equipment;
            public ActorStatus status;
        }

        public void HarmonyPatchSetup()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;
        }
    }

}
