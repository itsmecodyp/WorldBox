using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable CS0649

namespace SimplerGUI.Menus
{
    class GUIWorld
    {
        public bool hasRun;
        public void firstRunT()
        {
            Config.MODDED = true;
            hasRun = true;
        }
        public float bench() // maxims benchmark
        {
            return Time.realtimeSinceStartup;
        }

        public void BenchEnd(string message, float prevTime) // maxims benchmark
        {
            float time = Time.realtimeSinceStartup - prevTime;
        }

        public float BenchEndTime(float prevTime) // maxims benchmark
        {
            float time = Time.realtimeSinceStartup - prevTime;
            return time;
        }

        public static void undoLastReplaceInList()
        {
            if (listOfUndos.Count < 1)
            {
                Debug.Log("no undo to replace");
                return;
            }
            if (listOfUndos.Last() == null)
            {
                Debug.Log("no undo to replace");
                return;
            }
            List<WorldTile> targets = listOfUndos.Last();
            string newType = listOfUndoTypes.Last();
            replaceAllTilesOnMap("last", newType, targets);

            listOfUndoTypes.Remove(listOfUndoTypes.Last()); // remove type from end of list
            listOfUndos.Remove(listOfUndos.Last()); // remove list of undo tiles
        }

        public static void replaceAllTilesOnMap(string oldType, string newType, List<WorldTile> targetList = null)
        {
            if (targetList == null)
            {
                targetList = MapBox.instance.tilesList.ToList();
            }
            bool replacingToTopType = false;
            if (GuiMain.listOfTopTileTypes.Contains(newType))
            {
                replacingToTopType = true;
            }
            TileType tileType = null;
            TopTileType topTileType = null;
            if (replacingToTopType)
            {
                topTileType = AssetManager.topTiles.get(newType);
            }
            else
            {
                tileType = AssetManager.tiles.get(newType);
            }
            List<WorldTile> undoTiles = new List<WorldTile>();
            foreach (WorldTile tile in targetList)
            {
                if (oldType == "last")
                {// we are undoing, skip type check and just assign tiles
                    if (replacingToTopType)
                    {
                        MapAction.terraformTile(tile, tile.main_type, topTileType, null);
                    }
                    else
                    {
                        MapAction.terraformTile(tile, tileType, null, null);
                    }
                }
                else
                {
                    if (tile.main_type.id == oldType || (tile.top_type != null && tile.top_type.id == oldType))
                    {
                        if (replacingToTopType)
                        {
                            MapAction.terraformTile(tile, tile.main_type, topTileType, null);
                        }
                        else
                        {
                            MapAction.terraformTile(tile, tileType, null, null);
                        }
                        undoTiles.Add(tile);
                    }
                }

            }
            if (undoTiles.Count > 0)
            {
                listOfUndos.Add(undoTiles);
                listOfUndoTypes.Add(oldType);
            }
        }

        public void thingInCircle(Vector3 location, float radius, int howMany)
        {
            for (int i = 0; i < howMany; i++)
            {
                float angle = i * Mathf.PI * 2f / radius;
                Vector3 newPos = location + (new Vector3(Mathf.Cos(angle) * radius, -2, Mathf.Sin(angle) * radius));
            }
        }

