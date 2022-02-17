using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using System.Collections.Generic;
using System.Collections;

namespace MapSizes
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    class Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.map.sizes";
        public const string pluginName = "MapSizes";
        public const string pluginVersion = "0.0.0.3";
        public static int mapSizeX = 4;
        public static int mapSizeY = 4;
        public bool showHideMapSizeWindow;
        public Rect mapSizeWindowRect;
        public static string filename = "picture";
        public void Awake()
        {

            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(GeneratorTool), "applyTemplate");
            MethodInfo patch = AccessTools.Method(typeof(Main), "applyTemplate_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Pre patch: GeneratorTool.applyTemplate");

            original = AccessTools.Method(typeof(MapBox), "setMapSize");
            patch = AccessTools.Method(typeof(Main), "setMapSize_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Pre patch: MapBox.setMapSize");

            original = AccessTools.Method(typeof(MapBox), "finishMakingWorld");
            patch = AccessTools.Method(typeof(Main), "finishMakingWorld_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("Post patch: MapBox.finishMakingWorld");

            Debug.Log("MapSizes loaded");
        }
        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 25, 120, 30));
            if(GUILayout.Button("Map Sizes")) {
                showHideMapSizeWindow = !showHideMapSizeWindow;
            }
            if(showHideMapSizeWindow) {
                mapSizeWindowRect = GUILayout.Window(102, mapSizeWindowRect, new GUI.WindowFunction(mapSizesWindow), "Map Stuff", new GUILayoutOption[]
                    {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                    });
            }
            GUILayout.EndArea();
        }
        public bool syncResize;
        public void mapSizesWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button("Map size x: " + mapSizeX.ToString());
            GUILayout.Button("Map size y: " + mapSizeY.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("-")) {
                mapSizeX--;
                if(mapSizeX < 0) {
                    mapSizeX = 0;
                }
            }
            if(GUILayout.Button("+")) {
                mapSizeX++;
            }
            if(GUILayout.Button("-")) {
                mapSizeY--;
                if(mapSizeY < 0) {
                    mapSizeY = 0;
                }
            }
            if(GUILayout.Button("+")) {
                mapSizeY++;
            }
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Regenerate map")) {
                Debug.Log("MapSizes: regenerating map");
                //MapBox.instance.setMapSize(mapSizeX, mapSizeY);
                MapBox.instance.clickGenerateNewMap("islands");
                //MapBox.instance.finishMakingWorld();
            }
            if(GUILayout.Button("Resize current map")) ResizeCurrentMap();
            if(GUILayout.Button("copy map")) CopyMap();
            if(GUILayout.Button("clear map")) ClearMap();
            if(GUILayout.Button("paste map")) PasteMap();
            GUILayout.BeginHorizontal();
            GUILayout.Button("PicSizeX");
            pictureSizeX = (int)GUILayout.HorizontalSlider((float)pictureSizeX, 1f, 2000f);
            if(syncResize) {
                pictureSizeY = pictureSizeX;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("PicSizeY");
            pictureSizeY = (int)GUILayout.HorizontalSlider((float)pictureSizeY, 1f, 2000f);
            if(syncResize) {
                pictureSizeX = pictureSizeY;
            }
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Sync buttons: " + syncResize.ToString())) {
                syncResize = !syncResize;
            }
            filename = GUILayout.TextField(filename);
            if(File.Exists(imagePath)) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;

            }
            if(GUILayout.Button("Regenerate " + filename + ".png")) {
                MapBox.instance.setMapSize(mapSizeX, mapSizeY);
                imageToMap(filename); // why does this run twice?
                startingPicture = true;
                MapBox.instance.CallMethod("generateNewMap", new object[] { "earth" });
                MapBox.instance.finishMakingWorld();
            }
            GUI.DragWindow();
        }

        public void ClearMap()
        {
            TileType ocean = AssetManager.tiles.get("deep_ocean");
            foreach(WorldTile tile in MapBox.instance.tilesList) {
                MapAction.terraformMain(tile, ocean, AssetManager.terraform.get("flash"));
            }
        }

        public void CopyMap()
        {
            tilesCache.Clear();
            buildingCache.Clear();
            foreach(WorldTile tile in MapBox.instance.tilesList) {
                tilesCache.Add(tile.pos, tile.Type.id);
			}
            foreach(Building building in MapBox.instance.buildings) {
                buildingCache.Add(building.currentTile.pos, building);
			}

        }

        public void CreateBuilding(Building fromBuilding)
        {
            BuildingAsset fromStats = Reflection.GetField(fromBuilding.GetType(), fromBuilding, "stats") as BuildingAsset;
            BuildingData fromData = Reflection.GetField(fromBuilding.GetType(), fromBuilding, "data") as BuildingData;
            if(fromStats.type == "trees") {
                MapBox.instance.CallMethod("tryGrowVegetationRandom", new object[] { fromBuilding.currentTile, VegetationType.Trees, false, false });
            }
			else {
                Building building = MapBox.instance.CallMethod("addBuilding", new object[] { fromStats.id, fromBuilding.currentTile, fromData, false, false, BuildPlacingType.New }) as Building;
                building.CallMethod("updateBuild", new object[] { 100 });
                WorldTile currentTile = Reflection.GetField(building.GetType(), building, "currentTile") as WorldTile;
                if(currentTile.zone.city != null) {
                    building.CallMethod("setCity", new object[] { currentTile.zone.city, false });
                }
                if(building.city != null) {
                    building.city.addBuilding(building);
                    building.city.status.homesTotal += fromStats.housing * (fromStats.upgradeLevel + 1);
                    if(building.city.status.population > building.city.status.homesTotal) {
                        building.city.status.homesOccupied = building.city.status.homesTotal;
                    }
                    else {
                        building.city.status.homesOccupied = building.city.status.population;
                    }
                    building.city.status.homesFree = building.city.status.homesTotal - building.city.status.homesOccupied;
                }
            }
           
        }

        public List<string> tiles;
        public List<string> tilesTop;


        public void PasteMap()
        {
            if(tiles == null) {
                tilesTop = new List<string>();
                tiles = new List<string>();
                foreach(TileType tiletype1 in AssetManager.tiles.list) {
                    tiles.Add(tiletype1.id);
				}
                foreach(TopTileType tiletype2 in AssetManager.topTiles.list) {
                    tilesTop.Add(tiletype2.id);
                }
            }

            // logs spam hard from the checks in wrong libraries..
            foreach(Vector2Int tilePos in tilesCache.Keys) {
                WorldTile targetTile = MapBox.instance.GetTile(tilePos.x, tilePos.y);
                if(targetTile != null) {
                    if(tiles.Contains(tilesCache[tilePos])) {
                        MapAction.terraformMain(targetTile, AssetManager.tiles.get(tilesCache[tilePos]), AssetManager.terraform.get("flash"));
                    }
                    else {
                        if(tilesTop.Contains(tilesCache[tilePos])) {
                            MapAction.terraformTop(targetTile, AssetManager.topTiles.get(tilesCache[tilePos]), AssetManager.terraform.get("flash"));
                        }
                        else {
                            // tile type not found
                        }
                    }
				}
            }
            foreach(Vector2Int tilePos2 in buildingCache.Keys) {
                WorldTile targetTile = MapBox.instance.GetTile(tilePos2.x, tilePos2.y);
                if(targetTile != null) {
                    CreateBuilding(buildingCache[tilePos2]);
                }
            }
        }


        public Dictionary<Vector2Int, string> tilesCache= new Dictionary<Vector2Int, string>(); // tilePos, tileType
        public Dictionary<Vector2Int, Building> buildingCache = new Dictionary<Vector2Int, Building>(); // tilePos, building

        public List<WorldTile> tileList;
        public void ResizeCurrentMap()
		{
            CopyMap();
            hasFinishedLoading = false;
            MapBox.instance.clickGenerateNewMap("islands");
            waitingForLoading = true;
        }
        public static bool waitingForLoading = false;
        public void Update()
		{
            if(waitingForLoading) {
                if(hasFinishedLoading) {
                    ResizeFinish();
                    hasFinishedLoading = false;
                }
			}
		}

        public void ResizeFinish()
        {
            ClearMap();
            PasteMap();
        }

        public static string imagePath => Directory.GetCurrentDirectory() + "\\WorldBox_Data//images//" + filename + ".png";
        public static bool startingPicture;
        public static bool applyTemplate_Prefix(string pTexture, float pMod = 1f)
        {
            if (startingPicture && File.Exists(imagePath))
            {
                imageToMap(filename);
                startingPicture = false;
                return false;
            }
            return true;
        }

        public static bool setMapSize_Prefix(ref int pWidth, ref int pHeight)
        {
            pWidth = mapSizeX;
            pHeight = mapSizeY;
            return true;
        }

        public static void finishMakingWorld_Postfix()
        {
            hasFinishedLoading = true;
        }
        public static bool hasFinishedLoading = false;
        // for later: public static Dictionary<WorldTile, Color> customTileColors = new Dictionary<WorldTile, Color>();
        public static int pictureSizeX = 100;
        public static int pictureSizeY = 100;
        public static void imageToMap(string imageName)
        {
            // ModTest.isMapLoadedFromPicture = true;
            // Texture2D texture2D = (Texture2D)Resources.Load("mapTemplates/earth"); // default earth picture
            string path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//images//" + imageName + ".png"; // picture to convert
            Texture2D texture2D2 = null;
            byte[] data = File.ReadAllBytes(path);
            texture2D2 = new Texture2D(2, 2);
            texture2D2.LoadImage(data);
            TextureScale.Bilinear(texture2D2, pictureSizeX, pictureSizeY); 
            for (int i = 0; i < texture2D2.width; i++)
            {
                for (int j = 0; j < texture2D2.height; j++)
                {
                    WorldTile tile = MapBox.instance.GetTile(i, j);
                    if (tile != null)
                    {
                        int num2 = (int)((1f - texture2D2.GetPixel(i, j).g) * 255f); // change tile according to pixel color
                        tile.Height += num2;
                    }
                }
            }
            // tile.data.tileMinimapColor = texture2D2.GetPixel(i, j); // for when minimap showing pic is important
        }
    }
}
