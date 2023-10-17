using ai.behaviours;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SimpleGUI.Menus {
    class GuiDiplomacy {

        public Dictionary<Kingdom, List<Kingdom>> eternalAllies = new Dictionary<Kingdom, List<Kingdom>>();
        public Dictionary<Kingdom, List<Kingdom>> eternalEnemies = new Dictionary<Kingdom, List<Kingdom>>();

        public static bool showCultureSelectionWindow;
        public static string currentCultureFromSelectionWindow = "";

        public static bool forcingSettlement;

        public void diplomacyCultureSelectWindow(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(
          scrollPosition, GUILayout.Height(diplomacyWindowRect.height - 32.5f));
            foreach(Culture culture in MapBox.instance.cultures.list) {
				if(GUILayout.Button(culture.name) && selectedCity1 != null) {
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
                bool isAlly = false;
                bool isEnemy = false;
                Alliance alliance = selectedCity1.kingdom.getAlliance();
                if (alliance == null)
                {
                    
                }
                else
                {
                    if (alliance.kingdoms_list.Contains(selectedCity2.kingdom))
                    {
                        isAlly = true;
                    }
                }
                if(isAlly == false)
                {
                    if (selectedCity1.kingdom.isEnemy(selectedCity2.kingdom))
                    {
                        isEnemy = true;
                    }
                }
                if (isAlly) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("Alliance")) {
                    //dej's method of doing this
                    foreach (War war in World.world.wars.getWars(selectedCity1.kingdom))
                    {
                        if (war.isInWarWith(selectedCity1.kingdom, selectedCity2.kingdom))
                        {
                            war.removeFromWar(selectedCity1.kingdom);
                            war.removeFromWar(selectedCity2.kingdom);
                        }
                    }
                    if (Alliance.isSame(selectedCity1.kingdom.getAlliance(), selectedCity2.kingdom.getAlliance()))
                    {
                        selectedCity2.kingdom = null;
                    }
                    Alliance allianceA = selectedCity1.kingdom.getAlliance();
                    Alliance allianceB = selectedCity2.kingdom.getAlliance();
                    if (allianceA != null)
                    {
                        if (allianceB != null)
                        {
                            World.world.alliances.dissolveAlliance(allianceB);
                        }
                        forceAllianceJoin(allianceA, selectedCity2.kingdom, true);
                    }
                    else
                    {
                        forceNewAlliance(selectedCity1.kingdom, selectedCity2.kingdom);
                    }
                }
                if(isEnemy) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("At war")) {
                    if (isAlly)
                    {
                        selectedCity1.kingdom.getAlliance().leave(selectedCity2.kingdom, true);
                        MapBox.instance.diplomacy.startWar(selectedCity1Kingdom, selectedCity2Kingdom, WarTypeLibrary.whisper_of_war);
                        return;
                    }
                    MapBox.instance.diplomacy.startWar(selectedCity1Kingdom, selectedCity2Kingdom, WarTypeLibrary.whisper_of_war);
                }
                GUILayout.EndHorizontal(); 
                GUI.backgroundColor = Color.grey;
            }
            else {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("Need City1");
                GUILayout.Button("Need City2");
                GUILayout.EndHorizontal();
            }
            if(selectedCity1 != null) {
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.cyan;
                GUILayout.Button("City1:");
                GUI.backgroundColor = Color.grey;
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
                    if(GUILayout.Button("Transfer")) {
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
                if(forcingSettlement)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                if (GUILayout.Button("Force new settlement")) {
                    forcingSettlement = !forcingSettlement;
                }
                if(selectedCity1 == null) {
                    GUILayout.Button("NeedCity1");
				}
				else {
                    if(GUILayout.Button("Transfer")) {
                        selectedCity2.joinAnotherKingdom(selectedCity1Kingdom);
                    }
                }
                GUILayout.EndHorizontal();
            }
            else {
                GUI.backgroundColor = Color.yellow;
                GUILayout.Button("NeedCity2");
            }
            GUI.backgroundColor = Color.grey;
            GUILayout.BeginHorizontal();
            GUILayout.Button("Add/Remove zones:");
            GUI.backgroundColor = Color.grey;
            if(selectedCity1 != null) {
                if(city1PaintZone) {
                    GUI.backgroundColor = Color.green;
                }
                else {
                    GUI.backgroundColor = Color.red;
                }
                if(GUILayout.Button("City1")) {
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
                if(GUILayout.Button("City2")) {
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
                EnableConstantWar = false;
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
            WorldTile targetTile = MapBox.instance.getMouseTilePos();
            if (forcingSettlement && Input.GetMouseButton(0))
            {
                if (targetTile != null)
                {
                    if (targetTile.zone.city != null)
                    {
                       //city exists here already, dont force settle
                    }
                    else
                    {
                        City newCity = MapBox.instance.cities.buildNewCity(targetTile.zone, selectedCity2.race, selectedCity2.kingdom);
                        if (newCity == null)
                        {
                            return;
                        }
                        newCity.newCityEvent();
                        if(selectedCity2.data != null && selectedCity2.data.culture != null)
                        {
                            newCity.setCulture(selectedCity2.data.culture);
                        }
                        selectedCity2.kingdom.newCityBuiltEvent(newCity);
                        Actor startingCitizen = World.world.units.spawnNewUnit("unit_" + selectedCity2.race.id, targetTile.zone.tiles.GetRandom(), false, 0f);
                        Actor startingCitizen2 = World.world.units.spawnNewUnit("unit_" + selectedCity2.race.id, targetTile.zone.tiles.GetRandom(), false, 0f);
                        Actor startingCitizen3 = World.world.units.spawnNewUnit("unit_" + selectedCity2.race.id, targetTile.zone.tiles.GetRandom(), false, 0f);
                        startingCitizen.joinCity(newCity);
                        startingCitizen2.joinCity(newCity);
                        startingCitizen3.joinCity(newCity);
                        WorldLog.logNewCity(newCity);
                    }
                }
                forcingSettlement = false;
            }
            if (selectingCity1 && Input.GetMouseButton(0)) {
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
                if (Input.GetMouseButton(1) && MapBox.instance.getMouseTilePos().zone.city == selectedCity1 && selectedCity1.zones.Count > 1) {
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
                if(Input.GetMouseButton(1) && MapBox.instance.getMouseTilePos().zone.city == selectedCity2 && selectedCity2.zones.Count > 1) {
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
            if(SimpleSettings.showHideDiplomacyConfig != null && SimpleSettings.showHideDiplomacyConfig) {
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

        private static void forceAllianceJoin(Alliance alliance, Kingdom pKingdom, bool pRecalc)
        {
            alliance.kingdoms_hashset.Add(pKingdom);
            pKingdom.allianceJoin(alliance);
            if (pRecalc)
            {
                alliance.recalculate();
            }
            alliance.data.timestamp_member_joined = World.world.getCurWorldTime();
        }

        public static Alliance forceNewAlliance(Kingdom pKingdom, Kingdom pKingdom2)
        {
            Alliance alliance = World.world.alliances.newObject(null);
            alliance.createAlliance();
            forceAddFounders(alliance, pKingdom, pKingdom2);
            WorldLog.logAllianceCreated(alliance);
            return alliance;
        }

        public static void forceAddFounders(Alliance alliance, Kingdom pKingdom1, Kingdom pKingdom2)
        {
            alliance.data.founder_kingdom_1 = pKingdom1.data.name;
            if (pKingdom1.king != null)
            {
                alliance.data.founder_name_1 = pKingdom1.king.getName();
            }
            forceAllianceJoin(alliance, pKingdom1, true);
            forceAllianceJoin(alliance, pKingdom2, true);
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
