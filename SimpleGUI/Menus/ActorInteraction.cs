using System;
using System.Collections.Generic;
using ai.behaviours;
using UnityEngine;

namespace SimpleGUI.Menus {
    class ActorInteraction {

        public void actorInteractionWindowUpdate()
        {
            if(SimpleSettings.showHideActorInteractConfig.Value) {
                actorInteractionWindowRect = GUILayout.Window(41094, actorInteractionWindowRect, actorWindow, "Actor Interaction", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                if(showTaskWindow) {
                    actorTaskListWindowRect = GUILayout.Window(43095, actorTaskListWindowRect, actorTaskWindow, "Actor Tasks", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    actorTaskListWindowRect.position = new Vector2(actorInteractionWindowRect.x + actorInteractionWindowRect.width, (actorInteractionWindowRect.y));
                }
                if (showJobWindow) {
                    actorJobListWindowRect = GUILayout.Window(43096, actorJobListWindowRect, actorJobWindow, "Actor Jobs", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    actorJobListWindowRect.position = new Vector2(actorInteractionWindowRect.x + actorInteractionWindowRect.width, (actorInteractionWindowRect.y));
                }
                if (showHideDragSelectionWindow) {
                    dragSelectionWindowRect = GUILayout.Window(41888, dragSelectionWindowRect, dragSelectWindow, "Actor Drag");
                    dragSelectionWindowRect.position = new Vector2(actorInteractionWindowRect.x, (actorInteractionWindowRect.y + actorInteractionWindowRect.height));
                }

                if (showSkinWindow)
                {
                    actorSkinWindowRect = GUILayout.Window(43097, actorSkinWindowRect, actorSkinWindow, "Skin Selection", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    actorSkinWindowRect.position = new Vector2(actorInteractionWindowRect.x + actorInteractionWindowRect.width, (actorInteractionWindowRect.y));
                }

            }
        }

        //parent this to actor window on right side somehow
        public void actorTaskWindow(int windowID)
        {
            scrollPositionTask = GUILayout.BeginScrollView(
         scrollPositionTask, GUILayout.Width(300f), GUILayout.Height(actorInteractionWindowRect.height - 50f));
            foreach(BehaviourTaskActor libTask in AssetManager.tasks_actor.list) {
                if(GUILayout.Button(libTask.id)) {
                    selectedTask = libTask.id;
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        //parent this to actor window on right side somehow
        public void actorJobWindow(int windowID)
        {
            scrollPositionJob = GUILayout.BeginScrollView(
        scrollPositionJob, GUILayout.Width(300f), GUILayout.Height(actorInteractionWindowRect.height - 50f));
            foreach(ActorJob libJob in AssetManager.job_actor.list) {
                if(GUILayout.Button(libJob.id)) {
                    selectedJob = libJob.id;
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        //parent this to actor window on right side somehow
        public void actorSkinWindow(int windowID)
        {
            scrollPositionSkin = GUILayout.BeginScrollView(
          scrollPositionSkin, GUILayout.Width(300f), GUILayout.Height(actorInteractionWindowRect.height - 50f));
            foreach (string skinToSelect in textureSelectionList)
            {
                if (GUILayout.Button(skinToSelect))
                {
                    newTexture = skinToSelect;
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        //size (height) of window should be limited, OR resize if smaller
        public float dragSize => Math.Max(200f, lastSelectedActorList.Count * 25f);

        //parent this to actor window on BOTTOM side somehow
        public void dragSelectWindow(int windowID)
        {
            scrollPositionDragSelect = GUILayout.BeginScrollView(
                // maybe modify height later
        scrollPositionDragSelect, GUILayout.Width(300f), GUILayout.Height(200f));
            foreach(Actor actor in lastSelectedActorList) {
                if(GUILayout.Button(actor.getName())) {
                    lastSelected = actor;
                }
            }
            GUILayout.EndScrollView();
            //GUI.DragWindow();
        }

        public string selectedTask = "nothing";
        public string selectedJob = "mush";

        public bool showTaskWindow;
        public bool showJobWindow;
        public bool showSkinWindow;


        public bool showAiStuff;
        public Color ori;

        public void actorWindow(int windowID)
        {
            ori = GUI.backgroundColor;
            if((lastSelected == null && Config.selectedUnit != null) || (lastSelected != Config.selectedUnit && Config.selectedUnit != null)) {
                lastSelected = Config.selectedUnit;
            }
            if(lastSelected != null) {
                GUILayout.Button("Name: " + lastSelected.data.name);
                GUILayout.BeginHorizontal();
                float health = lastSelected.data.health; // replace with actual health value
                Color redColor = Color.red;
                Color greenColor = Color.green;

                // calculate interpolation value based on health percentage
                float t = health / lastSelected.getMaxHealth();

                // interpolate between red and green colors using t value
                Color healthColor = Color.Lerp(redColor, greenColor, t);
                GUI.backgroundColor = healthColor;
                GUILayout.Button("Health: " + lastSelected.data.health + "/" + lastSelected.getMaxHealth());
                if(GUILayout.Button("Heal")) {
                    lastSelected.restoreHealth(lastSelected.getMaxHealth());
                    lastSelected.setStatsDirty();
                }
                GUI.backgroundColor = ori;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("--")) {
                    lastSelected.data.level -= 1;
                    lastSelected.restoreHealth(lastSelected.getMaxHealth());
                    lastSelected.setStatsDirty();
                }
                GUILayout.Button("Level: " + lastSelected.data.level);
                if(GUILayout.Button("++")) {
                    lastSelected.data.level += 1;
                    lastSelected.restoreHealth(lastSelected.getMaxHealth());
                    lastSelected.setStatsDirty();
                }
                
                GUILayout.EndHorizontal();
                string mood = lastSelected.data.mood;
                if(mood == "sad") {
                    GUI.backgroundColor = Color.yellow;
				}
                if(mood == "angry") {
                    GUI.backgroundColor = Color.red;
                }
                if(mood == "dark") {
                    GUI.backgroundColor = Color.black;
                }
                if(mood == "normal") {
                    GUI.backgroundColor = ori;
                }
                if(mood == "happy") {
                    GUI.backgroundColor = Color.green;
                }
                if(GUILayout.Button("Mood: " + mood)) {
                    lastSelected.data.mood = moods.GetRandom();
                }
                GUI.backgroundColor = ori;
                if(GUILayout.Button("Favorite food: " + lastSelected.data.favoriteFood)) {
                    lastSelected.data.favoriteFood = lastSelected.race.preferred_food.GetRandom();
                }
                // dej did it first in collectionbox, fits here too
                if(GUILayout.Button("Force join city unit is standing on")) {
                    if(lastSelected.city != lastSelected.currentTile.zone.city) {
                        lastSelected.currentTile.zone.city.addNewUnit(lastSelected);
                    }
                }
                if(GUILayout.Button("Force to city leader") && lastSelected.city != null) {
                    // try to set king to someone else so leader can be set instead
                    if(lastSelected.kingdom != null && lastSelected.isKing()) {
                        lastSelected.kingdom.setKing(lastSelected.kingdom.units.GetRandom());
                    }
                    City.makeLeader(lastSelected, lastSelected.city);
                }
                if(GUILayout.Button("Force to kingdom king") && lastSelected.kingdom != null) {
                    lastSelected.kingdom.setKing(lastSelected);
                }
                /*
                // rgb pixels in the worst ways possible
                if(GUILayout.Button("Test c")) {
                    // Get the sprite renderer component from an object in the scene
                    SpriteRenderer spriteRenderer = lastSelected.spriteRenderer;

                    // Get the sprite's texture
                    Texture2D texture = spriteRenderer.sprite.texture;

                    // Loop through every pixel in the texture
                    for(int x = 0; x < texture.width; x++) {
                        for(int y = 0; y < texture.height; y++) {
                            // Get the color of the pixel
                            Color color = texture.GetPixel(x, y);

                            // Assign a new color to the pixel
                            if(color.a != 0) {
                                Color newColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);
                                texture.SetPixel(x, y, newColor);
                            }
                        }
                    }

                    // Apply the modified texture to the sprite
                    texture.Apply();

                    // Update the sprite renderer's sprite to use the modified texture
                    spriteRenderer.sprite = Sprite.Create(texture, spriteRenderer.sprite.rect, new Vector2(0.5f, 0.5f));
                }
                // made by chatgpt
                if(GUILayout.Button("Test e")) {
                    SpriteRenderer spriteRenderer = lastSelected.spriteRenderer;
                    Texture2D tex = spriteRenderer.sprite.texture;
                    Color[] pixels = tex.GetPixels();
                    int width = tex.width;
                    int height = tex.height;

                    for(int y = 0; y < height; y++) {
                        for(int x = 0; x < width; x++) {
                            int index = y * width + x;
                            Color pixel = pixels[index];
                            if(pixel.a > 0) {
                                float hue = (float)x / width;
                                Color newColor = Color.HSVToRGB(hue, 1f, 1f);
                                pixels[index] = newColor;
                            }
                        }
                    }
                    tex.SetPixels(pixels);
                    tex.Apply();

                    // Update the sprite renderer's sprite to use the modified texture
                    spriteRenderer.sprite = Sprite.Create(tex, spriteRenderer.sprite.rect, new Vector2(0.5f, 0.5f));
                }
                */

                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Set texture:"))
                {
                    if(string.IsNullOrEmpty(newTexture) == false)
                    {
                        string strToSubmit = newTexture;
                        //make sure string being used isnt in blacklist and also original actor asset isnt in blacklist // && textureSelectionBlacklist.Contains(lastSelected.asset.id) == false
                        if (textureSelectionBlacklist.Contains(strToSubmit) == false)
                        {
                            ActorAsset a = AssetManager.actor_library.get(newTexture);
                            //help i guess, can make some sort of selection for this after confirming it works
                            if (a != null)
                            {
                                if (a.unit)
                                {
                                    strToSubmit = AssetManager.raceLibrary.get(a.race).main_texture_path;
                                }
                                else
                                {
                                    strToSubmit = AssetManager.actor_library.get(newTexture).texture_path;
                                }
                            }

                            lastSelected.data.set("textureOverride", strToSubmit);

                            lastSelected.dirty_sprite_main = true;
                            lastSelected.setHeadSprite(null);
                        }
                    }
                }
                newTexture = GUILayout.TextField(newTexture);
                GUI.backgroundColor = Color.yellow;
                string x = "->";
                if (showSkinWindow)
                {
                    GUI.backgroundColor = Color.green;
                    x = "x";
                }
                if (GUILayout.Button(x))
                {
                    showSkinWindow = !showSkinWindow;
                }
                GUILayout.EndHorizontal();

                string s = "v AI stuff v";
                GUI.backgroundColor = Color.yellow;
                if(showAiStuff) {
                    GUI.backgroundColor = Color.green;
                    s = "^ AI stuff ^";
				}
                if(GUILayout.Button(s)) {
                    showAiStuff = !showAiStuff;
				}
                GUI.backgroundColor = ori;
                if(showAiStuff) {
                    BehaviourTaskActor task = lastSelected.ai.task;
                    if(task != null) {
                        GUILayout.Button("Current task: " + task.id);
                        if(GUILayout.Button("Force task: " + selectedTask)) {
                            lastSelected.ai.setTask(selectedTask, true, true);
                        }
                    }
                    if(GUILayout.Button("Show task window")) {
                        showTaskWindow = !showTaskWindow;
                        if(showTaskWindow) {
                            showJobWindow = false;
                        }
                    }
                    ActorJob job = lastSelected.ai.job;
                    if(job != null) {
                        GUILayout.Button("Current job: " + job.id);
                        GUILayout.Button("Is job locked: " + lockedActorJobs.ContainsKey(lastSelected));
                        if(GUILayout.Button("Force job: " + selectedJob)) {
                            lastSelected.ai.setJob(selectedJob);
                            if(lockActorJob) {
                                lockedActorJobs.Add(lastSelected, selectedJob);
                            }
                        }
                        if(GUILayout.Button("Force random citizen to " + selectedJob)) {
                            Actor randy = lastSelected.city.units.GetRandom();
                            randy.ai.setJob(selectedJob);
                            if(lockActorJob) {
                                lockedActorJobs.Add(randy, selectedJob);
                            }
                        }
                    }
                    if(GUILayout.Button("Show job window")) {
                        showJobWindow = !showJobWindow;
                        if(showJobWindow) {
                            showTaskWindow = false;
                        }
                    }
                    if(GUILayout.Button("Lock job after forced: " + lockActorJob)) {
                        lockActorJob = !lockActorJob;
                        if(lockActorJob == false) {
                            lockedActorJobs = new Dictionary<Actor, string>();
                        }
                    }
                }
               
            }
            else {
                GUILayout.Button("Inspect an actor");

            }

            if(lockedActorJobs.Count > 0) {
                foreach(KeyValuePair<Actor, string> pair in lockedActorJobs) {
                    if(pair.Key != null && pair.Key.data.alive) {
                        ActorJob job = pair.Key.ai.job;
                        if(job != null && job.id != pair.Value) {
                            pair.Key.ai.setJob(pair.Value);
						}
                    }
                }
			}

            //set task/behaviour?
            if(GUILayout.Button("Reset inspection")) {
                lastSelected = null;
            }
            if(GUILayout.Button("Select using hover: " + selectUsingHover)) {
                selectUsingHover = !selectUsingHover;
            }

            if(dragSelection) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;

            }
            if(GUILayout.Button("Drag selection")) {
                dragSelection = !dragSelection;
                if(dragSelection == false) {
                    startTile = null;
                    endTile = null;
                }
            }
            //basic control? or leave that for superrpg?
            GUI.DragWindow();
        }

        public static bool hasTextureOverride(ActorBase __instance)
        {
            if(__instance.data != null)
            {
                if (__instance.data.custom_data_string != null)
                {
                    if (__instance.data.custom_data_string.dict != null)
                    {
                        if (__instance.data.custom_data_string.dict.ContainsKey("textureOverride"))
                        {
                            Debug.Log("texture override, actor has a textureOverride: " + __instance.data.custom_data_string.dict["textureOverride"]);
                            return true;
                        }
                    }
                    else
                    {
                        Debug.Log("texture override, actor custom_string_data DICT is null");
                    }
                }
                else
                {
                    Debug.Log("texture override, actor custom_string_data is null");
                    __instance.data.custom_data_string = new CustomDataContainer<string>();
                }
            }
            else
            {
                Debug.Log("texture override, actor data is null");
            }
            return false;
        }

        public static bool checkAnimationContainer_Prefix(ActorBase __instance)
        {
            if (hasTextureOverride(__instance))
            {
                if (!__instance.dirty_sprite_main)
                {
                    return false;
                }
                __instance.dirty_sprite_main = false;
                string str = __instance.data.custom_data_string["textureOverride"];
                AnimationContainerUnit animationContainerUnit = ActorAnimationLoader.loadAnimationUnit("actors/" + str, __instance.asset);
                __instance.animationContainer = animationContainerUnit;
                return false;
            }
            return true;
            /*
            if (lockedActorTextures.ContainsKey(__instance)){
                if (!__instance.dirty_sprite_main)
                {
                    return false;
                }
                __instance.dirty_sprite_main = false;
                string str = lockedActorTextures[__instance];
                AnimationContainerUnit animationContainerUnit = ActorAnimationLoader.loadAnimationUnit("actors/" + str, __instance.asset);
                __instance.animationContainer = animationContainerUnit;
                return false;
            }
            else
            {
                return true;
            }
            */
        }

        //take all id from assetmanager and exclude ones that dont work
        //modded ones are ???
        public static List<string> textureSelectionBlacklist = new List<string> { 
            "_unit",
            "unit_human",
            "unit_elf",
            "unit_dwarf",
            "unit_orc",
            "baby_human",
            "baby_dwarf",
            "baby_elf",
            "baby_orc",
            "dragon",
            "zombie_dragon",
            "tornado",
            "crabzilla",
            "godFinger",
            "_mob",
            "_animal",
            "_peacefulAnimal",
            "_carnivore",
            "_herbivore",
            "_omnivore",
            "_insect",
            "_flying_insect",
            "_egg",
            "livingPlants",
            "livingHouse",
            "_boat",
            "boat_trading",
            "boat_transport",
            "boat_fishing"
        };

        //init with all ids of actor assets
        public static List<string> textureSelectionList = new List<string>();

        //prevent random heads popping back up
        public static bool checkSpriteHead_Prefix(ActorBase __instance)
        {
            if (hasTextureOverride(__instance))
            {
                return false;
            }
            return true;
        }

        public static string newTexture = "";

        public static bool selectUsingHover;
        public Rect actorInteractionWindowRect;
        public Rect actorTaskListWindowRect;
        public Rect actorJobListWindowRect;
        public Rect actorSkinWindowRect;


        public Rect dragSelectionWindowRect;


        public Vector2 scrollPositionTask;
        public Vector2 scrollPositionJob;
        public Vector2 scrollPositionDragSelect;
        public Vector2 scrollPositionSkin;

        public bool lockActorJob;
        Dictionary<Actor, string> lockedActorJobs = new Dictionary<Actor, string>();

        public List<string> moods = new List<string> { "happy", "normal", "sad", "angry" };

        public static Actor lastSelected;


        bool temp;
        public List<WorldTile> lastSelectedTiles;
        public List<Actor> selectedActorList;
        public List<Actor> lastSelectedActorList = new List<Actor>();

        public bool showHideDragSelectionWindow;
        public void actorDragSelectionUpdate()
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
                    lastSelectedActorList = new List<Actor>();
                    if(MapBox.instance.getMouseTilePos() != null) {
                        endTile = MapBox.instance.getMouseTilePos();
                        temp = false;
                        List<WorldTile> list = CheckTilesBetween2(startTile, endTile);
                        if(list != null) {
                            lastSelectedTiles = list;
                            foreach(WorldTile tile in list) {
                                foreach(Actor actor in tile._units)
                                {
                                    if(lastSelectedActorList.Contains(actor) == false)
                                    {
                                        lastSelectedActorList.Add(actor);
                                    }
                                }
							}
                        }

                        if(lastSelectedActorList.Count >= 1) {
                            selectedActorList = lastSelectedActorList;
                        }
                    }
                }
                if(Input.GetKeyDown(KeyCode.R)) {
                    startTile = null;
                    endTile = null;
                    lastSelectedTiles = null;
                }
                if(lastSelectedActorList.Count > 0) {
                    showHideDragSelectionWindow = true;
                }
                else {
                    showHideDragSelectionWindow = false;
                }

                // Perma show highlight while they move + after
                
                if (lastSelectedActorList != null && lastSelectedActorList.Count > 0)
                {
                    foreach (Actor unit in lastSelectedActorList)
                    {
                        flashEffects.flashPixel(unit.currentTile, 10, ColorType.Purple);
                    }
                }
                
            }

        }



        public static PixelFlashEffects flashEffects => MapBox.instance.flashEffects;
        public static bool dragCircular;
        public List<WorldTile> CheckTilesBetween2(WorldTile target1, WorldTile target2)
        {
            List<WorldTile> tilesToCheck = new List<WorldTile>(); // list for later
            Vector2Int pos1 = target1.pos;
            Vector2Int pos2 = target2.pos;
            float distanceBetween = Toolbox.DistTile(target1, target2);
            int pSize = (int)distanceBetween;
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
                    flashEffects.flashPixel(tile, 10, ColorType.White);
                }
                return tilesToCheck;
            }
            else {
                int x = pos1.x;
                int y = pos1.y;
                int radius = (int)distanceBetween;
                Vector2 center = new Vector2(x, y);
                for(int i = x - radius; i < x + radius + 1; i++) {
                    for(int j = y - radius; j < y + radius + 1; j++) {
                        if(Vector2.Distance(center, new Vector2(i, j)) <= radius) {
                            WorldTile tile = MapBox.instance.GetTile(i, j);
                            if(tile != null) {
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
        public bool dragSelection;
        public WorldTile startTile;
        public WorldTile endTile;

        // easy interaction by just hovering over units
        public static void showActor_Postfix(string pTitle, Tooltip pTooltip, TooltipData pData)
        {
			if(selectUsingHover) {
                if(pData.actor != null) {
                    lastSelected = pData.actor;
                }
            }
        }
    }
}
