using System.Collections.Generic;
using UnityEngine;
using BodySnatchers;
using SimplerGUI.Submods.SimpleMessages;
using SimplerGUI.Submods;
using FMOD;
using static UnityEngine.UI.Image;
using System;
using Debug = UnityEngine.Debug;
using static UnityEngine.GraphicsBuffer;
using Amazon.Runtime;
using System.Linq;
using UnityEngine.Tilemaps;
using Amazon.Runtime.Internal.Transform;
using System.Reflection.Emit;
using SimpleJSON;
using SimplerGUI.Submods.UnitClipboard;
using SimplerGUI.Submods.MapSizes;

#pragma warning disable CS0649

namespace SimplerGUI.Menus
{
    public class ActorControlMain : MonoBehaviour
    {
        public bool settingUpControl;
        // another shortcut
        public static Actor controlledActorSc => BodySnatchers.ControlledActor.GetActor();
        public static Squad squadSc => BodySnatchers.Main.squad;

        public bool settingUpHire;

        //singular escort for following player
        public bool settingUpEscort;
        public static Actor actorBeingEscorted;

        //mass escort, each squad member can have a follower
        public bool settingUpMassEscort;
        //key:actor following, value: actor to follow
        public static Dictionary<Actor, Actor> escortsAndTheirClients = new Dictionary<Actor, Actor>();
        public string lastTooltip = "";
        Color originalColor = GUI.backgroundColor;

        public void actorControlWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            if (controlledActorSc != null)
            {
                if (GUILayout.Button(new GUIContent("ControlledName: " + controlledActorSc.data.name, "Name of the actor being controlled")))
                {
                    BodySnatchers.ControlledActor.SetActor(null);
                }
                if (settingUpHire) { GUI.backgroundColor = Color.yellow; }
                else { GUI.backgroundColor = originalColor; }
                if (GUILayout.Button(new GUIContent("Hire target", "Click to begin process, then click a target in the world")))
                {
                    settingUpHire = !settingUpHire;
                }
                GUI.backgroundColor = originalColor;
            }
            else
            {
                if (settingUpControl) { GUI.backgroundColor = Color.yellow; }
                else { GUI.backgroundColor = originalColor; }
                if (GUILayout.Button("Start control"))
                {
                    settingUpControl = !settingUpControl;
                }
            }
            if (preventClicksOpeningWindows) { GUI.backgroundColor = Color.yellow; }
            else { GUI.backgroundColor = originalColor; }
            if (GUILayout.Button("Attack with left click"))
            {
                preventClicksOpeningWindows = !preventClicksOpeningWindows;
            }
            GUI.backgroundColor = originalColor;
            if (GUILayout.Button("Clear escorts"))
            {
                actorBeingEscorted = null;
                escortsAndTheirClients = new Dictionary<Actor, Actor>();
            }
            if (showSingleActionWindow) { GUI.backgroundColor = Color.yellow; }
            else { GUI.backgroundColor = originalColor; }
            if (GUILayout.Button("Actor actions"))
            {
                showSingleActionWindow = !showSingleActionWindow;
            }
            if (showSquadActionWindow) { GUI.backgroundColor = Color.yellow; }
            else { GUI.backgroundColor = originalColor; }
            if (GUILayout.Button("Squad actions"))
            {
                showSquadActionWindow = !showSquadActionWindow;
            }
            if (showSquadControlWindow) { GUI.backgroundColor = Color.yellow; }
            else { GUI.backgroundColor = originalColor; }
            if (GUILayout.Button("Squad control"))
            {
                showSquadControlWindow = !showSquadControlWindow;
            }
            if (GUILayout.Button("Survivor"))
            {
                showSurvivorWindow = !showSurvivorWindow;
            }
            if (GUILayout.Button("JoyTest: " + isJoyEnabled.ToString()))
            {
                isJoyEnabled = !isJoyEnabled;
                if (isJoyEnabled)
                {
                    World.world.joys.SetActive(true);//UltimateJoystick.EnableJoystick("JoyLeft");
                }
                else
                {
                    World.world.joys.SetActive(false);//UltimateJoystick.DisableJoystick("JoyLeft");
                }
                UltimateJoystick.ResetJoysticks();
                UltimateJoystick.DisableJoystick("JoyLeft");
                UltimateJoystick.EnableJoystick("JoyRight");

            }
            GUI.DragWindow();
        }

        public static bool isJoyEnabled;

        public void actorSingleActionWindow(int windowID)
        {
            if (settingUpEscort) { GUI.backgroundColor = Color.yellow; }
            else { GUI.backgroundColor = originalColor; }
            if (GUILayout.Button(new GUIContent("Start escort", "Click to begin process, then click a target in the world")))
            {
                settingUpEscort = !settingUpEscort;
            }
            if (actorBeingEscorted != null)
            {
                GUILayout.Button("Escorted Name: " + actorBeingEscorted.data.name);
                if (GUILayout.Button("Kill escorted"))
                {
                    actorBeingEscorted.getHit(1000000);
                    actorBeingEscorted = null;
                }
                if (GUILayout.Button("Convert escorted"))
                {
                    //apex converted this to bool, can check if successful and do new action later
                    BodySnatchers.Main.squad.HireActor(actorBeingEscorted);
                    actorBeingEscorted = null;
                }
                if (GUILayout.Button("Make escorted speak"))
                {
                    if (Messages.actorStartPhrases.ContainsKey(actorBeingEscorted.asset.id))
                    {
                        string phraseToSay = Messages.actorStartPhrases[actorBeingEscorted.asset.id].starterPhrases.GetRandom();
                        Messages.ActorSay(actorBeingEscorted, phraseToSay, 3);
                    }
                }
            }
        }

        public void actorSquadActionWindow(int windowID)
        {
            if (controlledActorSc != null && squadSc != null)
            {
                if (squadSc.squad.Count > 0)
                {
                    GUILayout.Button("Squad count: " + squadSc.squad.Count);
                }
                //once we have more actions to mess with, move to its own window and parent to this one

                //recruit action??
                if (GUILayout.Button("Dig test"))
                {
                    squadSc.squadAction = new SquadAction(squadActions.FindTilesToDig);
                }
                if (GUILayout.Button("Single escort"))
                {
                    //currently random selection and gross code
                    Actor a = SquadActorWithoutEscort();
                    if (a != null)
                    {
                        singleActorActions.StartSingleEscort(a);
                    }
                }
                if (GUILayout.Button("Mass escort"))
                {
                    squadSc.squadAction = new SquadAction(squadActions.StartMassEscort);
                }

                if (GUILayout.Button("SacrificialSpell"))
                {
                    squadSc.squadAction = new SquadAction(squadActions.StartSacrificeSpell);
                }

                //way to stop actions that accidentally (or not) loop
                if ((squadSc.squadAction != null || squadSc.squadAction != null))
                {
                    if (GUILayout.Button("Stop SquadAction"))
                    {
                        squadSc.squadAction = null;
                        squadSc.nextSquadAction = null;
                    }
                }
            }
        }

