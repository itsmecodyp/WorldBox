using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace SimpleGUI {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Draggables_Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.draggables";
        public const string pluginName = "Draggables";
        public const string pluginVersion = "0.0.0.2";
        public void Awake()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            //auto disable camera drag from mouse while this mod is active, just counter intuitive to what im doing
            harmony = new Harmony("Draggables");
            original = AccessTools.Method(typeof(MoveCamera), "updateMouseCameraDrag");
            patch = AccessTools.Method(typeof(Draggables_Main), "updateMouseCameraDrag_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony("Draggables");
            original = AccessTools.Method(typeof(ActorBase), "addForce");
            patch = AccessTools.Method(typeof(Draggables_Main), "addForce_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            //Debug.Log(harmony.Id + ": Harmony patch finished: " + patch.Name);
        }

        public static bool isEnabled => GuiOther.canDragCreaturesWithMouse;

        public static bool updateMouseCameraDrag_Prefix()
        {
            if(isEnabled) {
                return false;
            }

            return true;
        }

        public static void addForce_Prefix(float pX, float pY, float pZ, ActorBase __instance)
        {
			
            if(!__instance.asset.canBeMovedByPowers) {
                return;
            }
            if(__instance.zPosition.y > 0f) {
                if(isEnabled == false) {
                    return;
                }
            }
            __instance.forceVector.x = pX * 0.6f;
            __instance.forceVector.y = pY * 0.6f;
            __instance.forceVector.z = pZ * 2f;
        }


        public bool dragSelection;
        public bool temp;
        public WorldTile startTile;
        public WorldTile endTile;
        public List<WorldTile> lastSelectedTiles;
        List<Actor> allActors => MapBox.instance.units.ToList();

        public float lastClickTime;

        public void Update()
        {
			if(isEnabled) {
                if(Input.GetMouseButtonDown(0)) {
                    // if click timing is detected as double click
                    if(Time.realtimeSinceStartup < lastClickTime + 0.5f) {
                        dragSelection = true;
                    }

                    // if double click wasnt used but actor is found, drag single
                    if(dragSelection == false) {
                        //left control to grab one unit
                        if(Input.GetKey(KeyCode.LeftControl)) {
                            Actor cActor = MapBox.instance.getActorNearCursor();
                            if(MapBox.instance.getMouseTilePos() != null && cActor != null) {
                                lastSelectedActorList = new List<Actor>();
                                lastSelectedActorList.Add(cActor);
                                isDraggingUnits = true;
                            }
                        }
                    }
                    lastClickTime = Time.realtimeSinceStartup;
                }
                DragSelectionUpdate();

                if(Input.GetMouseButtonUp(0)) {
                    dragSelection = false; // make sure this is false first chance
                    lastSelectedActorList = actorsInLastSelectedTiles();
                }
                if(Input.GetMouseButtonDown(0)) {
                    if(dragSelection == false && lastSelectedActorList != null && lastSelectedActorList.Count > 0) {
                        isDraggingUnits = true;
                    }
                }
                float z = Input.GetAxis("Mouse Y");
                if(z < 0) {
                    z = -z;
                }
                if(isDraggingUnits) {
                    var v3 = Input.mousePosition;
                    v3.z = Camera.main.farClipPlane;
                    v3 = Camera.main.ScreenToWorldPoint(v3);

                    float x = Input.GetAxis("Mouse X");
                    if(x < 0) x = -x; // flip x if negative
                    foreach(Actor actor in lastSelectedActorList) {
                        if(!(actor == null) && actor.data.alive) {
                            Vector2 vector = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                            Vector3 posV = actor.currentPosition; // v3 for syncd cursor (comment the posv+= lines after), actor.currentpos for smoother group throws
                            if(Input.GetKey(KeyCode.LeftControl)) {
                                posV = v3;
                            }
                            posV.x += vector.x;
                            posV.y += vector.y + z;
                            actor.zPosition.y = z; // prevents the addForce from applying at the end, could patch addForce instead
                            actor.currentPosition = new Vector3(posV.x, posV.y - actor.zPosition.y);
                            actor.transform.localPosition = posV;
                        }
                    }

                    if(Input.GetMouseButtonUp(0)) {
                        Debug.Log("MouseX Vel:" + Input.GetAxis("Mouse X") + "; " + "MouseY Vel:" + Input.GetAxis("Mouse Y"));
                        isDraggingUnits = false;
                        Vector3 force = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0.5f + z);
                        foreach(Actor actor in lastSelectedActorList) {
                            actor.addForce(force.x, force.y, force.z);
                        }
                    }
                }
            }
            
        }

        public static bool dragCircular;

        public List<WorldTile> CheckTilesBetween2(WorldTile target1, WorldTile target2)
        {
            List<WorldTile> tilesToCheck = new List<WorldTile>(); // list for later
            Vector2Int pos1 = target1.pos;
            Vector2Int pos2 = target2.pos;
            float distanceBetween = Toolbox.DistTile(target1, target2);
            int pSize = (int)distanceBetween;
            PixelFlashEffects flashEffects = MapBox.instance.flashEffects;
            if(dragCircular == false) {
                int difx = dif(pos1.x, pos2.x) + 1;
                int dify = dif(pos1.y, pos2.y) + 1;
                if(pos1.x - pos2.x <= 0 && pos1.y - pos2.y <= 0) {
                    for(int x = 0; x < difx; x++) {
                        for(int y = 0; y < dify; y++) {
                            Vector2Int newPos = target1.pos + new Vector2Int(x, y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);

                        }
                    }
                }
                if(pos1.x - pos2.x >= 0 && pos1.y - pos2.y <= 0) {
                    for(int x = 0; x < difx; x++) {
                        for(int y = 0; y < dify; y++) {
                            Vector2Int newPos = target1.pos + new Vector2Int(-x, y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);
                        }
                    }
                }
                if(pos1.x - pos2.x <= 0 && pos1.y - pos2.y >= 0) {
                    for(int x = 0; x < difx; x++) {
                        for(int y = 0; y < dify; y++) {
                            Vector2Int newPos = target1.pos + new Vector2Int(x, -y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);

                        }
                    }
                }
                if(pos1.x - pos2.x >= 0 && pos1.y - pos2.y >= 0) {
                    for(int x = 0; x < difx; x++) {
                        for(int y = 0; y < dify; y++) {
                            Vector2Int newPos = target1.pos + new Vector2Int(-x, -y);
                            WorldTile checkedTile = MapBox.instance.GetTile(newPos.x, newPos.y);
                            tilesToCheck.Add(checkedTile);
                        }
                    }
                }
                foreach(WorldTile tile in tilesToCheck) {
                    flashEffects.flashPixel(tile, 10);
                }
                return tilesToCheck;
            }

            {
                int x = pos1.x;
                int y = pos1.y;
                int radius = (int)distanceBetween;
                Vector2 center = new Vector2(x, y);
                for(int i = x - radius; i < x + radius + 1; i++) {
                    for(int j = y - radius; j < y + radius + 1; j++) {
                        if(Vector2.Distance(center, new Vector2(i, j)) <= radius) {
                            WorldTile tile = MapBox.instance.GetTile(i, j);
                            if(tile != null) {
                                flashEffects.flashPixel(tile, 10);
                                tilesToCheck.Add(tile);
                            }
                        }
                    }
                }
                return tilesToCheck;
            }
        }

        public bool isDraggingUnits;
        public Actor draggingActor;

        int dif(int num1, int num2)
        {
            int cout;
            cout = Mathf.Max(num2, num1) - Mathf.Min(num1, num2);
            return cout;
        }
        public void DragSelectionUpdate()
        {
            if(dragSelection) {
                if(Input.GetMouseButtonDown(0) && !temp) {
                    if(MapBox.instance.getMouseTilePos() != null) {
                        startTile = MapBox.instance.getMouseTilePos();
                        temp = true;
                    }
                }
                if(Input.GetMouseButton(0)) {
                    if(startTile != null) {
                        List<WorldTile> tempList = CheckTilesBetween2(startTile, MapBox.instance.getMouseTilePos());
                    }
                }
                if(Input.GetMouseButtonUp(0) && temp) {
                    if(MapBox.instance.getMouseTilePos() != null) {
                        endTile = MapBox.instance.getMouseTilePos();
                        temp = false;
                        List<WorldTile> list = CheckTilesBetween2(startTile, endTile);
                        if(list != null) {
                            lastSelectedTiles = list;
                        }
                    }
                }
                if(Input.GetKeyDown(KeyCode.R)) {
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

        public List<Actor> lastSelectedActorList;

        public List<Actor> actorsInLastSelectedTiles()
        {
            List<Actor> returnList = new List<Actor>();
            if(lastSelectedTiles == null) {
                return returnList;
            }
            foreach(WorldTile tile in lastSelectedTiles) {
                if(tile._units != null && tile._units.Count >= 1) {
                    returnList.AddRange(tile._units);
                }
            }
            return returnList;
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ModSettings {
        // excluded from serialization
        // does not have JsonPropertyAttribute
        public Guid Id { get; set; }

        [JsonProperty]
        public string Name = "test";
    }
}
