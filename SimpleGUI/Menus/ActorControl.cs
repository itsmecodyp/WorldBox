using System.Collections.Generic;
using UnityEngine;
using BodySnatchers;
using SimpleGUI.Submods.SimpleMessages;
using SimpleGUI.Submods;
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

#pragma warning disable CS0649

namespace SimpleGUI.Menus
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
                if(GUILayout.Button(new GUIContent("ControlledName: " + controlledActorSc.data.name, "Name of the actor being controlled")))
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
            GUI.DragWindow();
        }

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
                if(GUILayout.Button("Kill escorted"))
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
                    squadSc.actionToDo = new SquadAction(squadActions.FindTilesToDig);
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
                    squadSc.actionToDo = new SquadAction(squadActions.StartMassEscort);
                }

                if (GUILayout.Button("SacrificialSpell"))
                {
                    squadSc.actionToDo = new SquadAction(squadActions.StartSacrificeSpell);
                }

                //way to stop actions that accidentally (or not) loop
                if ((squadSc.actionToDo != null || squadSc.nextActionToDo != null))
                {
                    if (GUILayout.Button("Stop SquadAction"))
                    {
                        squadSc.actionToDo = null;
                        squadSc.nextActionToDo = null;
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
                GUILayout.EndHorizontal();
                GUI.backgroundColor = originalColor;
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


        public List<CrabArm> customArms = new List<CrabArm>();
        public void updateCrabArms()
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

        public string[] escortCommandLines = new string[] { "Get over here!", "Line up!", "Surrender or die...", "Where's my rope?" };
        public string[] escortEncourageLines = new string[] { "Let's go!", "Move it!", "Faster!", "Pick your feet up!" };
        public string[] fearLines = new string[] { "Noooo!", "I don't wanna die!", "WHY", "Wha-", "My kids..." };
        public string[] sorryLines = new string[] { "...", "I'm sorry.." };

        public bool showSingleActionWindow;
        public bool showSquadActionWindow;
        public bool showSquadControlWindow;


        public Rect controlWindowRect = new Rect(126f, 1f, 1f, 1f);
        //subwindows for dividing categories
        public Rect singleActorActionWindowRect;
        public Rect squadActionWindowRect;
        public Rect squadControlsWindow;
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
            }
        }

        public void actorControlUpdate()
        {
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
            if (actorBeingEscorted != null)
            {
                if(controlledActorSc != null)
                {
                    actorBeingEscorted.moveTo(controlledActorSc.currentTile.tile_down);
                }
                else
                {
                    actorBeingEscorted = null;
                }
            }
            if(escortsAndTheirClients.Count > 0)
            {
                foreach(KeyValuePair<Actor, Actor> escortDictPair in escortsAndTheirClients)
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
            if(controlledActorSc != null && __instance == controlledActorSc)
            {
                return false;
            }
            if(squadSc != null && squadSc.squad.Contains(__instance))
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
                        if(following == __instance)
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
                            //makes it here once valid target found?
                            escortsAndTheirClients.Add(target1, toFollow);
                        }
                        else
                        {
                            escortsAndTheirClients.Add(target1, toFollow);
                        }
                        Messages.ActorSay(toFollow, GuiMain.ActorControl.escortCommandLines.GetRandom());
                        if (Toolbox.randomBool())
                        {
                            Messages.ActorSay(target1, GuiMain.ActorControl.fearLines.GetRandom());
                        }
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
                MapBox.instance.getObjectsInChunks(tile, 10, MapObjectType.Actor);
                foreach (Actor actor in MapBox.instance.temp_map_objects)
                {
                    if (squadSc.squad.Contains(actor) == false && controlledActorSc != actor)
                    {
                        escortTargets.Add(actor);
                    }
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
                squadSc.nextActionTimer = 3f;
                squadSc.nextActionToDo = new SquadAction(EscortEncouragement);
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
                squadSc.nextActionTimer = 3f;
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
                squadSc.nextActionToDo = new SquadAction(StopMoving);
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
                squadSc.nextActionTimer = 1f;
                squadSc.nextActionToDo = new SquadAction(DigTiles);
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
                squadSc.nextActionTimer = 3f;
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
                        if (followed.equipment.weapon.isEmpty() == false)
                        {
                            preSacrificeEquipment.Add(followed, followed.equipment.weapon.data);
                        }
                        followed.equipment.getSlot(EquipmentType.Weapon).setItem(sacrificialSword);
                        followed.setStatsDirty();
                    }
                }

                squadSc.nextActionTimer = 2f;
                squadSc.nextActionToDo = new SquadAction(GuiMain.ActorControl.squadActions.SacrificeStep);
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
                    squadSc.nextActionToDo = new SquadAction(SacrificeStep);
                }
                else
                {
                    finaleStep = 0;
                    squadSc.nextActionTimer = 0.25f;
                    GodPower godPower = AssetManager.powers.get("crabzilla");
                    //prep the patches blocking camera movement etc
                    hasStartedCrabMode = true;
                    //spawn the giantzilla out of view hopefully
                    World.world.units.createNewUnit(godPower.actor_asset_id, MapBox.instance.tilesList.Last(), godPower.actorSpawnHeight);
                    squadSc.nextActionToDo = new SquadAction(SacrificeFinaleStep);
                }
            }

            //crazy looking finale? lightning striking many places back to back, then final action
            public int finaleStep = 0;
            public void SacrificeFinaleStep(List<Actor> squadTarget)
            {
                WorldTile targetTile = controlledActorSc.currentTile;
                if (Toolbox.randomBool())
                {
                    targetTile = controlledActorSc.currentTile.neighbours.GetRandom().neighbours.GetRandom().neighbours.GetRandom();
                }
                BaseEffect lightningEffect = EffectsLibrary.spawnAtTile("fx_lightning_small", targetTile, 0.25f);
                if (finaleStep > 7 && finaleStep < 14)
                {
                    WorldTile targetTile2 = controlledActorSc.currentTile;
                    if (Toolbox.randomBool())
                    {
                        targetTile = controlledActorSc.currentTile.neighbours.GetRandom().neighbours.GetRandom().neighbours.GetRandom();
                    }
                    BaseEffect lightningEffect2 = EffectsLibrary.spawnAtTile("fx_lightning_small", targetTile, 0.25f);
                    WorldTile targetTile3 = controlledActorSc.currentTile;
                    if (Toolbox.randomBool())
                    {
                        targetTile = controlledActorSc.currentTile.neighbours.GetRandom().neighbours.GetRandom().neighbours.GetRandom();
                    }
                    BaseEffect lightningEffect3 = EffectsLibrary.spawnAtTile("fx_lightning_small", targetTile, 0.25f);
                    WorldTile targetTile4 = controlledActorSc.currentTile;
                    if (Toolbox.randomBool())
                    {
                        targetTile = controlledActorSc.currentTile.neighbours.GetRandom().neighbours.GetRandom().neighbours.GetRandom();
                    }
                    BaseEffect lightningEffect4 = EffectsLibrary.spawnAtTile("fx_lightning_small", targetTile, 0.25f);
                }
                if (finaleStep < 15)
                {
                    finaleStep++;
                    squadSc.nextActionToDo = new SquadAction(SacrificeFinaleStep);
                }
                else
                {
                    //final action here
                    squadSc.nextActionToDo = new SquadAction(SacrificeFinale);
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
                squadSc.nextActionTimer = 3f;
            }

            //crabzilla gets spawned beforehand, then practically ripped apart, arms given away
            public void crabThing()
            {
                if (controlledActorSc != null)
                {
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