        public void actorSquadControlWindow(int windowID)
        {
            if (controlledActorSc != null && squadSc != null)
            {
                //buttons to change x/y, formation type, squad action (wait/follow/roam)
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("X-1"))
                {
                    squadSc.IncrementLineX(-1);
                }
                if (GUILayout.Button("X+1"))
                {
                    squadSc.IncrementLineX(1);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Y-1"))
                {
                    squadSc.IncrementLineY(-1);
                }
                if (GUILayout.Button("Y+1"))
                {
                    squadSc.IncrementLineY(1);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (squadSc.formation == FormationType.Dot) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Dot"))
                {
                    squadSc.formation = FormationType.Dot;
                }
                if (squadSc.formation == FormationType.Line) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Line"))
                {
                    squadSc.formation = FormationType.Line;
                }
                if (squadSc.formation == FormationType.CenteredLine) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("CenteredLine"))
                {
                    squadSc.formation = FormationType.CenteredLine;
                }
                if (squadSc.formation == FormationType.Rectangle) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Rectangle"))
                {
                    squadSc.formation = FormationType.Rectangle;
                }
                if (squadSc.formation == FormationType.Circle) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Circle"))
                {
                    squadSc.formation = FormationType.Circle;
                }
                GUI.backgroundColor = originalColor;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (squadSc.action == FormationAction.Wait) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Wait"))
                {
                    squadSc.action = FormationAction.Wait;
                }
                if (squadSc.action == FormationAction.Follow) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Follow"))
                {
                    squadSc.action = FormationAction.Follow;
                }
                if (squadSc.action == FormationAction.Roam) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Roam"))
                {
                    squadSc.action = FormationAction.Roam;
                }
                if (squadSc.action == FormationAction.Aggression) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Aggression"))
                {
                    squadSc.action = FormationAction.Aggression;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.red;
                if (startingTarget) { GUI.backgroundColor = Color.yellow; }
                if (squadSc.targetedActor != null && squadSc.targetedActor.isAlive()) { GUI.backgroundColor = Color.green; }
                if(GUILayout.Button("Aggression target (actor)"))
                {
                    startingTarget = true;
                }
                GUI.backgroundColor = originalColor;
                if(GUILayout.Button("Reset target"))
                {
                    squadSc.targetedActor = null;
                    startingTarget = false;
                }
                GUILayout.EndHorizontal();

                GUI.backgroundColor = originalColor;
            }
        }

        public bool startingTarget;
        public bool isOverridingSpawnCount;

        public void survivorWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            Color ori = GUI.backgroundColor;
            /*
            if (controlledActorSc != null && squadSc != null)
            {
                if(GUILayout.Button("Copy squad"))
                {
                    UnitClipboard_Main.CopySquad(squadSc);
                }
            }
            if(UnitClipboard_Main.squadListForPaste != null)
            {
                GUI.backgroundColor = Color.red;
                if (isPasting)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                if(GUILayout.Button("Paste squad"))
                {
                    isPasting = !isPasting;
                }
                GUI.backgroundColor = ori;
            }

              if (GUILayout.Button("test"))
            {
                if(controlledActorSc != null)
                {
                    GodPower godPower = AssetManager.powers.get("crabzilla");
                    //prep the patches blocking camera movement etc
                    hasStartedCrabMode = true;
                    //spawn the giantzilla out of view hopefully
                    World.world.units.createNewUnit(godPower.actor_asset_id, MapBox.instance.tilesList.Last(), godPower.actorSpawnHeight);
                }
            }
            if (GUILayout.Button("test2"))
            {
                crabThing();
            }
            */
            GUILayout.Button("Time survived: " + ((int)timeSurvivedSoFar).ToString());
            GUILayout.Button("Enemies killed: " + enemyKills.ToString());
            GUILayout.Button("Active creatures: " + validEnemyTargets.Count.ToString());
            GUILayout.Button("Max creatures: " + maxCreatureCountFromDifficulty.ToString());
            GUILayout.Button("Creatures each wave: " + creaturesToActuallySpawn().ToString());
            GUILayout.BeginHorizontal();
            GUILayout.Button("Select creatures");
            if (showSpawnSelectWindow) GUI.backgroundColor = Color.yellow;
            else GUI.backgroundColor = ori;
            string s = "->";
            if (showSpawnSelectWindow)
            {
                s = "<-";
            }
            if (GUILayout.Button(s))
            {
                showSpawnSelectWindow = !showSpawnSelectWindow;

            }
            GUILayout.EndHorizontal();
            if (isOverridingSpawnCount) GUI.backgroundColor = Color.green;
            else GUI.backgroundColor = Color.red;
            if(GUILayout.Button("Override wave spawn")) { isOverridingSpawnCount = !isOverridingSpawnCount; }
            creaturesToSpawnOverride = (int)GUILayout.HorizontalSlider(creaturesToSpawnOverride, 0f, 500f);
            if (survivorActive) GUI.backgroundColor = Color.green;
            else GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Start") && !survivorActive)
            {
                if (controlledActorSc != null)
                {
                    if(firstSurvivorRun)
                    {
                        AddSurvivorTraits();
                        firstSurvivorRun = false;
                    }
                    wasLevelCapDisabled = GuiMain.Other.disableLevelCap;
                    float camZoom = MoveCamera.instance.targetZoom;
                    UnitClipboard_Main.CopySquad(squadSc);
                    MapBox.instance.generateNewMap(false);
                    SmoothLoader.add(delegate
                    {
                        WorldTile validTile = MapBox.instance.tilesList.GetRandom();
                        int attempt = 1;
                        while (validTile.Type.liquid && attempt < 10)
                        {
                            validTile = MapBox.instance.tilesList.GetRandom();
                            attempt++;
                        }
                        //if its still liquid after those attempts, assume world is liquid and cannot proceed
                        if (validTile.Type.liquid)
                        {
                            Debug.Log("World might be all liquid, cannot start survivor mode");
                        }
                        UnitClipboard_Main.PasteSquad(validTile);
                        MoveCamera.instance.targetZoom = camZoom;
                        survivorActive = true;
                        timeSurviveStarted = Time.realtimeSinceStartup;
                    }, "gen: Cody's Suvivor Mode", false);
                }
            }
            GUI.backgroundColor = ori;
          

            GUI.DragWindow();
        }
        public static Vector2 scrollPosition;
        public bool wasLevelCapDisabled;

