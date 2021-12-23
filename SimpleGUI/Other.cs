using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace SimpleGUI {
    class GuiOther {
        public void otherWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            Color original = GUI.backgroundColor;
            if(GUILayout.Button("Debug map")) {
                tools.debug.DebugMap.makeDebugMap(MapBox.instance);
            }
            if(GUILayout.Button("Randomize building color")) {
                List<Building> buildingList = MapBox.instance.buildings.getSimpleList();
                foreach(Building building in buildingList) {
                    BuildingData data = Reflection.GetField(building.GetType(), building, "data") as BuildingData;
                    if(data.state != BuildingState.Ruins && data.state != BuildingState.CivAbandoned) {
                        BuildingAsset stats = Reflection.GetField(building.GetType(), building, "stats") as BuildingAsset;
                        if(stats.hasKingdomColor) {
                            SpriteRenderer spriteRenderer = Reflection.GetField(building.GetType(), building, "spriteRenderer") as SpriteRenderer;
                            spriteRenderer.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // change color
                        }
                    }
                }
            }
            if(GUILayout.Button("Randomize building roof color")) {
                List<Building> buildingList = MapBox.instance.buildings.getSimpleList();
                foreach(Building building in buildingList) {
                    BuildingData data = Reflection.GetField(building.GetType(), building, "data") as BuildingData;
                    if(data.state != BuildingState.Ruins && data.state != BuildingState.CivAbandoned) {
                        BuildingAsset stats = Reflection.GetField(building.GetType(), building, "stats") as BuildingAsset;
                        if(stats.hasKingdomColor) {
                            SpriteRenderer roof = Reflection.GetField(building.GetType(), building, "roof") as SpriteRenderer;
                            roof.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // change color
                        }
                    }
                }
            }
            if(GuiMain.disableMinimap.Value) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Disable minimap")) {
                GuiMain.disableMinimap.Value = !GuiMain.disableMinimap.Value;
                QualityChanger qualityChanger = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "qualityChanger") as QualityChanger;
                bool lowRes = (bool)Reflection.GetField(qualityChanger.GetType(), qualityChanger, "lowRes");
                if(GuiMain.disableMinimap.Value) {
                    lowRes = false;
                }
                else {

                } // doesnt work properly
            }
            if(!GuiMain.disableMouseDrag.Value) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Mouse drags camera")) {
                GuiMain.disableMouseDrag.Value = !GuiMain.disableMouseDrag.Value;
            }
            if(GuiMain.showWindowMinimizeButtons.Value) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Window minimize buttons")) {
                GuiMain.showWindowMinimizeButtons.Value = !GuiMain.showWindowMinimizeButtons.Value;
            }
            if(disableBuildingDestruction) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Disable BuildingDestruction")) {
                disableBuildingDestruction = !disableBuildingDestruction;
            }

            if(disableLevelCap) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Disable Level Cap")) {
                disableLevelCap = !disableLevelCap;
            }

            if(allowMultipleSameTrait) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Allow multiple same trait")) {
                allowMultipleSameTrait = !allowMultipleSameTrait;
            }

            if(multiCrab) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Allow multiple CrabZilla")) {
                multiCrab = !multiCrab;
            }

            if(powersDuringCrab) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Allow powers during CrabZilla")) {
                powersDuringCrab = !powersDuringCrab;
            }


            GUI.DragWindow();
        }

        public static bool startRemove_Prefix(bool pSetRuinSprite)
        {
            if(disableBuildingDestruction) {
                return false;
            }
            return true;
        }

        public static bool startDestroyBuilding_Prefix(bool pRemove)
        {
            if(disableBuildingDestruction) {
                return false;
            }
            return true;
        }

        public static bool destroyBuilding_Prefix()
        {
            if(disableBuildingDestruction) {
                return false;
            }
            return true;
        }

        public void otherWindowUpdate()
        {
            if(GuiMain.showWindowMinimizeButtons.Value) {
                string buttontext = "O";
                if(GuiMain.showHideOtherConfig.Value) {
                    buttontext = "-";
                }
                if(GUI.Button(new Rect(otherWindowRect.x + otherWindowRect.width - 25f, otherWindowRect.y - 25, 25, 25), buttontext)) {
                    GuiMain.showHideOtherConfig.Value = !GuiMain.showHideOtherConfig.Value;
                }
            }

            //
            if(GuiMain.showHideOtherConfig.Value) {
                otherWindowRect = GUILayout.Window(50000, otherWindowRect, new GUI.WindowFunction(otherWindow), "Other options", new GUILayoutOption[]
                {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                });
            }
        }

        // minimap zoom patch
        public static bool update_Prefix(QualityChanger __instance)
        {
            if(GuiMain.disableMinimap.Value && !Config.worldLoading && Config.gameLoaded) {
                return false;
            }
            else {
                return true;
            }
        }

        // last namestuff
        public string actorLastName(string fullName)
        {
            string returning = "null";
            if(!fullName.Contains(" ")) {
                returning = fullName;
            }
            else {
                returning = fullName.Split(new char[] { ' ' }, 2).ToList().Last();
            }
            return returning;
        }

        public static bool updateMouseCameraDrag_Prefix()
        {
            if(GuiMain.disableMouseDrag.Value) {
                return false;
            }
            else {
                return true;
            }
        }



        public bool allowMultipleSameTrait;
        public static bool disableMinimap;
        public static bool preventMouseDrag;
        public static bool disableBuildingDestruction;
        public bool disableLevelCap = false;
        public bool showHideOther;
        public Rect otherWindowRect;
        public Actor selectedActor;
        public bool selectActor;
        public Actor lastActor {
            get => GuiItemGeneration.lastSelectedActor;
        }
        public ActorStatus lastActorData {
            get => Reflection.GetField(lastActor.GetType(), lastActor, "data") as ActorStatus;
        }
        public static Color originalColor;
        public bool multiCrab;
        public bool powersDuringCrab;

    }

    // crabzilla spams NRE here
    // not sure why, lazy fix
    [HarmonyPatch(typeof(GameObjectZSorter))]
    class GameObjectZSorter_Update {
        [HarmonyPatch("Update", MethodType.Normal)]
        public static bool Prefix(GameObjectZSorter __instance)
        {
            if(__instance.zParent == null) { return false; }
            else { return true; }
        }
    }
    

    // allow multiple crabs
    [HarmonyPatch(typeof(Config))]
    class Config_ControllingUnit {
        [HarmonyPatch("controllingUnit", MethodType.Getter)] // how do i do this the other way
        public static bool Prefix(Config __instance, ref bool __result)
        {
            if(GuiMain.Other.multiCrab) {
                __result = false;
                return false;
            }
			else {
                return true;
			}
        }
    }

    // prevent controllableUnit from dying when power is unselected
    [HarmonyPatch(typeof(PowerButtonSelector))]
    class PowerButtonSelector_unselectAll {
        [HarmonyPatch("unselectAll", MethodType.Normal)] 
        public static bool Prefix(PowerButtonSelector __instance)
        {

            PowerButton selectedButton = Reflection.GetField(__instance.GetType(), __instance, "selectedButton") as PowerButton;

            if(selectedButton != null) {
                if(selectedButton != null) {
                    selectedButton.unselectActivePower();
                }
                __instance.setPower(null);
                __instance.buttonSelectionSprite.SetActive(false);
                WorldTip.instance.CallMethod("startHide");
            }
            if(Config.controllableUnit != null) {
                if(GuiMain.Other.powersDuringCrab) {
                    // do nothing
                }
				else {
                    Config.controllableUnit.killHimself(true, AttackType.Other, true, true);
                }
            }
            if(MoveCamera.focusUnit != null) {
                MoveCamera.focusUnit = null;
            }
            return false;
        }
    }

    // trying to make camera stop fucking up after 1-2 crabzillas die in a row
    // just add an option to disable WASD camera?
    [HarmonyPatch(typeof(Actor))]
    class Actor_killHimself {

        [HarmonyPatch("killHimself", MethodType.Normal)]
        public static void Postfix(bool pDestroy, AttackType pType, bool pCountDeath, bool pLaunchCallbacks, Actor __instance)
        {
			if(GuiMain.Other.multiCrab && __instance.stats.id.Contains("zilla")) {
                Kingdom kingdomByID = MapBox.instance.kingdoms.getKingdomByID("crabzilla");
                if(kingdomByID.units.Count > 1) {
                    List<Actor> tempList = kingdomByID.units.getSimpleList();
                    if(tempList.Contains(__instance)) {
                        tempList.Remove(__instance);
                    }
                    Config.controllableUnit = tempList.GetRandom();
                }
            }
        }
    }

}
