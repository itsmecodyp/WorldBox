﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SimpleGUI.Menus {
    class GuiOther {
        public void otherWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            Color original = GUI.backgroundColor;
            /*
            if(GUILayout.Button("Debug map")) {
                tools.debug.DebugMap.makeDebugMap(MapBox.instance);
            }
           
            if(GUILayout.Button("Randomize building color")) {
                List<Building> buildingList = MapBox.instance.buildings.ToList();
                foreach(Building building in buildingList) {
                    BuildingData data = building.data; //= Reflection.GetField(building.GetType(), building, "data") as BuildingData;
                    if(data.state != BuildingState.Ruins && data.state != BuildingState.CivAbandoned) {
                        BuildingAsset stats = building.asset;//= Reflection.GetField(building.GetType(), building, "stats") as BuildingAsset;
                        if(stats.hasKingdomColor) {
                            SpriteRenderer spriteRenderer = Reflection.GetField(building.GetType(), building, "spriteRenderer") as SpriteRenderer;
                            spriteRenderer.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // change color
                        }
                    }
                }
            }
            if(GUILayout.Button("Randomize building roof color")) {
                List<Building> buildingList = MapBox.instance.buildings.ToList();
                foreach(Building building in buildingList) {
                    BuildingData data = building.data; //= Reflection.GetField(building.GetType(), building, "data") as BuildingData;
                    if(data.state != BuildingState.Ruins && data.state != BuildingState.CivAbandoned) {
                        BuildingAsset stats = building.asset;//= Reflection.GetField(building.GetType(), building, "stats") as BuildingAsset;
                        if(stats.hasKingdomColor) {
                            SpriteRenderer roof = Reflection.GetField(building.GetType(), building, "roof") as SpriteRenderer;
                            roof.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // change color
                        }
                    }
                }
            }
            */
            if(GuiMain.disableMinimap.Value) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Disable minimap")) {
                GuiMain.disableMinimap.Value = !GuiMain.disableMinimap.Value;
                QualityChanger qualityChanger = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "qualityChanger") as QualityChanger;
                bool lowRes = qualityChanger.lowRes;
                if(GuiMain.disableMinimap.Value) {
                    lowRes = false;
                }
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
            // seems useless with tile destruction prevention
            if(GUILayout.Button("Disable Building Destruction")) {
                disableBuildingDestruction = !disableBuildingDestruction;
            }
            if(disableTileDestruction) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Disable Tile Destruction")) {
                disableTileDestruction = !disableTileDestruction;
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
            
            /* became vanilla feature in 0.15+
            if(kingdomZonesMoreVisible) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Kingdom zones more visible")) {
                kingdomZonesMoreVisible = !kingdomZonesMoreVisible;
                // change visibility of existing kingdoms
                if(kingdomZonesMoreVisible) {
                    foreach(Kingdom existingKingdom in MapBox.instance.kingdoms.list) {
                        if(existingKingdom.kingdomColor != null)
                            existingKingdom.kingdomColor.colorBorderInsideAlpha.a = GuiMain.zoneAlpha.Value;
                    }
				}
                //change them back
                else {
                    foreach(Kingdom existingKingdom in MapBox.instance.kingdoms.list) {
                        if(existingKingdom.kingdomColor != null)
                            existingKingdom.kingdomColor.colorBorderInsideAlpha.a = 0.85f;
                    }
                }
                MapBox.instance.zoneCalculator.setDrawnZonesDirty();
                MapBox.instance.zoneCalculator.clearCurrentDrawnZones();
            }
            */
            if(stopBodyRemoval) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Prevent body decay")) {
                stopBodyRemoval = !stopBodyRemoval;
            }

            if(staticBorders) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Prevent border changes")) {
                staticBorders = !staticBorders;
            }

            if(expandedFarmRange) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Expand Windmill farm range")) {
                expandedFarmRange = !expandedFarmRange;
            }
            if(moreThanOneWindmills) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Allow multiple Windmills")) {
                moreThanOneWindmills = !moreThanOneWindmills;
            }
            if(kingdomGetsCapitalName) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Kingdom name becomes Capital")) {
                kingdomGetsCapitalName = !kingdomGetsCapitalName;
            }

            if(toggleSandFarmable) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Sand can be farmable")) {
                toggleSandFarmable = !toggleSandFarmable;
                TileLibrary.sand.can_be_farm = toggleSandFarmable;
            }
            if(toggleSnowFarmable) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Snow can be farmable")) {
                toggleSnowFarmable = !toggleSnowFarmable;
                TopTileLibrary.snow_block.can_be_farm = toggleSnowFarmable;
                TopTileLibrary.snow_sand.can_be_farm = toggleSnowFarmable;
                TopTileLibrary.snow_hills.can_be_farm = toggleSnowFarmable;
            }
            if(dockLimitOverride) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Dock limit: ")) {
                dockLimitOverride = !dockLimitOverride;
            }
            dockLimitOverrideAmount = Convert.ToInt32(GUILayout.TextField(dockLimitOverrideAmount.ToString()));
            GUILayout.EndHorizontal();

            if(canDragCreaturesWithMouse) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Drag creatures")) {
                canDragCreaturesWithMouse = !canDragCreaturesWithMouse;
            }
            
            if(hideGameGUI) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Hide GUI")) {
                hideGameGUI = !hideGameGUI;
                //reposition kingbox ui
                if (hideGameGUI == false)
                {
                    GameObject kingboxUI = GameObject.Find("DebugConfig");
                    if (kingboxUI != null)
                    {
                        kingboxUI.transform.position = new Vector3(0, 1194f, 0f);
                    }
                }
                if (hideGameGUI == true)
                {
                    GameObject kingboxUI = GameObject.Find("DebugConfig");
                    if (kingboxUI != null)
                    {
                        kingboxUI.transform.position = new Vector3(0, 1017, 0);
                    }
                }
            }
            
            if(fogOfWarTest) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("fogOfWarTest")) {
                fogOfWarTest = !fogOfWarTest;
            }
            
            GUI.backgroundColor = original;

            /* was for blueskye
            //get original name generator for mob, in this case sheep
            NameGeneratorAsset sheepNames = AssetManager.nameGenerator.get("sheep_name");
            //reset parts used
            sheepNames.part_groups = new List<string>();
            //reset templates
            sheepNames.templates = new List<string>();

            //add new parts
            sheepNames.part_groups.Add("C");
            sheepNames.part_groups.Add("o,oo,ooo");
            sheepNames.part_groups.Add("d");
            sheepNames.part_groups.Add("y,ie,ee");
            //use parts in new template
            sheepNames.templates.Add("part_group");

            //re-add generator with edited parts
            AssetManager.nameGenerator.add(sheepNames);
            */
            
            GUI.DragWindow();
        }

        public static void isBottomBarShowing_Postfix(ref bool __result)
        {
            if (hideGameGUI == false)
            {
                __result = true;
            }

            if (hideGameGUI == true)
            {
                __result = false;
            }
        }
        
        public static bool fogOfWarTest;

        public static bool hideGameGUI;

        public static bool canDragCreaturesWithMouse;

        public static int currentExtensionNumber = 0;

        public static bool preventArrowsOverMountain = true;
        public static bool disableTileDestruction;
        public static bool stopBodyRemoval;
        public static bool staticBorders;
        public static bool expandedFarmRange;
        public static bool moreThanOneWindmills;
        public static bool kingdomGetsCapitalName; //requested by shroomcrazy

        public static bool toggleSandFarmable = true; // sand is farmable by default, player turns it off
        public static bool toggleSnowFarmable = true;

        public static float dockLimitOverrideAmount = 1;
        public static bool dockLimitOverride;

        public static void docksAtBoatLimit_Postfix(ActorAsset pAsset, Docks __instance, ref bool __result)
        {
			if(dockLimitOverride) {
                __result = __instance.getList(pAsset).Count > dockLimitOverrideAmount;
            }
        }

        // fuck this right now
        public static bool tryToAttack_Prefix(BaseSimObject pTarget, ref bool __result)
		{
            return false;
		}

        public static void getLimitOfBuildingsType_Postfix(BuildOrder pElement, ref int __result)
		{
			//why am i doing this
			//i tell other people not to search by string
			//i also tell other people to patch conservatively
			//so why am i doing this
			if(moreThanOneWindmills && pElement.id.ToLower().Contains("windmill")) { __result = 999; }
		}

      

        private static bool checkZone_Prefix(TileZone pZone, Building pBuilding, City pCity)
        {
            if(!pZone.isSameCityHere(pCity)) {
                return false;
            }
            // (expandedFarmRange && Toolbox.DistTile(pBuilding.currentTile, worldTile) <= GuiMain.farmsNewRange.Value)
            if(expandedFarmRange) {
                foreach(TileZone cityZone in pCity.zones) {
                    foreach(WorldTile worldTile in cityZone.tiles) {
                        if(Toolbox.DistTile(pBuilding.currentTile, worldTile) <= GuiMain.farmsNewRange.Value) {
                            if(worldTile.Type.can_be_farm) {
                                pCity.calculated_place_for_farms.Add(worldTile);
                            }
                            if(worldTile.Type.farm_field) {
                                pCity.calculated_farm_fields.Add(worldTile);
                                if(worldTile.building != null && worldTile.building.asset.wheat) {
                                    pCity.calculated_crops.Add(worldTile);
                                }
                            }
                        }
                    }
                }
                return false;
            }

            foreach(WorldTile worldTile in pZone.tiles) {
                if(Toolbox.DistTile(pBuilding.currentTile, worldTile) <= 9) {
                    if(worldTile.Type.can_be_farm) {
                        pCity.calculated_place_for_farms.Add(worldTile);
                    }
                    if(worldTile.Type.farm_field) {
                        pCity.calculated_farm_fields.Add(worldTile);
                        if(worldTile.building != null && worldTile.building.asset.wheat) {
                            pCity.calculated_crops.Add(worldTile);
                        }
                    }
                }
            }
            return false;
        }

        public static bool removeZone_Prefix(TileZone pZone, bool pAbandonBuildings = false)
        {
            if(staticBorders) {
                return false;
            }
            return true;
        }

        public static bool addZone_Prefix(TileZone pZone)
        {
            if(staticBorders) {
                return false;
            }
            return true;
        }

        // prevent dead body removal, nice to see in wars
        // bodies can be removed still by zooming out to minimap view
        public static bool updateDeadBlackAnimation_Prefix()
        {
            if(stopBodyRemoval) {
                return false;
            }
            return true;
        }

        // only for heatray when we have applyTileDamage patch
        public static bool addTile_Prefix(WorldTile pTile, int pHeat = 1)
        {
            if(disableTileDestruction) {
                return false;
            }
            return true;
        }

        // main preventer #1
        public static bool terraformTile_Prefix(WorldTile pTile, TileType pNewTypeMain, TopTileType pTopType, TerraformOptions pOptions = null)
        {
            if(disableTileDestruction) {
                return false;
            }
            return true;
        }

        // main preventer #2
        public static bool applyTileDamage_Prefix(WorldTile pTargetTile, float pRad, TerraformOptions pOptions)
        {
            if(disableTileDestruction) {
                return false;
            }
            return true;
        }

        // why is fire so annoying
        public static bool setBurned_Prefix(int pForceVal = -1)
        {
            if(disableTileDestruction) {
                return false;
            }
            return true;
        }

        // why is fire so annoying
        public static bool setFireData_Prefix(bool pVal)
        {
            if(disableTileDestruction) {
                return false;
            }
            return true;
        }



        // changes visibilty of newly created kingdoms
        // doesnt work and im tired of trying this, the toggle works fine
        /*
        public static void createColors_Postfix(Kingdom __instance)
        {
            if(kingdomZonesMoreVisible) {
                if(__instance.kingdomColor != null && __instance.kingdomColor.colorBorderInsideAlpha != null)
                __instance.kingdomColor.colorBorderInsideAlpha.a = GuiMain.zoneAlpha.Value;
            }
        }
        */
        public static bool startRemove_Prefix()
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

        /* 
        public static bool setSpriteRuin_Prefix()
        {
            if(disableBuildingDestruction) {
                return false;
            }
            return true;
        }
        */

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
                otherWindowRect = GUILayout.Window(50000, otherWindowRect, otherWindow, "Other options", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
        }

        // minimap zoom patch
        public static bool update_Prefix(QualityChanger __instance)
        {
            if(GuiMain.disableMinimap.Value && !SmoothLoader.isLoading() && Config.gameLoaded) {
                return false;
            }

            return true;
        }

        // last namestuff
        public string actorLastName(string fullName)
        {
            string returning = "null";
            if(!fullName.Contains(" ")) {
                returning = fullName;
            }
            else {
                returning = fullName.Split(new[] { ' ' }, 2).ToList().Last();
            }
            return returning;
        }

        public static bool updateMouseCameraDrag_Prefix()
        {
            if(GuiMain.disableMouseDrag.Value) {
                return false;
            }

            return true;
        }



        public bool allowMultipleSameTrait;
        public static bool disableMinimap = false; // needs to be used instead of bepinex config
        //public static bool preventMouseDrag;
        public static bool disableBuildingDestruction;
        public bool disableLevelCap;
        //public bool showHideOther;
        public Rect otherWindowRect;
        //public Actor selectedActor;
        //public bool selectActor;
        public Actor lastActor {
            get => GuiItemGeneration.lastSelectedActor;
        }
        public ActorData lastActorData {
            get => lastActor.data;
        }
        //public static Color originalColor;
        public bool multiCrab;
        public bool powersDuringCrab;
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

            return true;
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
                    Config.controllableUnit.killHimself(true);
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
        public static void Postfix(bool pDestroy, AttackType pType, bool pCountDeath, bool pLaunchCallbacks, bool pLogFavorite, Actor __instance)
        {
			if(GuiMain.Other.multiCrab && __instance.asset.id.Contains("zilla")) {
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