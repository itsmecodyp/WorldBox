using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using SimplerGUI.Submods.WorldBox3D;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using System.IO;

namespace SimplerGUI.Submods.WorldBox3D {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [Obsolete]
    class _3D_Main : BaseUnityPlugin {
        
        [HarmonyPatch(typeof(GlowParticles), "spawn", new Type[] {typeof(float), typeof(float), typeof(bool)})]
        class ParticleSpawnPatch
        {
            public static bool Prefix(float pX, ref float pY, bool pRemoveCooldown, GlowParticles __instance)
            {
                if (_3dEnabled)
                {
                    __instance.particles.startSpeed = -5;
                }
                return true;
            }
        }


        //this at least prevents actor becoming flat again...
        //but now some of them are upside down?
        public static bool baseUpdateRotation_Prefix(ActorBase __instance)
		{
			if(_3dEnabled) {
                __instance.curAngle.x = -90;
			}
            return true;
		}

        public void Reset3d()
        {
            foreach(Building building in MapBox.instance.buildings) {
                building.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            foreach(Actor actor in MapBox.instance.units) {
                if(actor.sprite_animation != null) {
                    actor.sprite_animation.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
            }
            if(activeLines != null && activeLines.Count > 1) {
                foreach(LineRenderer line in activeLines) {
                    line.SetVertexCount(0);
                }
            }
            if(singleLine != null)
            {
                singleLine.SetVertexCount(0);
            }
            deleteAllExtraLayers(); // sprite thickening reset
            if (Camera.main != null) Camera.main.transform.rotation = Quaternion.Euler(cameraX, cameraY, cameraZ);
        }

        public async void window3D(int windowID)
        {
            Color temp = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if(autoPlacement == true) {
                GUI.backgroundColor = Color.green;
            }
            if(GUILayout.Button("Enable 3D: " + autoPlacement)) {
                //Camera.main.transparencySortMode = TransparencySortMode.Perspective;
                autoPlacement = !autoPlacement;
                _3dEnabled = !_3dEnabled;

                if(autoPlacement == false) {
                    Reset3d();
                    if (Camera.main != null) Camera.main.transform.rotation = Quaternion.Euler(cameraX, cameraY, cameraZ);
                }
            }
            GUI.backgroundColor = Color.red;
            if (tile3Denabled == true)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button("tile3d:" + tile3Denabled))
            {
                tile3Denabled = !tile3Denabled;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("DividerAmount: "))
            {
            }
            dividerAmount = Convert.ToInt32(GUILayout.TextField(dividerAmount.ToString()));
            GUILayout.EndHorizontal();
            if (tile3Denabled)
            {
                GUI.backgroundColor = Color.red;
                if (useThiccTile3D == true)
                {
                    GUI.backgroundColor = Color.green;
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("ThiccTile3D: " + useThiccTile3D))
                {
                    useThiccTile3D = !useThiccTile3D;
                }
                thiccTile3DAmount = Convert.ToInt32(GUILayout.TextField(thiccTile3DAmount.ToString()));
                GUILayout.EndHorizontal();
            }
            GUI.backgroundColor = Color.red;
            if(lockCameraControl == true) {
                GUI.backgroundColor = Color.green;
            }
            if(GUILayout.Button("Lock camera: " + lockCameraControl)) {
                lockCameraControl = !lockCameraControl;
                if(Camera.main != null) Camera.main.transform.rotation = Quaternion.Euler(cameraX, cameraY, cameraZ);
            }
            GUI.backgroundColor = temp;
            if(GUILayout.Button("Reset camera")) {
                if(Camera.main != null) Camera.main.transform.rotation = Quaternion.Euler(cameraX, cameraY, cameraZ);
            }
            if (GUILayout.Button("Send map"))
            {
                await SaveMapDataAsync();
            }
            CameraControls();
            if(publicBuild == false) {
                try
                {
                    ObjectRotationButtons();
                }
                catch (Exception e)
                {
                    Debug.Log("Exception:" + e.Message);
                }

            }
            GUI.DragWindow();
        }

        //chat-gpt
        public async Task SaveMapDataAsync()
        {
            MapData data = new MapData();
            HashSet<Vector3> posSet = new HashSet<Vector3>(); // Use HashSet for faster containment checks
            object lockObject = new object(); // Create a lock object

            // Parallelize the loop
            Parallel.ForEach(MapBox.instance.tilesList, tile =>
            {
                Vector3 pos = tile.posV3;
                pos.z = tile.Height / dividerAmount;

                lock (lockObject) // Synchronize access to the HashSet
                {
                    if (posSet.Add(pos)) // Add to HashSet directly, returns true if added successfully (i.e., not already contained)
                    {
                        data.posList.Add(pos);
                        data.idList.Add(tile.Type.id);
                    }
                }
            });

            // Batch writing to file using StreamWriter
            await Task.Run(() =>
            {
                using (StreamWriter writer = new StreamWriter("C:/test/test.json"))
                {
                    writer.Write(JsonUtility.ToJson(data));
                }
            });
        }

        /*
        //save map data for external use
        public void SaveMapData()
        {
            MapData data = new MapData();
            foreach(WorldTile tile in MapBox.instance.tilesList.ToList())
            {
                Vector3 pos = tile.posV3;
                pos.z = tile.Height / dividerAmount;

                if (data.posList.Contains(pos) == false)
                {
                    data.posList.Add(pos);
                    data.idList.Add(tile.Type.id);
                }
            }

            System.IO.File.WriteAllText("C:/test/test.json", JsonUtility.ToJson(data));
        }
        */
        [Serializable]
        public class MapData
        {
            public Dictionary<Vector3, string> tilePositions;
            //hack to rebuild dict since dict cant be serialized
            public List<Vector3> posList = new List<Vector3>();
            public List<string> idList = new List<string>();
        }

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
        public float cameraX
        {
            get
            {
                if (Camera.main != null) return Camera.main.transform.rotation.x;
                return 0;
            }
        }
        
        public float cameraY
        {
            get
            {
                if (Camera.main != null) return Camera.main.transform.rotation.y;
                return 0;
            }
        }

        public float cameraZ = 0f;
        public Transform cameraTransform
        {
            get
            {
                if (Camera.main != null) return Camera.main.transform;
                return null;
            }
        }

        public List<LineRenderer> activeLines = new List<LineRenderer>();
        public bool autoPlacement;
        public static bool _3dEnabled;

        //false = show experimental stuff
        public bool publicBuild = true;


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

        public static int dividerAmount = 2;

        // tile3d setup
        public static bool setTile_Prefix(WorldTile pWorldTile, Vector3Int pVec, Tile pGraphic, TilemapExtended __instance)
        {
            if (tile3Denabled)
            {
                //tilesAtPos = new Dictionary<Vector3, Sprite>();
                List<Vector3Int> vec = __instance._vec;
                List<Tile> tiles = __instance._tiles;
                Tile curGraphics = pWorldTile.curGraphics;

                pVec.z = (-pWorldTile.Height) / dividerAmount; // main change, everything else is recreation/replacement

                if (curGraphics == pGraphic && pGraphic != null)
                {
                    return false;
                }
                curGraphics = pGraphic;
                vec.Add(pVec);
                tiles.Add(pGraphic);

                if (useThiccTile3D)
                {
                    //doesnt work
                    int tileCloneAmount = thiccTile3DAmount;

                    while (tileCloneAmount > 0)
                    {
                        vec.Add(pVec + new Vector3Int(0, 0, tileCloneAmount));
                        tiles.Add(pGraphic);
                        tileCloneAmount--;
                    }
                    /* doesnt work
                    for (int i = 0; i < thiccTile3DAmount; i++)
                    {
                        vec.Add(pVec + new Vector3Int(0, 0, thiccTile3DAmount));
                        tiles.Add(pGraphic);
                    }
                    */
                    /* DOES WORK
                    vec.Add(pVec + new Vector3Int(0, 0, 1));
                    tiles.Add(pGraphic);
                    vec.Add(pVec + new Vector3Int(0, 0, 2));
                    tiles.Add(pGraphic);
                    vec.Add(pVec + new Vector3Int(0, 0, 3));
                    tiles.Add(pGraphic);
                    vec.Add(pVec + new Vector3Int(0, 0, 4));
                    tiles.Add(pGraphic);
                    */
                }
                return false;
            }
            //new try
            if (tilesAtPos != null && tilesAtPos.ContainsKey(pVec) == false && pGraphic.sprite != null)
            {
                tilesAtPos.Add(pVec, pGraphic.sprite);
            }
            return true;
        }

        public static bool useThiccTile3D;
        public static int thiccTile3DAmount = 4;

        public static Dictionary<Vector3, Sprite> tilesAtPos = new Dictionary<Vector3, Sprite>();

        public static bool tile3Denabled;
        public static List<Cloud> hurricaneList = new List<Cloud>();
        public static void update_CloudPostfix(float pElapsed, Cloud __instance)
        {
            if(_3dEnabled) {
                __instance.transform.position = new Vector3(__instance.transform.position.x, __instance.transform.position.y, -30f);
            }
            if(hurricaneList.Contains(__instance)) {
                __instance.transform.RotateAround(__instance.tile.posV3, Vector3.forward, 20 * Time.deltaTime * Toolbox.randomFloat(0f, 5f));
            }
            else {
                //why did i make clouds go backwards??
                //__instance.transform.Translate(-(1f * pElapsed), 0f, 0f);
            }
        }

        public void Update()
        {
            window3DRect.height = 0f;
            if(autoPlacement)
            ObjectPositioning();
        }
        public void Awake()
        {
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(Actor), "updatePos");
            MethodInfo patch = AccessTools.Method(typeof(_3D_Main), "updateActorPos_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Cloud), "update");
            patch = AccessTools.Method(typeof(_3D_Main), "update_CloudPostfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(ActorBase), "updateRotation");
            patch = AccessTools.Method(typeof(_3D_Main), "baseUpdateRotation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));


            

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BuildingSmokeEffect), "update");
            patch = AccessTools.Method(typeof(_3D_Main), "smokeUpdate_Prefix");
            //harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(SpriteAnimation), "update");
            patch = AccessTools.Method(typeof(_3D_Main), "updateSpriteAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Drop), "updatePosition");
            patch = AccessTools.Method(typeof(_3D_Main), "updateDropPos_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));


            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Actor), "punchTargetAnimation");
            patch = AccessTools.Method(typeof(_3D_Main), "punchTargetAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(ActorBase), "updateFlipRotation");
            patch = AccessTools.Method(typeof(_3D_Main), "updateFlipRotation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(ActorBase), "updateRotationBack");
            patch = AccessTools.Method(typeof(_3D_Main), "updateRotationBack_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(TilemapExtended), "setTile");
            patch = AccessTools.Method(typeof(_3D_Main), "setTile_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BaseEffect), "setAlpha"); // applies to clouds, explosions, fireworks
            patch = AccessTools.Method(typeof(_3D_Main), "setAlpha_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BurnedTilesLayer), "setTileDirty");
            patch = AccessTools.Method(typeof(_3D_Main), "setTileDirty_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(FireLayer), "setTileDirty");
            patch = AccessTools.Method(typeof(_3D_Main), "setTileDirty_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(GroupSpriteObject), "setRotation");
            //patch = AccessTools.Method(typeof(_3D_Main), "setRotation_Postfix");
            //harmony.Patch(original, new HarmonyMethod(patch));
            //Debug.Log("setTileDirty_Prefix");


            harmony.PatchAll();
            SettingSetup();
        }
        
        public static bool smokeUpdate_Prefix(float pElapsed, BuildingSmokeEffect __instance)
        {
            if(_3dEnabled)
            {
                if (__instance.building.asset.smoke && !__instance.building.isUnderConstruction())
                {
                    if (__instance.smokeTimer > 0f)
                    {
                        __instance.smokeTimer -= Time.deltaTime;
                        return false;
                    }
                    __instance.smokeTimer = __instance.building.asset.smokeInterval;
                    World.world.particlesSmoke.spawn(__instance.centerTopVec.x, __instance.centerTopVec.y, true);
                }
                return false;
            }
            return true;
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
                if (__instance.controller != null && __instance.controller.asset.id == "fx_cloud")
                {
                    //clouds need specific rotation
                    __instance.transform.rotation = Quaternion.Euler(-90, 0, 0);
                }
                else
                {
                    //this works for everything else (except smoke and fire?)
                    __instance.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
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
                Vector3 position = new Vector3(__instance.currentPosition.x, __instance.currentPosition.y, -__instance._currentHeightZ);
                __instance.m_transform.position = position;
                //__instance.m_transform.rotation = Quaternion.Euler(0, 0, -90);
                //__instance.transform.localPosition = new Vector3(__instance.currentPosition.x, __instance.currentPosition.y, __instance.zPosition.z);
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
        //public static List<LineRenderer> tileTypeLines;
        
        public bool showHide3D;
        public Rect window3DRect;

        public bool showSubMod = true;
        public void OnGUI()
        {
            if (showSubMod)
            {
                if (GUI.Button(new Rect(Screen.width - 120, 40, 95, 20), "WorldBox3D"))
                {
                    showHide3D = !showHide3D;
                }
                if (GUI.Button(new Rect(Screen.width - 25, 40, 25, 20), "x"))
                {
                    if (Camera.main != null) Camera.main.transform.rotation = Quaternion.Euler(cameraX, cameraY, cameraZ);
                    autoPlacement = false;
                    _3dEnabled = false;
                    tile3Denabled = false;
                    showHide3D = false;
                    showSubMod = false;
                }
                if (showHide3D)
                {
                    GUI.contentColor = Color.white;
                    window3DRect = GUILayout.Window(11015, window3DRect, window3D, "WorldBox3D", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }
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
            //newSprite2.transform.rotation = Quaternion.Euler(0, 90, 90);
            newSprite2.transform.rotation = Quaternion.Euler(-90, 0, 0);
            extraLayers.Add(newSprite);
            extraLayers.Add(newSprite2);
        }
        public void layerBuildingSprite(Building targetBuilding)
        {
            float height = 0f;
            WorldTile tTile = targetBuilding.currentTile;
            SpriteRenderer spriteRenderer = targetBuilding.spriteRenderer;
            SpriteRenderer newSprite = Instantiate(spriteRenderer);

            /* experimental test to fix layering, FAILED
            spriteRenderer.sortingLayerName = "Tiles";
            newSprite.sortingLayerName = "Tiles";
            spriteRenderer.sortingOrder = tTile.Type.render_z;
            newSprite.sortingOrder = tTile.Type.render_z;
            */

            if (tile3Denabled) {
                height = (-targetBuilding.currentTile.Height) / dividerAmount;
            }

            //detect intended rotation from vanilla data, not sure of better way right now

            if (targetBuilding.asset.tower == false && targetBuilding.asset.tower_projectile == "90")
            {
                targetBuilding.m_transform.rotation = new Quaternion(-0.5f, 0.5f, -0.5f, 0.5f);
                newSprite.transform.rotation = new Quaternion(-0.5f, 0.5f, -0.5f, 0.5f);

            }
            else
            {
                targetBuilding.m_transform.rotation = Quaternion.Euler(-90, 0, 0);
                newSprite.transform.rotation = Quaternion.Euler(-90, 0, 0);
            }

            //do the same for height
            if (targetBuilding.asset.tower == false && targetBuilding.asset.tower_projectile_offset > 1f)
            {
                height = targetBuilding.asset.tower_projectile_offset;
            }
            targetBuilding.m_transform.position = new Vector3(targetBuilding.currentPosition.x, targetBuilding.currentPosition.y, height);
            newSprite.transform.position = new Vector3(targetBuilding.currentPosition.x, targetBuilding.currentPosition.y, height);
            extraLayers.Add(newSprite);

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
            newActor.transform.rotation = Quaternion.Euler(0, 90, 90);
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
            spriteRenderer.transform.rotation = Quaternion.Euler(0, 90, 90);
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
        public void ObjectPositioning()
        {
            PositionAndRotateUnits();
            PositionAndRotateBuildings();
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
            
        }

        public bool lockCameraControl = true;
        public void CameraControls()
        {
            Camera.main.nearClipPlane = -500f; // makes world stop clipping when camera rotates
            //Camera.main.useOcclusionCulling = false;
            if(_3dEnabled) {
                if(Input.GetKey(KeyCode.LeftAlt)) {
                    float x = rotationRate * Input.GetAxis("Mouse Y");
                    float y = rotationRate * Input.GetAxis("Mouse X");
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
                    float y = rotationRate * Input.GetAxis("Mouse Y");
                    // MapBox.instance.GetTile(MapBox.width / 2, MapBox.height / 2); // CENTER TILE
                    Camera.main.transform.RotateAround(Input.mousePosition, Vector3.up, rotationRate * Input.GetAxis("Mouse Y"));
                    Camera.main.transform.RotateAround(Vector3.zero, transform.right, rotationRate * Input.GetAxis("Mouse X"));
                }
            }

        }
    }
}
