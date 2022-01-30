using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Globalization;

namespace SimpleGUI
{
    class GuiDiplomacy
    {
        public void diplomacyWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            GUI.backgroundColor = Color.grey;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("City1") || (Input.GetKeyDown(KeyCode.R) && selectedCity1 != null))
            {
                selectedCity1 = null;
            }
            if (GUILayout.Button("City2") || (Input.GetKeyDown(KeyCode.R) && selectedCity2 != null))
            {
                selectedCity2 = null;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (selectedCity1 == null)
            {
                if (selectingCity1)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Select"))
                {
                    selectingCity1 = true;
                }
            }
            else if (selectedCity1 != null)
            {
                CityData city1Data = selectedCity1.data; //Reflection.GetField(selectedCity1.GetType(), selectedCity1, "data") as CityData;
                if (selectingCity1)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button(city1Data.cityName))
                {
                    selectingCity1 = true;
                }
            }
            if (selectedCity2 == null)
            {
                if (selectingCity2)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Select"))
                {
                    selectingCity2 = true;
                }
            }
            else if (selectedCity2 != null)
            {
                CityData city2Data = Reflection.GetField(selectedCity2.GetType(), selectedCity2, "data") as CityData;
                if (selectingCity2)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button(city2Data.cityName))
                {
                    selectingCity2 = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(); // opening
            GUI.backgroundColor = Color.cyan;
            GUILayout.Button("Relation:");
            GUI.backgroundColor = Color.grey;
            if (selectedCity1 != null && selectedCity2 != null)
            {   // does this look better or does the normal format?
                selectedCity1Kingdom.allies.TryGetValue(selectedCity2Kingdom, out bool isAlly);
                selectedCity1Kingdom.civs_allies.TryGetValue(selectedCity2Kingdom, out bool isAlly2);
                if (isAlly || isAlly2)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Ally"))
                {
                    Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startPeace", new object[] { selectedCity1Kingdom, selectedCity2Kingdom, true });
                }
                selectedCity1Kingdom.enemies.TryGetValue(selectedCity2Kingdom, out bool isEnemy);
                if (isEnemy)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("Enemy"))
                {
                    Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startWar", new object[] { selectedCity1Kingdom, selectedCity2Kingdom, true });
                }
                GUILayout.EndHorizontal(); // closing #1
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("City1");
                GUILayout.Button("City2");
                GUILayout.EndHorizontal(); // closing #2
            }
            if (selectedCity1 != null)
            {
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.cyan;
                GUILayout.Button("City1:");
                GUI.backgroundColor = Color.grey;
                if (GUILayout.Button("War everyone"))
                {
                    foreach (Kingdom kingdom in MapBox.instance.kingdoms.list)
                    {
                        if (kingdom != selectedCity1Kingdom)
                        {
                            Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startWar", new object[] { selectedCity1Kingdom, kingdom, true });
                        }
                    }
                }
                if (GUILayout.Button("Peace everyone"))
                {
                    foreach (Kingdom kingdom in MapBox.instance.kingdoms.list)
                    {
                        if (kingdom != selectedCity1Kingdom)
                        {
                            Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startPeace", new object[] { selectedCity1Kingdom, kingdom, true });
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("NeedCity1");
            }
            GUI.backgroundColor = Color.grey;
            if (selectedCity2 != null)
            {
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.cyan;
                GUILayout.Button("City2:");
                GUI.backgroundColor = Color.grey;
                if (GUILayout.Button("War everyone"))
                {
                    foreach (Kingdom kingdom in MapBox.instance.kingdoms.list)
                    {
                        if (kingdom != selectedCity2Kingdom)
                        {
                            Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startWar", new object[] { selectedCity2Kingdom, kingdom, true });
                        }
                    }
                }
                if (GUILayout.Button("Peace everyone"))
                {
                    foreach (Kingdom kingdom in MapBox.instance.kingdoms.list)
                    {
                        if (kingdom != selectedCity2Kingdom)
                        {
                            Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startPeace", new object[] { selectedCity2Kingdom, kingdom, true });
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("NeedCity2");
            }
            GUI.backgroundColor = Color.grey;
            GUILayout.Button("Add/Remove zones:");
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey;
            if (selectedCity1 != null)
            {
                if (city1PaintZone)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("City1 border"))
                {
                    city1PaintZone = !city1PaintZone;
                    if (city1PaintZone)
                    {
                        city2PaintZone = false;
                    }
                    // SimpleLib.Other.ShowTextTip("Painting border: use left and right click");
                }
                if (city2PaintZone && Input.GetKeyDown(KeyCode.R))
                {
                    city2PaintZone = false;
                }
                if (city1PaintZone && Input.GetKeyDown(KeyCode.R))
                {
                    city1PaintZone = false;
                }
            }
            else
            {
                GUILayout.Button("NeedCity1");
            }
            if (selectedCity2 != null)
            {
                if (city2PaintZone)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("City2 border"))
                {
                    city2PaintZone = !city2PaintZone;
                    if (city2PaintZone)
                    {
                        city1PaintZone = false;
                    }
                    // SimpleLib.Other.ShowTextTip("Painting border: use left and right click");
                }
                if (city2PaintZone && Input.GetKeyDown(KeyCode.R))
                {
                    city2PaintZone = false;
                }
            }
            else
            {
                GUILayout.Button("NeedCity2");
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = targetColorForZone;
            GUILayout.Button("Color zone");
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey;
            if (selectedCity1 != null)
            {
                if (city1PaintColor)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("City1 color"))
                {
                    city1PaintColor = !city1PaintColor;
                    if (city1PaintColor)
                    {
                        city2PaintColor = false;
                    }
                    // SimpleLib.Other.ShowTextTip("Painting border: use left and right click");
                }
                if (city1PaintColor && Input.GetKeyDown(KeyCode.R))
                {
                    city1PaintColor = false;
                }
            }
            else
            {
                GUILayout.Button("NeedCity1");
            }
            GUI.backgroundColor = Color.grey;
            if (selectedCity2 != null)
            {
                if (city2PaintColor)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button("City2 color"))
                {
                    city2PaintColor = !city2PaintColor;
                    if (city2PaintColor)
                    {
                        city1PaintColor = false;
                    }
                    // SimpleLib.Other.ShowTextTip("Painting border: use left and right click");
                }
                if (city2PaintColor && Input.GetKeyDown(KeyCode.R))
                {
                    city2PaintColor = false;
                }
            }
            else
            {
                GUILayout.Button("NeedCity2");
            }
            GUILayout.EndHorizontal();
            targetColorForZone.r = (byte)GUILayout.HorizontalScrollbar((float)targetColorForZone.r, 1f, 0f, 256f);
            targetColorForZone.g = (byte)GUILayout.HorizontalScrollbar((float)targetColorForZone.g, 1f, 0f, 256f);
            targetColorForZone.b = (byte)GUILayout.HorizontalScrollbar((float)targetColorForZone.b, 1f, 0f, 256f);
            if (selectedCity1 != null && selectedCity2 != null)
            {
                if (GUILayout.Button("City2 joins city1 kingdom") && selectedCity1Kingdom != null)
                {
                    //Kingdom city2 Kingdom = selectedCity2.city
                    selectedCity2.joinAnotherKingdom(selectedCity1Kingdom);
                }
                if (!cityMergeConfirmation)
                {
                    if (GUILayout.Button("Merge cities together"))
                    {
                        cityMergeConfirmation = true;
                    }
                }
                else if (cityMergeConfirmation)
                {
                    GUILayout.BeginHorizontal();
                    GUI.backgroundColor = Color.yellow;
                    GUILayout.Button("Are you sure:");
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Merge cities"))
                    {
                        List<Actor> city2Actors = selectedCity2.units.getSimpleList();
                        for (int i = 0; i < selectedCity2.units.Count; i++)
                        {
                            Actor citizen = city2Actors[i];
                            selectedCity2.removeCitizen(citizen, false);
                            citizen.city = null;
                            citizen.CallMethod("becomeCitizen", new object[] { selectedCity1 });
                            cityMergeConfirmation = false;

                        }
                    }
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Cancel"))
                    {
                        cityMergeConfirmation = false;
                    }

                    GUILayout.EndHorizontal();
                }
                /*
                if (!kingdomMergeConfirmation)
                {
                    if (GUILayout.Button("Merge kingdoms together"))
                    {
                        kingdomMergeConfirmation = true;
                    }
                }
                else if(kingdomMergeConfirmation)
                {
                    GUILayout.BeginHorizontal();
                    GUI.backgroundColor = Color.cyan;
                    GUILayout.Button("Are you sure:");
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Merge kingdoms"))
                    {
                        Kingdom city1Kingdom = Reflection.GetField(selectedCity1.GetType(), selectedCity1, "kingdom") as Kingdom;
                        Kingdom city2Kingdom = Reflection.GetField(selectedCity2.GetType(), selectedCity2, "kingdom") as Kingdom;
                        if (city2Kingdom == null)
                        {
                            Debug.Log("Caught error: kingdom2 null");
                            kingdomMergeConfirmation = false;
                            return;
                        }
                        for (int i = 0; i < city2Kingdom.buildings.Count; i++)
                        {
                            city2Kingdom.buildings[i].CallMethod("setKingdom", new object[] { city1Kingdom });
                        }
                        for (int i = 0; i < city2Kingdom.cities.Count; i++)
                        {
                            city2Kingdom.cities[i].CallMethod("setKingdom", new object[] { city1Kingdom });
                        };
                        kingdomMergeConfirmation = false;

                    }
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Cancel"))
                    {
                        kingdomMergeConfirmation = false;
                    }
                    GUILayout.EndHorizontal();
                }
                */
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("Set both cities to merge");
                GUI.backgroundColor = Color.grey;
            }
            if (EnableConstantWar)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Toggle constant war"))
            {
                EnableConstantWar = !EnableConstantWar;
            }
            GUI.backgroundColor = Color.grey;

            GUI.DragWindow();
        }

        public void diplomacyWindowUpdate()
        {
            if (GuiMain.showWindowMinimizeButtons != null && GuiMain.showWindowMinimizeButtons.Value)
            {
                string buttontext = "D";
                if (GuiMain.showHideDiplomacyConfig != null && GuiMain.showHideDiplomacyConfig.Value)
                {
                    buttontext = "-";
                }
                if (GUI.Button(new Rect(diplomacyWindowRect.x + diplomacyWindowRect.width - 25f, diplomacyWindowRect.y - 25, 25, 25), buttontext))
                {
                    GuiMain.showHideDiplomacyConfig.Value = !GuiMain.showHideDiplomacyConfig.Value;
                }
            }
            //
            if (selectingCity1 && Input.GetMouseButton(0))
            {
                if (MapBox.instance.getMouseTilePos() != null)
                {
                    if (MapBox.instance.getMouseTilePos().zone.city != null)
                    {
                        selectedCity1 = MapBox.instance.getMouseTilePos().zone.city;
                        selectingCity1 = false;
                    }
                }
            }
            if (selectingCity2 && Input.GetMouseButton(0))
            {
                if (MapBox.instance.getMouseTilePos() != null)
                {
                    if (MapBox.instance.getMouseTilePos().zone.city != null)
                    {
                        selectedCity2 = MapBox.instance.getMouseTilePos().zone.city;
                        selectingCity2 = false;
                    }
                }
            }

            if (city1PaintZone && selectedCity1 != null)
            {
                if (Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos() != null && MapBox.instance.getMouseTilePos().zone != null && MapBox.instance.getMouseTilePos().zone.city != null &&  MapBox.instance.getMouseTilePos().zone.city != selectedCity1)
                {
                    foreach (City city in MapBox.instance.citiesList)
                    {
                        Reflection.CallMethod(city, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                    }
                    Reflection.CallMethod(selectedCity1, "addZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
                if (Input.GetMouseButton(1) && MapBox.instance.getMouseTilePos() != null && MapBox.instance.getMouseTilePos().zone != null && MapBox.instance.getMouseTilePos().zone.city != null && MapBox.instance.getMouseTilePos().zone.city == selectedCity1)
                {
                    Reflection.CallMethod(selectedCity1, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
            }
            if (city2PaintZone && selectedCity2 != null)
            {
                if (Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos().zone.city != selectedCity2)
                {
                    foreach (City city in MapBox.instance.citiesList)
                    {
                        Reflection.CallMethod(city, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                    }
                    Reflection.CallMethod(selectedCity2, "addZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
                if (Input.GetMouseButton(1) && MapBox.instance.getMouseTilePos().zone.city == selectedCity2)
                {
                    Reflection.CallMethod(selectedCity2, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
            }

            if (city1PaintColor && selectedCity1 != null)
            {
                if (Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos() != null && MapBox.instance.getMouseTilePos().zone != null && MapBox.instance.getMouseTilePos().zone.city != null && MapBox.instance.getMouseTilePos().zone.city == selectedCity1)
                {
                    ZoneCalculator zoneCalculator = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "zoneCalculator") as ZoneCalculator;
                    Kingdom kingdom = Reflection.GetField(MapBox.instance.getMouseTilePos().zone.city.GetType(), MapBox.instance.getMouseTilePos().zone.city, "kingdom") as Kingdom;
                    KingdomColor kingdomColor = Reflection.GetField(kingdom.GetType(), kingdom, "kingdomColor") as KingdomColor;
                    kingdomColor.colorBorderInside = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha.a = 0.6f;
                    kingdomColor.colorBorderOut = targetColorForZone;
                    foreach (City city in kingdom.cities)
                    {
                        List<TileZone> zones = Reflection.GetField(city.GetType(), city, "zones") as List<TileZone>;
                        foreach (TileZone pZone in zones)
                        {
                            zoneCalculator.CallMethod("colorCityZone", new object[] { pZone });
                            //MapBox.instance.zone.modTestColorCityZone(pZone);
                        }
                        foreach (Building building in city.buildings)
                        {
                            SpriteRenderer roof = Reflection.GetField(building.GetType(), building, "roof") as SpriteRenderer;
                            if (roof != null)
                            {
                                roof.color = targetColorForZone;
                            }
                        }
                    }
                    Reflection.CallMethod(selectedCity1, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                    Reflection.CallMethod(selectedCity1, "addZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
            }
            if (city2PaintColor && selectedCity2 != null)
            {
                if (Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos() != null && MapBox.instance.getMouseTilePos().zone != null && MapBox.instance.getMouseTilePos().zone.city != null && MapBox.instance.getMouseTilePos().zone.city == selectedCity2)
                {
                    ZoneCalculator zoneCalculator = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "zoneCalculator") as ZoneCalculator;
                    Kingdom kingdom = Reflection.GetField(MapBox.instance.getMouseTilePos().zone.city.GetType(), MapBox.instance.getMouseTilePos().zone.city, "kingdom") as Kingdom;
                    KingdomColor kingdomColor = Reflection.GetField(kingdom.GetType(), kingdom, "kingdomColor") as KingdomColor;
                    kingdomColor.colorBorderInside = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha.a = 0.6f;
                    kingdomColor.colorBorderOut = targetColorForZone;
                    foreach (City city in kingdom.cities)
                    {
                        List<TileZone> zones = Reflection.GetField(city.GetType(), city, "zones") as List<TileZone>;
                        foreach (TileZone pZone in zones)
                        {
                            zoneCalculator.CallMethod("colorCityZone", new object[] { pZone });
                            //MapBox.instance.zone.modTestColorCityZone(pZone);
                        }
                        foreach (Building building in city.buildings)
                        {
                            SpriteRenderer roof = Reflection.GetField(building.GetType(), building, "roof") as SpriteRenderer;
                            if (roof != null)
                            {
                                roof.color = targetColorForZone;
                            }
                        }
                    }
                    Reflection.CallMethod(selectedCity2, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                    Reflection.CallMethod(selectedCity2, "addZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
            }

            if (GuiMain.showHideDiplomacyConfig != null && GuiMain.showHideDiplomacyConfig.Value)
            {
                diplomacyWindowRect = GUILayout.Window(1004, diplomacyWindowRect, new GUI.WindowFunction(diplomacyWindow), "Diplomacy", new GUILayoutOption[]
                {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                });
            }
        }

        public static string ColorToHex(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        }

        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return new Color32(r, g, b, byte.MaxValue);
        }


        public static bool cityMergeConfirmation;
        public static bool kingdomMergeConfirmation;
        public static bool paintTilezoneColor;
        public static Color32 targetColorForZone = Color.white; public Rect diplomacyWindowRect;
        public static City selectedCity1;
        public static City selectedCity2;
        public static Kingdom selectedCity1Kingdom
        {
            get => Reflection.GetField(selectedCity1.GetType(), selectedCity1, "kingdom") as Kingdom;
        }
        public static Kingdom selectedCity2Kingdom
        {
            get => Reflection.GetField(selectedCity2.GetType(), selectedCity2, "kingdom") as Kingdom;
        }
        public bool EnableConstantWar;
        public static Kingdom selectedKingdom1;
        public static Kingdom selectedKingdom2;
        public static bool selectingCity1;
        public static bool selectingCity2;
        public static bool selectingKingdom1;
        public static bool selectingKingdom2;
        public static bool city1PaintZone;
        public static bool city2PaintZone;
        public static bool city1PaintColor;
        public static bool city2PaintColor;
    }
}
