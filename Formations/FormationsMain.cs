using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
//using System.Drawing;
using System.IO;
using HarmonyLib;
using System.Reflection;
using ai.behaviours;
using System;
using ai;

namespace Formations
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]

    public class FormationsMain : BaseUnityPlugin
    {

        // vars
        public const string pluginGuid = "cody.worldbox.simple.formations";
        public const string pluginName = "Formations";
        public const string pluginVersion = "0.0.0.1";
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);



        public void Awake()
        {
            InvokeRepeating("AISquadHireUpdate", 10, 10);
            InvokeRepeating("AISquadMoveUpdate", 2, 2);

        }

        public bool showHideMainMenu;

        public void Update()
        {
            DragSelectionUpdate();
            if (moveGlobalList || Input.GetKeyUp(KeyCode.Z))
            {
                moveFormationCircle(allActors, 15, MapBox.instance.getMouseTilePos().posV3);
            }
            if (moveSelectedList || Input.GetKeyDown(KeyCode.V))
            {
                moveFormationCircle(selectedActorList, 3, MapBox.instance.getMouseTilePos().posV3);
            }
            //AISquadUpdate();
        }


        public Dictionary<Actor, List<Actor>> leaderSquads = new Dictionary<Actor, List<Actor>>(); // Leader actor, squad list

        public void AISquadMoveUpdate()
        {
            if (leaderSquads != null && leaderSquads.Count >= 1)
            {
                foreach (KeyValuePair<Actor, List<Actor>> leaderAndSquad in leaderSquads)
                {
                    Actor leader = leaderAndSquad.Key;
                    List<Actor> squad = leaderAndSquad.Value;
                    if (leader != null && squad != null && squad.Count >= 1)
                    {
                        ActorStatus data = Reflection.GetField(leader.GetType(), leader, "data") as ActorStatus;
                        if (data != null && data.alive) // auto dismiss any squads with dead leaders since they wont have "orders to follow"
                        {
                            moveFormationCircle(squad, 3, leader.currentPosition);
                        }
                    }
                    else
                    {
                        // something is null or squad count too low
                    }
                    // key = actor / value = squad list
                }

            }

        }


        public void AISquadHireUpdate()
        {
            foreach (Actor unit in MapBox.instance.units)
            {
                if (unit.city != null)
                {
                    // setup new squads
                    if (unit.city.leader == unit)
                    {
                        if (leaderSquads.ContainsKey(unit) == false)
                        {
                            leaderSquads.Add(unit, new List<Actor>());
                        }
                    }
                    else
                    {
                        if (unit.city.leader != null)
                        {
                            if (leaderSquads.ContainsKey(unit.city.leader))
                            {
                                leaderSquads.TryGetValue(unit.city.leader, out List<Actor> unitLeadersSquad);
                                if (unitLeadersSquad != null && unitLeadersSquad.Count < 5)
                                {
                                    if (unitLeadersSquad.Contains(unit) == false)
                                    {
                                        unitLeadersSquad.Add(unit);
                                        ActorStatus unitData = Reflection.GetField(unit.GetType(), unit, "data") as ActorStatus;
                                        ActorStatus leaderData = Reflection.GetField(unit.city.leader.GetType(), unit.city.leader, "data") as ActorStatus;
                                        Debug.Log("AI hiring: " + leaderData.firstName + " hired " + unitData.firstName);
                                    }
                                }
                            }

                        }
                    }
                }
            }    
        }


        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 75, 120, 30));
            if (GUILayout.Button("Formations"))
            {
                showHideMainMenu = !showHideMainMenu;
            }
            if (showHideMainMenu)
            {
                mainWindowRect = GUILayout.Window(50001, mainWindowRect, new GUI.WindowFunction(mainWindow), "Formations", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            GUILayout.EndArea();
        }
        bool moveGlobalList;
        bool moveSelectedList;

        public void mainWindow(int windowID)
        {
            Color originalColor = GUI.backgroundColor;
            if (GUILayout.Button("Formations test"))
            {
                //
            }
            if (GUILayout.Button("Move all units || Z"))
            {
                moveGlobalList = !moveGlobalList;
            }
            if (GUILayout.Button("Move Selection || V"))
            {
                moveGlobalList = !moveGlobalList;
            }
          
            if (dragSelection)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;

            }
            if (GUILayout.Button("Drag selection"))
            {
                dragSelection = !dragSelection;
                if (dragSelection == false)
                {
                    startTile = null;
                    endTile = null;
                }
            }
            GUI.backgroundColor = originalColor;
            GUI.DragWindow();

        }

        public void TestTask(WorldTile targetTile) //??
        {
            BehaviourTaskActor newTask = new BehaviourTaskActor();
            newTask.id = "move_formations_circle";
            newTask.addBeh(new BehMoveToTileMod(targetTile));
            newTask.addBeh(new BehRandomWait(5f, 5f));
            newTask.addBeh(new BehEndJob());
            ActorJob soldier = new ActorJob();
            //soldier.addCondition;
        }

        public bool dragSelection;
        public bool temp;
        public WorldTile startTile;
        public WorldTile endTile;
        public List<WorldTile> lastSelectedTiles;


        List<Actor> allActors => MapBox.instance.units.getSimpleList();
        
        public static void moveFormationCircle(List<Actor> formation, int radius, Vector3 position)
        {
            if (formation != null)
            {
                float num = 6.28318548f / (float)formation.Count; // formation.count = number of points
                for (int i = 0; i < formation.Count; i++)
                {
                    float f = (float)i * num;
                    Vector3 vector = position + new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * (float)radius;
                    WorldTile tileFromVector = MapBox.instance.GetTile((int)vector.x, (int)vector.y);
                    if (tileFromVector == null)
                    {
                        Debug.Log("Movement tile not inside world, breaking");
                        return;
                    }
                    BasicMoveAndWait(formation[i], tileFromVector);
                    flashEffects.flashPixel(tileFromVector, 10, ColorType.White);
                    if (formation[i].GetComponent<Boat>() != null) // boat movement
                    {
                        formation[i].nextStepPosition = vector;
                    }

                }
            }
        }

        public static void BasicMoveAndWait(Actor targetActor, WorldTile targetTile)
        {
            targetActor.cancelAllBeh();
            targetActor.moveTo(targetTile);
            AiSystemActor actorAI = Reflection.GetField(targetActor.GetType(), targetActor, "ai") as AiSystemActor;
            actorAI.setTask("wait10", false, true);
        }

        public static PixelFlashEffects flashEffects => Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "flashEffects") as PixelFlashEffects;
        public static bool dragCircular;
        public List<WorldTile> CheckTilesBetween2(WorldTile target1, WorldTile target2)
        {
            List<WorldTile> tilesToCheck = new List<WorldTile>(); // list for later
            Vector2Int pos1 = target1.pos;
            Vector2Int pos2 = target2.pos;
            float distanceBetween = Toolbox.DistTile(target1, target2);
            int pSize = (int)distanceBetween;
            PixelFlashEffects flashEffects = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "flashEffects") as PixelFlashEffects;
            if (dragCircular == false)
            {
                int difx = dif(pos1.x, pos2.x) + 1;
                int dify = dif(pos1.y, pos2.y) + 1;
                if (pos1.x - pos2.x <= 0 && pos1.y - pos2.y <= 0)
                {
                    for (int x = 0; x < difx; x++)
                    {
                        for (int y = 0; y < dify; y++)
                        {
                            Vector2Int newPos = target1.pos + new Vector2Int(x, y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);

                        }
                    }
                }
                if (pos1.x - pos2.x >= 0 && pos1.y - pos2.y <= 0)
                {
                    for (int x = 0; x < difx; x++)
                    {
                        for (int y = 0; y < dify; y++)
                        {
                            Vector2Int newPos = target1.pos + new Vector2Int(-x, y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);
                        }
                    }
                }
                if (pos1.x - pos2.x <= 0 && pos1.y - pos2.y >= 0)
                {
                    for (int x = 0; x < difx; x++)
                    {
                        for (int y = 0; y < dify; y++)
                        {
                            Vector2Int newPos = target1.pos + new Vector2Int(x, -y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);

                        }
                    }
                }
                if (pos1.x - pos2.x >= 0 && pos1.y - pos2.y >= 0)
                {
                    for (int x = 0; x < difx; x++)
                    {
                        for (int y = 0; y < dify; y++)
                        {
                            Vector2Int newPos = target1.pos + new Vector2Int(-x, -y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);
                        }
                    }
                }
                foreach (WorldTile tile in tilesToCheck)
                {
                    flashEffects.flashPixel(tile, 10, ColorType.White);
                }
                return tilesToCheck;
            }
            else
            {
                int x = pos1.x;
                int y = pos1.y;
                int radius = (int)distanceBetween;
                Vector2 center = new Vector2(x, y);
                for (int i = x - radius; i < x + radius + 1; i++)
                {
                    for (int j = y - radius; j < y + radius + 1; j++)
                    {
                        if (Vector2.Distance(center, new Vector2(i, j)) <= radius)
                        {
                            WorldTile tile = MapBox.instance.GetTile(i, j);
                            if (tile != null)
                            {
                                flashEffects.flashPixel(tile, 10, ColorType.White);
                                tilesToCheck.Add(tile);
                            }
                        }
                    }
                }
                return tilesToCheck;
            }
        }

        public List<Actor> selectedActorList = new List<Actor>();

        public List<Actor> lastSelectedActorList => actorsInLastSelectedTiles();

        public List<Actor> actorsInLastSelectedTiles()
        {
            List<Actor> returnList = new List<Actor>();
            if (lastSelectedTiles == null)
            {
                return returnList;
            }
            foreach (WorldTile tile in lastSelectedTiles)
            {
                if (tile.units != null && tile.units.Count >= 1)
                {
                    returnList.AddRange(tile.units);
                }
            }
            return returnList;
        }

        int dif(int num1, int num2)
        {
            int cout;
            cout = Mathf.Max(num2, num1) - Mathf.Min(num1, num2);
            return cout;
        }


        public void DragSelectionUpdate()
        {
            if (dragSelection)
            {
                if (Input.GetMouseButtonDown(0) && !temp)
                {
                    if (MapBox.instance.getMouseTilePos() != null)
                    {
                        startTile = MapBox.instance.getMouseTilePos();
                        temp = true;
                    }
                }
                if (Input.GetMouseButton(0))
                {
                    if (startTile != null)
                    {
                        List<WorldTile> tempList = CheckTilesBetween2(startTile, MapBox.instance.getMouseTilePos());
                    }
                }
                if (Input.GetMouseButtonUp(0) && temp)
                {
                    if (MapBox.instance.getMouseTilePos() != null)
                    {
                        endTile = MapBox.instance.getMouseTilePos();
                        temp = false;
                        List<WorldTile> list = CheckTilesBetween2(startTile, endTile);
                        if (list != null)
                        {
                            lastSelectedTiles = list;
                        }

                        if (lastSelectedActorList.Count >= 1)
                        {
                            selectedActorList = lastSelectedActorList;
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    startTile = null;
                    endTile = null;
                    lastSelectedTiles = null;
                }

                // Perma show highlight while they move + after
                /*
                if (lastSelectedActorList != null)
                {
                    foreach (Actor unit in lastSelectedActorList)
                    {
                        flashEffects.flashPixel(unit.currentTile, 10, ColorType.Purple);
                    }
                }
                */
            }

        }


    }



    public class BehMoveToTileMod : BehaviourActionActor
    {
        public BehMoveToTileMod(WorldTile targetTile)
        {
            this.targetTile = targetTile;
        }

        public override BehResult execute(Actor pActor)
        {
            pActor.cancelAllBeh();
            //pActor.goTo(targetTile, false, false);
            if (pActor.goTo(targetTile, true, false) == ExecuteEvent.False)
            {
                return BehResult.Stop;
            }
            AiSystemActor ai = Reflection.GetField(pActor.GetType(), pActor, "ai") as AiSystemActor;
            ai.setTask("wait10", false, true);
            return BehResult.Continue;
        }
        public WorldTile targetTile;
    }

    public static class Reflection
    {
        // found on https://stackoverflow.com/questions/135443/how-do-i-use-reflection-to-invoke-a-private-method
        public static object CallMethod(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(o, args);
            }
            return null;
        }
        // ex: AccessExtensions.call(typeof(StackEffects), "startSpawnEffect", new object[] { actor.currentTile, "spawn" });
        public static object CallMethodAlternative<T>(this T obj, string methodName, params object[] args)
        {
            var type = typeof(T);
            var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
            return method.Invoke(obj, args);
        }
        // found on: https://stackoverflow.com/questions/3303126/how-to-get-the-value-of-private-field-in-c/3303182
        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        // ex: string firstName = AccessExtensions.GetField(data.GetType(), data, "firstName") as string;
        // .SetField(actor, "data", ) as ActorData;
        // List<SpriteRenderer> bodyParts = (List<SpriteRenderer>)Reflection.GetField(typeof(ActorBase), pActor, "bodyParts");
        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = originalObject.GetType().GetField(fieldName, bindFlags);
            field.SetValue(originalObject, newValue);
        }
        // Vector3 targetAngle = (Vector3)Reflection.GetField(typeof(ActorBase), actor, "targetAngle");
        // Reflection.SetField(targetAngle, "targetAngle", default(Vector3));

    }
}
