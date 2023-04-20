using System.Collections.Generic;
using ai.behaviours;
using UnityEngine;

namespace SimpleGUI {
    class ActorInteraction {

        public void actorInteractionWindowUpdate()
        {
            if(GuiMain.showHideActorInteractConfig.Value) {
                if(showTaskWindow) {
                    actorTaskListWindowRect = GUILayout.Window(43095, actorTaskListWindowRect, actorTaskWindow, "Actor Tasks", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }

                if(showJobWindow) {
                    actorJobListWindowRect = GUILayout.Window(43096, actorJobListWindowRect, actorJobWindow, "Actor Jobs", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }

                actorInteractionWindowRect = GUILayout.Window(41094, actorInteractionWindowRect, actorWindow, "Actor Interaction", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                if(showTaskWindow) {
                    actorTaskListWindowRect.position = new Vector2(actorInteractionWindowRect.x + actorInteractionWindowRect.width, (actorInteractionWindowRect.y));
                }
                if(showJobWindow) {
                    actorJobListWindowRect.position = new Vector2(actorInteractionWindowRect.x + actorInteractionWindowRect.width, (actorInteractionWindowRect.y));
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

        public string selectedTask = "nothing";
        public string selectedJob = "mush";

        public bool showTaskWindow;
        public bool showJobWindow;

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
            //basic control? or leave that for superrpg?
            GUI.DragWindow();
        }

        public static bool selectUsingHover;
        public Rect actorInteractionWindowRect;
        public Rect actorTaskListWindowRect;
        public Rect actorJobListWindowRect;

        public Vector2 scrollPositionTask;
        public Vector2 scrollPositionJob;

        public bool lockActorJob;
        Dictionary<Actor, string> lockedActorJobs = new Dictionary<Actor, string>();


        public List<string> moods = new List<string> { "happy", "normal", "sad", "angry" };

        public static Actor lastSelected;

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