        public List<WorldTile> CheckTilesBetween2(WorldTile target1, WorldTile target2)
        {
            List<WorldTile> tilesToCheck = new List<WorldTile>(); // list for later
            Vector2Int pos1 = target1.pos;
            Vector2Int pos2 = target2.pos;
            float distanceBetween = Toolbox.DistTile(target1, target2);
            int pSize = (int)distanceBetween;
            PixelFlashEffects flashEffects = MapBox.instance.flashEffects;
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
                    flashEffects.flashPixel(tile, 10);
                }
                return tilesToCheck;
            }

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
                                flashEffects.flashPixel(tile, 10);
                                tilesToCheck.Add(tile);
                            }
                        }
                    }
                }
                return tilesToCheck;
            }
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
                if (GuiMain.windowInUse == -1)
                {
                    if (Input.GetMouseButtonDown(0) && !temp)
                    {
                        if (MapBox.instance.getMouseTilePos() != null)
                        {
                            lastSelectedTiles = null;
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
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        startTile = null;
                        endTile = null;
                        lastSelectedTiles = null;
                    }
                    if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
                    {
                        CopyTileSelection();
                        WorldTip.showNowCenter("Tiles copied");
                    }
                    if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl))
                    {
                        PasteTileSelection();
                        WorldTip.showNowCenter("Tiles pasted");
                    }
                    if (pastedTerrains.Count > 0 && Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
                    {
                        UndoTilePaste();
                        WorldTip.showNowCenter("Tile paste undone");
                    }
                    if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftControl))
                    {
                        RotateTileSelectionLeft();
                        WorldTip.showNowCenter("Copied tiles rotated left");
                    }
                    if (Input.GetKeyDown(KeyCode.E) && Input.GetKey(KeyCode.LeftControl))
                    {
                        RotateTileSelectionRight();
                        WorldTip.showNowCenter("Copied tiles rotated right");
                    }
                    if (Input.GetKeyDown(KeyCode.Slash))
                    {
                        //ReorderTileSelection();
                    }
                }
            }
            worldOptionsRect.height = 0f;
        }

        public void CreatePngOutOfTileSelection()
        {
            //Texture2D texture2D = new Texture2D(texture.width, texture.height);
            //Color32[] pixels = texture.GetPixels32();
            //texture2D.SetPixels32(pixels);
            //texture2D.Apply();
            //return texture2D;
        }

        //tile copy paste, buildings, cityzones, actors come later (if ever)
        Dictionary<Vector2Int, string> tilesCopied;
        public void CopyTileSelection()
        {
            tilesCopied = new Dictionary<Vector2Int, string>();
            Vector2Int firstPos = startTile.pos;
            foreach (WorldTile tile in lastSelectedTiles)
            {
                Vector2Int tilePos = tile.pos;
                Vector2Int offsetPos = tilePos - firstPos;
                string tileType = tile.main_type.id + "`";
                if (tile.top_type != null)
                {
                    tileType += tile.top_type.id;
                }
                tilesCopied.Add(offsetPos, tileType);
            }
            /* preview of copied tiles using new texture, need to find a way to remove unused pixels
			//setup texture width/height for preview of copied tiles
			//whole section here will be moved to its own method later so rotations can recalculate
			int difx = dif(startTile.x, endTile.x) + 1; //width of selection
			int dify = dif(startTile.y, endTile.y) + 1; //height of selection
			//actually lets use use whole world size and only set section for now
			Texture2D texture = MapBox.instance.worldLayer.texture;
			Texture2D texture2D = new Texture2D(texture.width, texture.height);
			//Texture2D texture2D = new Texture2D(difx, dify);
			foreach(KeyValuePair<Vector2Int, string> tile in tilesCopied) {
				Vector2Int pos = tile.Key;
				Vector2Int newPos = firstPos + pos;
				WorldTile newTileSpot = MapBox.instance.GetTile(newPos.x, newPos.y);
				//texture2D.Compress(false);
				texture2D.SetPixel(newPos.x, newPos.y, newTileSpot.getColor());
				texture2D.Apply();
				lastCopiedTexture = texture2D;
			}
			//UnityEngine.Debug.Log("Difx:" + difx.ToString() + "/Dify:" + dify.ToString());
			*/
        }

        public static Texture2D lastCopiedTexture;

        public void RotateTileSelectionRight()
        {
            Dictionary<Vector2Int, string> newSelectionAfterEdit = new Dictionary<Vector2Int, string>();
            foreach (KeyValuePair<Vector2Int, string> tile in tilesCopied)
            {
                Vector2Int pos = tile.Key;
                Vector2Int newPos = new Vector2Int(pos.y, -pos.x);
                newSelectionAfterEdit.Add(newPos, tile.Value);
            }
            tilesCopied = newSelectionAfterEdit;
        }

        public void RotateTileSelectionLeft()
        {
            Dictionary<Vector2Int, string> newSelectionAfterEdit = new Dictionary<Vector2Int, string>();
            foreach (KeyValuePair<Vector2Int, string> tile in tilesCopied)
            {
                Vector2Int pos = tile.Key;
                Vector2Int newPos = new Vector2Int(-pos.y, pos.x);
                newSelectionAfterEdit.Add(newPos, tile.Value);
            }
            tilesCopied = newSelectionAfterEdit;
        }

        public void PasteTileSelection()
        {
            Vector2Int firstPos = MapBox.instance.getMouseTilePos().pos;

            Dictionary<Vector2Int, string> pasteToUndo = new Dictionary<Vector2Int, string>();

            foreach (KeyValuePair<Vector2Int, string> tile in tilesCopied)
            {
                Vector2Int newPos = firstPos + tile.Key;
                string[] types = tile.Value.Split('`');
                if (terrainPasteIgnoresWater && (types[0].Contains("ocean") || types[0].Contains("water")))
                {

                }
                else if (terrainPasteIgnoresLava && types[0].Contains("lava"))
                {

                }
                else
                {
                    WorldTile newTileSpot = MapBox.instance.GetTile(newPos.x, newPos.y);
                    if (newTileSpot != null)
                    {
                        string tileType = newTileSpot.main_type.id + "`";
                        if (newTileSpot.top_type != null)
                        {
                            tileType += newTileSpot.top_type.id;
                        }
                        pasteToUndo.Add(newTileSpot.pos, tileType);
                        if (types[1] != string.Empty)
                        {
                            MapAction.terraformTile(newTileSpot, AssetManager.tiles.get(types[0]), AssetManager.topTiles.get(types[1]));
                        }
                        else
                        {
                            MapAction.terraformMain(newTileSpot, AssetManager.tiles.get(types[0]));
                        }
                    }
                }
            }
            pastedTerrains.Add(pasteToUndo);
        }

        public void UndoTilePaste()
        {
            Dictionary<Vector2Int, string> pasteToUndo = pastedTerrains.Last();
            foreach (KeyValuePair<Vector2Int, string> tile in pasteToUndo)
            {
                string[] types = tile.Value.Split('`');

                WorldTile newTileSpot = MapBox.instance.GetTile(tile.Key.x, tile.Key.y);
                if (newTileSpot != null)
                {
                    if (types[1] != string.Empty)
                    {
                        MapAction.terraformTile(newTileSpot, AssetManager.tiles.get(types[0]), AssetManager.topTiles.get(types[1]));
                    }
                    else
                    {
                        MapAction.terraformMain(newTileSpot, AssetManager.tiles.get(types[0]));
                    }
                }
            }
            pastedTerrains.Remove(pastedTerrains.Last());
        }

        List<Dictionary<Vector2Int, string>> pastedTerrains = new List<Dictionary<Vector2Int, string>>();


        public void worldUpdate()
        {
            if (Config.gameLoaded && !SmoothLoader.isLoading())
            {
                if (hasRun == false)
                {
                    firstRunT();
                }
                DragSelectionUpdate();
                PixelFlashEffects flashEffects = MapBox.instance.flashEffects;
                if (lastSelectedTiles != null)
                {
                    foreach (WorldTile tile in lastSelectedTiles)
                    {
                        flashEffects.flashPixel(tile, 10);
                    }
                }
                //wip
                if (fillToolEnabled)
                {
                    if (MapBox.instance.getMouseTilePos() != null)
                    {
                        if (Input.GetMouseButtonDown(0) && selectedButton() != null)
                        {
                            if (activeFill != null)
                            {
                                Debug.Log("active fill in progress, stopping");
                                return;
                            }
                            activeFill = new List<WorldTile>();
                            fillOriginalTile = MapBox.instance.getMouseTilePos();
                            activeFill.Add(fillOriginalTile);
                            fillOriginalType = fillOriginalTile.Type;
                            alreadyChanged = new List<WorldTile>();
                        }
                    }
                }
                PopulationControlUpdateNew();
            }

        }


        public void worldOptionsWindowUpdate()
        {
            if (SimpleSettings.showHideWorldOptionsConfig)
            {
                GUI.contentColor = Color.white;
                worldOptionsRect = GUILayout.Window(1010, worldOptionsRect, worldOptionsWindow, "World Options", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                if (showRacePurgeWindow)
                {
                    racePurgeWindowRect = GUILayout.Window(30305, racePurgeWindowRect, racePurgeWindow, "Race Purge", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    racePurgeWindowRect.position = new Vector2(worldOptionsRect.x + worldOptionsRect.width, (worldOptionsRect.y));
                }

                if (showTileWindowLeft)
                {
                    tileWindowLeftRect = GUILayout.Window(30306, tileWindowLeftRect, tileSelectionWindowLeft, "TileType1 Selection", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    tileWindowLeftRect.position = new Vector2(worldOptionsRect.x - tileWindowLeftRect.width, (worldOptionsRect.y));
                }
                if (showTileWindowRight)
                {
                    tileWindowRightRect = GUILayout.Window(30307, tileWindowRightRect, tileSelectionWindowRight, "TileType2 Selection", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    tileWindowRightRect.position = new Vector2(worldOptionsRect.x + worldOptionsRect.width, (worldOptionsRect.y));
                }

                if (showNaturalSpawnWindow)
                {
                    naturalSpawnWindowRect = GUILayout.Window(30308, naturalSpawnWindowRect, NaturalSpawnWindow, "Minimum spawns", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    naturalSpawnWindowRect.position = new Vector2(worldOptionsRect.x + worldOptionsRect.width, (worldOptionsRect.y));
                }
                if (showRaceCapWindow)
                {
                    raceCapWindowRect = GUILayout.Window(30308, raceCapWindowRect, RaceCapWindow, "Race limits", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    raceCapWindowRect.position = new Vector2(worldOptionsRect.x + worldOptionsRect.width, (worldOptionsRect.y));
                }
            }
            if (enabledPopulationCap)
            {
                List<Actor> actorList = MapBox.instance.units.ToList();
                for (int i = populationCap; i < actorList.Count; ++i)
                {
                    // Do something with list[i]
                    actorList[i].killHimself(true, AttackType.Other, false, false, false);
                }
            }

            if (buildingLimitEnabled && MapBox.instance.buildings.Count > buildingLimit)
            {
                List<Building> buildingList = MapBox.instance.buildings.ToList();
                for (int i = buildingList.Count - 2; i > buildingLimit; i--)
                {
                    buildingList[i].startDestroyBuilding();
                }
            }
        }

        public static bool showRacePurgeWindow;
        public Rect racePurgeWindowRect;
        public Rect tileWindowLeftRect;
        public Rect tileWindowRightRect;
        public Rect naturalSpawnWindowRect;
        public Rect raceCapWindowRect;


        public Vector2 scrollPositionRacePurge;
        public Vector2 scrollPositionTileWindowLeft;
        public Vector2 scrollPositiotTileWindowRight;
        public Vector2 scrollPositiotAssetMin;
        public Vector2 scrollPositiotRaceMax;


        public static bool showPopulationControl;
        public static bool showTileWindowLeft;
        public static bool showTileWindowRight;
        public static bool showNaturalSpawnWindow;
        public static bool showRaceCapWindow;

        public void NaturalSpawnWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            GUILayout.Button("Value to set: " + currentValueMin);
            GUILayout.BeginHorizontal();
            currentValueMin = (int)GUILayout.VerticalSlider(currentValueMin, 50, 0);
            scrollPositiotAssetMin = GUILayout.BeginScrollView(
    scrollPositiotAssetMin, GUILayout.Width(300f), GUILayout.Height(200f));
            foreach (string assetID in GuiMain.listOfActorAssetIDs)
            {
                bool isActive = actorAssetMinimumSpawns.ContainsKey(assetID);
                if (isActive)
                {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button(assetID + ": " + actorAssetMinimumSpawns[assetID]))
                    {
                        actorAssetMinimumSpawns.Remove(assetID);
                    }
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button(assetID))
                    {
                        actorAssetMinimumSpawns.Add(assetID, currentValueMin);
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
        public static int currentValueMin;
        public static int currentValueMax;

        public static Dictionary<string, int> actorAssetMinimumSpawns = new Dictionary<string, int>();
        //cannot find actor of certain statsID, but can find actor with certain raceID
        //can create actor of certain statsID, but cannot create actor of certain raceID
        public static Dictionary<string, int> actorAssetMaximumSpawns = new Dictionary<string, int>();

        public static Dictionary<string, int> actorAssetCountCurrent = new Dictionary<string, int>();
        public static Dictionary<string, int> raceAssetCountCurrent = new Dictionary<string, int>();

        public void RaceCapWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            GUILayout.Button("Value to set: " + currentValueMax);
            GUILayout.BeginHorizontal();
            currentValueMax = (int)GUILayout.VerticalSlider(currentValueMax, 200, 0);

            scrollPositiotRaceMax = GUILayout.BeginScrollView(
    scrollPositiotRaceMax, GUILayout.Width(300f), GUILayout.Height(200f));
            foreach (string assetID in GuiMain.listOfActorAssetIDs)
            {
                bool isActive = actorAssetMaximumSpawns.ContainsKey(assetID);
                if (isActive)
                {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button(assetID + ": " + actorAssetMaximumSpawns[assetID]))
                    {
                        actorAssetMaximumSpawns.Remove(assetID);
                    }
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button(assetID))
                    {
                        actorAssetMaximumSpawns.Add(assetID, currentValueMax);
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

        }

        //parent this to world window on right side
        public void racePurgeWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            scrollPositionRacePurge = GUILayout.BeginScrollView(
        scrollPositionRacePurge, GUILayout.Width(300f), GUILayout.Height(200f));
            foreach (string race in GuiMain.listOfActorAssetIDs)
            {
                if (GUILayout.Button(race))
                {
                    raceIDToPurge = race;
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        //the game does this through WorldBehaviourActions.updateUnitSpawn, uses tile type and its biome
        //too complicated for what i want right now, but maybe convert it later
        public static bool naturalSpawnNewRaces;
        public static bool capRaceSpawns;


        //add the finalized actor to the "count"
        public static void finalizeActor_Postfix(string pStats, Actor pActor, WorldTile pTile, float pZHeight)
        {
            if (actorAssetCountCurrent.ContainsKey(pStats) == false)
            {
                actorAssetCountCurrent.Add(pStats, 0);
            }
            actorAssetCountCurrent[pStats] += 1;
            if (raceAssetCountCurrent.ContainsKey(pActor.asset.race) == false)
            {
                raceAssetCountCurrent.Add(pActor.asset.race, 0);
            }
            raceAssetCountCurrent[pActor.asset.race] += 1;
        }

        public static void killHimself_Prefix(bool pDestroy, AttackType pType, bool pCountDeath, bool pLaunchCallbacks, bool pLogFavorite, Actor __instance)
        {
            if (__instance.isActor() && __instance.isAlive())
            {
                string pStats = __instance.asset.id;
                if (actorAssetCountCurrent.ContainsKey(pStats) == false)
                {
                    actorAssetCountCurrent.Add(pStats, 0);
                }
                actorAssetCountCurrent[pStats] -= 1;

                if (raceAssetCountCurrent.ContainsKey(__instance.asset.race) == false)
                {
                    raceAssetCountCurrent.Add(__instance.asset.race, 0);
                }
                raceAssetCountCurrent[__instance.asset.race] -= 1;
            }
        }



        public static bool spawnUnit_Prefix(WorldTile pTile, string pPowerID)
        {
            if (enabledPopulationCap)
            {//global cap
                if (MapBox.instance.units.Count > populationCap)
                {
                    return false;
                }
            }

            if (capRaceSpawns)// per race cap
            {
                string assetID = AssetManager.powers.get(pPowerID).actor_asset_id;
                if (string.IsNullOrEmpty(assetID))
                {
                    Debug.Log("race cap: asset null");
                    return false;
                }
                if (actorAssetMaximumSpawns.ContainsKey(assetID) == false)
                {
                    return true;
                }

                int current = 0;

                if (actorAssetCountCurrent.ContainsKey(assetID))
                {
                    current = actorAssetCountCurrent[assetID];

                }
                int max = actorAssetMaximumSpawns[assetID];
                if (max != 0 && current > max)
                {
                    return false;
                }
            }
            return true;
        }


        public static bool createNewUnit_Prefix(string pStatsID, WorldTile pTile, float pZHeight, Actor __instance)
        {
            if (enabledPopulationCap)
            {//global cap
                if (MapBox.instance.units.Count > populationCap)
                {
                    return false;
                }
            }

            if (capRaceSpawns)// per race cap
            {
                string assetID = pStatsID;

                if (actorAssetMaximumSpawns.ContainsKey(assetID) == false)
                {
                    return true;
                }

                int current = 0;

                if (actorAssetCountCurrent.ContainsKey(assetID))
                {
                    current = actorAssetCountCurrent[assetID];

                }
                int max = actorAssetMaximumSpawns[assetID];
                if (max != 0 && current > max)
                {
                    return false;
                }
            }
            return true;
        }


        public void PopulationControlUpdateNew()
        {
            //spawn actors with statsID if there are not enough on map
            if (naturalSpawnNewRaces)
            {
                foreach (string typeToSpawn in actorAssetMinimumSpawns.Keys)
                {
                    if (actorAssetCountCurrent.ContainsKey(typeToSpawn))
                    {
                        if (actorAssetCountCurrent[typeToSpawn] < actorAssetMinimumSpawns[typeToSpawn])
                        {
                            TileIsland randomIslandGround = World.world.islandsCalculator.getRandomIslandGround(true);
                            WorldTile worldTile;
                            if (randomIslandGround == null)
                            {
                                //3 attempts for random land tile, if none found just go with it
                                int attempt = 1;
                                worldTile = World.world.tilesList.GetRandom();
                                while (worldTile.Type.liquid == false && attempt < 4)
                                {
                                    worldTile = World.world.tilesList.GetRandom();
                                    attempt++;
                                }
                            }
                            else
                            {
                                MapRegion random = randomIslandGround.regions.GetRandom();
                                worldTile = ((random != null) ? random.tiles.GetRandom<WorldTile>() : null);
                            }

                            Debug.Log("Spawning unit of type: " + typeToSpawn);
                            World.world.units.createNewUnit(typeToSpawn, worldTile, 0f);
                            //count should update automatically
                        }
                    }
                    //asset hasnt been spawned yet
                    else
                    {
                        //force spawn 1 unit so the rest completes
                        World.world.units.createNewUnit(typeToSpawn, MapBox.instance.tilesList.GetRandom(), 0f);
                    }
                }
            }

            //disgusting way of killing actors when they exceed cap
            if (capRaceSpawns)
            {
                foreach (string assetType in actorAssetMaximumSpawns.Keys)
                {
                    if (actorAssetCountCurrent.ContainsKey(assetType))
                    {
                        if (actorAssetCountCurrent[assetType] > actorAssetMaximumSpawns[assetType])
                        {
                            int dif = actorAssetCountCurrent[assetType] - actorAssetMaximumSpawns[assetType];
                            //make list of targets to kill all at once
                            List<Actor> deathTargets = new List<Actor>();
                            foreach (Actor actor in MapBox.instance.units.ToArray())
                            {
                                if (dif > 0)
                                {
                                    if (actor.asset.id == assetType)
                                    {
                                        deathTargets.Add(actor);
                                        dif--;
                                    }
                                }
                                else
                                {
                                    //break loop early, does this work for foreach?
                                    break;
                                }
                            }
                            foreach (Actor dyingActor in deathTargets)
                            {
                                dyingActor.killHimself();
                            }
                        }
                    }
                }
            }
        }

        public void tileSelectionWindowLeft(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            scrollPositionTileWindowLeft = GUILayout.BeginScrollView(
     scrollPositionTileWindowLeft, GUILayout.Width(300f), GUILayout.Height(200f));
            foreach (string tileType in GuiMain.listOfTileTypes)
            {
                if (GUILayout.Button(tileType))
                {
                    tileType1 = tileType;
                }
            }
            foreach (string tileType in GuiMain.listOfTopTileTypes)
            {
                if (GUILayout.Button(tileType))
                {
                    tileType1 = tileType;
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        public void tileSelectionWindowRight(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            scrollPositiotTileWindowRight = GUILayout.BeginScrollView(
    scrollPositiotTileWindowRight, GUILayout.Width(300f), GUILayout.Height(200f));
            foreach (string tileType in GuiMain.listOfTileTypes)
            {
                if (GUILayout.Button(tileType))
                {
                    tileType2 = tileType;
                }
            }
            foreach (string tileType in GuiMain.listOfTopTileTypes)
            {
                if (GUILayout.Button(tileType))
                {
                    tileType2 = tileType;
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        public void worldOptionsWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            Color originalColor = GUI.backgroundColor;
            string showPopControl = "v Population Control v";
            if (showPopulationControl)
            {
                showPopControl = "^ Population Control ^";
            }
            if (showPopulationControl)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.yellow;

            }
            if (GUILayout.Button(showPopControl))
            {
                showPopulationControl = !showPopulationControl;
            }
            GUI.backgroundColor = originalColor;
            if (showPopulationControl)
            {
                if (enabledPopulationCap) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                populationCap = (int)GUILayout.HorizontalSlider(populationCap, 0, 10000);
                int num = populationCap;
                if (populationCap <= 0)
                {
                    populationCap = 1;
                }
                if (GUILayout.Button("World population limit: " + populationCap))
                {
                    populationCap = MapBox.instance.units.Count;
                    enabledPopulationCap = !enabledPopulationCap;
                }
                GUILayout.BeginHorizontal();
                if (naturalSpawnNewRaces) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Enable minLimits: " + naturalSpawnNewRaces))
                {
                    naturalSpawnNewRaces = !naturalSpawnNewRaces;
                }
                string buttonString3 = ">";
                if (showNaturalSpawnWindow) { buttonString3 = "<"; GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.yellow; }
                if (GUILayout.Button(buttonString3))
                {
                    showNaturalSpawnWindow = !showNaturalSpawnWindow;
                    if (showNaturalSpawnWindow)
                    {
                        showRacePurgeWindow = false;
                        showRaceCapWindow = false;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (capRaceSpawns) { GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.red; }
                if (GUILayout.Button("Enable MaxLimits: " + capRaceSpawns))
                {
                    capRaceSpawns = !capRaceSpawns;
                }
                string buttonString4 = ">";
                if (showRaceCapWindow) { buttonString4 = "<"; GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.yellow; }
                if (GUILayout.Button(buttonString4))
                {
                    showRaceCapWindow = !showRaceCapWindow;
                    if (showRaceCapWindow)
                    {
                        showRacePurgeWindow = false;
                        showNaturalSpawnWindow = false;
                    }
                }
                GUILayout.EndHorizontal();

                /* currently bugged, but also a terrible solution overall
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Enable race limits: " + capRaceSpawns))
                {
                    capRaceSpawns = !capRaceSpawns;
                }
                string buttonString4 = ">";
                if (showRaceCapWindow) { buttonString4 = "<"; GUI.backgroundColor = Color.green; }
                else { GUI.backgroundColor = Color.yellow; }
                if (GUILayout.Button(buttonString4))
                {
                    showRaceCapWindow = !showRaceCapWindow;
                }
                GUILayout.EndHorizontal();
                */
                GUI.backgroundColor = originalColor;
                if (buildingLimitEnabled)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                int num2 = buildingLimit;
                if (buildingLimit <= 0)
                {
                    buildingLimit = 1;
                }
                if (GUILayout.Button("Building limit: " + buildingLimit))
                {
                    buildingLimit = MapBox.instance.buildings.Count;
                    buildingLimitEnabled = !buildingLimitEnabled;
                }
                buildingLimit = (int)GUILayout.HorizontalSlider(buildingLimit, 0, 10000);
                GUI.backgroundColor = originalColor;
                if (GUILayout.Button("Kill all creatures"))
                {
                    foreach (Actor actor in MapBox.instance.units)
                    {
                        actor.killHimself();
                    }
                }
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Kill race:"))
                {
                    foreach (Actor actor2 in MapBox.instance.units)
                    {
                        Race actorRace = actor2.race;
                        if (actor2.asset.id == raceIDToPurge || actorRace.id == raceIDToPurge || actorRace.id.ToLower() == raceIDToPurge)
                        {
                            actor2.killHimself();
                        }
                    }
                }
                string buttonString = " >";
                if (showRacePurgeWindow)
                {
                    buttonString = " <";
                }
                if (GUILayout.Button(raceIDToPurge))
                {
                }
                if (showRacePurgeWindow)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.yellow;
                }
                if (GUILayout.Button(buttonString))
                {
                    showRacePurgeWindow = !showRacePurgeWindow;
                    if (showRacePurgeWindow)
                    {
                        showRaceCapWindow = false;
                        showNaturalSpawnWindow = false;
                    }
                }
                GUILayout.EndHorizontal();
                GUI.backgroundColor = originalColor;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Kill civs"))
                {
                    foreach (Actor actor3 in MapBox.instance.units)
                    {
                        Race actorRace = actor3.race;
                        if (actorRace.civilization)
                        {
                            actor3.killHimself();
                        }
                    }
                }
                if (GUILayout.Button("Kill beasts"))
                {
                    foreach (Actor actor4 in MapBox.instance.units)
                    {
                        Race actorRace = actor4.race;
                        if (!actorRace.civilization)
                        {
                            actor4.killHimself();
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            string buttonString1 = "<";
            if (showTileWindowLeft)
            {
                buttonString1 = ">";
            }
            if (showTileWindowLeft) { GUI.backgroundColor = Color.green; }
            else { GUI.backgroundColor = Color.yellow; }
            if (GUILayout.Button(buttonString1))
            {
                showTileWindowLeft = !showTileWindowLeft;
            }
            GUI.backgroundColor = originalColor;
            GUILayout.Button(tileType1);
            GUILayout.Button(tileType2);
            string buttonString2 = ">";
            if (showTileWindowRight)
            {
                buttonString2 = "<";
            }
            if (showTileWindowRight) { GUI.backgroundColor = Color.green; }
            else { GUI.backgroundColor = Color.yellow; }
            if (GUILayout.Button(buttonString2))
            {
                showTileWindowRight = !showTileWindowRight;
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = originalColor;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("replace tiles"))
            {
                replaceAllTilesOnMap(tileType1, tileType2);
                if (dragSelection)
                {

                }
            }
            if (GUILayout.Button("undo last"))
            {
                undoLastReplaceInList();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (dragSelection)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;

            }
            if (GUILayout.Button("Drag select"))
            {
                dragSelection = !dragSelection;
                if (dragSelection == false)
                {
                    /* let player reset this with hotkey/button
					 * most will disable drag select in order to try powers
					lastSelectedTiles = null;
					startTile = null;
					endTile = null;
					*/
                }
            }

            if (dragCircular)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;

            }
            if (GUILayout.Button("Circle shape"))
            {
                dragCircular = !dragCircular;
            }

            GUILayout.EndHorizontal();

            GUI.backgroundColor = originalColor;


            if (GUILayout.Button("Reset selection"))
            {
                lastSelectedTiles = null;
                dragSelection = false;
                startTile = null;
                endTile = null;
            }

            if (GUILayout.Button("Replace buildings in selection with construction"))
            {
                string buildingName = GuiMain.Construction.selectedBuildingAssetName;
                if (lastSelectedTiles != null)
                    foreach (WorldTile tile in lastSelectedTiles)
                    {
                        if (tile.building != null)
                        {
                            BuildingAsset selectedBuildingAsset = AssetManager.buildings.get(buildingName);
                            if (selectedBuildingAsset != null)
                            {
                                Building building = MapBox.instance.buildings.addBuilding(buildingName,
                                    tile.building.getConstructionTile());

                                /*
							if (AssetManager.buildings.get(buildingName).construction_site_texture != null)
							{
								building.CallMethod("updateBuild", new object[] { 100 });
							}
							*/
                                WorldTile currentTile = building.currentTile;
                                if (currentTile.zone.city != null)
                                {
                                    building.setCity(currentTile.zone.city);
                                }

                                if (building.city != null)
                                {
                                    building.city.addBuilding(building);
                                    building.city.status.housingTotal += selectedBuildingAsset.housing *
                                                                         (selectedBuildingAsset.upgradeLevel + 1);
                                    if (building.city.status.population > building.city.status.housingTotal)
                                    {
                                        building.city.status.housingOccupied = building.city.status.housingTotal;
                                    }
                                    else
                                    {
                                        building.city.status.housingOccupied = building.city.status.population;
                                    }

                                    building.city.status.housingFree = building.city.status.housingTotal -
                                                                       building.city.status.housingOccupied;
                                }
                            }
                        }
                    }
            }
            if (GUILayout.Button("Power on every tile in selection"))
            {
                if (lastSelectedTiles != null)
                    foreach (WorldTile tile in lastSelectedTiles)
                    {
                        if (selectedPower().id != null)
                        {
                            if (string.IsNullOrEmpty(selectedPower().tileType) == false)
                            {
                                MapAction.terraformMain(tile, AssetManager.tiles.get(selectedPower().tileType),
                                    AssetManager.terraform.get("flash"));
                            }
                            else if (string.IsNullOrEmpty(selectedPower().dropID) == false)
                            {
                                Drop newPixel = MapBox.instance.dropManager.spawn(tile, selectedPower().dropID, 5f);
                            }
                            //UsePower(tile, selectedPower().id);
                        }
                    }
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear all buildings"))
            {
                List<Building> buildingsToClear = MapBox.instance.buildings.ToList();
                if (dragSelection && lastSelectedTiles != null)
                {
                    List<Building> tempList = new List<Building>();
                    foreach (WorldTile tile in lastSelectedTiles)
                    {
                        if (tile.building != null && !tempList.Contains(tile.building))
                        {
                            tempList.Add(tile.building);
                        }
                    }
                    buildingsToClear = tempList;
                }
                foreach (Building building in buildingsToClear)
                {
                    building.startDestroyBuilding();
                }
            }
            if (GUILayout.Button("Clear natural buildings"))
            {
                List<Building> buildingsToClear = MapBox.instance.buildings.ToList();
                if (dragSelection && lastSelectedTiles != null)
                {
                    List<Building> tempList = new List<Building>();
                    foreach (WorldTile tile in lastSelectedTiles)
                    {
                        if (tile.building != null && !tempList.Contains(tile.building))
                        {
                            tempList.Add(tile.building);
                        }
                    }
                    buildingsToClear = tempList;
                }
                foreach (Building building in buildingsToClear)
                {
                    /*
					BuildingAsset stats = Reflection.GetField(building.GetType(), building, "stats") as BuildingAsset;
					if (stats.resourceType != ResourceType.None && stats.resourceType != ResourceType.Wheat)
					{
						building.CallMethod("startDestroyBuilding", new object[] { true });
					}
					*/
                }
            }
            GUILayout.EndHorizontal();
            /* repopulation copied vanilla generation code which has changed, needs redone
			GUILayout.BeginHorizontal();
			GUILayout.Button("Repopulate");
			if (GUILayout.Button("Plants"))
			{
				RepopulatePlants();
			}
			if (GUILayout.Button("Ores"))
			{
				RepopulateTerrain();
			}
			if (GUILayout.Button("All"))
			{
				RepopulateTerrain();
			}
			GUILayout.EndHorizontal();
			*/
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill/PaintBucket: " + fillToolEnabled))
            {
                fillToolEnabled = !fillToolEnabled;
            }
            /*
			if (GUILayout.Button("Undo last fill"))
			{
				undoLastFillInList();
			}
			*/// how to fix?
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Disable clouds: " + disableClouds))
            {
                disableClouds = !disableClouds;
            }
            if (terrainPasteIgnoresWater)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Terrain paste ignores ocean"))
            {
                terrainPasteIgnoresWater = !terrainPasteIgnoresWater;
            }
            if (terrainPasteIgnoresLava)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Terrain paste ignores lava"))
            {
                terrainPasteIgnoresLava = !terrainPasteIgnoresLava;
            }
            if (lastCopiedTexture != null)
            {
                GUILayout.Button(lastCopiedTexture);
            }
            GUI.DragWindow();
        }

        public static bool terrainPasteIgnoresWater;
        public static bool terrainPasteIgnoresLava;



        public static bool spawnNewUnit_Prefix(string pStatsID, WorldTile pTile, float pSpawnHeight = 6f)
        {
            if (!enabledPopulationCap) { return true; }
            if (MapBox.instance.units.Count > populationCap)
            {
                return false;
            }
            return true;
        }

        public static bool destroyObject_Prefix(Actor pActor)
        {
            if (!enabledPopulationCap) { return true; }
            if (pActor == null) { return false; }
            return true;
        }
        /*
		harmony = new Harmony(pluginGuid);
		original = AccessTools.Method(typeof(MapBox), "Clicked");
		patch = AccessTools.Method(typeof(GUIWorld), "Clicked_Postfix");
		harmony.Patch(original, null, new HarmonyMethod(patch));
		UnityEngine.Debug.Log("Post patch: MapBox.Clicked");

		if (GUILayout.Button("MultiPowers: " + useMultiPowers))
		{
		useMultiPowers = !useMultiPowers;
		}
		PowerCount = (int) GUILayout.HorizontalSlider((float) PowerCount, 1f, 100f);
		public static bool useMultiPowers;
		public static int PowerCount = 1;

		public static void Clicked_Postfix(int pChange, GodPower pPower = null)
		{
		if (useMultiPowers)
		{
		for (int i = 0; i < PowerCount; i++)
		{
		MapBox.instance.CallMethod("Clicked", new object[] { pChange, pPower });
		}
		}
		}
		*/

        public static bool spawn_Prefix()
        {
            if (disableClouds)
            {
                return false;
            }
            return true;
        }

        // old/unused
        public PowerButton selectedButton()
        {
            PowerButton selectedButton = MapBox.instance.selectedButtons.selectedButton;
            if (selectedButton != null)
            {
                return selectedButton;
            }

            return null;
        }
        public GodPower selectedPower()
        {
            if (selectedButton() != null)
            {
                GodPower godPower = selectedButton().godPower;
                if (godPower != null)
                {
                    return godPower;
                }
            }
            return null;
        }
       
        public static bool spawnResource_Prefix(int pAmount, string pType, bool pRandomSize = true)
        {
            List<WorldTile> targetTileList = MapBox.instance.tilesList.ToList();
            if (dragSelection && lastSelectedTiles != null)
            {
                targetTileList = lastSelectedTiles;
            }
            for (int i = 0; i < pAmount; i++)
            {
                WorldTile random = targetTileList.GetRandom();
                if (random.Type.ground)
                {
                    MapBox.instance.buildings.addBuilding(pType, random, true);
                }
            }
            return false;
        }

        public void FillBenchmark()
        {
            if (lastBenchedTime > maxTimeToWait)
            {
                Debug.Log("ERROR: Fill stopping due to timer, last benched time: " + lastBenchedTime);
                ResetFill();
            }
        }

        public bool tileActionComplete;
        public void tileChangingUpdate() // fill over time
        {
            if (Time.realtimeSinceStartup - lastUpdate <= timerBetweenFill)
            {
                return;
            }
            lastUpdate = Time.realtimeSinceStartup;
            FillBenchmark();
            if (activeFill != null)
            {
                if (selectedPower() == null)
                {
                    Debug.Log("Fill has no power, stopping");
                    ResetFill();
                    return;
                }
                if (activeFill.Count >= 1)
                {
                    FillBenchmark();
                    if (alreadyChanged.Count <= fillToolIterations) // checking to make sure the total changed tiles matches the limit
                    {
                        int position = 0;
                        WorldTile activeTile = activeFill.GetRandom();
                        while (activeFill.Count >= 1 && position <= fillTileCount)
                        {
                            if (SimpleSettings.fillByLines.Value == "first") { activeTile = activeFill.First(); }
                            if (SimpleSettings.fillByLines.Value == "random") { activeTile = activeFill.GetRandom(); }
                            if (SimpleSettings.fillByLines.Value == "last") { activeTile = activeFill.Last(); }
                            activeFill.Remove(activeTile);
                            tileActionComplete = false;
                            if (!alreadyChanged.Contains(activeTile)) // change tiles that havent been already
                            {
                                if (GuiMain.Construction.placingToggleEnabled && (GUIConstruction.placingRoad || GUIConstruction.placingField))
                                {
                                    if (GUIConstruction.placingRoad)
                                    {
                                        GuiMain.Construction.createRoad(activeTile);
                                        tileActionComplete = true;
                                    }
                                    else if (GUIConstruction.placingField)
                                    {
                                        //MapAction.terraformMain(activeTile, TileTypeShortcut.field, AssetManager.terraform.get("flash"));
                                        tileActionComplete = true;
                                    }
                                }
                                else if (!GuiMain.Construction.placingToggleEnabled && !string.IsNullOrEmpty(selectedPower().tileType))
                                {
                                    MapAction.terraformMain(activeTile, AssetManager.tiles.get(selectedPower().tileType), AssetManager.terraform.get("flash"));
                                    tileActionComplete = true;
                                }
                                else if (!GuiMain.Construction.placingToggleEnabled && !string.IsNullOrEmpty(selectedPower().dropID))
                                {
                                    Drop newPixel = MapBox.instance.dropManager.spawn(activeTile, selectedPower().dropID, 5f);
                                    tileActionComplete = true;
                                }
                                if (!tileActionComplete)
                                {
                                    //UsePower(activeTile, selectedPower().id); @@@@@2
                                    tileActionComplete = true;
                                }
                                /* // this stuff went before the else above
								
								*/
                                alreadyChanged.Add(activeTile);
                                trySpread(activeTile);
                                position++;
                            }

                        }
                    }
                    else
                    {
                        ResetFill();
                    }
                }
                else
                {
                    ResetFill();
                }
            }
        }

        private void ResetFill()
        {
            if (alreadyChanged != null)
            {
                listOfFills.Add(alreadyChanged);
                if (alreadyChanged.Count >= 1)
                {
                    listOfFillTypes.Add(alreadyChanged.GetRandom().Type.id);
                }
            }
            alreadyChanged = null;
            activeFill = null;
            fillOriginalTile = null;
            fillOriginalType = null;
        }

        public void trySpread(WorldTile target)
        {
            if (target.tile_left != null)
            {
                if ((target.tile_left.Type == fillOriginalType || fillOriginalType.id.Contains("grass") && target.tile_left.Type.id.Contains("grass") || fillOriginalType.id.Contains("forest") && target.tile_left.Type.id.Contains("forest")) && !alreadyChanged.Contains(target.tile_left))
                {
                    activeFill.Add(target.tile_left);
                }
            }
            if (target.tile_right != null)
            {
                if ((target.tile_right.Type == fillOriginalType || fillOriginalType.id.Contains("grass") && target.tile_right.Type.id.Contains("grass") || fillOriginalType.id.Contains("forest") && target.tile_right.Type.id.Contains("forest")) && !alreadyChanged.Contains(target.tile_right))
                {
                    activeFill.Add(target.tile_right);
                }
            }
            if (target.tile_up != null)
            {
                if ((target.tile_up.Type == fillOriginalType || fillOriginalType.id.Contains("grass") && target.tile_up.Type.id.Contains("grass") || fillOriginalType.id.Contains("forest") && target.tile_up.Type.id.Contains("forest")) && !alreadyChanged.Contains(target.tile_up))
                {
                    activeFill.Add(target.tile_up);
                }
            }
            if (target.tile_down != null)
            {
                if ((target.tile_down.Type == fillOriginalType || fillOriginalType.id.Contains("grass") && target.tile_down.Type.id.Contains("grass") || fillOriginalType.id.Contains("forest") && target.tile_down.Type.id.Contains("forest")) && !alreadyChanged.Contains(target.tile_down))
                {
                    activeFill.Add(target.tile_down);
                }
            }
            /*
			foreach (WorldTile neighbor in target.neighbours)
			{
				if ((neighbor.Type == fillOriginalType || fillOriginalType.name.Contains("grass") && neighbor.Type.name.Contains("grass") || fillOriginalType.name.Contains("forest") && neighbor.Type.name.Contains("forest")) && !alreadyChanged.Contains(neighbor))
				{
					activeFill.Add(neighbor);
				}
			}
			*/
            fillIterationPosition++;
        }
        public void fill()
        {

        }
        public static void MassUpdatePausePrePatch()
        {
            if (Config.paused)
            {
            }
        }
        public static void undoLastFillInList()
        {
            if (listOfFills.Last() == null)
            {
                Debug.Log("no fill to replace");
                return;
            }
            foreach (WorldTile pTile in listOfFills.Last())
            {
                MapAction.terraformMain(pTile, AssetManager.tiles.get(listOfFillTypes.Last()), AssetManager.terraform.get("flash"));
            }
            listOfFillTypes.Remove(listOfFillTypes.Last());
            listOfFills.Remove(listOfFills.Last());
        }

        public static bool disableClouds;
        public bool fillToolEnabled;
        public static bool enabledPopulationCap;
        public static bool buildingLimitEnabled;
        public static int populationCap;
        public static int buildingLimit;
        public static string raceIDToPurge;
        public static string tileType1 = "deep_ocean";
        public static string tileType2 = "random";
        public static List<List<WorldTile>> listOfUndos = new List<List<WorldTile>>();
        public static List<string> listOfUndoTypes = new List<string>();
        public static List<List<WorldTile>> listOfFills = new List<List<WorldTile>>();
        public static List<string> listOfFillTypes = new List<string>();
        public float lastNumberOfTilesFilledOrReplaced;
        public int fillToolIterations;
#pragma warning disable CS0414
        public float lastFillOrReplace = 0f;
#pragma warning restore CS0414
        public List<WorldTile> activeFill;
        public List<WorldTile> alreadyChanged;
        public WorldTile fillOriginalTile;
        public TileTypeBase fillOriginalType;
        public float lastUpdate;
        public int fillTileCount;
        public int fillIterationPosition;
        public float maxTimeToWait => SimpleSettings.maxTimeToWait.Value;
        public static bool dragSelection;
        public static bool dragCircular = true;
        public bool temp;
        public List<WorldTile> tileListDragSelection = new List<WorldTile>();
        public WorldTile startTile;
        public WorldTile endTile;
        public bool showHideWorldOptions;
        public Rect worldOptionsRect;
        public float timerBetweenFill;
        public float lastBenchedTime;
        public static List<WorldTile> lastSelectedTiles;

    }
}
