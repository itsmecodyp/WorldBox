using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SimpleGUI.Menus {
    class GuiDiplomacy {

        public Dictionary<Kingdom, List<Kingdom>> eternalAllies = new Dictionary<Kingdom, List<Kingdom>>();
        public Dictionary<Kingdom, List<Kingdom>> eternalEnemies = new Dictionary<Kingdom, List<Kingdom>>();

        public static bool showCultureSelectionWindow;
        public static string currentCultureFromSelectionWindow = "";

        public void diplomacyCultureSelectWindow(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(
          scrollPosition, GUILayout.Height(diplomacyWindowRect.height - 32.5f));
            foreach(Culture culture in MapBox.instance.cultures.list) {
				if(GUILayout.Button(culture.name)) {
                    currentCultureFromSelectionWindow = culture.name;
                    Culture city1Culture = selectedCity1.getCulture();
                    if(city1Culture != null) {
                        /* maybe not necessary
                        foreach(TileZone zone in selectedCity1.zones) {
                            zone.removeCulture();
						}
                        */
                        selectedCity1.setCulture(culture);
                    }
                    foreach(Actor unit in selectedCity1.units) {
                        unit.setCulture(culture);
					}
                }
			}
            GUILayout.EndScrollView();
        }

        public void diplomacyWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            GUI.backgroundColor = Color.grey;
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("City1") || (Input.GetKeyDown(KeyCode.R) && selectedCity1 != null)) {
                selectedCity1 = null;
            }
            if(GUILayout.Button("City2") || (Input.GetKeyDown(KeyCode.R) && selectedCity2 != null)) {
                selectedCity2 = null;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(selectedCity1 == null) {
                if(selectingCity1) {
                    GUI.backgroundColor = Color.yellow;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("Select")) {
                    selectingCity1 = true;
                }
            }
            else if(selectedCity1 != null) {
                CityData city1Data = selectedCity1.data; //Reflection.GetField(selectedCity1.GetType(), selectedCity1, "data") as CityData;
                if(selectingCity1) {
                    GUI.backgroundColor = Color.yellow;
                }
                else {
                    GUI.backgroundColor = Color.green;
                }
                if(GUILayout.Button(city1Data.name)) {
                    selectingCity1 = true;
                }
            }
            if(selectedCity2 == null) {
                if(selectingCity2) {
                    GUI.backgroundColor = Color.yellow;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("Select")) {
                    selectingCity2 = true;
                }
            }
            else if(selectedCity2 != null) {
                CityData city2Data = selectedCity2.data;
                if(selectingCity2) {
                    GUI.backgroundColor = Color.yellow;
                }
                else {
                    GUI.backgroundColor = Color.green;
                }
                if(GUILayout.Button(city2Data.name)) {
                    selectingCity2 = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(); // opening
            GUI.backgroundColor = Color.cyan;
            GUILayout.Button("Relation:");
            GUI.backgroundColor = Color.grey;
            if(selectedCity1 != null && selectedCity2 != null) {   // does this look better or does the normal format?
                //isEnemy
                bool isEnemy = selectedCity1Kingdom.getEnemiesKingdoms().Contains(selectedCity2Kingdom);
                //isEnemyv2
                bool isEnemyv2 = selectedCity1Kingdom.isEnemy(selectedCity2Kingdom);
                //isAlly
                Alliance city1KingdomAlliance = selectedCity1Kingdom.getAlliance();
                Alliance city2KingdomAlliance = selectedCity2Kingdom.getAlliance();
                bool isAlly = Alliance.isSame(city1KingdomAlliance, city2KingdomAlliance);
                //selectedCity1Kingdom.allies.TryGetValue(selectedCity2Kingdom, out bool isAlly); //????????
                //selectedCity1Kingdom.civs_allies.TryGetValue(selectedCity2Kingdom, out bool isAlly2);
                if(isAlly/*|| isAlly2*/) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("Alliance")) {
					if(selectedCity1Kingdom.hasAlliance()) {
                        Alliance city1Alliance = selectedCity1Kingdom.getAlliance();
                        //MapBox.instance.alliances.
                        city1KingdomAlliance.join(selectedCity2Kingdom); // selectedCity2Kingdom.allianceJoin(city1Alliance);
                        //selectedCity2Kingdom.allianceJoin(city1Alliance);
                    }
                    else {
                        MapBox.instance.alliances.newAlliance(selectedCity1Kingdom, selectedCity2Kingdom);
                    }
                   
                }
                if(isEnemy) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("Start war") && isEnemy == false) {
					if(isAlly == false) {
                        MapBox.instance.diplomacy.startWar(selectedCity1Kingdom, selectedCity2Kingdom, WarTypeLibrary.whisper_of_war);
                    }
					else {
                        selectedCity2Kingdom.allianceLeave(city1KingdomAlliance);
                        MapBox.instance.diplomacy.startWar(selectedCity1Kingdom, selectedCity2Kingdom, WarTypeLibrary.normal);
                    }
                }
                GUILayout.EndHorizontal(); 
                GUI.backgroundColor = Color.grey;
            }
            else {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("City1");
                GUILayout.Button("City2");
                GUILayout.EndHorizontal();
            }
            if(selectedCity1 != null) {
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.cyan;
                GUILayout.Button("City1:");
                GUI.backgroundColor = Color.grey;
                if(GUILayout.Button("Send settler out")) {
                    Actor selectedNewSettler = selectedCity1.units.GetRandom();
                    //selectedNewSettler.removeFromCity(); //missing method exception?
                    selectedNewSettler.ai.setTask("nomad_try_build_city", true, true);
                }
                Culture city1Culture = selectedCity1.getCulture();
                if(city1Culture != null) {
                    if(GUILayout.Button("Culture: " + city1Culture.name)) {
                        showCultureSelectionWindow = !showCultureSelectionWindow;
                    }
                }
                if(selectedCity2 == null) {
                    GUILayout.Button("NeedCity2");
                }
                else {
                    if(GUILayout.Button("Transfer to Kingdom2")) {
                        selectedCity1.joinAnotherKingdom(selectedCity2Kingdom);
                    }
                }
                /*
                if(GUILayout.Button("War everyone")) {
                    foreach(Kingdom kingdom in MapBox.instance.kingdoms.list) {
                        if(kingdom != selectedCity1Kingdom) {
                            MapBox.instance.diplomacy.startWar(selectedCity1Kingdom, kingdom, WarTypeLibrary.normal, false);
                        }
                    }
                }
                if(GUILayout.Button("Peace everyone")) {
                    foreach(War kingdomWar in MapBox.instance.wars.getWars(selectedCity1Kingdom)) {
                        kingdomWar.removeFromWar(selectedCity1Kingdom);
                    }
                }
                */
                GUILayout.EndHorizontal();
            }
            else {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("NeedCity1");
            }
            GUI.backgroundColor = Color.grey;
            if(selectedCity2 != null) {
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.cyan;
                GUILayout.Button("City2:");
                GUI.backgroundColor = Color.grey;
                if(GUILayout.Button("Send settler out")) {
                    Actor selectedNewSettler = selectedCity2.units.GetRandom();
                    //selectedNewSettler.removeFromCity(); missing method exception?
                    selectedNewSettler.ai.setTask("nomad_try_build_city", true, true);
                }
                if(selectedCity1 == null) {
                    GUILayout.Button("NeedCity1");
				}
				else {
                    if(GUILayout.Button("Transfer to Kingdom1")) {
                        selectedCity2.joinAnotherKingdom(selectedCity1Kingdom);
                    }
                }
               
                /*
                if(GUILayout.Button("War everyone")) {
                    foreach(Kingdom kingdom in MapBox.instance.kingdoms.list) {
                        if(kingdom != selectedCity2Kingdom) {
                            MapBox.instance.diplomacy.startWar(selectedCity2Kingdom, kingdom, WarTypeLibrary.normal, false);
                        }
                    }
                }
                if(GUILayout.Button("Peace everyone")) {
                    foreach(War kingdomWar in MapBox.instance.wars.getWars(selectedCity2Kingdom)) {
                        kingdomWar.removeFromWar(selectedCity2Kingdom);
                    }
                }
                */
                GUILayout.EndHorizontal();
            }
            else {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("NeedCity2");
            }
            GUI.backgroundColor = Color.grey;
            GUILayout.Button("Add/Remove zones:");
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey;
            if(selectedCity1 != null) {
                if(city1PaintZone) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("City1 border")) {
                    city1PaintZone = !city1PaintZone;
                    if(city1PaintZone) {
                        city2PaintZone = false;
                    }
                    // SimpleLib.Other.ShowTextTip("Painting border: use left and right click");
                }
                if(city2PaintZone && Input.GetKeyDown(KeyCode.R)) {
                    city2PaintZone = false;
                }
                if(city1PaintZone && Input.GetKeyDown(KeyCode.R)) {
                    city1PaintZone = false;
                }
            }
            else {
                GUILayout.Button("NeedCity1");
            }
            if(selectedCity2 != null) {
                if(city2PaintZone) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("City2 border")) {
                    city2PaintZone = !city2PaintZone;
                    if(city2PaintZone) {
                        city1PaintZone = false;
                    }
                    // SimpleLib.Other.ShowTextTip("Painting border: use left and right click");
                }
                if(city2PaintZone && Input.GetKeyDown(KeyCode.R)) {
                    city2PaintZone = false;
                }
            }
            else {
                GUILayout.Button("NeedCity2");
            }
            GUILayout.EndHorizontal();
            if(EnableConstantWar) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("Toggle constant war")) {
                EnableConstantWar = !EnableConstantWar;
            }
            GUI.backgroundColor = Color.grey;
            if(GUILayout.Button("World War")) {
                foreach(Kingdom kingdom in MapBox.instance.kingdoms.list_civs) {
                    foreach(Kingdom otherKingdom in MapBox.instance.kingdoms.list_civs) {
                        if(otherKingdom != kingdom && kingdom.isEnemy(otherKingdom) == false) {
                            MapBox.instance.diplomacy.startWar(kingdom, otherKingdom, WarTypeLibrary.normal, false);
                        }
                    }
                }
            }
            if(GUILayout.Button("Peaceful World")) {
                MapBox.instance.wars.stopAllWars();
                /*
                foreach(Kingdom kingdom in MapBox.instance.kingdoms.list_civs) {
                    foreach(Kingdom otherKingdom in MapBox.instance.kingdoms.list_civs) {
                        if(otherKingdom != kingdom && kingdom.isEnemy(otherKingdom) == true) {
                            MapBox.instance.kingdoms.diplomacyManager.startPeace(kingdom, otherKingdom, true);
                        }
                    }
                }
                */
            }
            GUI.DragWindow();
        }

        public Vector2 scrollPosition;


        public void diplomacyWindowUpdate()
        {
            if(GuiMain.showWindowMinimizeButtons != null && GuiMain.showWindowMinimizeButtons.Value) {
                string buttontext = "D";
                if(GuiMain.showHideDiplomacyConfig != null && GuiMain.showHideDiplomacyConfig.Value) {
                    buttontext = "-";
                }
                if(GUI.Button(new Rect(diplomacyWindowRect.x + diplomacyWindowRect.width - 25f, diplomacyWindowRect.y - 25, 25, 25), buttontext))
                {
                    if (GuiMain.showHideDiplomacyConfig != null)
                        GuiMain.showHideDiplomacyConfig.Value = !GuiMain.showHideDiplomacyConfig.Value;
                }
            }
            //
            if(selectingCity1 && Input.GetMouseButton(0)) {
                if(MapBox.instance.getMouseTilePos() != null) {
                    if(MapBox.instance.getMouseTilePos().zone.city != null) {
                        selectedCity1 = MapBox.instance.getMouseTilePos().zone.city;
                        selectingCity1 = false;
                    }
                }
            }
            if(selectingCity2 && Input.GetMouseButton(0)) {
                if(MapBox.instance.getMouseTilePos() != null) {
                    if(MapBox.instance.getMouseTilePos().zone.city != null) {
                        selectedCity2 = MapBox.instance.getMouseTilePos().zone.city;
                        selectingCity2 = false;
                    }
                }
            }

            if(city1PaintZone && selectedCity1 != null) {
                if(Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos().zone.city != selectedCity1) {
                    foreach(City city in MapBox.instance.cities.list) {
                        city.removeZone(MapBox.instance.getMouseTilePos().zone);
                    }
                    selectedCity1.addZone(MapBox.instance.getMouseTilePos().zone);
                }
                if(Input.GetMouseButton(1) && MapBox.instance.getMouseTilePos().zone.city == selectedCity1) {
                    selectedCity1.removeZone(MapBox.instance.getMouseTilePos().zone);
                }
            }
            if(city2PaintZone && selectedCity2 != null) {
                if(Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos().zone.city != selectedCity2) {
                    foreach(City city in MapBox.instance.cities.list) {
                        city.removeZone(MapBox.instance.getMouseTilePos().zone);
                    }
                    selectedCity2.addZone(MapBox.instance.getMouseTilePos().zone);
                }
                if(Input.GetMouseButton(1) && MapBox.instance.getMouseTilePos().zone.city == selectedCity2) {
                    selectedCity2.removeZone(MapBox.instance.getMouseTilePos().zone);
                }
            }
            /* pretty sure kingdom coloring is vanilla feature now
            if(city1PaintColor && selectedCity1 != null) {
                if(Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos() != null && MapBox.instance.getMouseTilePos().zone != null && MapBox.instance.getMouseTilePos().zone.city != null && MapBox.instance.getMouseTilePos().zone.city == selectedCity1) {
                    ZoneCalculator zoneCalculator = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "zoneCalculator") as ZoneCalculator;
                    Kingdom kingdom = Reflection.GetField(MapBox.instance.getMouseTilePos().zone.city.GetType(), MapBox.instance.getMouseTilePos().zone.city, "kingdom") as Kingdom;
                    ColorAsset kingdomColor = kingdom.kingdomColor;
                    kingdomColor. = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha.a = 0.6f;
                    kingdomColor.colorBorderOut = targetColorForZone;
                    foreach(City city in kingdom.cities) {
                        List<TileZone> zones = Reflection.GetField(city.GetType(), city, "zones") as List<TileZone>;
                        foreach(TileZone pZone in zones) {
                            zoneCalculator.CallMethod("colorCityZone", new object[] { pZone });
                            //MapBox.instance.zone.modTestColorCityZone(pZone);
                        }
                        foreach(Building building in city.buildings) {
                            SpriteRenderer roof = Reflection.GetField(building.GetType(), building, "roof") as SpriteRenderer;
                            if(roof != null) {
                                roof.color = targetColorForZone;
                            }
                        }
                    }
                    Reflection.CallMethod(selectedCity1, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                    Reflection.CallMethod(selectedCity1, "addZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
            }
            if(city2PaintColor && selectedCity2 != null) {
                if(Input.GetMouseButton(0) && MapBox.instance.getMouseTilePos() != null && MapBox.instance.getMouseTilePos().zone != null && MapBox.instance.getMouseTilePos().zone.city != null && MapBox.instance.getMouseTilePos().zone.city == selectedCity2) {
                    ZoneCalculator zoneCalculator = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "zoneCalculator") as ZoneCalculator;
                    Kingdom kingdom = Reflection.GetField(MapBox.instance.getMouseTilePos().zone.city.GetType(), MapBox.instance.getMouseTilePos().zone.city, "kingdom") as Kingdom;
                    KingdomColor kingdomColor = Reflection.GetField(kingdom.GetType(), kingdom, "kingdomColor") as KingdomColor;
                    kingdomColor.colorBorderInside = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha = targetColorForZone;
                    kingdomColor.colorBorderInsideAlpha.a = 0.6f;
                    kingdomColor.colorBorderOut = targetColorForZone;
                    foreach(City city in kingdom.cities) {
                        List<TileZone> zones = Reflection.GetField(city.GetType(), city, "zones") as List<TileZone>;
                        foreach(TileZone pZone in zones) {
                            zoneCalculator.CallMethod("colorCityZone", new object[] { pZone });
                            //MapBox.instance.zone.modTestColorCityZone(pZone);
                        }
                        foreach(Building building in city.buildings) {
                            SpriteRenderer roof = Reflection.GetField(building.GetType(), building, "roof") as SpriteRenderer;
                            if(roof != null) {
                                roof.color = targetColorForZone;
                            }
                        }
                    }
                    Reflection.CallMethod(selectedCity2, "removeZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                    Reflection.CallMethod(selectedCity2, "addZone", new object[] { MapBox.instance.getMouseTilePos().zone });
                }
            }
            */
            if(GuiMain.showHideDiplomacyConfig != null && GuiMain.showHideDiplomacyConfig.Value) {
                diplomacyWindowRect = GUILayout.Window(96850, diplomacyWindowRect, diplomacyWindow, "Diplomacy", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
			if(showCultureSelectionWindow) {
                cultureSelectWindowRect = GUILayout.Window(96851, cultureSelectWindowRect, diplomacyCultureSelectWindow, "Select culture", GUILayout.MinWidth(400f), GUILayout.ExpandWidth(false));
            }
            if(showCultureSelectionWindow) {
                cultureSelectWindowRect.position = new Vector2(diplomacyWindowRect.x + diplomacyWindowRect.width, (diplomacyWindowRect.y));
            }
        }

        public Rect cultureSelectWindowRect;

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
        
        public static Color32 targetColorForZone = Color.white; 
        public static Rect diplomacyWindowRect;
        public static City selectedCity1;
        public static City selectedCity2;
        public static Kingdom selectedCity1Kingdom => selectedCity1.kingdom;

        public static Kingdom selectedCity2Kingdom => selectedCity2.kingdom;

        public bool EnableConstantWar;
        //public bool EnableWarOfTheWorld;
        //public static Kingdom selectedKingdom1;
        //public static Kingdom selectedKingdom2;
        public static bool selectingCity1;
        public static bool selectingCity2;
        //public static bool selectingKingdom1;
        //public static bool selectingKingdom2;
        public static bool city1PaintZone;
        public static bool city2PaintZone;
        //public static bool city1PaintColor;
        //public static bool city2PaintColor;
    }
}
