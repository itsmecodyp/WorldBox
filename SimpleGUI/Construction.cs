using UnityEngine;
using System.Linq;

namespace SimpleGUI
{
    class GUIConstruction
    {
        public string buildingAssetName()
        {
            if(selectedBuildingAsset == null)
            {
                if (placingRoad)
                {
                    return "road";
                }
                if (placingField)
                {
                    return "field";
                }
                return "none";
            }
            return selectedBuildingAsset.id;
        }
       
        public void constructionControl()
        {
            if(Input.GetMouseButton(0))
            {
                if (placingToggleEnabled && !placedOnce)
                {
                    CreateBuilding();
                    if (placingField || placingRoad)
                    {
                        return;
                    }
                    placedOnce = true;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                    placedOnce = false;
            }
        }
        public bool placedOnce;
        public void SetBuilding(string buildingName)
        {
            // Just in case
            if (AssetManager.buildings.get(buildingName) != null)
            {
                selectedBuildingAsset = AssetManager.buildings.get(buildingName);
                Debug.Log("Force changed selected construction");
            }
        }
        public void createRoad(WorldTile pTile)
        {
            MapAction.createRoad(pTile);
        }

        public static bool startDestroyBuilding_Prefix(bool pRemove = false)
        {
            if (placingRoad || placingField)
            {
                if (Input.GetMouseButton(0))
                {
                    return false;
                }
            }
            return true;
        }

        public void CreateBuilding()
        {
            if(placingRoad)
            {
                createRoad(MapBox.instance.getMouseTilePos());
            }
            else if (placingField)
            {
                MapAction.terraformTop(MapBox.instance.getMouseTilePos(), TopTileLibrary.field, AssetManager.terraform.get("flash"));
            }
            else
            {
                Building building = MapBox.instance.addBuilding(selectedBuildingAssetName, MapBox.instance.getMouseTilePos(), null, false, true, BuildPlacingType.New); //CallMethod("addBuilding", new object[] { selectedBuildingAssetName, MapBox.instance.getMouseTilePos(), null, false, true, BuildPlacingType.New }) as Building;
                building.updateBuild(100); //CallMethod("updateBuild", new object[] { 100 });
                WorldTile currentTile = building.currentTile; //Reflection.GetField(building.GetType(), building, "currentTile") as WorldTile;
                if (currentTile.zone.city != null)
                {
                    building.setCity(currentTile.zone.city, false); //CallMethod("setCity", new object[] { currentTile.zone.city, false });
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

        public void constructionPreviewUpdate()
        {
            if(placingRoad || placingField)
            {
                if (MapBox.instance.getMouseTilePos() != null)
                {
                    PixelFlashEffects flashEffects = MapBox.instance.flashEffects; //Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "flashEffects") as PixelFlashEffects;
                    flashEffects.flashPixel(MapBox.instance.getMouseTilePos(), 10, ColorType.White);
                }
            }
            else // placing buildings
            {

                if (selectedBuildingAsset != null && MapBox.instance.getMouseTilePos() != null)
                {
                    // Building construction
                    BuildingAsset constructionTemplate = selectedBuildingAsset;
                    int num = MapBox.instance.getMouseTilePos().x - constructionTemplate.fundament.left;
                    int num2 = MapBox.instance.getMouseTilePos().y - constructionTemplate.fundament.bottom;
                    int num3 = constructionTemplate.fundament.right + constructionTemplate.fundament.left + 1;
                    int num4 = constructionTemplate.fundament.top + constructionTemplate.fundament.bottom + 1;
                    PixelFlashEffects flashEffects = MapBox.instance.flashEffects; //Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "flashEffects") as PixelFlashEffects;

                    for(int j = 0; j < num3; j++)
                    {
                        for (int k = 0; k < num4; k++)
                        {
                            WorldTile tile = MapBox.instance.GetTile(num + j, num2 + k);
                            if (tile != null)
                            {
                                flashEffects.flashPixel(tile, 10, ColorType.White);
                            }
                        }
                    }
                    // constructionControl();
                }
            }
           
        }

        public void constructionWindow(int windowID)
        {
            if (Config.gameLoaded && !Config.worldLoading)
            {
                GuiMain.SetWindowInUse(windowID);
                Color defaultColor = GUI.backgroundColor;
                GUILayout.BeginHorizontal();
                GUILayout.Button("Selected:");
                GUILayout.Button(selectedBuildingAssetName);
                if (filterEnabled)
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("FilterToggle"))
                {
                    filterEnabled = !filterEnabled;
                }
                GUI.backgroundColor = defaultColor;
                filterString = GUILayout.TextField(filterString);
                if (placingToggleEnabled)
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("Toggle placing"))
                {
                    placingToggleEnabled = !placingToggleEnabled;
                }
                GUI.backgroundColor = defaultColor;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                int Position = 2;
                GUILayout.BeginVertical();
                if (GUILayout.Button("none"))
                {
                    selectedBuildingAsset = null;
                    placingRoad = false;
                }
                Position++;
                if (GUILayout.Button("road"))
                {
                    selectedBuildingAsset = null;
                    placingRoad = true;
                    placingField = false;
                }
                if (GUILayout.Button("field"))
                {
                    selectedBuildingAsset = null;
                    placingRoad = false;
                    placingField = true;
                }
                Position++;
                foreach (BuildingAsset buildingType in AssetManager.buildings.list)
                {
                    if (!buildingType.id.Contains("!") && (!filterEnabled || (filterEnabled && buildingType.id.Contains(filterString))))
                    {
                        if (GUILayout.Button(buildingType.id))
                        {
                            selectedBuildingAsset = buildingType;
                            placingRoad = false;
                            placingField = false;

                        }
                        if (Position % 10 == 0)
                        {
                            GUILayout.EndVertical();
                            GUILayout.BeginVertical();
                        }
                        Position++;
                    }
                }
                Position = 2;
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            
            GUI.DragWindow();
        }

        public void constructionWindowUpdate()
        {
            if (GuiMain.showWindowMinimizeButtons.Value)
            {
                string buttontext = "C";
                if (GuiMain.showHideConstructionConfig.Value)
                {
                    buttontext = "-";
                }
                if (GUI.Button(new Rect(ConstructionWindowRect.x + ConstructionWindowRect.width - 25f, ConstructionWindowRect.y - 25, 25, 25), buttontext))
                {
                    GuiMain.showHideConstructionConfig.Value = !GuiMain.showHideConstructionConfig.Value;
                }
            }
            if (GuiMain.showHideConstructionConfig.Value)
            {
                ConstructionWindowRect = GUILayout.Window(1007, ConstructionWindowRect, new GUI.WindowFunction(constructionWindow), "Construction", new GUILayoutOption[]
                {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                });
            }
            if(placingToggleEnabled) {
                constructionPreviewUpdate();
            }
        }

        public bool showHideConstruction;
        public Rect ConstructionWindowRect;
        public BuildingAsset selectedBuildingAsset = null;
        public bool placingToggleEnabled;
        public bool filterEnabled;
        public string filterString = "human";
        public static bool placingRoad;
        public static bool placingField;
        public string selectedBuildingAssetName
        {
            get => buildingAssetName();
        }
    }
}