        public void spawnSelectWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            Color ori = GUI.backgroundColor;
            scrollPosition = GUILayout.BeginScrollView(
         scrollPosition, GUILayout.Height(survivorWindowRect.height - 31.5f), GUILayout.Width(200f));
            foreach (string actorType in GuiMain.listOfActorAssetIDs)
            {
                if (monstersToSpawn.Contains(actorType)){
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button(actorType)) {
                    if (monstersToSpawn.Contains(actorType)){
                        monstersToSpawn.Remove(actorType);
                    }
                    else
                    {
                        monstersToSpawn.Add(actorType);
                    }
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }
        public bool showSpawnSelectWindow;


        public static void newKillAction_Postfix(Actor pDeadUnit, Kingdom pPrevKingdom, Actor __instance)
        {
            if (__instance == controlledActorSc && validEnemyTargets.Contains(pDeadUnit))
            {
                enemyKills++;
                if (controlledActorSc.hasTrait("death_arrow") && Toolbox.randomChance(0.1f))
                {
                    //1 sec = 1, 60 sec = 3, 180sec = 7
                    for (int i = 0; i < (1 + ((int)timeSurvivedSoFar / 30)); i++)
                    {
                        Actor validTarget = validEnemyTargets.GetRandom();
                        shootAtTile(controlledActorSc, validTarget.currentTile, "arrow");
                    }
                }
                if (controlledActorSc.hasTrait("death_bomb") && Toolbox.randomChance(0.1f))
                {
                    //1 sec = 1, 60 sec = 2?, 180sec = 3
                    for (int i = 0; i < (1 + ((int)timeSurvivedSoFar / 90)); i++)
                    {
                        Actor validTarget = validEnemyTargets.GetRandom();
                        shootAtTile(controlledActorSc, validTarget.currentTile, "firebomb");
                    }
                }
                //spawn unit x amount of times, increasing scale
                if (pDeadUnit.hasTrait("death_undying"))
                {
                    int deathsSoFar = 1;
                    if(pDeadUnit.data.custom_data_int != null && pDeadUnit.data.custom_data_int.TryGetValue("deaths", out int deathCount) == true)
                    {
                        deathsSoFar = deathCount + 1;
                    }
                    if(deathsSoFar < 5)
                    {
                        //all of this is the same as "default" enemy creations, except death_undying trait and death counter data
                        Actor newMonster = MapBox.instance.units.createNewUnit(pDeadUnit.asset.id, pDeadUnit.currentTile.neighboursAll.GetRandom().neighboursAll.GetRandom(), 0f);
                        validEnemyTargets.Add(newMonster);
                        ActorTrait customTrait = new ActorTrait();
                        customTrait.id = "customT" + pDeadUnit.asset.id + "undying";
                        customTrait.base_stats = IntendedStats(AssetManager.actor_library.get(pDeadUnit.asset.id).base_stats, deathsSoFar); //scaling up with deaths
                        AssetManager.traits.add(customTrait);
                        newMonster.data.removeTrait("peaceful");
                        newMonster.addTrait(customTrait.id);
                        newMonster.addTrait("survivor_enemy");
                        
                        newMonster.addTrait("death_undying");
                        newMonster.data.set("deaths", deathsSoFar);

                        newMonster.updateStats();
                    }
                }
                if (pDeadUnit.hasTrait("death_multiply"))
                {
                    //1 sec = 1, 60 sec = 7, 180sec = 19 (maybe this is high)
                    for (int i = 0; i < (1 + ((int)timeSurvivedSoFar/10)); i++)
                    {
                        Actor newMonster = MapBox.instance.units.createNewUnit(pDeadUnit.asset.id, pDeadUnit.currentTile.neighboursAll.GetRandom().neighboursAll.GetRandom(), 5f);
                        validEnemyTargets.Add(newMonster);
                        ActorTrait customTrait = new ActorTrait(); // create trait that applies "balanced" stats
                        customTrait.id = "customT" + pDeadUnit.asset.id;
                        customTrait.base_stats = IntendedStats(AssetManager.actor_library.get(pDeadUnit.asset.id).base_stats); //multiply
                        AssetManager.traits.add(customTrait); // constant update and replace
                        newMonster.data.removeTrait("peaceful");
                        newMonster.addTrait(customTrait.id);
                        newMonster.addTrait("survivor_enemy");
                        newMonster.updateStats();
                    }
                }
                
            }
        }


        public static bool killHimself_Prefix(bool pDestroy, AttackType pType, bool pCountDeath, bool pLaunchCallbacks, bool pLogFavorite, Actor __instance)
        {
            if(validEnemyTargets.Contains(__instance))
            {
                validEnemyTargets.Remove(__instance);
            }
            return true;
        }

        public bool firstSurvivorRun = true;

        public bool isPasting;
        public static bool survivorActive;
        public float timeSurviveStarted;
        public static float timeSurvivedSoFar;
        public static int enemyKills;
        public float lastSpawnUpdate = 0f;
        public static bool preventClicksOpeningWindows = true;


        public void SurvivorUpdate()
        {
            if (survivorActive)
            {
                if(global::Config.paused == false)
                {
                    timeSurvivedSoFar += Time.deltaTime;
                }
                if(controlledActorSc == null || controlledActorSc.data.alive == false)
                {
                    //controlled actor has died, mode should finish
                    float lastTime = timeSurvivedSoFar;
                    WorldTip.showNow("You died. Kills: " + enemyKills.ToString(), false, "center", 10f);
                    //maybe save highest stats for record
                    survivorActive = false;
                    timeSurvivedSoFar = 0f;
                    timeSurviveStarted = 0f;
                    GuiMain.Other.disableLevelCap = wasLevelCapDisabled;
                    wasLevelCapDisabled = false;

                    for (int x = 0; x < AssetManager.traits.list.Count; x++)
                    {
                        ActorTrait trait = AssetManager.traits.list[x];
                        if (trait.id.Contains("customT"))
                        {
                            AssetManager.traits.list.Remove(trait);
                        }
                    }
                }
                else
                {
                    UpdateMonsterSpawn();
                    //show status text at top of screen
                    WorldTip.showNow("Kills: " + enemyKills.ToString(), false, "top");
                }
            }
        }

        public void AddSurvivorTraits()
        {
            ActorTrait survivorEnemy = new ActorTrait();
            survivorEnemy.id = "survivor_enemy";
            survivorEnemy.path_icon = "ui/Icons/iconGreedy";
            survivorEnemy.action_special_effect = new WorldAction(SurvivorAction);
            AssetManager.traits.add(survivorEnemy);

            ActorTrait death_arrow = new ActorTrait();
            death_arrow.id = "death_arrow";
            death_arrow.path_icon = "ui/Icons/iconGreedy";
            //survivorEnemy.action_special_effect = new WorldAction(SurvivorAction);
            AssetManager.traits.add(death_arrow);

            ActorTrait death_bomb = new ActorTrait();
            death_bomb.id = "death_bomb";
            death_bomb.path_icon = "ui/Icons/iconGreedy";
            //survivorEnemy.action_special_effect = new WorldAction(SurvivorAction);
            AssetManager.traits.add(death_bomb);

            ActorTrait death_undying = new ActorTrait();
            death_undying.id = "death_undying";
            death_undying.path_icon = "ui/Icons/iconGreedy";
            //survivorEnemy.action_special_effect = new WorldAction(SurvivorAction);
            AssetManager.traits.add(death_undying);

            ActorTrait death_multiply = new ActorTrait();
            death_multiply.id = "death_multiply";
            death_multiply.path_icon = "ui/Icons/iconGreedy";
            //survivorEnemy.action_special_effect = new WorldAction(SurvivorAction);
            AssetManager.traits.add(death_multiply);
        }


        public static int difficultyScaling = 10; // higher = easier
        public static int difficultyHealth => (int)timeSurvivedSoFar / difficultyScaling; // 5 = 10 seconds = +2hp, 120seconds = +24, 600sec = +120 etc

        public static BaseStats IntendedStats(BaseStats original, float scale = 1)
        {
            int intendedHealth = difficultyHealth;
            BaseStats returnBaseStats = new BaseStats();

            returnBaseStats["health"] = (scale * 1 + (intendedHealth - original["health"]));
            returnBaseStats["damage"] = (scale * 2 + (intendedHealth - original["damage"]));

            returnBaseStats["scale"] = -0.25f + (scale * 0.25f);
            return returnBaseStats;
        }


        // vamp survivor has 500 creatures max
        public static int maxCreatureCountFromDifficulty => 10 + ((int)timeSurvivedSoFar / 2); // 10 sec = 15max, 120sec = 70max, 600sec = 310max etc
        public static int creaturesToSpawnEachUpdate => 1 + (maxCreatureCountFromDifficulty / 10); // 10sec = 2, 120sec = 13, 600sec = 61
        public static int creaturesToSpawnOverride = 0;

        public int creaturesToActuallySpawn()
        {
            if (isOverridingSpawnCount && creaturesToSpawnOverride != 0)
            {
                return creaturesToSpawnOverride;
            }
            else
            {
                return creaturesToSpawnEachUpdate;
            }
        }

        public List<string> monstersToSpawn = new List<string>() { "baby_orc" }; 
                                                                                 
        public static int bossesToSpawn = 0;
        public static int lastBossAmount = 0;
        public void UpdateMonsterSpawn()
        {
            if (lastSpawnUpdate + 5f < Time.realtimeSinceStartup)
            {
                int minutes = ((int)timeSurvivedSoFar / 60);
                if (minutes % 3 == 0)
                {
                    if ((minutes / 3) > lastBossAmount)
                    {
                        bossesToSpawn = (minutes / 3);
                        //Debug.Log("+3 minutes detected, bosses spawning: " + bossesToSpawn.ToString());
                        lastBossAmount = bossesToSpawn;
                    }

                }
                if (validEnemyTargets.Count < maxCreatureCountFromDifficulty)
                {
                    for (int i = 0; i < creaturesToActuallySpawn(); i++)
                    {
                        string monsterID = monstersToSpawn.GetRandom();
                        WorldTile spawnTile = MapBox.instance.tilesList.GetRandom();
                        // keep monster spawns a certain distance away
                        if (Toolbox.DistTile(spawnTile, controlledActorSc.currentTile) > 20f)
                        {
                            Actor newMonster = MapBox.instance.units.createNewUnit(monsterID, spawnTile, 5f);
                            validEnemyTargets.Add(newMonster);
                            ActorTrait customTrait = new ActorTrait(); // create trait that applies "balanced" stats
                            customTrait.id = "customT" + monsterID;
                            customTrait.base_stats = IntendedStats(AssetManager.actor_library.get(monsterID).base_stats); // newMonster
                            if (bossesToSpawn > 0)
                            {
                                customTrait.id = "customT" + monsterID + "Boss";
                                customTrait.base_stats["speed"] = 20f;
                                customTrait.base_stats["damage"] = 20;
                                //customTrait.baseStats.size = 1.25f;
                                customTrait.base_stats["scale"] = 0.75f;
                                bossesToSpawn--;
                            }
                            AssetManager.traits.add(customTrait); // constant update and replace
                            newMonster.data.removeTrait("peaceful");
                            newMonster.addTrait(customTrait.id);
                            newMonster.addTrait("survivor_enemy");
                            if (Toolbox.randomChance(0.05f))
                            {
                                newMonster.addTrait("death_multiply");
                            }
                            if (Toolbox.randomChance(0.05f))
                            {
                                newMonster.addTrait("death_undying");
                            }
                            if (Toolbox.randomChance(0.01f) && controlledActorSc.hasTrait("death_bomb") == false)
                            {
                                controlledActorSc.addTrait("death_bomb");
                            }
                            if (Toolbox.randomChance(0.01f) && controlledActorSc.hasTrait("death_arrow") == false)
                            {
                                controlledActorSc.addTrait("death_arrow");
                            }
                            newMonster.updateStats();
                        }
                    }
                    //creaturesToSpawnOverride = 0; // after potential override, reset
                }
                lastSpawnUpdate = Time.realtimeSinceStartup;
            }
        }


        //direct enemies towards controlled actor(s) and force attacks
        public static bool SurvivorAction(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (survivorActive == false)
            {
                return false;
            }
            if (controlledActorSc != null && controlledActorSc.data.alive)
            {
                if (pTarget.a.isInAttackRange(controlledActorSc) == false)
                {
                    pTarget.a.goTo(controlledActorSc.currentTile);
                    //Debug.Log("Survivor attack status: " + "running to target");
                }
                else
                {
                    bool wasSuccessful = pTarget.a.tryToAttack(controlledActorSc, false);
                    //Debug.Log("Survivor attack status: " + wasSuccessful);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        //runs after pActor dies, but before newKillAction
        public static bool unitKilled_Prefix(Actor pActor)
        {
            if (survivorActive)
            {
                if (pActor.hasTrait("survivor_enemy"))
                {
                    Actor newTarget = validEnemyTargets.GetRandom();
                    if (newTarget != pActor)
                    {
                        if(controlledActorSc != null)
                        {
                            //shootAtTile(controlledActorSc, newTarget.currentTile);
                        }
                    }
                }
            }
            return true;
        }

        public static void shootAtTile(BaseSimObject fromActor, WorldTile toTile, string projectileID)
        {
            if(toTile != null)
            {
                Vector3 pos = toTile.posV3;
                float num = Vector2.Distance(fromActor.currentPosition, pos);
                Vector3 newPoint = Toolbox.getNewPoint(fromActor.currentPosition.x, fromActor.currentPosition.y, (float)pos.x, (float)pos.y, num, true);
                Vector3 newPoint2 = Toolbox.getNewPoint(fromActor.currentPosition.x, fromActor.currentPosition.y, (float)pos.x, (float)pos.y, fromActor.a.stats[S.size], true);
                newPoint2.y += 0.5f;
                Projectile arrow = EffectsLibrary.spawnProjectile(projectileID, newPoint2, newPoint, fromActor.getZ());
                arrow.byWho = fromActor;
                arrow.setStats(fromActor.stats);
            }
        }

        //way of finding an actor who isnt escorting anyone
        public Actor SquadActorWithoutEscort(/*potentially make param*/)
        {
            if (squadSc == null || squadSc.squad == null)
            {
                return null;
            }
            Actor returnActor = null;
            //create new list we can loop and remove from without causing error//we are causing errors anyway gg
            List<Actor> sl = new List<Actor>();
            sl.AddRange(squadSc.squad);
            if (sl.Count > 0)
            {
                returnActor = sl.GetRandom();

                //loop removing target and finding new one
                while (escortsAndTheirClients.ContainsValue(returnActor))
                {
                    sl.Remove(returnActor);
                    returnActor = sl.GetRandom();
                }
                //will return random actor once theyre all full
            }
            return returnActor;
        }

        //self-updating list of actors to target easier
        public static List<Actor> validEnemyTargets = new List<Actor>();

        public static int sacrificeCount = 1;

        public static Dictionary<Actor, ItemData> preSacrificeEquipment = new Dictionary<Actor, ItemData>();

        SquadActions squadActions = new SquadActions();
        SingleActorActions singleActorActions = new SingleActorActions();

        //lower the laser dmg for mini-arm
        public static bool hasStartedCrabMode;
        public static bool damageWorld_Prefix(CrabArm __instance)
        {
            if (hasStartedCrabMode)
            {
                WorldTile tile = MapBox.instance.GetTile((int)__instance.laserPoint.transform.position.x, (int)__instance.laserPoint.transform.position.y);
                if (tile != null)
                {
                    MapAction.damageWorld(tile, sacrificeCount, AssetManager.terraform.get("crab_laser"));
                }
                return false;
            }
            return true;
        }

        public static bool followCamera_Prefix()
        {
            if (hasStartedCrabMode) return false;
            return true;
        }

        public static bool checkClickTouchInspect_Prefix()
        {
            if(controlledActorSc != null && controlledActorSc.data.alive)
            {
                if (preventClicksOpeningWindows)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool checkEmptyClick_Prefix()
        {
            if (controlledActorSc != null && controlledActorSc.data.alive)
            {
                if (preventClicksOpeningWindows)
                {
                    return false;
                }
            }
            return true;
        }

        public List<CrabArm> customArms = new List<CrabArm>();
        public void updateCrabArms()
        {
            if(controlledActorSc != null && controlledActorSc.isAlive())
            {
                foreach (CrabArm arm in customArms)
                {
                    arm.update(MapBox.instance.deltaTime);
                    if (arm.giantzilla == null || (arm.giantzilla.actor != null && arm.giantzilla.actor.data.alive == false))
                    {
                        arm.gameObject.SetActive(false);
                    }
                    if (Input.GetMouseButton(0))
                    {
                        //UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // rgb laser
                    }
                }
            }
            else
            {
                if(customArms.Count > 1)
                {
                    customArms.GetRandom().giantzilla.actor.killHimself();
                    customArms = new List<CrabArm>();
                }
            }
        }

        public string[] escortCommandLines = new string[] { "Get over here!", "Line up!", "Surrender or die...", "Where's my rope?" };
        public string[] escortEncourageLines = new string[] { "Let's go!", "Move it!", "Faster!", "Pick your feet up!" };
        public string[] fearLines = new string[] { "Noooo!", "I don't wanna die!", "WHY", "Wha-", "My kids..." };
        public string[] sorryLines = new string[] { "...", "I'm sorry.." };

        public bool showSingleActionWindow;
        public bool showSquadActionWindow;
        public bool showSquadControlWindow;
        public bool showSurvivorWindow;


        public Rect controlWindowRect = new Rect(126f, 1f, 1f, 1f);
        //subwindows for dividing categories
        public Rect singleActorActionWindowRect;
        public Rect squadActionWindowRect;
        public Rect squadControlsWindow;
        public Rect survivorWindowRect;
        public static Rect spawnSelectWindowRect;


        public void actorControlWindowUpdate()
        {
            if (SimpleSettings.showHideActorControlConfig)
            {
                controlWindowRect = GUILayout.Window(17064, controlWindowRect, actorControlWindow, "Control Main", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));

                if (showSingleActionWindow)
                {
                    singleActorActionWindowRect = GUILayout.Window(17065, singleActorActionWindowRect, actorSingleActionWindow, "Single Actions", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    singleActorActionWindowRect.position = new Vector2(controlWindowRect.x + controlWindowRect.width, (controlWindowRect.y));
                }
                if (showSquadActionWindow)
                {
                    squadActionWindowRect = GUILayout.Window(17066, squadActionWindowRect, actorSquadActionWindow, "Squad Actions", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    squadActionWindowRect.position = new Vector2(controlWindowRect.x + controlWindowRect.width, (controlWindowRect.y));
                }
                if (showSquadControlWindow)
                {
                    squadControlsWindow = GUILayout.Window(17067, squadControlsWindow, actorSquadControlWindow, "Squad Control");
                    squadControlsWindow.position = new Vector2(controlWindowRect.x, (controlWindowRect.y + controlWindowRect.height));
                }
                if (showSurvivorWindow)
                {
                    survivorWindowRect = GUILayout.Window(17068, survivorWindowRect, survivorWindow, "Survivor");
                    if (showSpawnSelectWindow)
                    {
                        spawnSelectWindowRect = GUILayout.Window(17069, spawnSelectWindowRect, spawnSelectWindow, "Select creatures");
                        spawnSelectWindowRect.position = new Vector2(survivorWindowRect.x + survivorWindowRect.width, (survivorWindowRect.y));
                    }
                }

            }
        }

        public void actorControlUpdate()
        {
            SurvivorUpdate();
            updateCrabArms();
            //this is ugly, cant we do it better?
            if ((Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0)) && settingUpEscort)
            {
                Actor actor = MapBox.instance.getActorNearCursor();
                if (actor != null)
                {
                    if (actor != controlledActorSc)
                    {
                        if (squadSc != null)
                        {
                            if (squadSc.squad.Contains(actor) == false)
                            {
                                Messages.ActorSay(controlledActorSc, escortCommandLines.GetRandom());
                                actorBeingEscorted = actor;
                            }
                            else
                            {
                                Debug.Log("Tried to escort squad member");
                            }
                        }
                        else
                        {
                            Messages.ActorSay(controlledActorSc, escortCommandLines.GetRandom());
                            actorBeingEscorted = actor;
                        }
                    }
                    else
                    {
                        Debug.Log("Tried to escort self");
                    }
                }
                settingUpEscort = false;
            }
            if ((Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0)) && settingUpControl)
            {
                Actor actor = MapBox.instance.getActorNearCursor();
                if (actor != null)
                {
                    BodySnatchers.ControlledActor.SetActor(actor);
                    BodySnatchers.Main.squad = new Squad(actor, 10);
                }
                settingUpControl = false;
            }
            if ((Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0)) && settingUpHire)
            {
                Actor actor = MapBox.instance.getActorNearCursor();
                if (actor != null)
                {
                    BodySnatchers.Main.squad.HireActor(actor);
                }
                settingUpHire = false;
            }
            if ((Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0)) && startingTarget)
            {
                Actor actor = MapBox.instance.getActorNearCursor();
                if (actor != null)
                {
                    squadSc.targetedActor = actor;
                }
                startingTarget = false;
            }
            if (actorBeingEscorted != null)
            {
                if (controlledActorSc != null)
                {
                    actorBeingEscorted.moveTo(controlledActorSc.currentTile.tile_down);
                }
                else
                {
                    actorBeingEscorted = null;
                }
            }
            if (escortsAndTheirClients.Count > 0)
            {
                foreach (KeyValuePair<Actor, Actor> escortDictPair in escortsAndTheirClients)
                {
                    Actor following = escortDictPair.Key;
                    //should cleanup instead but im tired and dont want to, so we skip
                    if (following == null || following.data.alive == false)
                    {
                        //do nothing
                    }
                    else
                    {
                        Actor followed = escortDictPair.Value;
                        if (followed == null || followed.data.alive == false)
                        {

                        }
                        else
                        {
                            following.moveTo(followed.currentTile.tile_down);
                        }
                    }
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (isPasting)
                {
                    if(MapBox.instance.getMouseTilePos() != null)
                    {
                        UnitClipboard_Main.PasteSquad(MapBox.instance.getMouseTilePos());
                        isPasting = false;
                    }
                }
            }
            controlWindowRect.height = 0f;
            singleActorActionWindowRect.height = 0f;
            squadActionWindowRect.height = 0f;
            squadControlsWindow.height = 0f;
        }

        //prevent controlled actors from hitting random shit until we tell them to
        public static bool checkEnemyTargets_Prefix(Actor __instance)
        {
            if (__instance == actorBeingEscorted)
            {
                return false;
            }
            if (controlledActorSc != null && __instance == controlledActorSc)
            {
                return false;
            }
            if (squadSc != null && squadSc.squad.Contains(__instance))
            {
                return false;
            }
            if (escortsAndTheirClients.Count > 0)
            {
                foreach (KeyValuePair<Actor, Actor> escortDictPair in escortsAndTheirClients)
                {
                    Actor following = escortDictPair.Key;
                    //should cleanup instead but im tired and dont want to, so we skip
                    if (following == null || following.data.alive == false)
                    {
                        //do nothing
                    }
                    else
                    {
                        if (following == __instance)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        class SingleActorActions
        {
            public void StartSingleEscort(Actor toFollow, Actor target = null)
            {
                if (target != null)
                {
                    if (escortsAndTheirClients.ContainsKey(target))
                    {
                        //might have to revisit later to check dead escort, etc
                        Debug.Log("Actor is being escorted already, cancelling");
                    }
                    else if (squadSc.squad.Contains(target))
                    {
                        Debug.Log("Actor is in squad, cancelling");
                    }
                    else
                    {
                        if (escortsAndTheirClients.ContainsKey(target) == false)
                            escortsAndTheirClients.Add(target, toFollow);
                    }
                }
                else
                {
                    List<Actor> escortTargets = new List<Actor>();
                    MapBox.instance.getObjectsInChunks(toFollow.currentTile, 10, MapObjectType.Actor);
                    if (MapBox.instance.temp_map_objects.Count > 0)
                    {
                        foreach (Actor actor in MapBox.instance.temp_map_objects)
                        {
                            //no check if escortingActor is in here, just guranteeing controlled actors arent hostages
                            if (squadSc.squad.Contains(actor) == false && controlledActorSc != actor)
                            {
                                escortTargets.Add(actor);
                            }
                        }
                    }
                    if (escortTargets.Count > 0)
                    {
                        Actor target1 = escortTargets.GetRandom();
                        //if target is being escorted already
                        if (escortsAndTheirClients.ContainsKey(target1) == true)
                        {
                            //loop removing target and finding new one
                            while (escortTargets.Count > 0 && escortsAndTheirClients.ContainsKey(target1))
                            {
                                escortTargets.Remove(target1);
                                target1 = escortTargets.GetRandom();
                            }
                            target = target1;
                            //makes it here once valid target found?
                            escortsAndTheirClients.Add(target, toFollow);
                        }
                        else
                        {
                            target = target1;
                            escortsAndTheirClients.Add(target1, toFollow);
                        }
                    }
                }

                if (target != null)
                {


                    Messages.ActorSay(toFollow, GuiMain.ActorControl.escortCommandLines.GetRandom());
                    if (Toolbox.randomBool())
                    {
                        Messages.ActorSay(target, GuiMain.ActorControl.fearLines.GetRandom());
                        WorldTile moveTile = target.currentTile;
                        //move "leader" of escort towards captive, looks a little cleaner
                        toFollow.goTo(moveTile);
                    }
                }
            }
        }

        class SquadActions
        {
            #region escortActions
            public void StartMassEscort(List<Actor> squadTarget)
            {
                WorldTile tile = controlledActorSc.currentTile;
                List<Actor> escortTargets = new List<Actor>();
                MapBox.instance.getObjectsInChunks(tile, 15, MapObjectType.Actor);
                foreach (Actor actor in MapBox.instance.temp_map_objects)
                {
                    if (squadSc.squad.Contains(actor) == false && controlledActorSc != actor)
                    {
                        escortTargets.Add(actor);
                    }
                }
                if(escortTargets.Count == 0)
                {
                    Messages.ActorSay(controlledActorSc, "No targets nearby!");
                    return;
                }
                //setup pairs of followers and the people they follow
                escortsAndTheirClients = new Dictionary<Actor, Actor>();
                foreach (Actor target in escortTargets)
                {
                    //make them follow random squad mate
                    //could make sure 1 per squad mate later, but this way makes more prisoners easy to handle
                    Actor toFollow = squadSc.squad.GetRandom();
                    if (Toolbox.randomChance(1f))
                    {
                        Messages.ActorSay(toFollow, GuiMain.ActorControl.escortCommandLines.GetRandom());
                        WorldTile moveTile = target.currentTile;
                        //move "leader" of escort towards captive, looks a little cleaner
                        toFollow.goTo(moveTile);
                    }
                    escortsAndTheirClients.Add(target, toFollow);
                }
                foreach (Actor actor in squadTarget)
                {

                    //nothing happens, they already had a command if theyre chosen to escort
                }
                squadSc.nextActionTime = 3f;
                squadSc.nextSquadAction = new SquadAction(EscortEncouragement);
            }

            public void EscortEncouragement(List<Actor> squadTarget)
            {
                foreach (Actor actor in squadTarget)
                {
                    actor.stopMovement();
                    if (Toolbox.randomChance(0.2f))
                    {
                        Messages.ActorSay(actor, GuiMain.ActorControl.escortEncourageLines.GetRandom());
                    }
                }
                squadSc.nextActionTime = 3f;
            }
            #endregion

            public void MoveSquadOnce(List<Actor> squadTarget)
            {
                for (int i = 0; i < squadTarget.Count; i++)
                {
                    squadSc.MoveActor(squadTarget[i]);
                }
            }

            #region exampleActionChain
            public void FindTilesToDig(List<Actor> squadTarget)
            {
                foreach (Actor actor in squadTarget)
                {
                    Messages.ActorSay(actor, "Time to dig...");
                    WorldTile moveTile = actor.currentTile.zone.tiles.GetRandom();
                    actor.goTo(moveTile);
                }
                squadSc.nextSquadAction = new SquadAction(StopMoving);
            }
            public void StopMoving(List<Actor> squadTarget)
            {
                foreach (Actor actor in squadTarget)
                {
                    actor.stopMovement();
                    //this helps but isnt a fix
                    actor.setPosDirty();
                    actor.dirty_current_tile = true;
                }
                squadSc.nextActionTime = 1f;
                squadSc.nextSquadAction = new SquadAction(DigTiles);
            }
            public void DigTiles(List<Actor> squadTarget)
            {
                foreach (Actor actor in squadTarget)
                {
                    Messages.ActorSay(actor, "*Digs*");
                    if (actor.currentTile.Type.decreaseToID != null)
                    {
                        MapAction.decreaseTile(actor.currentTile);
                    }
                }
                squadSc.nextActionTime = 3f;
            }
            #endregion

            #region sacrificeAction
            public void StartSacrificeSpell(List<Actor> squadTarget)
            {

                if (escortsAndTheirClients.Count < 1)
                {
                    Messages.ActorSay(controlledActorSc, "I need more to offer...");
                    return;
                }
                //get number of sacrificed, allows scaling the effect at the end
                sacrificeCount = escortsAndTheirClients.Count;
                //hard limit the scaling
                if (sacrificeCount > 4) { sacrificeCount = 4; }

                //reset dict
                preSacrificeEquipment = new Dictionary<Actor, ItemData>();

                //create the sword that will be used by everyone
                //randomly picking weapon, and material from weapon, maybe a specific one later
                string weaponID = AssetManager.items.weapons_id_melee.GetRandom();
                ItemAsset weaponAsset = AssetManager.items.get(weaponID);
                string material = weaponAsset.materials.GetRandom();
                ItemData sacrificialSword = ItemGenerator.generateItem(weaponAsset, material, World.world.mapStats.year);

                //store actor equipment, give them a weapon to perform the sacrifice with
                foreach (KeyValuePair<Actor, Actor> escortDictPair in escortsAndTheirClients)
                {
                    Actor following = escortDictPair.Key;
                    Actor followed = escortDictPair.Value;
                    if (followed.asset.use_items)
                    {
                        if (followed.equipment == null)
                        {
                            followed.equipment = new ActorEquipment();
                        }
                        if (followed.equipment.weapon.isEmpty() == false && preSacrificeEquipment.ContainsKey(followed) == false)
                        {
                            preSacrificeEquipment.Add(followed, followed.equipment.weapon.data);
                        }
                        followed.equipment.getSlot(EquipmentType.Weapon).setItem(sacrificialSword);
                        followed.setStatsDirty();
                    }
                }

                squadSc.nextActionTime = 2f;
                squadSc.nextSquadAction = new SquadAction(GuiMain.ActorControl.squadActions.SacrificeStep);
            }

     

            //make sacrifices die 1 by 1
            public void SacrificeStep(List<Actor> squadTarget)
            {
                if (escortsAndTheirClients != null && escortsAndTheirClients.Count > 0)
                {
                    Actor target = escortsAndTheirClients.Keys.First();
                    Actor escort = escortsAndTheirClients[target];
                    escortsAndTheirClients.Remove(target);
                    //make executor say something
                    Messages.ActorSay(escort, GuiMain.ActorControl.sorryLines.GetRandom());
                    target.getHit(1000000f, true, AttackType.Other, null, false, false);
                    //BaseEffect lightningEffect = EffectsLibrary.spawnAtTile("fx_lightning_small", target.currentTile, 0.25f);
                    //target.killHimself();
                }
                if (escortsAndTheirClients.Count > 0)
                {
                    //make remaining actors panic
                    foreach (Actor squad in escortsAndTheirClients.Keys)
                    {
                        if (Toolbox.randomChance(0.2f))
                        {
                            Messages.ActorSay(squad, GuiMain.ActorControl.fearLines.GetRandom());
                        }
                    }
                    squadSc.nextSquadAction = new SquadAction(SacrificeStep);
                }
                else
                {
                    finaleStep = 0;
                    squadSc.nextActionTime = 0.25f;
                    GodPower godPower = AssetManager.powers.get("crabzilla");
                    //prep the patches blocking camera movement etc
                    hasStartedCrabMode = true;
                    //spawn the giantzilla out of view hopefully
                    World.world.units.createNewUnit(godPower.actor_asset_id, MapBox.instance.tilesList.Last(), godPower.actorSpawnHeight);
                    squadSc.nextSquadAction = new SquadAction(SacrificeFinaleStep);
                }
                squadSc.nextActionTime = 0.5f;
            }

            //crazy looking finale? lightning striking many places back to back, then final action
            public int finaleStep = 0;
            public void SacrificeFinaleStep(List<Actor> squadTarget)
            {
                WorldTile targetTile = controlledActorSc.currentTile.neighbours.GetRandom().neighbours.GetRandom().neighbours.GetRandom();
                BaseEffect lightningEffect = EffectsLibrary.spawnAtTile("fx_lightning_small", targetTile, 0.25f);
              
                if (finaleStep < 5)
                {
                    finaleStep++;
                    squadSc.nextSquadAction = new SquadAction(SacrificeFinaleStep);
                }
                else
                {
                    //final action here
                    squadSc.nextSquadAction = new SquadAction(SacrificeFinale);
                }
            }

            public void SacrificeFinale(List<Actor> squadTarget)
            {
                crabThing();
                controlledActorSc.addTrait("giant");
                Messages.ActorSay(controlledActorSc, "I feel stronger...");

                //reset weapons that were swapped for sacrifice
                foreach (Actor sm in squadTarget)
                {
                    if (preSacrificeEquipment.ContainsKey(sm))
                    {
                        sm.equipment.getSlot(EquipmentType.Weapon).setItem(preSacrificeEquipment[sm]);
                        sm.setStatsDirty();
                    }
                }
                squadSc.nextActionTime = 3f;
            }

            //crabzilla gets spawned beforehand, then practically ripped apart, arms given away
            public void crabThing()
            {
                if (controlledActorSc != null)
                {
                    if(GuiMain.ActorControl.customArms.Count > 1)
                    {
                        for (int i = 0; i < GuiMain.ActorControl.customArms.Count; i++)
                        {
                            CrabArm arm = GuiMain.ActorControl.customArms[i];
                            Destroy(arm);
                        }
                    }
                    GuiMain.ActorControl.customArms = new List<CrabArm>();
                    foreach (Actor actor in MapBox.instance.units)
                    {
                        if (actor.asset.id == "crabzilla")
                        {
                            ChangeLaserTerraform();
                            Giantzilla crab = actor.GetComponent<Giantzilla>();

                            CrabArm crabArm1 = crab.arm1;
                            CrabArm duplicate1 = UnityEngine.Object.Instantiate(crabArm1);
                            duplicate1.transform.position = controlledActorSc.transform.position + new Vector3(1f, 0, 0);
                            duplicate1.transform.parent = controlledActorSc.transform;
                            duplicate1.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                            duplicate1.transform.localPosition = new Vector3(3, 5, 0);
                            duplicate1.laser.color = Color.red;
                            GuiMain.ActorControl.customArms.Add(duplicate1);

                            CrabArm crabArm2 = crab.arm2;
                            CrabArm duplicate2 = UnityEngine.Object.Instantiate(crabArm2);
                            duplicate2.transform.position = controlledActorSc.transform.position + new Vector3(-1f, 0, 0);
                            duplicate2.transform.parent = controlledActorSc.transform;
                            duplicate2.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                            duplicate2.transform.localPosition = new Vector3(-3, 5, 0);
                            duplicate2.laser.color = Color.red;
                            GuiMain.ActorControl.customArms.Add(duplicate2);

                            //disable rest of crab
                            //crab.enabled = false;
                            crab.arm1.gameObject.SetActive(false);
                            crab.arm2.gameObject.SetActive(false);
                            crab.mouthSprite.GetComponent<SpriteRenderer>().enabled = false;
                            crab.transform.Find("Shadow").gameObject.SetActive(false);
                            crab.transform.Find("Main Body").gameObject.SetActive(false);
                            foreach (GiantLeg leg in crab.list_legs)
                            {
                                leg.gameObject.SetActive(false);
                            }
                            foreach (LegJoint legJoint in crab.list_joints)
                            {
                                legJoint.gameObject.SetActive(false);
                            }
                            //crab.mainBody.enabled = false;
                            //crab.mouthSprite.active = false;
                        }
                    }
                    //CrabArm crabArm = NCMS.Utils.GameObjects.FindEvenInactive("Giantzilla").GetComponent<CrabArm>();
                }
            }

            //lower shake
            public void ChangeLaserTerraform()
            {
                TerraformOptions laser = AssetManager.terraform.get("crab_laser");
                laser.shake_intensity = 0f;
            }
            #endregion
        }
    }
}
