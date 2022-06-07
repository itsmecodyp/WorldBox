using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace TWrecks_RPG {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TWrecks_Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.rpg.twrecks";
        public const string pluginName = "RPG";
        public const string pluginVersion = "0.0.0.2";

        public void Awake()
        {
            HarmonyPatchSetup();
            SettingSetup();
        }

		[Obsolete] // stops warning about gameObject.active
		public void Update()
        {
            UpdateControls();
            UpdateSquadBehaviour();
            
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.H)) {
                if(controlledActor != null) {
                    AddActorToSquad(currentlySelectedFormation, (ClosestActorToTile(MouseTile, 3f)));
                    //rpgWindow.OpenControlledWindow();
                }
                //TWrecks_RPG
                //hiredActorList.Add(ClosestActorToTile(MouseTile, controlledActor, 30f));
            }
            if(Input.GetKeyDown(KeyCode.X)) {
                if(currentlySelectedFormation != null && MapBox.instance.getMouseTilePos() != null) {
                    if(listOfSquadsWithLeaders.Contains(currentlySelectedFormation.squadID)) {
                        currentlySelectedFormation.movementPos = MapBox.instance.getMouseTilePos().posV3;
                        currentlySelectedFormation.offsetPos = currentlySelectedFormation.movementPos - leaderDict[currentlySelectedFormation.squadID].squadLeaderActor.currentTile.posV3;
                    }
                    else {
                        currentlySelectedFormation.movementPos = MapBox.instance.getMouseTilePos().posV3;
                        currentlySelectedFormation.offsetPos = currentlySelectedFormation.movementPos - lastTile.posV3;
                    }
                    //Debug.Log("offset saved: " + currentlySelectedFormation.offsetPos.ToString());
                }
            }
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M)) {
                for(int i = 0; i < 5; i++) {
                    SquadFormation newSquad = new SquadFormation();
                    newSquad.movementPos = MapBox.instance.getMouseTilePos().posV3;
                    for(int j = 0; j < 10; j++) {
                        Actor newActor = MapBox.instance.createNewUnit("unit_human", MapBox.instance.getMouseTilePos(), null, 10f, null);
                        AddActorToSquad(newSquad, newActor);
                    }
                    Actor newActorLeader = MapBox.instance.createNewUnit("unit_human", MapBox.instance.getMouseTilePos(), null, 10f, null);
                    SquadLeader leader = new SquadLeader(newActorLeader);
                    leader.squad = newSquad;
                    listOfSquadsWithLeaders.Add(newSquad.squadID);
                    squadsInUse.Add(newSquad.squadID, newSquad);
                    leaderDict.Add(newSquad.squadID, leader);
                }
            }
            if(controlledActor != null && Input.GetKeyDown(KeyCode.B)) {
                guiConstruction.showHideConstruction = !guiConstruction.showHideConstruction;
            }
            // need a better check later
            if(controlledActor == null && MapBox.instance != null && MapBox.instance.canvas.gameObject.active == false) {
                MapBox.instance.canvas.gameObject.SetActive(true); // hide power bar and other UI..
            }
        }

        public bool showActorInteract;
        public Rect actorInteractWindowRect;

        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public static bool showHideLeadersWindow;
        public static Rect leadersWindowRect = new Rect(0f, 1f, 1f, 1f);

        /*
        public static int kingAgeExp = 20;
        public static int leaderAgeExp = 10;
        public static int otherAgeExp = 2;
        public static int intendedExpToLevelup = 100; // default: 100 + (this.data.level - 1) * 20;
        */
        public static string kingExpInput = "20";
        public static string leaderExpInput = "10";
        public static string otherExpInput = "2";
        public static string expToLevel = "100";
        public static string expScale = "20";
        public static string expGainedOnKill = "10";

        public static List<string> listOfSquadsWithLeaders = new List<string>();
        public static Dictionary<string, SquadLeader> leaderDict = new Dictionary<string, SquadLeader>();

        public void mainWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button("KingAgeExp");
            kingExpInput = GUILayout.TextField(kingExpInput);
            int newKingExp;
            if(int.TryParse(kingExpInput, out newKingExp)) {
                settings.kingAgeExp = newKingExp;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("LeaderAgeExp");
            leaderExpInput = GUILayout.TextField(leaderExpInput);
            int newLeaderExp;
            if(int.TryParse(leaderExpInput, out newLeaderExp)) {
                settings.leaderAgeExp = newLeaderExp;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("OtherAgeExp");
            otherExpInput = GUILayout.TextField(otherExpInput);
            int newOtherExp;
            if(int.TryParse(otherExpInput, out newOtherExp)) {
                settings.otherAgeExp = newOtherExp;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("BaseExpToLevel");
            expToLevel = GUILayout.TextField(expToLevel);
            int newExpToLevel;
            if(int.TryParse(expToLevel, out newExpToLevel)) {
                settings.baseExpToLevelup = newExpToLevel;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("ExpGainOnKill");
            expGainedOnKill = GUILayout.TextField(expGainedOnKill);
            int newExpGainOnKill;
            if(int.TryParse(expGainedOnKill, out newExpGainOnKill)) {
                settings.expGainOnKill = newExpGainOnKill;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("LevelExpScale");
            expScale = GUILayout.TextField(expScale);
            int newExpScale;
            if(int.TryParse(expScale, out newExpScale)) {

                settings.expToLevelUpScale = newExpScale;
            }
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Reset")) {
                expGainedOnKill = "10";
                kingExpInput = "20";
                leaderExpInput = "10";
                otherExpInput = "2";
                expToLevel = "100";
                expScale = "20";
            }
            GUI.DragWindow();
        }
        public GUIConstruction guiConstruction = new GUIConstruction();

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 75, 120, 30));
            if(GUILayout.Button("TWreck settings")) {
                showHideMainWindow = !showHideMainWindow;
            }
            GUILayout.EndArea();

            if(showHideMainWindow) {
                mainWindowRect = GUILayout.Window(79001, mainWindowRect, new GUI.WindowFunction(mainWindow), "Main", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            if(leaderDict.Count >= 1) {
                leadersWindowRect = GUILayout.Window(79003, leadersWindowRect, new GUI.WindowFunction(SquadLeadersWindow), "Leaders", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            if(lastInteractionActor != null) // showActorInteract && 
            {
                actorInteractWindowRect = GUILayout.Window(79002, actorInteractWindowRect, new GUI.WindowFunction(ActorInteractWindow), "Actor interact", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            if(showInventory) // show personal inventory
            {
                actorInventoryWindowRect = GUILayout.Window(79004, actorInventoryWindowRect, new GUI.WindowFunction(ActorInventoryWindow), "Inventory", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            guiConstruction.constructionWindowUpdate();
        }
        public static bool showInventory;
        public Rect actorInventoryWindowRect;

        public SquadFormation currentlySelectedFormation;

        public void SquadLeadersWindow(int windowID)
        {
            if(leaderDict.Count >= 1) {
                for(int i = 0; i < leaderDict.Count; i++) {
                    SquadFormation formation = leaderDict.Values.ToList()[i].squad;
                    Actor leaderActor = leaderDict.Values.ToList()[i].squadLeaderActor;
                    if(leaderActor != null) {
                        ActorStatus data = leaderActor.data;
                        if(data.alive) {
                            GUILayout.BeginHorizontal();
                            if(GUILayout.Button("Leader: " + data.firstName)) {
                                lastInteractionActor = leaderActor;
                            }
                            if(GUILayout.Button("Squad: " + formation.squadName)) {
                                currentlySelectedFormation = formation;

                            }
                            GUILayout.EndHorizontal();
                        }
                        else {
                        }
                    }
                    else {

                    }
                }
            }
            GUI.DragWindow();
        }

        public void ActorInventoryWindow(int windowID)
		{
            SetWindowInUse(windowID);
            if(controlledInventory != null) {
                GUILayout.Button("Wood: " + controlledInventory.wood.ToString());
                GUILayout.Button("Stone: " + controlledInventory.wood.ToString());
                GUILayout.Button("Ore: " + controlledInventory.wood.ToString());
                GUILayout.Button("Gold: " + controlledInventory.wood.ToString());
                if(GUILayout.Button("Berries:" + controlledInventory.berries.ToString())) {
                    if(controlledInventory.berries > 0) {
                        controlledInventory.berries--; // eat the berries
                        controlledActor.restoreStatsFromEating(20, 0.1f, true);
                    }
                }
                GUILayout.Button("Wheat: " + controlledInventory.wood.ToString());
            }
            GUI.DragWindow();
        }

        public Dictionary<Actor, PlayerInventory> actorInventories = new Dictionary<Actor, PlayerInventory>(); // to save mod inventories if player swaps actors for a bit

        public void ActorInteractWindow(int windowID)
        {
            SetWindowInUse(windowID);
            ActorStatus currentActorData = lastInteractionActor.data;
            if(controlledActor == null && lastInteractionActor != null) {
                if(GUILayout.Button("Take control of " + currentActorData.firstName)) {
                    controlledActor = lastInteractionActor;
					if(actorInventories.ContainsKey(controlledActor)) {
                        controlledInventory = actorInventories[controlledActor];
                    }
					else {
                        controlledInventory = new PlayerInventory();
                    }
                    MapBox.instance.canvas.gameObject.SetActive(false);
                }
            }
            if(controlledActor != null) {
                if(controlledActor == lastInteractionActor) {
                    if(GUILayout.Button("Stop control")) {
                        if(actorInventories.ContainsKey(controlledActor)) {
                            actorInventories[controlledActor] = controlledInventory;
                        }
                        else {
                            actorInventories.Add(controlledActor, controlledInventory);
                        }
                        controlledActor = null;
                        controlledInventory = null;
                    }
                }
                if(controlledActor != null) {
                    ActorStatus controlledActorData = controlledActor.data;
                    GUILayout.Button(controlledActorData.firstName);
                }
            }
            if(lastInteractionActor == controlledActor) {
               
            }

            if(currentlySelectedFormation != null) {
                GUILayout.Button("SquadName: " + currentlySelectedFormation.squadName);
                GUILayout.BeginHorizontal();
                nextSquadName = GUILayout.TextField(nextSquadName);
                if(GUILayout.Button("Set name")) {
                    currentlySelectedFormation.squadName = nextSquadName;
                }
                GUILayout.EndHorizontal();
                if(listOfSquadsWithLeaders.Contains(currentlySelectedFormation.squadID)) {
                    ActorStatus data = leaderDict[currentlySelectedFormation.squadID].squadLeaderActor.data;
                    GUILayout.Button("Leader: " + data.firstName);
                }
                else {
                    GUILayout.Button("Leader: " + "none");
                }
                if(lastInteractionActor != null && GUILayout.Button("Assign inspected to squad leader")) {
                    SquadLeader newLeader = new SquadLeader(lastInteractionActor);
                    newLeader.squad = currentlySelectedFormation;
                    if(listOfSquadsWithLeaders.Contains(currentlySelectedFormation.squadID)) {
                        leaderDict[currentlySelectedFormation.squadID] = newLeader;
                    }
                    else {
                        listOfSquadsWithLeaders.Add(currentlySelectedFormation.squadID);
                        leaderDict.Add(currentlySelectedFormation.squadID, newLeader);
                    }



                }
                GUILayout.Button("Unit count: " + currentlySelectedFormation.actorList.Count.ToString());
                if(GUILayout.Button("Formation shape: " + currentlySelectedFormation.formationType)) {
                    CycleFormation(currentlySelectedFormation);
                }
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("-")) {
                    currentlySelectedFormation.radius--;
                    if(currentlySelectedFormation.radius < 0) {
                        currentlySelectedFormation.radius = 0;
                    }
                }
                if(GUILayout.Button("Radius: " + currentlySelectedFormation.radius.ToString())) {
                    currentlySelectedFormation.radius = 3;
                }
                if(GUILayout.Button("+")) {
                    currentlySelectedFormation.radius++;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("-")) {
                    currentlySelectedFormation.lineX--;
                    if(currentlySelectedFormation.lineX < 0) {
                        currentlySelectedFormation.lineX = 0;
                    }
                }
                if(GUILayout.Button("LineX: " + currentlySelectedFormation.lineX.ToString())) {
                    currentlySelectedFormation.lineX = 5;
                }
                if(GUILayout.Button("+")) {
                    currentlySelectedFormation.lineX++;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("-")) {
                    currentlySelectedFormation.lineY--;
                    if(currentlySelectedFormation.lineY < 0) {
                        currentlySelectedFormation.lineY = 0;
                    }
                }
                if(GUILayout.Button("LineY: " + currentlySelectedFormation.lineY.ToString())) {
                    currentlySelectedFormation.lineY = 5;
                }
                if(GUILayout.Button("+")) {
                    currentlySelectedFormation.lineY++;
                }
                GUILayout.EndHorizontal();
                Color og = GUI.backgroundColor;
                if(currentlySelectedFormation.followsControlledPos) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("Toggle formation follows you")) {
                    currentlySelectedFormation.followsControlledPos = !currentlySelectedFormation.followsControlledPos;
                }
                if(currentlySelectedFormation.followsOffset) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("Toggle formation follows offset")) {
                    currentlySelectedFormation.followsOffset = !currentlySelectedFormation.followsOffset;
                }
                GUI.backgroundColor = og;
                GUI.backgroundColor = Color.red;
                if(GUILayout.Button("Reset hired list")) {
                    currentlySelectedFormation.actorList.RemoveRange(0, currentlySelectedFormation.actorList.Count);
                }
                GUI.backgroundColor = og;
            }

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("<-")) {
                squadDictPos--;
                if(squadDictPos <= 0) {
                    squadDictPos = 0;
                }
                if(squadsInUse.ContainsKey(squadDictPos.ToString()))
                    currentlySelectedFormation = squadsInUse[squadDictPos.ToString()];
            }
            if(GUILayout.Button("New squad") && lastTile != null) {
                SquadFormation newSquad = new SquadFormation();
                newSquad.movementPos = lastTile.posV3;
                squadsInUse.Add(newSquad.squadID, newSquad);
                currentlySelectedFormation = squadsInUse[newSquad.squadID];
            }
            if(GUILayout.Button("->")) {


                if(squadsInUse.ContainsKey(squadDictPos + 1.ToString())) {
                    squadDictPos++;
                    currentlySelectedFormation = squadsInUse[squadDictPos.ToString()];
                }
            }
            GUILayout.EndHorizontal();

            // hiring/firing squad stuff
            if(lastInteractionActor != controlledActor) {
                if(totalSquadActorList.Contains(lastInteractionActor)) {
                    if(GUILayout.Button("Fire " + currentActorData.firstName)) {
                        RemoveActorFromSquad(lastInteractionActor);
                    }
                }
                else {
                    if(GUILayout.Button("Hire " + currentActorData.firstName) && currentlySelectedFormation != null) {
                        AddActorToSquad(currentlySelectedFormation, lastInteractionActor);
                    }
                }
            }



            /*
            if (GUILayout.Button("Cycle power (Q)"))
            {
                CyclePower();
            }
            GUILayout.Button("Current power: " + powerInUse);
            */
            GUI.DragWindow();
        }

        public string nextSquadName = "";
        public bool assigningLeader;

        public static int squadDictPos = 0;

        public static void ClearDicts()
        {
            controlledActor = null;
            lastTile = null;
            leaderDict.Clear();
            listOfSquadsWithLeaders.Clear();
            squadsInUse.Clear();
            totalSquadActorList.Clear();
            squadDictPos = 0;
        }

        public static void loadData_Prefix(SavedMap pData)
        {
            ClearDicts();
        }

        public static void GenerateMap_Prefix(string pType = "islands")
        {
            ClearDicts();
        }

        public void HarmonyPatchSetup()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(ActorBase), "canAttackTarget");
            patch = AccessTools.Method(typeof(TWrecks_Main), "canAttackTarget_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(ActorBase), "stopMovement");
            patch = AccessTools.Method(typeof(TWrecks_Main), "stopMovement_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Actor), "getExpToLevelup");
            patch = AccessTools.Method(typeof(TWrecks_Main), "getExpToLevelup_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Actor), "updateAge");
            patch = AccessTools.Method(typeof(TWrecks_Main), "updateAge_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Actor), "increaseKillCount");
            patch = AccessTools.Method(typeof(TWrecks_Main), "increaseKillCount_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Actor), "addExperience");
            patch = AccessTools.Method(typeof(TWrecks_Main), "addExperience_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);


            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MoveCamera), "updateZoom");
            patch = AccessTools.Method(typeof(TWrecks_Main), "updateZoom_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(SaveManager), "loadData");
            patch = AccessTools.Method(typeof(TWrecks_Main), "loadData_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MapBox), "GenerateMap");
            patch = AccessTools.Method(typeof(TWrecks_Main), "GenerateMap_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MapBox), "canInspectUnitWithCurrentPower");
            patch = AccessTools.Method(typeof(TWrecks_Main), "canInspectUnitWithCurrentPower_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MoveCamera), "updateMouseCameraDrag");
            patch = AccessTools.Method(typeof(TWrecks_Main), "updateMouseCameraDrag_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            
            // auto retaliate feature, disabled because of enum error i cant find
            /*
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Actor), "getHit");
            patch = AccessTools.Method(typeof(TWrecks_Main), "getHit_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */
        }
        public static bool updateZoom_Prefix()
        {
            if(controlledActor != null) {
                if(Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E)) {
                    return false;
                }
            }
            return true;
        }

        // prevent inspection when clicking units when controlling
        public static bool canInspectUnitWithCurrentPower_Prefix()
        {
            if(controlledActor != null) {
                return false;
            }
            return true;
        }

        public void UpdateControls()
        {
            if(MapBox.instance != null) {
                if(guiConstruction != null) {
                    guiConstruction.constructionControl();
                }
                Camera camera = MapBox.instance.camera;
                if(camera != null) {
                    if(controlledActor != null) {
                        if(Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.KeypadPlus)) {
                            camera.GetComponent<MoveCamera>().cameraZoomSpeed = 5f;
                        }
                        else {
                            camera.GetComponent<MoveCamera>().cameraZoomSpeed = 0f;
                            camera.GetComponent<MoveCamera>().cameraMoveSpeed = 0f;
                        }
                    }
                    else {
                        camera.GetComponent<MoveCamera>().cameraMoveSpeed = 0.01f;
                        camera.GetComponent<MoveCamera>().cameraZoomSpeed = 5f;
                    }
                }

                if(MouseTile != null && Input.GetKeyDown(KeyCode.F)) {
                    lastInteractionActor = ClosestActorToTile(MouseTile, 3f);
                }

                if(controlledActor != null) {
                    followActor(controlledActor, 10f); // camera
                    movementStuff(); // movement controls

                    // click interaction (harvest resources, build structures)
                    WorldTile clickedTile = null;
                    if(Input.GetMouseButtonDown(0) && MapBox.instance.getMouseTilePos() != null) {
                        clickedTile = MapBox.instance.getMouseTilePos();
                        if(Toolbox.DistTile(clickedTile, lastTile) > 5f) return;
                        // building interactions
                        if(clickedTile.building != null) {
                            // Check building for resources
                            bool haveResources = clickedTile.building.haveResources;
                            BuildingAsset stats = clickedTile.building.stats;
                            BuildingData data = clickedTile.building.data;
                            CityData citydata = controlledActor.city.data;
                            if(haveResources && stats.resource_id == "berries") {
                                // If controlled unit is part of a city, add the resources to it's inventory
                                if(controlledActor.city != null) {
                                    controlledInventory.berries++; // personal food storage separate from city
                                    clickedTile.building.extractResources(controlledActor, clickedTile.building.stats.resources_given);
                                }
                                //controlledActor.timer_action = 1f; // delay after interact
                            }

                            // add progress to unfinished building
                            if(clickedTile.building.city != null && controlledActor.city != null && clickedTile.building.city == controlledActor.city && data.underConstruction) {
                                clickedTile.building.updateBuild(3);
                                //controlledActor.timer_action = 1f;
                                Sfx.play("hammer1", false, clickedTile.building.transform.localPosition.x, clickedTile.building.transform.localPosition.y);
                            }
                        }
                        // Non-building interactions (click)
                        else {

                        }

                    }

                    // combat interaction (melee only for now, need to determine what weapon is in-hand)
                    /*
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        Actor target = ClosestActorToTile(lastTile, controlledActor, 3f);
                        if (target != null)
                        {
                            AttackActor(controlledActor, target, 15f);
                        }
                    }
                    */
                    if(Input.GetKey(KeyCode.E)) {
                        Actor target = ClosestActorToTile(MouseTile, 3f);
                        if(target != null && target != controlledActor) {
                            try {
                                if((bool)controlledActor.tryToAttack(target)) {
                                    // Debug.Log("Attack success")
                                }
                            }
                            catch(Exception e) {
                                Debug.Log("exception caught");
                            }

                        }
                    }
                }
            }
        }

        public static void canAttackTarget_Postfix(BaseSimObject pTarget, ActorBase __instance, ref bool __result)
        {
            if(controlledActor == __instance) {
                __result = true;
            }
        }
        public static int formationRadius = 3;
        public void UpdateSquadBehaviour()
        {
            // for group behaviour, individual behaviours are applied through traits and updateSpecialTraitEffects_Postfix
            if(squadsInUse != null && squadsInUse.Count >= 1) {
                UpdateSquadList();
                //MoveSquad(hiredActorList, lastTile.posV3, formationInUse, formationRadius); // single squad/old movement
                foreach(SquadFormation activeSquad in squadsInUse.Values) {
                    if(listOfSquadsWithLeaders.Contains(activeSquad.squadID) == false) {
                        MoveSquad(activeSquad);
                    }
                    else {
                        MoveSquad(leaderDict[activeSquad.squadID]);
                    }
                    // need to update the list somewhere and clear dead/nulls
                }
                if(Input.GetKey(KeyCode.E)) {
                    Actor target = ClosestActorToTile(MouseTile, 3f);
                    if(target != null) {
                        foreach(Actor squadMate in totalSquadActorList) {
                            try {
                                if((bool)squadMate.tryToAttack(target)) {
                                    // Debug.Log("Attack success")
                                }
                            }
                            catch(Exception e) {
                                Debug.Log("exception caught");
                            }
                        }
                    }


                }
            }
        }

        public int xLimit = 6;
        public int yLimit = 6;
        public static Dictionary<string, SquadFormation> squadsInUse = new Dictionary<string, SquadFormation>();
        public void MoveSquad(SquadFormation targetFormation)
        {
            Vector3 targetPos = lastTile.posV3;
            if(targetFormation.followsControlledPos == false) {
                targetPos = targetFormation.movementPos;
            }
            if(targetFormation.offsetPos != null && targetFormation.followsOffset && targetFormation.offsetPos != Vector3.one) {
                targetPos = lastTile.posV3 + targetFormation.offsetPos;
            }
            switch(targetFormation.formationType) {
                case "circle":
                    moveFormationCircle(targetFormation.actorList, targetPos, targetFormation.radius);
                    break;
                case "wedge1":
                    moveFormationWedge1(targetFormation.actorList, targetPos);
                    break;
                case "wedge2":
                    moveFormationWedge2(targetFormation.actorList, targetPos);
                    break;
                case "hline":
                    moveFormationLineNonCentered(targetFormation.actorList, targetPos, targetFormation.lineX, 1);
                    break;
                case "vline":
                    moveFormationLineNonCentered(targetFormation.actorList, targetPos, 1, targetFormation.lineY);
                    break;
                case "rect":
                    moveFormationLineNonCentered(targetFormation.actorList, targetPos, targetFormation.lineX, targetFormation.lineY);
                    break;
                case "dot":
                    moveFormationDot(targetFormation.actorList, targetPos);
                    break;
                default:
                    break;
            }
        }

        public void MoveSquad(SquadLeader leaderAndHisFormation)
        {
            if(leaderAndHisFormation.squad != null) {
                if(leaderAndHisFormation.squadLeaderActor == null && leaderAndHisFormation.squad.actorList.Count >= 2) {
                    Debug.Log("Squad leader likely dead/null, replacing (removes 1 actor from the list)");
                    leaderAndHisFormation.squadLeaderActor = leaderAndHisFormation.squad.actorList.GetRandom();
                    leaderAndHisFormation.squad.actorList.Remove(leaderAndHisFormation.squadLeaderActor);
                }
                SquadFormation targetFormation = leaderAndHisFormation.squad;
                Vector3 targetPos = leaderAndHisFormation.squadLeaderActor.currentTile.posV3;
                if(targetFormation.followsControlledPos == false) {
                    targetPos = targetFormation.movementPos;
                }
                if(targetFormation.offsetPos != null && targetFormation.followsOffset && targetFormation.offsetPos != Vector3.one) {
                    targetPos = leaderAndHisFormation.squadLeaderActor.currentTile.posV3 + targetFormation.offsetPos;
                }
                switch(targetFormation.formationType) {
                    case "circle":
                        moveFormationCircle(targetFormation.actorList, targetPos, targetFormation.radius);
                        break;
                    case "wedge1":
                        moveFormationWedge1(targetFormation.actorList, targetPos);
                        break;
                    case "wedge2":
                        moveFormationWedge2(targetFormation.actorList, targetPos);
                        break;
                    case "hline":
                        moveFormationLineNonCentered(targetFormation.actorList, targetPos, targetFormation.lineX, 1);
                        break;
                    case "vline":
                        moveFormationLineNonCentered(targetFormation.actorList, targetPos, 1, targetFormation.lineY);
                        break;
                    case "rect":
                        moveFormationLineNonCentered(targetFormation.actorList, targetPos, targetFormation.lineX, targetFormation.lineY);
                        break;
                    case "dot":
                        moveFormationDot(targetFormation.actorList, targetPos);
                        break;
                    default:
                        break;
                }
            }

        }

        public List<string> formationType = new List<string>();
        public string formationInUse = "circle";
        public void CycleFormation(SquadFormation targetSquad)
        {
            switch(targetSquad.formationType) {
                case "circle":
                    targetSquad.formationType = "wedge1"; // wedge
                    break;
                case "wedge1":
                    targetSquad.formationType = "wedge2";
                    break;
                case "wedge2":
                    targetSquad.formationType = "hline";
                    break;
                case "hline":
                    targetSquad.formationType = "vline";
                    break;
                case "vline":
                    targetSquad.formationType = "rect";
                    break;
                case "rect":
                    targetSquad.formationType = "dot";
                    break;
                case "dot":
                    targetSquad.formationType = "circle";
                    break;
                default:
                    break;
            }
        }

        public static List<ActorTrait> positiveTraitsForLevels => AssetManager.traits.list.OrderBy(x => x.type == TraitType.Positive).ToList();

        public void UpdateSquadList()
        {
            if(squadsInUse != null && squadsInUse.Count >= 1) {
                for(int i = 0; i < squadsInUse.Count; i++) {
                    List<Actor> workingList = squadsInUse.Values.ToList()[i].actorList;
                    if(workingList != null && workingList.Count >= 1) {
                        for(int j = 0; j < workingList.Count; j++) {
                            if(workingList[j] == null) {
                                workingList.Remove(workingList[j]);
                            }
                            else {
                                ActorStatus data = workingList[j].data;
                                if(data.alive == false) {
                                    workingList.Remove(workingList[j]);
                                }
                            }
                        }
                    }
                }
            }

            // check if anyone is dead or null and make sure theyre not in the list

        }

        public Actor lastInteractionActor;

        public void AttackActor(Actor attacker, Actor target, float damage)
        {
            attacker.punchTargetAnimation(target.currentPosition, target.currentTile, true, false, 40f);
            Sfx.play("punch", true, attacker.currentPosition.x, attacker.currentPosition.y);
            target.getHit(damage, true, AttackType.Other, null, true);
        }

        public static List<Actor> totalSquadActorList = new List<Actor>();
        public void AddActorToSquad(SquadFormation targetSquad, Actor targetActor)
        {
            if(targetSquad.actorList.Contains(targetActor) == false)
                targetSquad.actorList.Add(targetActor);
            if(totalSquadActorList.Contains(targetActor) == false)
                totalSquadActorList.Add(targetActor);
        }
        public void RemoveActorFromSquad(Actor targetActor)
        {
            if(squadsInUse != null && squadsInUse.Count >= 1) {
                for(int i = 0; i < squadsInUse.Count; i++) {
                    List<Actor> workingList = squadsInUse.Values.ToList()[i].actorList;
                    if(workingList != null && workingList.Count >= 1) {
                        for(int j = 0; j < workingList.Count; j++) {
                            if(workingList[j] == targetActor) {
                                workingList.Remove(workingList[j]);
                                return;
                            }
                        }
                    }
                }
            }
            if(totalSquadActorList.Contains(targetActor)) {
                totalSquadActorList.Remove(targetActor);
            }
        }

        public static Actor ClosestActorToTile(WorldTile pTarget, float range)
        {
            Actor returnActor = null;
            foreach(Actor actorToCheck in MapBox.instance.units) {
                if(controlledActor != null && actorToCheck != controlledActor) // exclude hired actors if they exist
                {
                    if(totalSquadActorList != null && totalSquadActorList.Count >= 1) {
                        if(totalSquadActorList.Contains(actorToCheck) == false) {
                            float actorDistanceFromTile = Toolbox.Dist(actorToCheck.currentPosition.x, actorToCheck.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                            if(actorDistanceFromTile < range) {
                                range = actorDistanceFromTile;
                                returnActor = actorToCheck;
                            }
                        }
                    }
                    else {
                        float actorDistanceFromTile = Toolbox.Dist(actorToCheck.currentPosition.x, actorToCheck.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                        if(actorDistanceFromTile < range) {
                            range = actorDistanceFromTile;
                            returnActor = actorToCheck;
                        }
                    }
                }
                else // if hired actors dont exist, only exclude the controlled guy
                {
                    float actorDistanceFromTile = Toolbox.Dist(actorToCheck.currentPosition.x, actorToCheck.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                    if(actorDistanceFromTile < range) {
                        range = actorDistanceFromTile;
                        returnActor = actorToCheck;
                    }
                }
            }
            return returnActor;
        }

        public static void moveFormationDot(List<Actor> formation, Vector3 position)
        {
            if(formation != null) {
                float num = 6.28318548f / (float)formation.Count;
                for(int i = 0; i < formation.Count; i++) {
                    float f = (float)i * num;
                    Vector3 vector = position; // the radius * count/10f makes the radius grow slightly the more people are used0
                    WorldTile tileFromVector = MapBox.instance.GetTile((int)vector.x, (int)vector.y);
                    if(tileFromVector == null) {
                        Debug.Log("Movement tile not inside world, breaking");
                        return;
                    }
                    BasicMoveAndWait(formation[i], tileFromVector);
                    if(useFlashOnMove)
                        flashEffects.flashPixel(tileFromVector, 10, ColorType.White);
                    if(formation[i].GetComponent<Boat>() != null) // boat movement
                    {
                        formation[i].nextStepPosition = vector;
                    }

                }
            }
        }

        public static bool useFlashOnMove = true;
        public static void moveFormationCircle(List<Actor> formation, Vector3 position, int radius)
        {
            if(formation != null) {
                float num = 6.28318548f / (float)formation.Count;
                for(int i = 0; i < formation.Count; i++) {
                    float f = (float)i * num;
                    Vector3 vector = position + new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * ((float)radius + ((float)formation.Count / 10f)); // the radius * count/10f makes the radius grow slightly the more people are used
                    WorldTile tileFromVector = MapBox.instance.GetTile((int)vector.x, (int)vector.y);
                    if(tileFromVector == null) {
                        Debug.Log("Movement tile not inside world, breaking");
                        return;
                    }
                    BasicMoveAndWait(formation[i], tileFromVector);
                    if(useFlashOnMove)
                        flashEffects.flashPixel(tileFromVector, 10, ColorType.White);
                    if(formation[i].GetComponent<Boat>() != null) // boat movement
                    {
                        formation[i].nextStepPosition = vector;
                    }

                }
            }
        }

        public static void moveFormationWedge1(List<Actor> formation, Vector3 position)
        {
            Vector3 posV = position;
            int x = -(formation.Count / 2);
            int y = -(formation.Count / 2);
            foreach(Actor actor in formation) {
                WorldTile tileFromVector = MapBox.instance.GetTile((int)position.x + x, (int)position.y + y);
                BasicMoveAndWait(actor, tileFromVector);

                if(y != x) {
                    y++;
                }
                else {
                    x++;
                }
            }
        }
        public static void moveFormationWedge2(List<Actor> formation, Vector3 position)
        {
            Vector3 posV = position;
            int num = formation.Count / 2;
            int num2 = formation.Count / 2;
            foreach(Actor actor in formation) {
                WorldTile tileFromVector = MapBox.instance.GetTile((int)position.x - num, (int)position.y + num2);
                BasicMoveAndWait(actor, tileFromVector);

                if(num2 != num) {
                    num2--;
                }
                else {
                    num--;
                }
            }
        }
        public static bool lineFormationCentered = true;
        public void lineFormation(List<Actor> formation, Vector3 position, int xLimit, int yLimit) // lines, squares, rectangles
        {
            if(lineFormationCentered) {
                moveFormationLineNonCentered(formation, position, xLimit, yLimit);
            }
            else {

            }
        }
        public static void moveFormationLineNonCentered(List<Actor> formation, Vector3 position, int xLimit, int yLimit) // lines, squares, rectangles
        {
            int positionX = 0;
            int positionY = 0;
            if(yLimit == 1) {
                positionX = 1;
            }
            if(xLimit == 1) {
                positionY = 1;
            }

            for(int i = 0; i < formation.Count; i++) {
                if(positionX == xLimit) {
                    positionY++;
                    positionX = 0;
                }
                if(positionY == yLimit) {
                    positionY = 0;
                    positionX = 0;
                }
                Vector3 vector = new Vector3(position.x + positionX, position.y + positionY);
                WorldTile targetTile = MapBox.instance.GetTile((int)vector.x, (int)vector.y);
                if(targetTile == null) {
                    Debug.Log("Movement tile not inside world, breaking");
                    return;
                }
                BasicMoveAndWait(formation[i], targetTile);
                if(useFlashOnMove)
                    flashEffects.flashPixel(targetTile, 10, ColorType.White);
                if(formation[i].GetComponent<Boat>() != null) // boat movement
                {
                    formation[i].nextStepPosition = vector;
                }
                positionX++;
            }
        }
        public static void moveFormationLineCentered(List<Actor> formation, Vector3 position, int xLimit, int yLimit) // lines, squares, rectangles
        {
            // needs work
            int positionX = 0;
            int positionY = 0;

            int offsetX = 0;
            int offsetY = 0;
            if(xLimit != 1) {
                offsetX = (-(xLimit));
            }
            if(yLimit != 1) {
                offsetY = (-(yLimit));

            }

            for(int i = 0; i < formation.Count; i++) {
                if(positionX == (xLimit / 2)) {
                    positionY++;
                    positionX = 0;
                }
                if(positionY == (yLimit / 2)) {
                    positionY = 0;
                    positionX = 0;
                }
                Vector3 vector = new Vector3(position.x + positionX, position.y + positionY);
                WorldTile targetTile = MapBox.instance.GetTile((int)vector.x, (int)vector.y);
                if(targetTile == null) {
                    Debug.Log("Movement tile not inside world, breaking");
                    return;
                }
                BasicMoveAndWait(formation[i], targetTile);
                if(useFlashOnMove)
                    flashEffects.flashPixel(targetTile, 10, ColorType.White);
                if(formation[i].GetComponent<Boat>() != null) // boat movement
                {
                    formation[i].nextStepPosition = vector;
                }
                positionX++;
            }
        }


        public void followActor(Actor target, float speed = 5f)
        {
            Vector3 posV = target.currentPosition;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, posV, speed * Time.deltaTime);
        }

        private void movementStuff()
        {
            // RPG movement
            lastTile = MapBox.instance.GetTileSimple((int)controlledActor.currentPosition.x, (int)controlledActor.currentPosition.y);
            if(Input.GetKey(KeyCode.W)) {
                directionMoving = "up";
                if(Input.GetKey(KeyCode.A)) {
                    directionMoving = "upLeft";
                }
                else if(Input.GetKey(KeyCode.D)) {
                    directionMoving = "upRight";
                }
            }
            else if(Input.GetKey(KeyCode.S)) {
                directionMoving = "down";
                if(Input.GetKey(KeyCode.A)) {
                    directionMoving = "downLeft";
                }
                else if(Input.GetKey(KeyCode.D)) {
                    directionMoving = "downRight";
                }
            }
            else if(Input.GetKey(KeyCode.A)) {
                directionMoving = "left";
                if(Input.GetKey(KeyCode.W)) {
                    directionMoving = "upLeft";
                }
                else if(Input.GetKey(KeyCode.S)) {
                    directionMoving = "downLeft";
                }
            }
            else if(Input.GetKey(KeyCode.D)) {
                directionMoving = "right";
                if(Input.GetKey(KeyCode.W)) {
                    directionMoving = "upRight";
                }
                else if(Input.GetKey(KeyCode.S)) {
                    directionMoving = "downRight";
                }
            }
            else {
                directionMoving = null;
            }
            // when user isnt moving (or interacting?), make controlled be still
            // could add some kind of command queue
            if(directionMoving == null) {
                controlledActor.stopMovement();
                controlledActor.cancelAllBeh();
            }
            if(directionMoving != null) {
                WorldTile movementTile;
                switch(directionMoving) {
                    case "up":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x, lastTile.pos.y + 1);
                        doMovement(movementTile);
                        break;
                    case "down":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x, lastTile.pos.y - 1);
                        doMovement(movementTile);
                        break;
                    case "left":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x - 1, lastTile.pos.y);
                        doMovement(movementTile);
                        break;
                    case "right":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x + 1, lastTile.pos.y);
                        doMovement(movementTile);
                        break;
                    case "upLeft":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x - 1, lastTile.pos.y + 1);
                        doMovement(movementTile);
                        break;
                    case "upRight":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x + 1, lastTile.pos.y + 1);
                        doMovement(movementTile);
                        break;
                    case "downLeft":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x - 1, lastTile.pos.y - 1);
                        doMovement(movementTile);
                        break;
                    case "downRight":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x + 1, lastTile.pos.y - 1);
                        doMovement(movementTile);
                        break;
                    default:
                        break;
                }
            }

        }

        public void doMovement(WorldTile targetTile)
        {
            // strict movement
			if(targetTile.Type.liquid) { // moving into/out of water is weird.. add for blocking building:  || targetTile.building != null
                return;
            }
            lastTile = targetTile;
            BasicMoveAndWait(controlledActor, targetTile);
        }

        public static PixelFlashEffects flashEffects => MapBox.instance.flashEffects;

        public static bool addExperience_Prefix(int pValue, Actor __instance)
        {
            ActorStats stats = __instance.stats;
            ActorStatus data = __instance.data;
            if(stats.canLevelUp) {
                int expToLevelup = __instance.getExpToLevelup();
                data.experience += pValue;
                if(data.experience >= expToLevelup) {
                    data.experience = 0;
                    data.level++;
                    if(__instance == controlledActor) {
                        if(data.level % 5 == 0) {
                            ActorTrait newTrait = null;
                            int attempts = 0;
                            while((newTrait == null || controlledActor.haveTrait(newTrait.id)) && attempts < 3) {
                                newTrait = positiveTraitsForLevels.GetRandom();
                                attempts++;
                            }
                            controlledActor.addTrait(newTrait.id);
                        }
                    }
                    string trait = AssetManager.traits.list.GetRandom().id;
                    __instance.addTrait(trait);
                    __instance.removeTrait(trait);
                    BaseStats curStats = __instance.curStats;
                    __instance.restoreHealth(curStats.health);
                }
            }
            return false;
        }

        // auto retaliate, known to cause errors but balances out players relentless killing innocents
        // maybe filter to only retaliate/do something else if people hit "innocent" actor
        public static void getHit_Postfix(float pDamage, bool pFlash, AttackType pType, BaseSimObject pAttacker, bool pSkipIfShake, Actor __instance)
        {
            ActorStatus victimData = null;

            ActorStatus attackerData = null;

            if(__instance != null) {
                victimData = __instance.data;
            }
            if(pAttacker != null) {
                //attackerData = pAttacker.data; // was data, probably null
            }
            if(attackerData != null && victimData != null && victimData.alive && attackerData.alive) {
                try {
                    if(Toolbox.randomBool() && Toolbox.randomBool() && (bool)__instance.tryToAttack(pAttacker)) {
                        // Debug.Log("Retaliation/recoil success")
                    }
                }
                catch(Exception e) {
                    Debug.Log("exception caught");
                }

            }
        }

        public static void getExpToLevelup_Postfix(Actor __instance, ref int __result)
        {
            ActorStatus data = __instance.data;

            __result = settings.baseExpToLevelup + (data.level - 1) * settings.expToLevelUpScale;
            /* heal controlled actor on level up?
            if (__instance == controlledActor)
            {
                BaseStats curStats = Reflection.GetField(controlledActor.GetType(), controlledActor, "curStats") as BaseStats;
                controlledActor.restoreHealth(curStats.health / 10); // 10%
            }
            */
        }

        public static bool updateMouseCameraDrag_Prefix()
        {
            if(controlledActor != null) {
                return false;
            }
            else {
                return true;
            }
        }
        public static bool increaseKillCount_Prefix(Actor __instance)
        {
            ActorStatus data = __instance.data;

            data.kills++;
            if(data.kills > 10) {
                __instance.addTrait("veteran");
            }
            __instance.addExperience(settings.expGainOnKill);

            if(controlledActor != null) {
                BaseStats curStats = controlledActor.curStats;
                if(__instance == controlledActor) {
                    // heal on kill, kinda op
                    controlledActor.restoreHealth(curStats.health / 10); // 10%
                }
            }
            return false;
        }

        public static ModSettings settings;

        //initial setup and loading
        public void SettingSetup()
        {
            Debug.Log("Settings start");
            //copied and only slightly changed from igniz's traitbonanza
            string path = Path.Combine(Application.streamingAssetsPath, "Mods");
            string text = Path.Combine(path, pluginGuid + ".json");
            bool flag = Application.platform == RuntimePlatform.WindowsPlayer;
            if(flag) {
                text = text.Replace("\\", "/");
            }

            bool fileExist = File.Exists(text);
			if(fileExist) {
                string value = File.ReadAllText(text);
                settings = JsonConvert.DeserializeObject<ModSettings>(value);
                Debug.Log("loaded saved settings");
            }
            else{//file doesnt exist, setup defaults
                settings = new ModSettings();
                string text2 = JsonConvert.SerializeObject(settings, Formatting.Indented);
                Debug.Log("created and saved default");
                File.WriteAllText(text, text2);
            }
            Debug.Log("Settings end");
            // why do i do all of this with strings? //for menu input/display??
            expGainedOnKill = settings.expGainOnKill.ToString();
            kingExpInput = settings.kingAgeExp.ToString();
            leaderExpInput = settings.leaderAgeExp.ToString();
            otherExpInput = settings.otherAgeExp.ToString();
            expToLevel = settings.baseExpToLevelup.ToString();
            expScale = settings.expToLevelUpScale.ToString();
        }
        //standalone methods for calling later if needed
        public void saveSettings()
		{
            string path = Path.Combine(Application.streamingAssetsPath, "Mods");
            string text = Path.Combine(path, pluginGuid + ".json");
            string text2 = JsonConvert.SerializeObject(settings, Formatting.Indented);
            Debug.Log("saved settings");
            File.WriteAllText(text, text2);
        }
        public void loadSettings()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Mods");
            string text = Path.Combine(path, pluginGuid + ".json");
            string value = File.ReadAllText(text);
            settings = JsonConvert.DeserializeObject<ModSettings>(value);
            Debug.Log("loaded settings");
        }

        //public static int expGainOnKill = 10;
        //public static int kingAgeExp = 20;
        //public static int leaderAgeExp = 10;
        //public static int otherAgeExp = 2;
        //public static int baseExpToLevelup = 100; 
        // default: 100 + (this.data.level - 1) * 20;
        //public static int expToLevelUpScale = 20;

        public static bool updateAge_Prefix(Actor __instance)
        {
            ActorStatus data = __instance.data;
            int currentAge = data.age;
            Race race = __instance.race;
            bool updateAge = data.updateAge(race);
            if(!updateAge && !__instance.haveTrait("immortal")) {
                __instance.killHimself(false, AttackType.Age, true, true);
                return false;
            }
            int currentAge2 = data.age;
            //Debug.Log("\nDebug: old age: " + currentAge + "\n" + "Debug: new age: " + currentAge2);
            if(__instance.city != null) {
                Kingdom kingdom = __instance.city.kingdom;
                if(kingdom != null) {
                    if(kingdom.king == __instance) {
                        __instance.addExperience(settings.kingAgeExp);
                    }
                }
                if(__instance.city.leader == __instance) {
                    __instance.addExperience(settings.leaderAgeExp);
                }
                if(
                    (kingdom == null || (kingdom != null && kingdom.king != __instance))
                    &&
                    (__instance.city == null || (__instance.city != null && __instance.city.leader != __instance))
                    ) {
                    __instance.addExperience(settings.otherAgeExp);
                }
            }
            if(data.age > 40 && Toolbox.randomChance(0.3f)) {
                __instance.addTrait("wise");
            }
            return false;
        }

        public static bool stopMovement_Prefix(ActorBase __instance) // stop controlled actor canceling movement from AI behaviour
        {
            if((controlledActor != null && __instance == controlledActor) || totalSquadActorList.Contains(__instance)) {
                if(directionMoving != null) {
                    return false;
                }
            }
            return true;
        }

        public static void BasicMoveAndWait(Actor targetActor, WorldTile targetTile)
        {
            // simple way of making an actor move the way i want
            targetActor.stopMovement();
            targetActor.cancelAllBeh();
            targetActor.moveTo(targetTile);
            if(useFlashOnMove)
                flashEffects.flashPixel(targetTile, 10, ColorType.White);
            AiSystemActor actorAI = targetActor.ai;
            actorAI.setTask("wait10", false, true);
        }

        public static void SetWindowInUse(int windowID)
        {
            Event current = Event.current;
            bool inUse = current.type == EventType.MouseDown || current.type == EventType.MouseUp || current.type == EventType.MouseDrag || current.type == EventType.MouseMove;
            if(inUse) {
                windowInUse = windowID;
            }
        }

        // click-through prevention for menus
        public static void isActionHappening_Postfix(ref bool __result)
        {
            if(windowInUse != -1) {
                __result = true; // "menu in use" is the action happening
            }
        }

        // cancel all control input if a window is in use
        public static bool updateControls_Prefix()
        {
            if(windowInUse != -1) {
                return false;
            }
            return true;
        }

        // cancel empty click usage when windows in use, unused right now
        public static bool checkEmptyClick_Prefix()
        {
            if(windowInUse != -1) {
                return false;
            }
            return true;
        }

        public WorldTile MouseTile => MapBox.instance.getMouseTilePos();

        public static string directionMoving = null;

        public static Actor controlledActor = null;
        public static WorldTile lastTile = null;

        public static int windowInUse = 0;

        public static PlayerInventory controlledInventory; // currently controlled actor's custom inventory
    }

    public class PlayerInventory {
        public int wood = 0;
        public int stone = 0;
        public int ore = 0;
        public int gold = 0;
        public int berries = 0;
        public int wheat = 0;

        public int bread = 0; //not a real item, can make it out of wheat though
        public int eggs = 0; //interact with chicken, chickens need dict and cooldown
        public int milk = 0; // interact with cow
	}

    [JsonObject(MemberSerialization.OptIn)]
    public class ModSettings {
        // excluded from serialization
        // does not have JsonPropertyAttribute
        public Guid Id { get; set; }

        [JsonProperty]
        public string Name = "test";

        [JsonProperty]
        public int Size = 10;

        [JsonProperty]
        public int expGainOnKill = 10;

        [JsonProperty]
        public int kingAgeExp = 20;

        [JsonProperty]
        public int leaderAgeExp = 10;

        [JsonProperty]
        public int otherAgeExp = 2;

        [JsonProperty]
        public int baseExpToLevelup = 100;
        // default: 100 + (this.data.level - 1) * 20;

        [JsonProperty]
        public int expToLevelUpScale = 20;
    }
}
