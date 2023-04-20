using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace SimpleGUI {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [Obsolete]
    class _3D_Main : BaseUnityPlugin {

        public void window3D(int windowID)
        {
            Color temp = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if(autoPlacement == true) {
                GUI.backgroundColor = Color.green;
            }
            if(GUILayout.Button("Enable 3D: " + autoPlacement)) {
                autoPlacement = !autoPlacement;
                _3dEnabled = !_3dEnabled;
            }
            if(GUILayout.Button("Refresh buildings")) {
                hasCreatedNewCopy = false;
            }
            if(GUILayout.Button("Refresh tiles")) {
                MapBox.instance.allTilesDirty();
                MapBox.instance.tilemap.redrawTiles(true);
            }
            GUI.backgroundColor = Color.red;
            if(lockCameraControl == true) {
                GUI.backgroundColor = Color.green;
            }
            if(GUILayout.Button("Lock camera: " + lockCameraControl)) {
                lockCameraControl = !lockCameraControl;
                Camera.main.transform.rotation = Quaternion.Euler(cameraX, cameraY, cameraZ);
            }
            GUI.backgroundColor = temp;
            if(GUILayout.Button("Reset camera")) {
                Camera.main.transform.rotation = Quaternion.Euler(cameraX, cameraY, cameraZ);
            }
            try {
                ObjectPositioningButtons();
            }
            catch(Exception e) {

            }

            //ObjectRotationButtons();
            CameraControls();
            if(publicBuild == false) {
                /*
                if(GUILayout.Button("Spawn 100 cloud hurricane (C)") || Input.GetKeyDown(KeyCode.C)) {
                    SpawnCloudsInCircle(MapBox.instance.getMouseTilePos(), 100); // hurricane spin is bugged, so this is disabled
                }
                */
                if(GUILayout.Button("tile3d: " + tile3Denabled)) {
                    tile3Denabled = !tile3Denabled;
                }
                if(Input.GetKeyUp(KeyCode.R)) {
                    if(activeLines != null && activeLines.Count > 1) {
                        foreach(LineRenderer line in activeLines) {
                            line.SetVertexCount(0);
                        }
                    }
                    if(tileTypeLines != null && tileTypeLines.Count > 1) {
                        foreach(LineRenderer line2 in tileTypeLines) {
                            line2.SetVertexCount(0);
                        }
                    }
                    singleLine.SetVertexCount(0);
                    deleteAllExtraLayers(); // sprite thickening reset
                }
                if(GUILayout.Button("Single line per tile type")) {
                    /*
                    int scaleFactor = 5;
                    if(tileTypeLines == null) {
                        tileTypeLines = new List<LineRenderer>();

                    }
                    int currentVertexCount = 0;
                   //fuck this for now
                    // List<TileTypeBase> tileTypes = AssetManager.tiles.list.AddRange(AssetManager.topTiles.list);

                    foreach(TileType tileType in) {
                        List<WorldTile> tilesOfType = new List<WorldTile>();
                        foreach(WorldTile tile in MapBox.instance.tilesList) {
                            if(tile.Type == tileType) {
                                tilesOfType.Add(tile);
                            }
                        }
                        LineRenderer tileTypesNewLine = new GameObject("Line").AddComponent<LineRenderer>();
                        LineRenderer lineRenderer = tileTypesNewLine;
                        lineRenderer.SetWidth(1f, 1f);
                        lineRenderer.SetColors(tileType.color, tileType.color);
                        Material whiteDiffuseMat = new Material(Shader.Find("UI/Default"));
                        lineRenderer.material = whiteDiffuseMat;
                        lineRenderer.material.color = tileType.color;
                        lineRenderer.SetVertexCount(tilesOfType.Count);
                        for(int i = 2; i < tilesOfType.Count; i++) {
                            WorldTile tile = tilesOfType[i];
                            lineRenderer.SetPosition(i - 2, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f));
                            lineRenderer.SetPosition(i - 1, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f) + new Vector3(0, 0, -10));
                            currentVertexCount++;
                        }
                        tileTypeLines.Add(lineRenderer);
                    }
                    */
                }
                if(GUILayout.Button("Single line for all tiles")) {
                    int scaleFactor = 5;
                    Color color1 = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                    Color color2 = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                    if(singleLine == null) {
                        singleLine = new GameObject("Line").AddComponent<LineRenderer>();
                    }
                    LineRenderer lineRenderer = singleLine;
                    lineRenderer.SetWidth(1f, 1f);
                    lineRenderer.SetVertexCount(MapBox.instance.tilesList.Length);
                    lineRenderer.SetColors(color1, color2);
                    Material whiteDiffuseMat = new Material(Shader.Find("UI/Default"));
                    lineRenderer.material = whiteDiffuseMat;
                    lineRenderer.material.color = color1;
                    for(int i = 2; i < MapBox.instance.tilesList.Length; i++) {
                        WorldTile tile = MapBox.instance.tilesList[i];
                        lineRenderer.SetPosition(i - 2, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f));
                        lineRenderer.SetPosition(i - 1, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f) + new Vector3(0, 0, -10));
                    }
                }
                if(GUILayout.Button("Single line for all tiles except liquid")) {
                    int scaleFactor = 5;
                    if(singleLine == null) {
                        singleLine = new GameObject("Line").AddComponent<LineRenderer>();
                    }
                    LineRenderer lineRenderer = singleLine;
                    lineRenderer.SetWidth(1f, 1f);
                    lineRenderer.SetVertexCount(MapBox.instance.tilesList.Length);
                    lineRenderer.SetColors(Color.green, Color.red);
                    Material whiteDiffuseMat = new Material(Shader.Find("UI/Default"));
                    lineRenderer.material = whiteDiffuseMat;
                    lineRenderer.material.color = Color.green;
                    for(int i = 2; i < MapBox.instance.tilesList.Length; i++) {
                        if(!MapBox.instance.tilesList[i].Type.liquid) {
                            WorldTile tile = MapBox.instance.tilesList[i];
                            lineRenderer.SetPosition(i - 2, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f));
                            lineRenderer.SetPosition(i - 1, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f) + new Vector3(0, 0, -10));
                            activeLines.Add(lineRenderer);
                        }
                    }
                }
                if(GUILayout.Button("Line for every tile on the map")) {
                    int scaleFactor = 5;
                    foreach(WorldTile tile in MapBox.instance.tilesList) {
                        // if (!tile.Type.water)
                        // {
                        Color tileColor = tile.getColor();
                        LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                        lineRenderer.SetWidth(1f, 1f);
                        lineRenderer.startColor = tileColor;
                        lineRenderer.endColor = tileColor;
                        lineRenderer.SetColors(tileColor, tileColor);
                        lineRenderer.SetVertexCount(2);
                        lineRenderer.SetPosition(0, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f));
                        lineRenderer.SetPosition(1, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f) + new Vector3(0, 0, -10));
                        Material whiteDiffuseMat = new Material(Shader.Find("UI/Default"));
                        lineRenderer.material = whiteDiffuseMat;
                        lineRenderer.material.color = tileColor;
                        // lineRenderer.sortingOrder = 50;
                        activeLines.Add(lineRenderer);
                        // }
                    }
                }
                if(GUILayout.Button("Line for every tile on the map except water")) {
                    int scaleFactor = 5;
                    foreach(WorldTile tile in MapBox.instance.tilesList) {
                        if(!tile.Type.liquid) {
                            Color tileColor = tile.getColor();
                            LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                            lineRenderer.SetWidth(1f, 1f);
                            lineRenderer.startColor = tileColor;
                            lineRenderer.endColor = tileColor;
                            lineRenderer.SetColors(tileColor, tileColor);
                            lineRenderer.SetVertexCount(2);
                            lineRenderer.SetPosition(0, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f));
                            lineRenderer.SetPosition(1, new Vector3(tile.posV3.x, tile.posV3.y, (-(float)tile.Height / scaleFactor) - 150f) + new Vector3(0, 0, -10));
                            Material whiteDiffuseMat = new Material(Shader.Find("UI/Default"));
                            lineRenderer.material = whiteDiffuseMat;
                            lineRenderer.material.color = tileColor;
                            // lineRenderer.sortingOrder = 50;
                            activeLines.Add(lineRenderer);
                        }
                    }
                }
                if(GUILayout.Button("Thicken tiles")) {
                    foreach(WorldTile tile in MapBox.instance.tilesList) {
                        for(int i = 1; i < 5; i++) {
                            GameObject tileRepresentation = new GameObject();
                            tileRepresentation.AddComponent<SpriteRenderer>().sprite = tile.curGraphics.sprite;
                            tileRepresentation.transform.position = new Vector3(tile.posV3.x, tile.posV3.y - i, -(tile.Height / dividerAmount));
                        }
                    }
                }
                if(GUILayout.Button("Save snapshot")) {
                    foreach(WorldTile tile in MapBox.instance.tilesList) {
                        tileSprites.Add(tile.curGraphics.sprite, new Vector3(tile.posV3.x, tile.posV3.y, -(tile.Height / dividerAmount)));
                    }
                    foreach(Building building in MapBox.instance.buildings.ToList()) {
                        WorldTile tile = building.currentTile;
                        buildingSprites.Add(building.spriteRenderer.sprite, new Vector3(building.currentPosition.x, building.currentPosition.y, -(tile.Height / dividerAmount)));
                    }
                }
                //clear/empty map in between or risk massive performance loss
                if(GUILayout.Button("Load snapshot")) {
                    foreach(KeyValuePair<Sprite, Vector3> tileEntry in tileSprites) {
                        GameObject tileRepresentation = new GameObject();
                        tileRepresentation.AddComponent<SpriteRenderer>().sprite = tileEntry.Key;
                        tileRepresentation.transform.position = tileEntry.Value;
                    }
                }
            }

            GUI.DragWindow();
        }

        public static Dictionary<Sprite, Vector3> tileSprites = new Dictionary<Sprite, Vector3>();
        public static Dictionary<Sprite, Vector3> buildingSprites = new Dictionary<Sprite, Vector3>();
        public static Dictionary<Sprite, Vector3> actorSprites = new Dictionary<Sprite, Vector3>();


        public static Dictionary<string, int> buildingCustomHeight = new Dictionary<string, int>();
        public static Dictionary<string, int> buildingCustomThickness = new Dictionary<string, int>();
        public static Dictionary<string, int> buildingCustomAngle = new Dictionary<string, int>();

        public void SettingSetup()
        {
            regularThickeningScale = Config.AddSetting("3D - Scaling", "Regular Thickness", 7, "How many extra layers the buildings get");
            regularThickeningCityMultiplier = Config.AddSetting("3D - Scaling", "City Multiplier", 2, "Multiplier for town buildings");
        }
        public static ConfigEntry<int> regularThickeningScale {
            get; set;
        }
        public static ConfigEntry<int> regularThickeningCityMultiplier {
            get; set;
        }
        public const string pluginGuid = "cody.worldbox.3d";
        public const string pluginName = "WorldBox3D";
        public const string pluginVersion = "0.0.0.4";
        public float rotationRate = 2f;
        public float manipulationRate = 0.01f;
        public float cameraX => Camera.main.transform.rotation.x;
        public float cameraY => Camera.main.transform.rotation.y;
        public float cameraZ;
        public Transform cameraTransform => Camera.main.transform;
        public List<LineRenderer> activeLines = new List<LineRenderer>();
        bool firstRun;
        static bool finishedLoading;
        public bool autoPlacement;
        public static bool _3dEnabled;


        public Vector3 RandomCircle(Vector3 center, float radius, int a)
        {
            Debug.Log(a);
            float ang = a;
            Vector3 pos;
            pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
            pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
            pos.z = center.z;
            return pos;
        }

        /* cloud code changed, plus this feature wasnt used much anyway
        public void SpawnCloudsInCircle(WorldTile centerTile, int count)
        {
            Vector3 center = centerTile.posV3; //transform.position;
            for(int i = 0; i < count; i++) {
                int a = i * 30;
                Vector3 pos = RandomCircle(center, 30f, a);
                Cloud cloud = MapBox.instance.cloudController.getNext();
                cloud.setScale(Toolbox.randomFloat(0f, 0.1f));
                cloud.tile = MapBox.instance.GetTile((int)center.x, (int)center.y);
                cloud.transform.localPosition = new Vector3(pos.x, pos.y, -Toolbox.randomFloat(1f, 10f));
                hurricaneList.Add(cloud);
                // Instantiate(prefab, pos, Quaternion.identity);
            }
        }
        */

        public static int dividerAmount = 10;

        // tile3d setup
        public static bool setTile_Prefix(WorldTile pWorldTile, Vector3Int pVec, Tile pGraphic, TilemapExtended __instance)
        {
            if(tile3Denabled) {
                List<Vector3Int> vec = __instance._vec;
                List<Tile> tiles = __instance._tiles;
                Tile curGraphics = pWorldTile.curGraphics;

                pVec.z = (-pWorldTile.Height) / dividerAmount; // main change, everything else is recreation/replacement

                if(curGraphics == pGraphic && pGraphic != null) {
                    return false;
                }
                curGraphics = pGraphic;
                vec.Add(pVec);
                tiles.Add(pGraphic);
                return false;
            }
            return true;
        }

        public static bool tile3Denabled;
        public static List<Cloud> hurricaneList = new List<Cloud>();
        public static void update_CloudPostfix(float pElapsed, Cloud __instance)
        {
            if(_3dEnabled) {
                __instance.transform.position = new Vector3(__instance.transform.position.x, __instance.transform.position.y, -20f);
            }
            if(hurricaneList.Contains(__instance)) {
                __instance.transform.RotateAround(__instance.tile.posV3, Vector3.forward, 20 * Time.deltaTime * Toolbox.randomFloat(0f, 5f));
            }
            else {
                __instance.transform.Translate(-(1f * pElapsed), 0f, 0f);
            }
        }

        public void Update()
        {
            window3DRect.height = 0f;

        }
        public void Awake()
        {
            Debug.Log("WorldBox3D loaded");
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(Actor), "updatePos");
            MethodInfo patch = AccessTools.Method(typeof(_3D_Main), "updateActorPos_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Cloud), "update");
            patch = AccessTools.Method(typeof(_3D_Main), "update_CloudPostfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("Post patch: Cloud.update");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(SpriteAnimation), "update");
            patch = AccessTools.Method(typeof(_3D_Main), "updateSpriteAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("updateSpriteAnimation_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Drop), "updatePosition");
            patch = AccessTools.Method(typeof(_3D_Main), "updateDropPos_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("updateDropPos_Prefix");


            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Actor), "punchTargetAnimation");
            patch = AccessTools.Method(typeof(_3D_Main), "punchTargetAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("punchTargetAnimation");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(ActorBase), "updateFlipRotation");
            patch = AccessTools.Method(typeof(_3D_Main), "updateFlipRotation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("updateFlipRotation_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(ActorBase), "updateRotationBack");
            patch = AccessTools.Method(typeof(_3D_Main), "updateRotationBack_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("updateRotationBack_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(TilemapExtended), "setTile");
            patch = AccessTools.Method(typeof(_3D_Main), "setTile_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("setTile_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BaseEffect), "setAlpha"); // applies to clouds, explosions, fireworks
            patch = AccessTools.Method(typeof(_3D_Main), "setAlpha_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("setAlpha_Postfix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BurnedTilesLayer), "setTileDirty");
            patch = AccessTools.Method(typeof(_3D_Main), "setTileDirty_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("setTileDirty_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(FireLayer), "setTileDirty");
            patch = AccessTools.Method(typeof(_3D_Main), "setTileDirty_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("setTileDirty_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(GroupSpriteObject), "setRotation");
            //patch = AccessTools.Method(typeof(_3D_Main), "setRotation_Postfix");
            //harmony.Patch(original, new HarmonyMethod(patch));
            //Debug.Log("setTileDirty_Prefix");



            Debug.Log("Post patch: Actor.updatePos");
            SettingSetup();
        }
        public static bool setTileDirty_Prefix(WorldTile pTile)
        {
            if(_3dEnabled && tile3Denabled) {
                return false;
            }
            return true;
        }
        public static WorldTile tileFromVector(Vector3 input)
        {
            WorldTile returnTile = MapBox.instance.GetTile((int)input.x, (int)input.y);
            if(returnTile != null) {
                return returnTile;
            }
            return null;
        }

        public int BuildingThickness(Building target)
        {
            int thickness = thickenCount;
            BuildingAsset stats = target.asset;
            if(buildingCustomThickness.ContainsKey(stats.id)) {
                thickness = buildingCustomThickness[stats.id];
            }
            return thickness;
        }

        public int BuildingAngle(Building target)
        {
            int angle = 90;
            BuildingAsset stats = target.asset;
            if(buildingCustomAngle.ContainsKey(stats.id)) {
                angle = buildingCustomAngle[stats.id];
            }
            if(angle > 360) {
                angle = 360;
            }
            return angle;
        }


        public float BuildingHeight(Building target)
        {
            float height = 3f;
            BuildingAsset stats = target.asset;

            if(buildingCustomHeight.ContainsKey(stats.id)) {
                height = buildingCustomHeight[stats.id];
            }
            if(tile3Denabled) {
                if(buildingCustomHeight.ContainsKey(stats.id)) {
                    height = buildingCustomHeight[stats.id] + (target.currentTile.Height / dividerAmount);

                }
                else {
                    height = target.currentTile.Height / dividerAmount;
                }
            }
            return -height;
        }

        public static void setAlpha_Postfix(float pVal, BaseEffect __instance) // applies to clouds, explosions, fireworks
        {
            if(_3dEnabled) {
                float height = 0f;
                WorldTile tile = tileFromVector(__instance.transform.localPosition);
                if(tile3Denabled && tile != null) {
                    height = (-tile.Height) / dividerAmount;
                }

                __instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, __instance.transform.localPosition.y, height);
                __instance.transform.rotation = Quaternion.Euler(-90, 0, 0); // Quaternion.Euler(-90, 0, 0);
            }
        }
        public static bool updateFlipRotation_Prefix(float pElapsed, ActorBase __instance)
        {
            if(_3dEnabled) {
                return false;
            }
            return true;
        }
        public static bool updateRotationBack_Prefix(float pElapsed, ActorBase __instance)
        {
            if(_3dEnabled) {
                return false;
            }
            return true;
        }
        /*
        public static bool updateRotation_Prefix(ActorBase __instance)
        {
            if (spriteReplacedActors.Contains(__instance))
            {
                return false;
            }
            return true;
        }
        */
        public static bool punchTargetAnimation_Prefix(Vector3 pDirection, bool pFlip, bool pReverse, float pAngle, Actor __instance)
        {
            if(_3dEnabled) {
                return false;
            }
            return true;
        }

        public static bool updateSpriteAnimation_Prefix(SpriteAnimation __instance)
        {
            if(_3dEnabled) {
                __instance.transform.rotation = Quaternion.Euler(-90, 0, 0); // Quaternion.Euler(-90, 0, 0);
                return true;
            }
            return true;
        }
        public static bool updateDropPos_Prefix(Drop __instance)
        {
            if(_3dEnabled) {
                __instance.transform.localPosition = new Vector3(__instance.currentPosition.x, __instance.currentPosition.y, -__instance.zPosition.z);
                return false;
            }
            return true;
        }
        public static void updateActorPos_Postfix(Actor __instance)
        {
            float height = 0f;
            if(tile3Denabled) {
                height = (-__instance.currentTile.Height) / dividerAmount;
            }
            if(_3dEnabled) {
                __instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, __instance.transform.localPosition.y, height);
            }
        }
        public static LineRenderer singleLine;
        public static List<LineRenderer> tileTypeLines;

        public bool publicBuild;

        public bool showHide3D;
        public Rect window3DRect;
        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 50, 120, 30));
            if(GUILayout.Button("WorldBox3D")) // "WorldBox3D"
            {
                showHide3D = !showHide3D;
            }
            GUILayout.EndArea();
            if(showHide3D) {
                GUI.contentColor = Color.white;
                window3DRect = GUILayout.Window(11015, window3DRect, window3D, "WorldBox3D", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
        }
        // unused thickening test
        /*
        public static SpriteRenderer createNewBodyPart_Postfix(SpriteRenderer __result, string pName)
        {
            for (int i = 0; i <= 100; i++)
            {
                SpriteRenderer newSprite = Instantiate(__result);

                newSprite.transform.localPosition = new Vector3(newSprite.transform.localPosition.x, newSprite.transform.localPosition.y + (float)i / 10f, newSprite.transform.localPosition.z);

            }
            return __result;
        }
        */
        public List<SpriteRenderer> extraLayers = new List<SpriteRenderer>();

        public void layerActorSprite(Actor targetActor)
        {
            SpriteRenderer spriteRenderer = targetActor.spriteRenderer;
            SpriteRenderer newSprite = Instantiate(spriteRenderer);
            SpriteRenderer newSprite2 = Instantiate(spriteRenderer);
            newSprite2.transform.rotation = Quaternion.Euler(0, 90, -90);
            extraLayers.Add(newSprite);
            extraLayers.Add(newSprite2);

        }
        public void layerBuildingSprite(Building targetBuilding, bool twoLayer = false)
        {
            float height = 0f;
            SpriteRenderer spriteRenderer = targetBuilding.spriteRenderer;
            if(twoLayer) {
                SpriteRenderer newSprite2 = Instantiate(spriteRenderer);
                newSprite2.transform.rotation = Quaternion.Euler(0, 90, -90);
                if(tile3Denabled) {
                    height = (-targetBuilding.currentTile.Height) / dividerAmount;
                    targetBuilding.curTransformPosition = new Vector3(targetBuilding.currentPosition.x, targetBuilding.currentPosition.y, height);
                }
                newSprite2.transform.position = new Vector3(targetBuilding.currentPosition.x, targetBuilding.currentPosition.y, height);
                extraLayers.Add(newSprite2);
            }
            SpriteRenderer newSprite = Instantiate(spriteRenderer);
            newSprite.transform.rotation = Quaternion.Euler(-90, 0, 0);
            if(tile3Denabled) {
                height = (-targetBuilding.currentTile.Height) / dividerAmount;
            }
            newSprite.transform.position = new Vector3(targetBuilding.currentPosition.x, targetBuilding.currentPosition.y, height);
            extraLayers.Add(newSprite);
            targetBuilding.m_transform.rotation = Quaternion.Euler(-90, 0, 0);
            targetBuilding.m_transform.position = new Vector3(targetBuilding.currentPosition.x, targetBuilding.currentPosition.y, height);
        }
        public void thickenActorSprite(Actor targetActor)
        {
            Actor newActor = Instantiate(targetActor); // why dont i use the original?? check later
            newActor.transform.parent = targetActor.transform;
            SpriteRenderer spriteRenderer = targetActor.spriteRenderer;
            /*
            // new stuff
            Sprite actorSprite = spriteRenderer.sprite;
            Mesh actorAsMesh = SpriteToMesh(actorSprite);
            MeshFilter filter = newActor.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshRenderer render = newActor.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            filter.mesh = actorAsMesh;
            //render.material = 
            Material newMaterial = new Material(spriteRenderer.material);
            //newMaterial.
            Graphics.DrawMesh(actorAsMesh, position: newActor.currentTile.posV3, rotation: Quaternion.Euler(-90, 0, 0), newMaterial, 0);
            // end new stuff
            */
            int distanceScaling = 25;
            for(int i = 0; i <= 5; i++) {
                SpriteRenderer newSprite = Instantiate(spriteRenderer);
                newSprite.transform.localPosition = spriteRenderer.transform.localPosition + new Vector3(0f, (0f + i) / distanceScaling, 0f);
                extraLayers.Add(newSprite);
            }
            for(int i = 0; i <= 5; i++) {
                SpriteRenderer newSprite = Instantiate(spriteRenderer);
                newSprite.transform.localPosition = spriteRenderer.transform.localPosition + new Vector3(0f, -(0f + i) / distanceScaling, 0f);
                extraLayers.Add(newSprite);
            }
        }
        public void thickenActorSpriteRotated(Actor targetActor)
        {
            Actor newActor = Instantiate(targetActor);
            newActor.transform.parent = targetActor.transform;
            newActor.transform.rotation = Quaternion.Euler(0, 90, -90);
            SpriteRenderer spriteRenderer = targetActor.spriteRenderer;
            int distanceScaling = 25;
            for(int i = 0; i <= 15; i++) {
                SpriteRenderer newSprite = Instantiate(spriteRenderer);
                newSprite.transform.localPosition = spriteRenderer.transform.localPosition + new Vector3((0f + i) / distanceScaling, 0f, 0f);
                extraLayers.Add(newSprite);
            }
            for(int i = 0; i <= 15; i++) {
                SpriteRenderer newSprite = Instantiate(spriteRenderer);
                newSprite.transform.localPosition = spriteRenderer.transform.localPosition + new Vector3(-(0f + i) / distanceScaling, 0f, 0f);
                extraLayers.Add(newSprite);
            }
        }
        public void deleteAllExtraLayers()
        {
            for(int i = 0; i < extraLayers.Count; i++) {
                Destroy(extraLayers[i]);
            }
            foreach(Building building in MapBox.instance.buildings) {
                building.spriteRenderer.enabled = true;
            }
        }
        public bool rotateBuildingBecauseAssetLoader(Building target) // assetload rotate buildings
        {
            bool returnBool = false;
            BuildingAsset stats = target.asset;
            if(buildingCustomAngle.ContainsKey(stats.id)) // weird combo to enable rotation, need something better
            {
                returnBool = true;
            }
            return returnBool;
        }

        int thickenCount = 3;
        int distanceScaling = 25;
        // upgradelevel assigned through assetloader, custom thickness for 3d (stats.upgradeLevel + 1) * 4; neat value
        public void thickenBuilding(Building targetBuilding)
        {
            int layerCount = 0;
            if(rotateBuildingBecauseAssetLoader(targetBuilding)) {
                thickenBuildingRotated(targetBuilding);
            }
            else {
                BuildingAsset stats = targetBuilding.asset;
                layerCount = BuildingThickness(targetBuilding);
                if(layerCount <= 5) {
                    layerCount = 5;
                }// whew this could be simpler
                if(targetBuilding.city != null) {
                    layerCount *= regularThickeningCityMultiplier.Value;
                }
                SpriteRenderer spriteRenderer = targetBuilding.spriteRenderer;
                spriteRenderer.transform.rotation = Quaternion.Euler(-90, 0, 0);
                for(int i = 0; i <= layerCount; i++) {
                    SpriteRenderer newSprite = Instantiate(spriteRenderer);
                    newSprite.transform.localPosition = targetBuilding.transform.localPosition + new Vector3(0f, (0f + i) / distanceScaling, 0f);
                    extraLayers.Add(newSprite);
                }
                for(int i = 0; i <= layerCount; i++) {
                    SpriteRenderer newSprite = Instantiate(spriteRenderer);
                    newSprite.transform.localPosition = targetBuilding.transform.localPosition + new Vector3(0f, -(0f + i) / distanceScaling, 0f);
                    extraLayers.Add(newSprite);
                }
            }
        }
        public void thickenBuildingRotated(Building targetBuilding)
        {
            int layerCount = 0;
            BuildingAsset stats = targetBuilding.asset;
            layerCount = BuildingThickness(targetBuilding);
            if(layerCount <= 5) {
                layerCount = 5;
            }// whew this could be simpler
            if(targetBuilding.city != null) {
                layerCount *= regularThickeningCityMultiplier.Value;
            }
            SpriteRenderer spriteRenderer = targetBuilding.spriteRenderer;
            spriteRenderer.transform.rotation = Quaternion.Euler(0, 90, -90);
            for(int i = 0; i <= layerCount; i++) {
                SpriteRenderer newSprite = Instantiate(spriteRenderer);
                newSprite.transform.localPosition = spriteRenderer.transform.localPosition + new Vector3((0f + i) / distanceScaling, 0f, 0f);
                extraLayers.Add(newSprite);
            }
            for(int i = 0; i <= layerCount; i++) {
                SpriteRenderer newSprite = Instantiate(spriteRenderer);
                newSprite.transform.localPosition = spriteRenderer.transform.localPosition + new Vector3(-(0f + i) / distanceScaling, 0f, 0f);
                extraLayers.Add(newSprite);
            }
        }
        public void ObjectPositioningButtons()
        {
            PositionAndRotateUnits();
            PositionAndRotateBuildings();
            if(GUILayout.Button("Thickened Sprite - Buildings - Normal")) {
                foreach(Building building in MapBox.instance.buildings) {
                    thickenBuilding(building);
                }
            }
            if(lockCameraControl == false) {
                if(GUILayout.Button("1 Rotated Sprite - Actors")) {
                    foreach(Actor actor in MapBox.instance.units) {
                        layerActorSprite(actor);
                    }
                }
                if(GUILayout.Button("1 Rotated Sprite - Buildings")) {
                    foreach(Building building in MapBox.instance.buildings) {
                        layerBuildingSprite(building);
                    }
                }
            }
            if(GUILayout.Button("Reset extra sprites")) {
                deleteAllExtraLayers();
            }
            /*
             *             if (GUILayout.Button("Thickened Sprite - Buildings - Rotated"))
            {
                foreach (Building building in MapBox.instance.buildings)
                {
                    thickenBuildingRotated(building, regularThickeningScale.Value);
                }
            }

            if (GUILayout.Button("Thickened Sprite - Actors - Normal"))
            {
                foreach (Actor actor in MapBox.instance.units)
                {
                    thickenActorSprite(actor);
                }
            }
            if (GUILayout.Button("Thickened Sprite - Actors - Rotated"))
            {
                foreach (Actor actor in MapBox.instance.units)
                {
                    thickenActorSpriteRotated(actor);
                }
            }
            */
            if(publicBuild == false) {
                if(GUILayout.Button("Place everything on linedRender")) {
                    foreach(Building building in MapBox.instance.buildings) {
                        WorldTile currentTile = building.currentTile;
                        //MapObjectShadow shadow = building.shadow;
                        if(currentTile != null) {
                            building.transform.position = new Vector3(building.transform.position.x, building.transform.position.y, (-(float)currentTile.Height / 5) - 150f) + new Vector3(0, 0, -10);
                            building.transform.rotation = Quaternion.Euler(-90, 0, 0);
                            //shadow.transform.position = new Vector3(shadow.transform.position.x, shadow.transform.position.y, (-(float)currentTile.Height / 5) - 150f) + new Vector3(0, 0, -10);
                            //shadow.transform.position = new Vector3(shadow.transform.position.x, shadow.transform.position.y, 0f);
                        }
                    }
                }
            }
        }
        public bool hasCreatedNewCopy;
        private void PositionAndRotateBuildings()
        {
            if(Toolbox.randomChance(manipulationRate)) {
                deleteAllExtraLayers();
                hasCreatedNewCopy = false;
            }
            if(autoPlacement) {
                if(!hasCreatedNewCopy) {
                    foreach(Building building in MapBox.instance.buildings) {
                        layerBuildingSprite(building);
                    }
                    hasCreatedNewCopy = true;
                    /*
                    building.transform.position = new Vector3(building.transform.position.x, building.transform.position.y, BuildingHeight(building));
                    if(rotateBuildingBecauseAssetLoader(building)) {
                        building.transform.rotation = Quaternion.Euler(0, 90, -90);
                    }
                    else {
                        building.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        SpriteRenderer spriteRenderer = building.spriteRenderer;
                        spriteRenderer.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    }
                    */
                }
            }
        }

        private void PositionAndRotateUnits()
        {
            if(autoPlacement && Toolbox.randomChance(manipulationRate)) {
                foreach(Actor actor in MapBox.instance.units) {
                    float height = 0f;

                    if(actor.currentTile != null) {
                        if(tile3Denabled) {
                            height = (-actor.currentTile.Height) / dividerAmount;
                        }
                        actor.transform.position = new Vector3(actor.transform.position.x, actor.transform.position.y, height);
                        SpriteRenderer spriteRenderer = actor.spriteRenderer;
                        spriteRenderer.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        //actor.transform.rotation = Quaternion.Euler(-90, 0, 0);

                        //Vector3 curAngle = (Vector3)Reflection.GetField(actor.GetType(), actor, "curAngle");
                        //curAngle.(actor.transform.rotation);
                        //Sprite s_item_sprite = Reflection.GetField(actor.GetType(), actor, "s_item_sprite") as Sprite;
                        //s_item_sprite..rotation = Quaternion.Euler(-90, 0, 0);


                    }
                }
            }
        }
        /*
        public static void setRotation_Postfix(Vector3 pVec, GroupSpriteObject __instance)
        {
            if(_3dEnabled)
                __instance.transform.rotation = Quaternion.Euler(-90, 0, 0);

            //__instance.transform.position = new Vector3(__instance.transform.position.x, __instance.transform.position.y, 0f);
            Transform m_transform = (Transform)Reflection.GetField(__instance.GetType(), __instance, "m_transform");
            if(_3dEnabled)
                m_transform.rotation = Quaternion.Euler(-90, 0, 0);
            //m_transform.position = new Vector3(m_transform.transform.position.x, m_transform.transform.position.y, 0f);
        }
        */
        public void ObjectRotationButtons()
        {
            if(GUILayout.Button("Actor flip")) {
                foreach(Actor actor in MapBox.instance.units) {
                    SpriteAnimation spriteAnimation = actor.sprite_animation;
                    spriteAnimation.transform.Rotate(-90, 0, 0); // was southup
                    actor.transform.position = new Vector3(actor.transform.position.x, actor.transform.position.y, 0f);
                }
            }
            if(GUILayout.Button("Building flip")) {
                foreach(Building building in MapBox.instance.buildings) {
                    SpriteRenderer spriteRenderer = building.spriteRenderer;
                    spriteRenderer.transform.Rotate(-90, 0, 0);
                }
            }
            if(GUILayout.Button("RotateXBuildings")) {
                foreach(Building building in MapBox.instance.buildings) {

                    building.transform.position = new Vector3(building.transform.position.x, building.transform.position.y, 0f);
                    SpriteRenderer spriteRenderer = building.spriteRenderer;
                    spriteRenderer.transform.Rotate(0, 90, 0);
                }
            }
            if(GUILayout.Button("RotateBuildingsXRandomized")) {
                foreach(Building building in MapBox.instance.buildings) {
                    if(Toolbox.randomBool()) {

                        building.transform.position = new Vector3(building.transform.position.x, building.transform.position.y, 0f);
                        SpriteRenderer spriteRenderer = building.spriteRenderer;
                        spriteRenderer.transform.Rotate(0, 15, 0);
                    }

                }
            }
            if(GUILayout.Button("RotateActorsX")) {
                foreach(Actor building in MapBox.instance.units) {

                    building.transform.position = new Vector3(building.transform.position.x, building.transform.position.y, 0f);
                    SpriteRenderer spriteRenderer = building.spriteRenderer;
                    spriteRenderer.transform.Rotate(0, 90, 0);
                }

            }
            if(GUILayout.Button("RotateActorsXRandomized")) {
                foreach(Actor actor in MapBox.instance.units) {
                    if(Toolbox.randomBool()) {
                        actor.transform.position = new Vector3(actor.transform.position.x, actor.transform.position.y, 0f);
                        SpriteRenderer spriteRenderer = actor.spriteRenderer;
                        spriteRenderer.transform.Rotate(0, 15, 0);
                    }

                }
            }
        }

        public bool lockCameraControl = true;
        public void CameraControls()
        {
            Camera.main.nearClipPlane = -500f; // makes world stop clipping when camera rotates
            //Camera.main.useOcclusionCulling = false;
            if(_3dEnabled) {
                if(Input.GetKey(KeyCode.LeftAlt)) {
                    float x = rotationRate * Input.GetAxis("Mouse Y");
                    float y = rotationRate * -Input.GetAxis("Mouse X");
                    if(lockCameraControl) y = 0;
                    Camera.main.transform.Rotate(x, y, 0);

                }
                if(lockCameraControl) {
                    if(Camera.main.transform.eulerAngles.x > 330) {
                        Camera.main.transform.rotation = Quaternion.Euler(330, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z);
                    }
                    if(Camera.main.transform.eulerAngles.x < 275) {
                        Camera.main.transform.rotation = Quaternion.Euler(275, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z);
                    }
                }
                if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)) {
                    float x = rotationRate * Input.GetAxis("Mouse X");
                    float y = rotationRate * -Input.GetAxis("Mouse Y");
                    // MapBox.instance.GetTile(MapBox.width / 2, MapBox.height / 2); // CENTER TILE
                    Camera.main.transform.RotateAround(Input.mousePosition, -Vector3.up, rotationRate * Input.GetAxis("Mouse Y"));
                    Camera.main.transform.RotateAround(Vector3.zero, transform.right, rotationRate * Input.GetAxis("Mouse X"));
                }
            }

        }
    }


}
