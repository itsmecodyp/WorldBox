using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;

namespace SimpleGUI
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
			if (listOfUndos.Last<List<WorldTile>>() == null)
			{
				Debug.Log("no undo to replace");
			}
			foreach (WorldTile pTile in listOfUndos.Last<List<WorldTile>>()) // each tile inside the last list
			{
				MapAction.terraformMain(pTile, AssetManager.tiles.get(listOfUndoTypes.Last<string>()), AssetManager.terraform.get("flash"));
			}
			listOfUndoTypes.Remove(listOfUndoTypes.Last<string>()); // remove type from end of list
			listOfUndos.Remove(listOfUndos.Last<List<WorldTile>>()); // remove list of undo tiles
		}

		public void replaceAllTilesOnMap(string oldType, string newType)
		{
			if (newType == "mouse")
			{
				newType = MapBox.instance.getMouseTilePos().Type.id;
			}
			if (oldType == "mouse")
			{
				oldType = MapBox.instance.getMouseTilePos().Type.id;
			}
			if (listOfUndos == null)
			{
				listOfUndos = new List<List<WorldTile>>();
			}
			if (listOfUndoTypes == null)
			{
				listOfUndoTypes = new List<string>();
			}
			string[] typeArray = listOfUndoTypes.ToArray();
			List<WorldTile> list = new List<WorldTile>();
			List<WorldTile> list2 = MapBox.instance.tilesList;
			if (oldType == "last" && listOfUndos.Last<List<WorldTile>>() != null)
			{
				list2 = listOfUndos.Last<List<WorldTile>>();
			}
			if (lastSelectedTiles != null)
			{
				list2 = lastSelectedTiles;
			}
			foreach (WorldTile worldTile in list2)
			{
				if (oldType == "all")
				{
					list.Add(worldTile);
					MapAction.terraformMain(worldTile, AssetManager.tiles.get(newType), AssetManager.terraform.get("flash"));
				}
				if (oldType != "last")
				{
					if (worldTile.Type.id.ToString() == oldType)
					{
						list.Add(worldTile);
						MapAction.terraformMain(worldTile, AssetManager.tiles.get(newType), AssetManager.terraform.get("flash"));
					}
				}
				else if (oldType == "last")
				{
					list.Add(worldTile);
					MapAction.terraformMain(worldTile, AssetManager.tiles.get(newType), AssetManager.terraform.get("flash"));
				}
			}
			listOfUndos.Add(list);
			lastNumberOfTilesFilledOrReplaced = list.Count;
			listOfUndoTypes.Add(oldType);
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
				for (int i = x - radius; i < x + radius+1; i++)
				{
					for (int j = y - radius; j < y + radius+1; j++)
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
				}
				
			}

		}

		public void worldUpdate()
		{
			if (Config.gameLoaded && !Config.worldLoading)
			{
				if(hasRun == false) {
					firstRunT();
				}
				DragSelectionUpdate();
				PixelFlashEffects flashEffects = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "flashEffects") as PixelFlashEffects;
				if (lastSelectedTiles != null)
				{
					foreach (WorldTile tile in lastSelectedTiles)
					{
						flashEffects.flashPixel(tile, 10, ColorType.White);
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
			}

		}


		public void worldOptionsUpdate()
		{
			if (GuiMain.showWindowMinimizeButtons.Value)
			{
				string buttontext = "W";
				if (GuiMain.showHideWorldOptionsConfig.Value)
				{
					buttontext = "-";
				}
				if (GUI.Button(new Rect(worldOptionsRect.x + 280f, worldOptionsRect.y - 25, 25, 25), buttontext))
				{
					GuiMain.showHideWorldOptionsConfig.Value = !GuiMain.showHideWorldOptionsConfig.Value;
				}
			}
			if (GuiMain.showHideWorldOptionsConfig.Value)
			{
				GUI.contentColor = UnityEngine.Color.white;
				worldOptionsRect = GUILayout.Window(1010, worldOptionsRect, new GUI.WindowFunction(worldOptionsWindow), "World Options", new GUILayoutOption[]
				{
				GUILayout.MaxWidth(300f),
				GUILayout.MinWidth(200f)
				});
				
			}

			if (enabledPopulationCap)
			{
				List<Actor> actorList = MapBox.instance.units.getSimpleList();
				for (int i = actorList.Count; i > populationCap; i--)
				{
					actorList.Last().killHimself(true, AttackType.Other, true);
				}
				
			}
			if (buildingLimitEnabled && MapBox.instance.buildings.Count > buildingLimit)
			{
				List<Building> buildingList = MapBox.instance.buildings.getSimpleList();
				for (int i = buildingList.Count - 2; i > buildingLimit; i--)
				{
					buildingList[i].CallMethod("startDestroyBuilding", new object[] { true });
				}
			}
		}

		public void worldOptionsWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
			Color originalColor = GUI.backgroundColor;
			GUILayout.BeginHorizontal();
			if (enabledPopulationCap)
			{
				GUI.backgroundColor = Color.green;
			}
			else
			{
				GUI.backgroundColor = Color.red;
			}
			int num = populationCap;
			if (populationCap <= 0)
			{
				populationCap = 1;
			}
			if (GUILayout.Button("Population limit"))
			{
			//	populationCap = MapBox.instance.units.Count;
				enabledPopulationCap = !enabledPopulationCap;
			}
			GUI.backgroundColor = originalColor;
			populationCap = Convert.ToInt32(GUILayout.TextField(populationCap.ToString()));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
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
			if (GUILayout.Button("Building limit"))
			{
				buildingLimit = MapBox.instance.buildings.Count;
				buildingLimitEnabled = !buildingLimitEnabled;
			}
			GUI.backgroundColor = originalColor;
			buildingLimit = Convert.ToInt32(GUILayout.TextField(buildingLimit.ToString()));
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Kill all creatures"))
			{
				foreach (Actor actor in MapBox.instance.units)
				{
					actor.killHimself(false, AttackType.Other, true);
				}
			}
			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Kill race:"))
			{
				foreach (Actor actor2 in MapBox.instance.units)
				{
					Race actorRace = Reflection.GetField(actor2.GetType(), actor2, "race") as Race;

					if (actorRace.id.ToString() == raceIDToPurge || actorRace.id.ToString().ToLower() == raceIDToPurge)
					{
						actor2.killHimself(false, AttackType.Other, true);
					}
				}
			}
			raceIDToPurge = GUILayout.TextField(raceIDToPurge);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Kill civs"))
			{
				foreach (Actor actor3 in MapBox.instance.units)
				{
					Race actorRace = Reflection.GetField(actor3.GetType(), actor3, "race") as Race;

					if (actorRace.civilization)
					{
						actor3.killHimself(false, AttackType.Other, true);
					}
				}
			}
			if (GUILayout.Button("Kill beasts"))
			{
				foreach (Actor actor4 in MapBox.instance.units)
				{
					Race actorRace = Reflection.GetField(actor4.GetType(), actor4, "race") as Race;

					if (!actorRace.civilization)
					{
						actor4.killHimself(false, AttackType.Other, true);
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (tileType1 == null)
			{
				tileType1 = "deep_ocean";
			}
			if (tileType2 == null)
			{
				tileType2 = "random";
			}
			tileType1 = GUILayout.TextField(tileType1);
			tileType2 = GUILayout.TextField(tileType2);
			GUILayout.EndHorizontal();
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
					lastSelectedTiles = null;
					startTile = null;
					endTile = null;
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
				foreach (WorldTile tile in lastSelectedTiles)
				{
					if (tile.building != null)
					{
						BuildingAsset selectedBuildingAsset = AssetManager.buildings.get(buildingName);
						if (selectedBuildingAsset != null)
						{

							Building building = MapBox.instance.CallMethod("addBuilding", new object[] { buildingName, tile.building.getConstructionTile(), null, false, true, BuildPlacingType.New }) as Building;
							/*
							if (AssetManager.buildings.get(buildingName).construction_site_texture != null)
							{
								building.CallMethod("updateBuild", new object[] { 100 });
							}
							*/
							WorldTile currentTile = Reflection.GetField(building.GetType(), building, "currentTile") as WorldTile;
							if (currentTile.zone.city != null)
							{
								building.CallMethod("setCity", new object[] { currentTile.zone.city, false });
							}
							if (building.city != null)
							{
								building.city.addBuilding(building);
								building.city.status.homesTotal += selectedBuildingAsset.housing * (selectedBuildingAsset.upgradeLevel + 1);
								if (building.city.status.population > building.city.status.homesTotal)
								{
									building.city.status.homesOccupied = building.city.status.homesTotal;
								}
								else
								{
									building.city.status.homesOccupied = building.city.status.population;
								}
								building.city.status.homesFree = building.city.status.homesTotal - building.city.status.homesOccupied;
							}
						}
					}
				}
			}
			if (GUILayout.Button("Power on every tile in selection"))
			{
				foreach (WorldTile tile in lastSelectedTiles)
				{
					if (selectedPower().id != null)
					{
						if (selectedPower().tileType != null && selectedPower().tileType != "")
						{
							MapAction.terraformMain(tile, AssetManager.tiles.get(selectedPower().tileType), AssetManager.terraform.get("flash"));
						}
						else if (selectedPower().dropID != null && selectedPower().dropID != "")
						{
							Drop newPixel = MapBox.instance.dropManager.spawn(tile, selectedPower().dropID, 5f);
						}
						else
						{
							//UsePower(tile, selectedPower().id);
						}
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
					building.CallMethod("startDestroyBuilding", new object[] { true });
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

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Fill/PaintBucket: " + fillToolEnabled.ToString()))
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
			
						GUI.DragWindow();
		}

		/*
		harmony = new Harmony(pluginGuid);
		original = AccessTools.Method(typeof(MapBox), "Clicked");
		patch = AccessTools.Method(typeof(GUIWorld), "Clicked_Postfix");
		harmony.Patch(original, null, new HarmonyMethod(patch));
		Debug.Log("Post patch: MapBox.Clicked");

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
			PowerButton selectedButton = Reflection.GetField(MapBox.instance.selectedButtons.GetType(), MapBox.instance.selectedButtons, "selectedButton") as PowerButton;
			if (selectedButton != null)
			{
				return selectedButton;
			}
			else
			{
				return null;
			}
		}
		public GodPower selectedPower()
		{
			if (selectedButton() != null)
			{
				GodPower godPower = Reflection.GetField(selectedButton().GetType(), selectedButton(), "godPower") as GodPower;
				if (godPower != null)
				{
					return godPower;
				}
			}
			return null;
		}
		public static void RepopulatePlants()
		{
			List<WorldTile> tileArea = MapBox.instance.tilesList;
			if (dragSelection && lastSelectedTiles != null)
			{
				tileArea = lastSelectedTiles;
			}
			int num3 = tileArea.Count / 2;
			for (int i = 0; i < num3; i++)
			{
				WorldTile random = tileArea.GetRandom<WorldTile>();
				if (random.Type.ground && random.zone.trees.Count < 3)
				{
					MapBox.instance.CallMethod("tryGrowPlant", new object[] { random, true, false, true, null });
				}
			}
		}
		public static void RepopulateOres()
		{
			// this needs more work, spawnResource needs to check this list instead of globally
			List<WorldTile> tileArea = MapBox.instance.tilesList;
			if (dragSelection && lastSelectedTiles != null)
			{
				tileArea = lastSelectedTiles;
			}
			int num4 = tileArea.Count / 1000;
			int num5 = num4 / 2;
			int num6 = num5 / 2;
			int num7 = num6;
			MapBox.instance.CallMethod("spawnResource", new object[] { num4, "stone", true });
			MapBox.instance.CallMethod("spawnResource", new object[] { num5, "ore_deposit", true });
			MapBox.instance.CallMethod("spawnResource", new object[] { num6, "gold", true });
			MapBox.instance.CallMethod("spawnBeehives", new object[] { num7 / 2 });
		}

		public static bool spawnResource_Prefix(int pAmount, string pType, bool pRandomSize = true)
		{
			List<WorldTile> targetTileList = MapBox.instance.tilesList;
			if (dragSelection && lastSelectedTiles != null)
			{
				targetTileList = lastSelectedTiles;
			}
			for (int i = 0; i < pAmount; i++)
			{
				WorldTile random = targetTileList.GetRandom<WorldTile>();
				if (random.Type.ground)
				{
					string str = "";
					if (pRandomSize)
					{
						if (Toolbox.randomBool())
						{
							str = "_s";
						}
						else if (Toolbox.randomBool())
						{
							str = "_m";
						}
					}
					MapBox.instance.CallMethod("addBuilding", new object[] { pType + str, random, null, true, false, BuildPlacingType.New });
				}
			}
			return false;
		}

		public static void RepopulateTerrain()
		{
			List<WorldTile> tileArea = MapBox.instance.tilesList;
			if (dragSelection && lastSelectedTiles != null)
			{
				tileArea = lastSelectedTiles;
			}
			int num3 = tileArea.Count / 2;
			/*
			for (int i = 0; i < num3; i++)
			{
				WorldTile random = tileArea.GetRandom<WorldTile>();
				if (random.Type.ground && random.zone.trees.Count < 3)
				{
					MapBox.instance.CallMethod("tryGrowPlant", new object[] { random, true, false, true, null });
				}
			}
			*/
			int num4 = MapBox.instance.tilesList.Count / 1000;
			int num5 = num4 / 2;
			int num6 = num5 / 2;
			int num7 = num6;
			MapBox.instance.CallMethod("spawnResource", new object[] { num4, "stone", true });
			MapBox.instance.CallMethod("spawnResource", new object[] { num5, "ore_deposit", true });
			MapBox.instance.CallMethod("spawnResource", new object[] { num6, "gold", true });
			MapBox.instance.CallMethod("spawnResource", new object[] { num7, "fruit_bush", false });
			MapBox.instance.CallMethod("spawnBeehives", new object[] { num7 / 2 });
		}

		public void FillBenchmark()
		{
			if (lastBenchedTime > maxTimeToWait)
			{
				Debug.Log("ERROR: Fill stopping due to timer, last benched time: " + lastBenchedTime.ToString());
				ResetFill();
				return;
			}
		}

		public bool tileActionComplete = false;
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
							if (GuiMain.fillByLines.Value == "first") { activeTile = activeFill.First(); }
							if (GuiMain.fillByLines.Value == "random") { activeTile = activeFill.GetRandom(); }
							if (GuiMain.fillByLines.Value == "last") { activeTile = activeFill.Last(); }
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
								else if (!GuiMain.Construction.placingToggleEnabled && selectedPower().tileType != null && selectedPower().tileType != "")
								{
                                    MapAction.terraformMain(activeTile, AssetManager.tiles.get(selectedPower().tileType), AssetManager.terraform.get("flash"));
									tileActionComplete = true;
								}
								else if (!GuiMain.Construction.placingToggleEnabled && (selectedPower().dropID != null && selectedPower().dropID != ""))
								{
                                    Drop newPixel = MapBox.instance.dropManager.spawn(activeTile, selectedPower().dropID, 5f);
									tileActionComplete = true;
								}
								if(!tileActionComplete)
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
						return;
					}
				}
				else
				{
					ResetFill();
					return;
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
				return;
			}
		}
		public static void undoLastFillInList()
		{
			if (listOfFills.Last<List<WorldTile>>() == null)
			{
				Debug.Log("no fill to replace");
				return;
			}
			foreach (WorldTile pTile in listOfFills.Last<List<WorldTile>>())
			{
				MapAction.terraformMain(pTile, AssetManager.tiles.get(listOfFillTypes.Last<string>()), AssetManager.terraform.get("flash"));
			}
			listOfFillTypes.Remove(listOfFillTypes.Last<string>());
			listOfFills.Remove(listOfFills.Last<List<WorldTile>>());
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
		public static List<List<WorldTile>> listOfUndos;
		public static List<string> listOfUndoTypes;
		public static List<List<WorldTile>> listOfFills = new List<List<WorldTile>>();
		public static List<string> listOfFillTypes = new List<string>();
		public float lastNumberOfTilesFilledOrReplaced = 0f;
		public int fillToolIterations;
		public float lastFillOrReplace = 0f;
		public List<WorldTile> activeFill;
		public List<WorldTile> alreadyChanged;
		public WorldTile fillOriginalTile;
		public TileTypeBase fillOriginalType;
		public float lastUpdate;
		public int fillTileCount;
		public int fillIterationPosition;
		public float maxTimeToWait
		{
			get => GuiMain.maxTimeToWait.Value;
		}
		public static bool dragSelection = false;
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
