using ai.behaviours;
using BepInEx;
using SimplerGUI;
using SimplerGUI.Menus;
using SimplerGUI.Submods.UnitClipboard;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

namespace BodySnatchers {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin {
        private const string pluginGuid = "apexlite.worldbox.bodysnatchers";
        private const string pluginName = "BodySnatchers";
        private const string pluginVersion = "1.0.0";
        public static Squad squad;

        void Update() {
            if (Input.GetKeyDown(KeyCode.H))
            {
               // squad.FireActor(MapBox.instance.getActorNearCursor());
            }
            //commented out all the stuff in the menu
            /*
            if (Input.GetKeyDown(KeyCode.P)) {
                Actor actor = ControlledActor.GetActor() == null ? MapBox.instance.getActorNearCursor() : null;
                ControlledActor.SetActor(actor);
                squad = new Squad(actor, 10);
            }

            if (Input.GetKeyDown(KeyCode.Y)) {
                squad.HireActor(MapBox.instance.getActorNearCursor());
            }

           
            if (Input.GetKeyDown(KeyCode.G)) {
                squad.IncrementLineX();
            }

            if (Input.GetKeyDown(KeyCode.J)) {
                squad.IncrementLineX(-1);
            }

            if (Input.GetKeyDown(KeyCode.L)) {
                squad.IncrementLineY();
            }

            if (Input.GetKeyDown(KeyCode.B)) {
                squad.IncrementLineY(-1);
            }

            if (Input.GetKeyDown(KeyCode.Z)) {
                //squad.CycleFormation();
            }

            if (Input.GetKeyDown(KeyCode.V)) {
                //squad.CycleAction();
            }

           
            */
            if(ActorControlMain.isJoyEnabled == false)
            {
                if (Input.GetKeyDown(KeyCode.Z) && ActorControlMain.preventClicksOpeningWindows == false)
                {
                    ControlledActor.Attack(MapBox.instance.getActorNearCursor());
                }
                if (Input.GetMouseButtonDown(0) && ActorControlMain.preventClicksOpeningWindows == true)
                {
                    Actor target = MapBox.instance.getActorNearCursor();
                    ControlledActor.Attack(target);
                    if (squad != null && squad.squad != null && squad.squad.Count > 0)
                    {
                        foreach (Actor squadMate in squad.squad)
                        {
                            squadMate.tryToAttack(target, false);
                        }
                    }
                }
            }
            else //isJoyEnabled is true, use right click for attack
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Actor controlled = ControlledActor.GetActor();
                    Actor target = ClosestActorToTile(controlled.currentTile, controlled.stats["range"]); //MapBox.instance.getActorNearCursor();
                    ControlledActor.Attack(target);
                    if(squad != null && squad.squad != null && squad.squad.Count > 0)
                    {
                        foreach(Actor squadMate in squad.squad)
                        {
                            if(target != null && target.isAlive())
                            {
                                squadMate.tryToAttack(target, false);
                            }
                        }
                    }
                }
            }
           

            Squad.UpdateAll();
            ControlledActor.Update();
        }

        public static List<Actor> cachedList;
        public static List<Actor> actorsToNotHit()
        {
            int count = 1;
            if(squad != null && squad.squad != null) {
                count = squad.squad.Count + 1;
            }
            if (cachedList != null && cachedList.Count == count)
            {
                //we can assume cached list is the same still, and not re-create the list
                return cachedList;
            }
            List<Actor> listOfActorsToNotHit = new List<Actor>();
            listOfActorsToNotHit.Add(ControlledActor.GetActor());
            if(squad != null && squad.squad.Count > 0)
            {
                listOfActorsToNotHit.AddRange(squad.squad);
            }
            cachedList = listOfActorsToNotHit;
            return listOfActorsToNotHit;
        }

        public static Actor ClosestActorToTile(WorldTile pTarget, float range)
        {
            Actor returnActor = null;
            foreach (Actor actorToCheck in MapBox.instance.units)
            {
                float actorDistanceFromTile = Toolbox.Dist(actorToCheck.currentPosition.x, actorToCheck.currentPosition.y, pTarget.pos.x, pTarget.pos.y);
                if (actorDistanceFromTile < range && actorsToNotHit().Contains(actorToCheck) == false)
                {
                    range = actorDistanceFromTile;
                    returnActor = actorToCheck;
                }
            }
            return returnActor;
        }
    }
}
