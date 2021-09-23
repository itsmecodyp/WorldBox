using System.Collections.Generic;
using UnityEngine;
using BepInEx;
//using System.Drawing;
using HarmonyLib;
using System.Reflection;
using System;
using System.Linq;
using System.Collections;

namespace LuBuMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class LuBuMain : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.rpg.lubu";
        public const string pluginName = "Lu Bu";
        public const string pluginVersion = "0.0.0.1";

        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(10, 10, 100, 100);


        // mapbox.start initiates lists
        // mapbox.updateactor clears the lists and does benchmarks
        // actor.updatetimers adds them back into list

        public static void AddTraits()
        {
            ActorTrait LubuTrait = new ActorTrait();
            LubuTrait.id = "lubu_unmounted";
            LubuTrait.icon = "lubu_unmounted";
            LubuTrait.baseStats.health = 350;
            LubuTrait.baseStats.damage = 35;
            LubuTrait.baseStats.armor = 50;
            LubuTrait.baseStats.dodge = 50f;
            LubuTrait.baseStats.size = 3f;
            LubuTrait.baseStats.targets = 2;
            LubuTrait.type = TraitType.Positive;

            AssetManager.traits.add(LubuTrait);
            addTraitToLocalizedLibrary(LubuTrait.id, "The one and only!");

            ActorTrait lubu_redhare = new ActorTrait();
            lubu_redhare.id = "lubu_rdhare";
            lubu_redhare.icon = "lubu_rdhare";
            LubuTrait.baseStats.health = 150;
            lubu_redhare.baseStats.speed = 25f;
            lubu_redhare.baseStats.size = 3f;
            lubu_redhare.type = TraitType.Positive;

            AssetManager.traits.add(lubu_redhare);
            addTraitToLocalizedLibrary(lubu_redhare.id, "Lu Bu's beautiful red horse.");

            ActorTrait LubuRedHareArmoured = new ActorTrait();
            LubuRedHareArmoured.id = "lubu_redhare_armoured";
            LubuRedHareArmoured.icon = "lubu_redhare_armoured";
            LubuRedHareArmoured.baseStats.health = 500;
            LubuRedHareArmoured.baseStats.damage = 35;
            LubuRedHareArmoured.baseStats.speed = 25f;
            LubuRedHareArmoured.baseStats.armor = 50;
            LubuRedHareArmoured.baseStats.dodge = 50f;
            LubuRedHareArmoured.baseStats.targets = 5;
            LubuRedHareArmoured.baseStats.size = 3f;
            LubuRedHareArmoured.type = TraitType.Positive;

            AssetManager.traits.add(LubuRedHareArmoured);
            addTraitToLocalizedLibrary(LubuRedHareArmoured.id, "Lu Bu atop his personal horse adorned with armour.");
        }

        /*
        public static bool LuBuTraitAction(BaseSimObject pTarget = null, WorldTile pTile = null)
        {

        }
        */

        public static void updateSpecialTimers_Postfix(Actor __instance) // add trait logic, IE lubu and mount logic
        {
            ActorStatus data = Reflection.GetField(__instance.GetType(), __instance, "data") as ActorStatus;
            //Debug.Log("update trait effects");
            if (__instance.haveTrait("lubu_rdhare")) // normal horse without rider should follow and wait
            {
                if (LuBu != null && LuBu.haveTrait("lubu_unmounted"))
                {
                    if (lastHorseFollow == 0f || lastHorseFollow + horseFollowCooldown < Time.realtimeSinceStartup)
                    {
                        WorldTile targetTile = MapBox.instance.GetTile(
                           lastTile.x + UnityEngine.Random.Range(-3, 3),
                           lastTile.y + UnityEngine.Random.Range(-3, 3)
                           );
                        if (targetTile != null)
                        {
                            BasicMoveAndWait(__instance, targetTile);
                        }
                        lastHorseFollow = Time.realtimeSinceStartup;
                        Debug.Log("horse should have moved");
                    }
                }


            }
            /*
            if (controlledActor == null && (__instance == LuBu || (__instance.haveTrait("lubu_unmounted") || __instance.haveTrait("lubu_redhare_armoured"))))
            {
                if (lubuAIlastTime == 0f || lubuAIlastTime + lubuAICooldown < Time.realtimeSinceStartup)
                {
                    WorldTile targetTile = MapBox.instance.GetTile(
                      __instance.currentTile.x + UnityEngine.Random.Range(-5, 5),
                      __instance.currentTile.y + UnityEngine.Random.Range(-5, 5)
                      );
                    if (targetTile != null)
                    {
                        BasicMoveAndWait(__instance, targetTile);
                    }
                    lubuAIlastTime = Time.realtimeSinceStartup;
                    Debug.Log("lubu (AI) should have moved");

                }
            }
            */
        }


        static float horseFollowCooldown = 5f;
        static float lastHorseFollow = 0f;

        public void HarmonyPatchSetup()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MapBox), "isActionHappening");
            patch = AccessTools.Method(typeof(LuBuMain), "isActionHappening_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MapBox), "updateControls");
            patch = AccessTools.Method(typeof(LuBuMain), "updateControls_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(ItemGenerator), "init");
            patch = AccessTools.Method(typeof(LuBuMain), "init_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Actor), "updateSpecialTimers");
            patch = AccessTools.Method(typeof(LuBuMain), "updateSpecialTimers_Postfix");
            //harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            // patching my own mod.. modding the mod
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(TextureLoader.TextureLoader_Main), "detectCustomActorTexture");
            patch = AccessTools.Method(typeof(LuBuMain), "detectCustomActorTexture_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorBase), "updateAnimation");
            patch = AccessTools.Method(typeof(LuBuMain), "updateAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Post patch: ActorBase.updateAnimation");

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(LocalizedTextManager), "loadLocalizedText");
            patch = AccessTools.Method(typeof(LuBuMain), "loadLocalizedText_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Projectile), "targetReached");
            patch = AccessTools.Method(typeof(LuBuMain), "targetReached_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(ActorBase), "stopMovement");
            patch = AccessTools.Method(typeof(LuBuMain), "stopMovement_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            /*
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Actor), "takeItems");
            patch = AccessTools.Method(typeof(LuBuMain), "takeItems_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(ActorBase), "generatePersonality");
            patch = AccessTools.Method(typeof(LuBuMain), "generatePersonality_Postfix");
            harmony.Patch(original,null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(ActorEquipmentSlot), "setItem");
            patch = AccessTools.Method(typeof(LuBuMain), "setItem_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */
        }
        /*
        public static bool takeItems_Prefix(Actor pActor, bool pIgnoreRangeWeapons = false)
        {
            if (LuBu != null && pActor == LuBu)
            {
                return false;
            }
            return true;
        }
        public static void generatePersonality_Postfix(ActorBase __instance)
        {
            ActorStatus data = Reflection.GetField(__instance.GetType(), __instance, "data") as ActorStatus;
            if (data.firstName == "Lü Bu")
            {
                string name = "Lü Bu";
                manualGeneration = true;
                ItemGenerator.generateItem(AssetManager.items.get("sword"), "silver", LuBu.equipment.weapon, MapBox.instance.mapStats.year, "Lü Bu", name, 1);
                ItemGenerator.generateItem(AssetManager.items.get("ring"), "silver", LuBu.equipment.ring, MapBox.instance.mapStats.year, "Lü Bu", name, 1);
                ItemGenerator.generateItem(AssetManager.items.get("amulet"), "silver", LuBu.equipment.amulet, MapBox.instance.mapStats.year, "Lü Bu", name, 1);
                ItemGenerator.generateItem(AssetManager.items.get("armor"), "adamantine", LuBu.equipment.armor, MapBox.instance.mapStats.year, "Lü Bu", name, 1);
                ItemGenerator.generateItem(AssetManager.items.get("boots"), "adamantine", LuBu.equipment.boots, MapBox.instance.mapStats.year, "Lü Bu", name, 1);
                ItemGenerator.generateItem(AssetManager.items.get("helmet"), "adamantine", LuBu.equipment.helmet, MapBox.instance.mapStats.year, "Lü Bu", name, 1);
                manualGeneration = false;
            }
        }
        public static bool manualGeneration;
        public static bool setItem_Prefix(ItemData pData, ActorEquipmentSlot __instance)
        {
            if (manualGeneration)
            {
                pData.prefix = "perfect";
                pData.suffix = "terror";
                __instance.data = pData;
                return false;
            }
            return true;
        }
        */
        public void SettingSetup()
        {

        }

        public void Awake()
        {
            SettingSetup();
            HarmonyPatchSetup();
        }

        public static string StringWithFirstUpper(string targetstring)
        {
            return char.ToUpper(targetstring[0]) + targetstring.Substring(1);
        }

        public void OnGUI()
        {
            if (GUILayout.Button("ToggleMenu"))
            {
                showHideMainWindow = !showHideMainWindow;
            }
            if (showHideMainWindow)
            {
                mainWindowRect = GUILayout.Window(77001, mainWindowRect, new GUI.WindowFunction(mainWindow), "Main", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            if (lastInteractionActor != null) // showActorInteract && 
            {
                actorInteractWindowRect = GUILayout.Window(77002, actorInteractWindowRect, new GUI.WindowFunction(ActorInteractWindow), "LuBu interaction menu", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
           
            SetWindowInUse(-1); // reset menu in use
        }

        public void UpdateControls()
        {
            if (MapBox.instance != null)
            {
                Camera camera = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "camera") as Camera;
                if (camera != null)
                {
                    if (controlledActor != null) // need a better check
                    {
                        if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.KeypadPlus))
                        {
                            camera.GetComponent<MoveCamera>().cameraZoomSpeed = 5f;
                        }
                        else
                        {
                            camera.GetComponent<MoveCamera>().cameraZoomSpeed = 0f;
                            camera.GetComponent<MoveCamera>().cameraMoveSpeed = 0f;
                        }
                    }
                    else
                    {
                        camera.GetComponent<MoveCamera>().cameraMoveSpeed = 0.01f;
                        camera.GetComponent<MoveCamera>().cameraZoomSpeed = 5f;
                    }
                }
                if (MouseTile != null && Input.GetKeyDown(KeyCode.Q))
                {
                    CyclePower();
                }
                if (MouseTile != null && Input.GetKeyDown(KeyCode.E))
                {
                    UsePower(powerInUse);
                }
                if (MouseTile != null && Input.GetKeyDown(KeyCode.F))
                {
                    lastInteractionActor = ClosestActorToTile(MouseTile, null, 3f);
                }

                if (Input.GetKeyDown(KeyCode.K))
                {
                    if (LuBu == null)
                    {
                        if (controlledActor == null)
                        {
                            timesFired = 0;
                            lubuSpawnTime = Time.realtimeSinceStartup; // set up a timer so he will leave later
                            Actor LuBu = MapBox.instance.createNewUnit("unit_human", MapBox.instance.getMouseTilePos(), null, 0f, null);
                            if (MapBox.instance.getMouseTilePos().zone.city != null)
                            {
                                MapBox.instance.getMouseTilePos().zone.city.addNewUnit(LuBu, true, true);
                                LuBu.city = MapBox.instance.getMouseTilePos().zone.city;
                            }
                            ActorStatus data = Reflection.GetField(LuBu.GetType(), LuBu, "data") as ActorStatus;
                            data.firstName = "Lü Bu";
                            controlledActor = LuBu;
                            // position interaction window
                            lastInteractionActor = LuBu;
                            Vector3 screenPos = new Vector3();
                            SpriteRenderer spriteRendererBody = Reflection.GetField(LuBu.GetType(), LuBu, "spriteRenderer") as SpriteRenderer;
                            Vector3 pos = new Vector3();
                            pos.x = spriteRendererBody.transform.localPosition.x;
                            pos.y = spriteRendererBody.transform.localPosition.y;
                            pos.z = spriteRendererBody.transform.localPosition.z;
                            screenPos = Camera.main.WorldToScreenPoint(pos);
                            screenPos.y = (Screen.height - screenPos.y);
                            actorInteractWindowRect.x = screenPos.x - 200;
                            actorInteractWindowRect.y = screenPos.y - 100;
                            //
                            LuBuMain.LuBu = LuBu;
                            SimpleLib.Main.ActorSay(LuBu, entranceQuote, 3f);
                            WorldTile targetTile = MapBox.instance.GetTile(
                                   MapBox.instance.getMouseTilePos().x + UnityEngine.Random.Range(-3, 3),
                                   MapBox.instance.getMouseTilePos().y + UnityEngine.Random.Range(-3, 3)
                                  );
                            Actor RedHare = MapBox.instance.createNewUnit("unit_human", targetTile, null, 10f, null);
                            if (MapBox.instance.getMouseTilePos().zone.city != null)
                            {
                                MapBox.instance.getMouseTilePos().zone.city.addNewUnit(RedHare, true, true);
                                RedHare.city = MapBox.instance.getMouseTilePos().zone.city;
                            }
                            //AddTraitToActor(LuBu, "lubu_unmounted");
                            ActorStatus RedHareData = Reflection.GetField(RedHare.GetType(), RedHare, "data") as ActorStatus;
                            RedHareData.firstName = "Red Hare";
                            dedicatedMount = RedHare;
                            LuBu.addTrait("lubu_unmounted");
                            RedHare.addTrait("lubu_rdhare");
                        }
                        else
                        {
                            controlledActor = null;
                        }
                    }
                }
            }
            if (controlledActor != null)
            {
                followActor(controlledActor, 10f); // camera follow
                movementStuff(); // movement controls
                WorldTile clickedTile = null;
                if (Input.GetMouseButtonDown(0) && MapBox.instance.getMouseTilePos() != null)
                {
                    clickedTile = MapBox.instance.getMouseTilePos();
                    // building interactions
                    if (clickedTile.building != null)
                    {
                        // Check building for resources
                        bool haveResources = (bool)Reflection.GetField(clickedTile.building.GetType(), clickedTile.building, "haveResources");
                        BuildingAsset stats = Reflection.GetField(clickedTile.building.GetType(), clickedTile.building, "stats") as BuildingAsset;
                        BuildingData data = Reflection.GetField(clickedTile.building.GetType(), clickedTile.building, "data") as BuildingData;
                        CityData citydata = Reflection.GetField(controlledActor.city.GetType(), controlledActor.city, "data") as CityData;
                        if (haveResources && stats.resourceType != ResourceType.None)
                        {
                            // If controlled unit is part of a city, add the resources to it's inventory
                            if (controlledActor.city != null)
                            {
                                if (stats.resourceType == ResourceType.Gold)
                                {
                                    string pRes = "gold";
                                    citydata.storage.change(pRes, 2);
                                }
                                else if (stats.resourceType == ResourceType.Stone)
                                {
                                    string pRes2 = "stone";
                                    citydata.storage.change(pRes2, 3);
                                }
                                else if (stats.resourceType == ResourceType.Ore)
                                {
                                    string pRes3 = "ore";
                                    citydata.storage.change(pRes3, 3);
                                }
                                if (stats.resourceType == ResourceType.Fruits)
                                {
                                    string pRes4 = "food";
                                    clickedTile.building.CallMethod("extractResources");
                                    controlledActor.CallMethod("eat", new object[] { 100 });
                                    citydata.storage.change(pRes4, 1);
                                }
                                if (stats.id.Contains("tree") || stats.id.Contains("palm"))
                                {
                                    Sfx.play("chopping", true, clickedTile.building.transform.localPosition.x, clickedTile.building.transform.localPosition.y);
                                    clickedTile.building.startShake(0.5f);
                                    clickedTile.building.CallMethod("chopTree");
                                    citydata.storage.change("wood", 3);
                                }
                                else
                                {
                                    Sfx.play("mining", true, clickedTile.building.transform.localPosition.x, clickedTile.building.transform.localPosition.y);
                                }
                            }
                            //controlledActor.timer_action = 1f; // delay after interact
                        }
                        // add progress to unfinished building
                        if (clickedTile.building.city != null && controlledActor.city != null && clickedTile.building.city == controlledActor.city && data.underConstruction)
                        {
                            clickedTile.building.CallMethod("updateBuild", new object[] { 3 });
                            //controlledActor.timer_action = 1f;
                            Sfx.play("hammer1", false, clickedTile.building.transform.localPosition.x, clickedTile.building.transform.localPosition.y);
                        }
                    }
                    // Non-building interactions (click)
                    else
                    {

                    }

                }
            }
            else
            {
                if (LuBu != null && LuBu.currentTile != null)
                {
                    lastTile = LuBu.currentTile;
                }
            }
        }
      
        public static Actor LuBu;

        public bool enableAILuBu = true;

        public void Update()
        {
            UpdateControls();
            UpdateFear();
            if (enableAILuBu && LuBu == null)
            {
                if (MapBox.instance != null && MapBox.instance.tilesList != null)
                    if (UnityEngine.Random.Range(0, 100f) > 99f)
                    {
                        Debug.Log("Spawning LuBu");
                        timesFired = 0;
                        lubuSpawnTime = Time.realtimeSinceStartup; // set up a timer so he will leave later
                        WorldTile spawnTile = null;
                        while (spawnTile == null || spawnTile.building != null || spawnTile.Type.liquid || spawnTile.Type.name.Contains("ountain"))
                        {
                            //Debug.Log("trying for new tile");
                            spawnTile = MapBox.instance.tilesList.GetRandom();
                        }
                        MapBox.instance.tilesList.GetRandom();
                        Actor lubuTemp = MapBox.instance.createNewUnit("unit_human", spawnTile, null, 10f, null);
                        lubuTemp.addTrait("lubu_unmounted");
                        if (spawnTile.zone.city != null)
                        {
                            spawnTile.zone.city.addNewUnit(lubuTemp, true, true);
                            lubuTemp.city = spawnTile.zone.city;
                        }
                        ActorStatus data = Reflection.GetField(lubuTemp.GetType(), lubuTemp, "data") as ActorStatus;
                        data.firstName = "Lü Bu";
                        lastInteractionActor = lubuTemp;
                        // game crashes if you dont control lubu as hes created
                        LuBu = lubuTemp;
                        SimpleLib.Main.ActorSay(LuBu, entranceQuote, 3f);
                        WorldTile targetTile = MapBox.instance.GetTile(
                               spawnTile.x + UnityEngine.Random.Range(-3, 3),
                               spawnTile.y + UnityEngine.Random.Range(-3, 3)
                              );
                        Actor RedHare = MapBox.instance.createNewUnit("unit_human", targetTile, null, 10f, null);
                        if (spawnTile.zone.city != null)
                        {
                            spawnTile.zone.city.addNewUnit(RedHare, true, true);
                            RedHare.city = spawnTile.zone.city;
                        }
                        RedHare.addTrait("lubu_rdhare");
                        ActorStatus RedHareData = Reflection.GetField(RedHare.GetType(), RedHare, "data") as ActorStatus;
                        RedHareData.firstName = "Red Hare";
                        dedicatedMount = RedHare;
                    }
            }
            // ai logic
            if (controlledActor == null && LuBu != null)
            {
                ActorStatus data = Reflection.GetField(LuBu.GetType(), LuBu, "data") as ActorStatus;
                if (data.alive)
                {
                    // ai movement
                    if (lastAIRoam + roamCooldown < Time.realtimeSinceStartup)
                    {
                        Actor target = ClosestActorToTile(LuBu.currentTile, LuBu, 10f);
                        WorldTile targetTile = null;
                        if (target != null)
                        {

                            targetTile = target.currentTile;
                        }
                        targetTile = MapBox.instance.GetTile(
                                                          LuBu.currentTile.x + UnityEngine.Random.Range(-6, 6),
                                                          LuBu.currentTile.y + UnityEngine.Random.Range(-6, 6)
                                                          );
                        BasicMoveAndWait(LuBu, targetTile);
                        if (dedicatedMount != null)
                        {
                            ActorStatus dataHorse = Reflection.GetField(dedicatedMount.GetType(), dedicatedMount, "data") as ActorStatus;
                            if (lastHorseFollow == 0f || lastHorseFollow + horseFollowCooldown < Time.realtimeSinceStartup)
                            {
                                WorldTile targetTileHorse = MapBox.instance.GetTile(
                                   targetTile.x + UnityEngine.Random.Range(-3, 3),
                                   targetTile.y + UnityEngine.Random.Range(-3, 3)
                                   );
                                if (targetTileHorse != null)
                                {
                                    BasicMoveAndWait(dedicatedMount, targetTileHorse);
                                }
                                lastHorseFollow = Time.realtimeSinceStartup;
                                Debug.Log("horse should have moved (AI)");
                            }
                        }
                        lastAIRoam = Time.realtimeSinceStartup;
                    }
                    // ai attacks
                    if (lastAttack + attackCooldown < Time.realtimeSinceStartup)
                    {
                        Actor target = ClosestActorToTile(LuBu.currentTile, LuBu, 3f);
                        if (target != null)
                        {
                            Debug.Log("AI Attack: target found, starting");
                            // 50/50 to attack normally or use a power
                            if (Toolbox.randomChance(0.5f)) 
                            {
                                if ((bool)LuBu.CallMethod("tryToAttack", new object[] { target }))
                                {
                                    Debug.Log("AI Attack normal");
                                }
                            }
                            else
                            {
                                UsePower(powerList.GetRandom());
                                Debug.Log("AI Attack power");
                            }
                            lastAttack = Time.realtimeSinceStartup;
                        }
                    }
                    // ai unmonut/mount
                    if (lastMountOrUnmount + mountOrUnMountCooldown < Time.realtimeSinceStartup)
                    {
                        if (LuBu.haveTrait("lubu_redhare_armoured"))
                        {
                            UnmountLubu(LuBu);
                            //Debug.Log("Unmounted (AI)");
                        }
                        else if (LuBu.haveTrait("lubu_unmounted"))
                        {
                            MountLubuArmoured(LuBu);
                            //Debug.Log("Mounted (AI)");
                        }
                        lastMountOrUnmount = Time.realtimeSinceStartup;
                        mountOrUnMountCooldown = UnityEngine.Random.Range(30f, 60f);
                    }
                }
            }
            if (lubuSpawnTime != 0f) // checks if lubu has been spawned
            {
                if (LuBu != null && controlledActor == null) // check if lubu still exists and isnt under manual control
                {
                    if (lubuSpawnTime + lubuDuration < Time.realtimeSinceStartup) // checks a timer 
                    {
                        StartCoroutine(MakeLubuLeave()); // makes him leave after the time is up
                    }
                }
            }
        }

        public float lastMountOrUnmount = 0f;
        public float mountOrUnMountCooldown = 60f;

        public float lastAttack = 0f;
        public float attackCooldown = 1.2f;

        public float lastAIRoam = 0f;
        public float roamCooldown = 5f;

        public IEnumerator MakeLubuLeave()
        {
            if (LuBu != null)
            {
                if (LuBu == controlledActor)
                {
                    controlledActor = null;
                }
                SimpleLib.Main.ActorSay(LuBu, exitQuote, 5f); // display entrance message 
                LuBu.killHimself();
                LuBu = null;
            }
            yield return new WaitForSeconds(5);
            if (dedicatedMount != null)
            {
                dedicatedMount.killHimself();
                dedicatedMount = null;
            }
        }
        public IEnumerator ResetControl()
        {
            yield return new WaitForSeconds(1.5f);
            controlledActor = null;
        }

        private void UpdateFear()
        {
            // fear power

            if (fearTargets && lastFearTime + fearLength > Time.realtimeSinceStartup) // overall power cooldown
            {
                if (lastFearTick == 0f || (lastFearTick + lastFearTickCooldown < Time.realtimeSinceStartup)) // tick cooldown, faster = more panic
                {
                   
                        FearActors(lastTargetedList, controlledActor);
                        lastFearTick = Time.realtimeSinceStartup;
                    
                }
            }
            else
            {
                fearTargets = false;
            }
        }

        public bool hasManuallySpawned = false;

        public void mainWindow(int windowID)
        {
            SetWindowInUse(windowID);
            GUILayout.Button("Press K");
            GUI.DragWindow();
        }

        public void AttackActor(Actor attacker, Actor target, float damage)
        {
            attacker.CallMethod("punchTargetAnimation", new object[] { target.currentPosition, target.currentTile, true, false, 40f });
            Sfx.play("punch", true, attacker.currentPosition.x, attacker.currentPosition.y);
            target.CallMethod("getHit", new object[] { damage, true, AttackType.Other, null, true});
        }

        public static string powerInUse = "slam";
        public void UsePower(string power)
        {
            // power cooldowns are checked individually, might be smarter to use globals?
            if (power == "fear")
            {
                StartFear();
            }
            else if (power == "slam")
            {
                StartSlam();
            }
            else if (power == "slash")
            {
                StartSlash();
            }
            else if (power == "arrowNormal")
            {
                rpgFireProjectile(LuBu, ClosestActorToTile(MouseTile, controlledActor, 3f), power);
            }
            else if (power == "arrowExplode")
            {
                rpgFireProjectile(LuBu, ClosestActorToTile(MouseTile, controlledActor, 3f), power);
            }
            else if (power == "arrowVolley")
            {
                rpgFireProjectile(LuBu, ClosestActorToTile(MouseTile, controlledActor, 3f), power, 5);
            }
        }
        // copy of StackEffects.slash
        public void ModdedSlash(Vector3 pVec, string pType, float pAngle)
        {
            //bool flag = !MapBox.instance.qualityChanger.lowRes;
            // if (flag)
            if (true)
            {
                BaseEffect slashEffect = (MapBox.instance.stackEffects.CallMethod("get", new object[] { "slash" }) as BaseEffectController).spawnAtRandomScale(MapBox.instance.GetTile((int)pVec.x, (int)pVec.y), 0.5f, 1.5f);
                if (slashEffect != null)
                {
                    SpriteAnimation component = slashEffect.GetComponent<SpriteAnimation>();
                    Sprite[] frames = Resources.LoadAll<Sprite>("effects/slashes/slash_" + pType);
                    component.CallMethod("setFrames", new object[] { frames });
                    slashEffect.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, pAngle));
                }
            }
        }

        public static bool rpgFireOnlyAtEnemies;

        public static List<Projectile> firedProjectiles = new List<Projectile>();

        // patch runs as arrows land, add effects
        public static bool targetReached_Prefix(Projectile __instance)
        {
            Vector3 vecTarget = (Vector3)Reflection.GetField(__instance.GetType(), __instance, "vecTarget");
            WorldTile tileLanded = null;
            if (vecTarget != null)
            {
                tileLanded = MapBox.instance.GetTile((int)vecTarget.x, (int)vecTarget.y);
            }
            if (tileLanded != null)
            {
                if (firedProjectiles != null && firedProjectiles.Contains(__instance))
                {
                    Debug.Log("arrow landed that was fired from actor");
                    if (powerInUse == "arrowExplode")
                    {
                        DropsLibrary.action_grenade(tileLanded);
                        firedProjectiles.Remove(__instance);
                    }
                    if (powerInUse == "arrowNormal" || powerInUse == "arrowVolley")
                    {
                        Actor closestTarget = ClosestActorToTile(tileLanded, null, 5f);
                        if (closestTarget != null)
                        {
                            if (Toolbox.DistTile(closestTarget.currentTile, tileLanded) <= 1.4f)
                            {
                                List<Actor> oneTargetList = new List<Actor>();
                                oneTargetList.Add(closestTarget);
                                DamageActors(oneTargetList, null);
                            }
                        }
                        firedProjectiles.Remove(__instance);
                    }
                }
            }
            return true;
        }

        public static int timesFired = 0;

        public static void rpgFireProjectile(Actor fromActor, Actor targetActor, string projectileType, int volleyCount = 1)
        {
            if (timesFired < 3)
            {
                float actorHeight;
                if (targetActor != null)
                {
                    actorHeight = 0f;
                    bool targetIsInAir = (bool)targetActor.CallMethod("isInAir", new object[] { });
                    Debug.Log("targetIsInAir: " + targetIsInAir.ToString());
                    float getZ = (float)targetActor.CallMethod("getZ", new object[] { });
                    Debug.Log("getZ: " + getZ.ToString());
                    if (projectileType == "fireball")
                    {
                        // unused in lubu mod
                        if (!rpgFireOnlyAtEnemies)
                        {
                            MapBox.instance.stackEffects.CallMethod("startProjectile", new object[] { fromActor.currentTile.posV3, targetActor.currentTile.posV3, "fireball", 0f });
                        }
                    }
                    else if (projectileType == "arrowNormal" || projectileType == "arrowExplode")
                    {
                        if (targetIsInAir)
                        {
                            actorHeight = getZ;
                        }
                        BaseStats curStats = Reflection.GetField(targetActor.GetType(), targetActor, "curStats") as BaseStats;
                        Vector3 attemptTowards = Vector3.MoveTowards(targetActor.currentTile.posV3, targetActor.nextStepPosition, curStats.size * 3f); // - ???
                        Projectile arrow = null;
                        if (!rpgFireOnlyAtEnemies)
                        {
                            arrow = MapBox.instance.stackEffects.CallMethod("startProjectile", new object[] { fromActor.currentTile.posV3, attemptTowards, "arrow", actorHeight }) as Projectile;
                        }
                        if (fromActor.haveTrait("giant"))
                        {
                            arrow.transform.localScale *= 3f;
                        }
                        arrow.targetObject = targetActor;
                        Actor byUnit = Reflection.GetField(arrow.GetType(), arrow, "byWho") as Actor;
                        byUnit = fromActor;
                        firedProjectiles.Add(arrow);
                        timesFired++;
                    }
                    else if (projectileType == "arrowVolley")
                    {
                        for (int i = 0; i < volleyCount; i++)
                        {
                            if (targetIsInAir)
                            {
                                actorHeight = getZ;
                            }
                            BaseStats curStats = Reflection.GetField(targetActor.GetType(), targetActor, "curStats") as BaseStats;
                            Vector3 attemptTowards = Vector3.MoveTowards(targetActor.currentTile.posV3, targetActor.nextStepPosition, curStats.size * 3f); // - ???
                            Projectile arrow = null;
                            if (!rpgFireOnlyAtEnemies)
                            {
                                arrow = MapBox.instance.stackEffects.CallMethod("startProjectile", new object[] { fromActor.currentTile.posV3, attemptTowards, "arrow", actorHeight }) as Projectile;
                            }
                            if (fromActor.haveTrait("giant"))
                            {
                                arrow.transform.localScale *= 3f;
                            }
                            arrow.targetObject = targetActor;
                            Actor byUnit = Reflection.GetField(arrow.GetType(), arrow, "byUnit") as Actor;
                            byUnit = fromActor;
                            firedProjectiles.Add(arrow);
                        }
                        timesFired++;
                    }
                }
            }
          
        }

        public void StartSlash()
        {
            if (lastSlashTime + slashCooldown < Time.realtimeSinceStartup)
            {
                Actor closestTarget = ClosestActorToTile(MouseTile, LuBu, 3f); // finds an actor/target close to the mouse
                if (closestTarget != null)
                {
                    SimpleLib.Main.ActorSay(LuBu, tauntList.GetRandom(), 3);
                    lastTargetedList = ActorsWithinRangeOfTile(closestTarget.currentTile, LuBu, 3f);
                    DamageActors(lastTargetedList, LuBu);
                    // then we start the slash animation
                    float angle = 1f;
                    BaseStats curStats = Reflection.GetField(closestTarget.GetType(), closestTarget, "curStats") as BaseStats;
                    Vector3 attemptTowards = Vector3.MoveTowards(lastTile.posV3, closestTarget.currentTile.posV3, curStats.size * 3f); // - ???
                    float pAngle = Toolbox.getAngle(lastTile.posV3.x, lastTile.posV3.y, attemptTowards.x, attemptTowards.y) * 57.29578f;
                    ModdedSlash(MouseTile.posV3, "claws", pAngle);
                    lastSlashTime = Time.realtimeSinceStartup;
                }
            }
        }
        public float slashCooldown = 1.5f;
        public float slamCooldown = 5f;
        public float fearCooldown = 5f;

        private void StartSlam()
        {
            if (lastSlamTime + slamCooldown < Time.realtimeSinceStartup)
            {
                SimpleLib.Main.ActorSay(LuBu, tauntList.GetRandom(), 3);
                lastTargetedList = ActorsWithinRangeOfTile(lastTile, LuBu, 10f);
                SlamActors(lastTargetedList, LuBu);
                MapBox.instance.spawnFlash(lastTile, 3);
                lastSlamTime = Time.realtimeSinceStartup;
            }
            // no cooldown yet.. it needs a radial unit count + cooldown

        }

        private void StartFear()
        {
            if (lastFearTime + fearCooldown < Time.realtimeSinceStartup)
            {
                lastTargetedList = ActorsWithinRangeOfTile(lastTile, LuBu, 5f);
                if (lastTargetedList != null && lastTargetedList.Count >= 1)
                {
                    SimpleLib.Main.ActorSay(LuBu, tauntList.GetRandom(), 3);
                    foreach (Actor unit in lastTargetedList)
                    {
                        SimpleLib.Main.ActorSay(unit, panicStrings.GetRandom(), 2f);
                    }
                    fearTargets = true; // starts behaviour in UpdateFear
                    lastFearTime = Time.realtimeSinceStartup;
                }
            }
        }


     
        public float lastSlamTime = 0f;
        public float lastSlashTime = 0f;


        public bool fearTargets = false;
        public float fearLength = 3f;
        public float lastFearTime = 0f;
        public float lastFearTickCooldown = 0.2f;
        public float lastFearTick = 0f;

        public List<Actor> lastTargetedList;

        public List<string> panicStrings = new List<string>
        { 
            "Ahhh!",
            "Run!",
            "What do I do?",
            "Someone save me!",
            "Help!"
        };

        public void FearActors(List<Actor> targets = null, Actor exclusionTarget = null)
        {
            foreach (Actor unit in targets)
            {
                if (unit != LuBu)
                {
                    if (unit.currentTile != null)
                    {
                        BasicMoveAndWait(unit, unit.currentTile.zone.neighbours.GetRandom().tiles.GetRandom());
                    }
                    else
                    {
                        Debug.Log("current tile was null");
                    }
                    // random neighbor random neighbor random neighbor random neighbor random neighbor random neighbor 
                }
            }
        }

        public static void DamageActors(List<Actor> targets, Actor exclusionTarget = null) // for slash and normalArrow
        {
            foreach (Actor unit in targets)
            {
                if (unit != LuBu)
                {
                    unit.CallMethod("getHit", new object[] { 10f, true, AttackType.Other, controlledActor, false });
                    // need a small knockback
                }
            }
        }


        public static void SlamActors(List<Actor> targets, Actor exclusionTarget = null) // for slam
        {
            foreach (Actor unit in targets)
            {
                if (unit != LuBu)
                {
                        BaseEffectController hitCritical = MapBox.instance.stackEffects.CallMethod("get", new object[] { "hitCritical" }) as BaseEffectController;
                        if (hitCritical != null)
                        {
                            hitCritical.spawnAtRandomScale(unit.currentTile, 0.1f, 0.3f);
                        }
                        //hitCritical.CallMethod("spawnAt", new object[] { unit.currentPosition, 0.1f });
                    
                        MapBox.instance.applyForce(unit.currentTile, 1, 1f /*range*/, true, false, 10, null, LuBu);

                    //
                }
            }
            /*
            if (exclusionTarget != null)
            {
                MapBox.instance.applyForce(exclusionTarget.currentTile, 1, 5f, true, false, 10, null, controlledActor);
            }
            */

        }

        // checks a tile and a range around it for a list of targets, with the option of excluding someone
        // if the excluded actor is the controlled one (Lu Bu), it excludes his squad as well
        public static List<Actor> ActorsWithinRangeOfTile(WorldTile pTarget, Actor exclusionTarget, float range)
        {
            List<Actor> returnActors = new List<Actor>();
            foreach (Actor unit in MapBox.instance.units)
            {
                float actorDistanceFromTile;
                if (unit != exclusionTarget)
                {
                    if (LuBu != null && exclusionTarget == LuBu)
                    {
                        if (unit != dedicatedMount)
                        {
                            actorDistanceFromTile = Toolbox.Dist(unit.currentPosition.x, unit.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                            if (actorDistanceFromTile < range)
                            {
                                returnActors.Add(unit);
                            }
                        }                        
                    }
                    else
                    {
                        actorDistanceFromTile = Toolbox.Dist(unit.currentPosition.x, unit.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                        if (actorDistanceFromTile < range)
                        {
                            returnActors.Add(unit);
                        }
                    }
                  
                }
            }
            return returnActors;
        }

        // quick reference
        public WorldTile MouseTile => MapBox.instance.getMouseTilePos();

        public bool showActorInteract;
        public Rect actorInteractWindowRect;

        public Actor lastInteractionActor = null;


        // almost the same as ActorsWithinRangeOfTile, except it only targets one actor 
        public static Actor ClosestActorToTile(WorldTile pTarget, Actor exclusionTarget, float range)
        {
            Actor returnActor = null;
            foreach (Actor unit in MapBox.instance.units)
            {
                float actorDistanceFromTile;
                if (unit != exclusionTarget)
                {
                    if (LuBu != null && exclusionTarget == LuBu)
                    {
                        if (unit != dedicatedMount)
                        {
                            actorDistanceFromTile = Toolbox.Dist(unit.currentPosition.x, unit.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                            if (actorDistanceFromTile < range)
                            {
                                returnActor = unit;
                            }
                        }
                    }
                    else
                    {
                        actorDistanceFromTile = Toolbox.Dist(unit.currentPosition.x, unit.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                        if (actorDistanceFromTile < range)
                        {
                            returnActor = unit;
                        }
                    }

                }
            }
            return returnActor;
        }
        public bool displayMessageWindow;
        public Rect displayMessageWindowRect;

        public bool displayMessage = false;
        public float displayMessageLength = 3f;
        public float displayMessageCooldown = 3f;
        public float lastMessageTime = 0f;
        public string currentMessage = "";
        public string lastMessage = "";

        // need this to track onto specific unit on screen, not just center
        public void MessageDisplayWindow(int windowID)
        {
            SetWindowInUse(windowID);
            if (displayMessage)
            {
                if (currentMessage != null && currentMessage != "")
                {
                    GUILayout.Label(currentMessage);
                }
            }
            if (lastMessageTime + displayMessageLength < Time.realtimeSinceStartup)
            {
                displayMessage = false;
            }
            GUI.DragWindow();
        }

        public void ActorInteractWindow(int windowID)
        {
            SetWindowInUse(windowID);
            ActorStatus currentActorData = Reflection.GetField(lastInteractionActor.GetType(), lastInteractionActor, "data") as ActorStatus;
            if (controlledActor == null && lastInteractionActor != null)
            {
                if (GUILayout.Button("Take control of " + currentActorData.firstName))
                {
                    controlledActor = lastInteractionActor;
                }
            }
            if (controlledActor != null)
            {
                if (controlledActor.haveTrait("lubu_redhare_armoured"))
                {
                    if (GUILayout.Button("Dismount"))
                    {
                        UnmountLubu(controlledActor);
                    }
                }
                else if (controlledActor.haveTrait("lubu_unmounted"))
                {
                    if (dedicatedMount != null)
                    {
                        ActorStatus dedicatedMountData = Reflection.GetField(dedicatedMount.GetType(), dedicatedMount, "data") as ActorStatus;
                        if (dedicatedMountData.alive && dedicatedMount.haveTrait("lubu_rdhare") && GUILayout.Button("Mount"))
                        {
                            MountLubuArmoured(controlledActor);
                        }
                    }
                }
                if (controlledActor == lastInteractionActor)
                {
                    if (GUILayout.Button("Stop control")) // remove this later
                    {
                        controlledActor = null;
                    }
                }
                if (controlledActor != null)
                {
                    ActorStatus controlledActorData = Reflection.GetField(controlledActor.GetType(), controlledActor, "data") as ActorStatus;

                    GUILayout.Button(controlledActorData.firstName);
                }
            }
          
            // hiring/firing squad stuff
            if (GUILayout.Button("Cycle power (Q)"))
            {
                CyclePower();
            }
            GUILayout.Button("Current power: " + powerInUse);
            GUI.DragWindow();
        }
        public static int windowInUse = 0;

        public List<string> powerList = new List<string>()
        {
            "slam",
            "fear",
            "slash",
            "arrowNormal",
            "arrowVolley",
            "arrowExplode"
        };

        public void CyclePower()
        {
            switch (powerInUse)
            {
                case "slam":
                    powerInUse = "fear";
                    break;
                case "fear":
                    powerInUse = "slash";
                    break;
                case "slash":
                    powerInUse = "arrowNormal";
                    break;
                case "arrowNormal":
                    powerInUse = "arrowExplode";
                    break;
                case "arrowExplode":
                    powerInUse = "arrowVolley";
                    break;
                case "arrowVolley":
                    powerInUse = "slam";
                    break;
                default:
                    break;
            }
        }

        public static void SetWindowInUse(int windowID)
        {
            Event current = Event.current;
            bool inUse = current.type == EventType.MouseDown || current.type == EventType.MouseUp || current.type == EventType.MouseDrag || current.type == EventType.MouseMove;
            if (inUse)
            {
                windowInUse = windowID;
            }
        }

        public WorldTile randomCityTile()
        {
            WorldTile returnTile = null;
            for (int i = 0; i < MapBox.instance.tilesList.Count; i++)
            {
                returnTile = MapBox.instance.tilesList.GetRandom();
                if (returnTile.zone.city != null)
                {
                    return returnTile;
                }
            }
            return returnTile;
        }


        public static void addTraitToLocalizedLibrary(string id, string description)
        {
            string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
            Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
            if (language == "en")
            {
                localizedText.Add("trait_" + id, id); 
                localizedText.Add("trait_" + id + "_info", description);
            }
        }

        public static Actor controlledActor = null; // Lu Bu himself
        public static Actor dedicatedMount = null; // Lu Bu's horse

        public static string directionMoving = null;

        public void followActor(Actor target, float speed = 5f)
        {
            Vector3 posV = target.currentPosition;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, posV, speed * Time.deltaTime);
        }
        private void movementStuff()
        {
            // RPG movement
            lastTile = MapBox.instance.GetTileSimple((int)controlledActor.currentPosition.x, (int)controlledActor.currentPosition.y);
            if (Input.GetKey(KeyCode.W))
            {
                directionMoving = "up";
                if (Input.GetKey(KeyCode.A))
                {
                    directionMoving = "upLeft";
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    directionMoving = "upRight";
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                directionMoving = "down";
                if (Input.GetKey(KeyCode.A))
                {
                    directionMoving = "downLeft";
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    directionMoving = "downRight";
                }
            }
            else if (Input.GetKey(KeyCode.A))
            {
                directionMoving = "left";
                if (Input.GetKey(KeyCode.W))
                {
                    directionMoving = "upLeft";
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    directionMoving = "downLeft";
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                directionMoving = "right";
                if (Input.GetKey(KeyCode.W))
                {
                    directionMoving = "upRight";
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    directionMoving = "downRight";
                }
            }
            else
            {
                directionMoving = null;
            }
            // when user isnt moving (or interacting?), make controlled be still
            // could add some kind of command queue
            if (directionMoving == null)
            {
                controlledActor.stopMovement();
                controlledActor.cancelAllBeh();
            }
            if (directionMoving != null)
            {
                WorldTile movementTile;
                switch (directionMoving)
                {
                    case "up":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x, lastTile.pos.y + 1);
                        doMovement(movementTile);
                        break;
                    case "down":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x, lastTile.pos.y - 1);
                        doMovement(movementTile);
                        break;
                    case "left":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x - 1, lastTile.pos.y);
                        doMovement(movementTile);
                        break;
                    case "right":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x + 1, lastTile.pos.y);
                        doMovement(movementTile);
                        break;
                    case "upLeft":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x - 1, lastTile.pos.y + 1);
                        doMovement(movementTile);
                        break;
                    case "upRight":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x + 1, lastTile.pos.y + 1);
                        doMovement(movementTile);
                        break;
                    case "downLeft":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x - 1, lastTile.pos.y - 1);
                        doMovement(movementTile);
                        break;
                    case "downRight":
                        movementTile = MapBox.instance.GetTile(lastTile.pos.x + 1, lastTile.pos.y - 1);
                        doMovement(movementTile);
                        break;
                    default:
                        break;
                }
            }

        }

        public static WorldTile lastTile = null;
        public void doMovement(WorldTile targetTile)
        {
            lastTile = targetTile;
            BasicMoveAndWait(controlledActor, targetTile);
        }

        public Dictionary<Actor, float> actorsApproached = new Dictionary<Actor, float>();

        public static void BasicMoveAndWait(Actor targetActor, WorldTile targetTile)
        {
            // simple way of making an actor move the way i want
            targetActor.stopMovement();
            targetActor.cancelAllBeh();
            if (targetTile != null)
            {
                if (controlledActor == null && (targetActor == LuBu))
                {
                    targetActor.goTo(targetTile, true, true);
                }
                else
                {
                    targetActor.moveTo(targetTile);
                }
                flashEffects.flashPixel(targetTile, 10, ColorType.White);
                AiSystemActor actorAI = Reflection.GetField(targetActor.GetType(), targetActor, "ai") as AiSystemActor;
                actorAI.setTask("wait10", false, true);
            }
           
        }

        public static PixelFlashEffects flashEffects => Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "flashEffects") as PixelFlashEffects;

        
        public List<string> tauntList = new List<string>()
        {
            "You insects dare to oppose me?",
            "Ragh! Get out of my way!",
            "You dare face the mighty Lu Bu?",
            "Worthless scum!",
            "You dare challenge me!?",
            "I'll wipe these insects out in one blow!",
            "Scum! Do you wish to die?",
            "Stay out of my way if you want to live!",
            "I fear no man!",
            "I will show you what true might is!",
            "Get out of my sight you vermin!",
            "No one can surpass me! Not now, not ever!",
            "There's no one out there who can beat me!"
        };

        public float lubuDuration = 300f;
        public float lubuSpawnTime = 0f;

        public string exitQuote = "This battle bores me! I shall take my leave now!";
        public string entranceQuote = "You dare face the mighty Lu Bu?";

        // click-through prevention for menus
        public static void isActionHappening_Postfix(ref bool __result)
        {
            if (windowInUse != -1)
            {
                __result = true; // "menu in use" is the action happening
            }
        }

        // cancel all control input if a window is in use
        public static bool updateControls_Prefix()
        {
            if (windowInUse != -1)
            {
                return false; 
            }
            return true;
        }

        // cancel empty click usage when windows in use, unused right now
        public static bool checkEmptyClick_Prefix()
        {
            if (windowInUse != -1)
            {
                return false; 
            }
            return true;
        }

        // add traits after asset library is set up
        public static void init_Postfix()
        {
            AddTraits();
        }

        // ActorBase patch: cancel animations, forcing the 1 replaced sprite to always remain
        public static bool updateAnimation_Prefix(float pElapsed, bool pForce, ActorBase __instance)
        {
            if (__instance.haveTrait("lubu_redhare_armoured") || __instance.haveTrait("lubu_unmounted") || __instance.haveTrait("lubu_rdhare")) // maybe add a dictionary of extra skins?
            {
                return false;
            }
            return true;
        }
        public static void loadLocalizedText_Postfix(string pLocaleID)
        {
            addTraitToLocalizedLibrary("lubu", "LuBu Description");
        }

        public void MountLubuArmoured(Actor target)
        {
            if (dedicatedMount != null)
            {
                ActorStatus dataHorse = Reflection.GetField(dedicatedMount.GetType(), dedicatedMount, "data") as ActorStatus;
                if (dataHorse != null && dataHorse.alive)
                {
                    SpriteRenderer spriteRendererBody = Reflection.GetField(dedicatedMount.GetType(), dedicatedMount, "spriteRenderer") as SpriteRenderer;
                    spriteRendererBody.enabled = false;
                    dedicatedMount.killHimself();
                    dedicatedMount = null;
                }
            }
            //Actor closest = ClosestActorToTile(lastTile, controlledActor, 5f);
            //closest.killHimself(true, AttackType.None, false, false);
            if (target.haveTrait("lubu_unmounted"))
            {
                target.removeTrait("lubu_unmounted");
                ActorStatus data = Reflection.GetField(target.GetType(), target, "data") as ActorStatus;
                data.health += 150;
                if (TextureLoader.TextureLoader_Main.traitReplacedActors.Contains(target))
                {
                    TextureLoader.TextureLoader_Main.traitReplacedActors.Remove(target);
                }
                target.addTrait("lubu_redhare_armoured");
            }
        }

        public void UnmountLubu(Actor target)
        {
            if (target.haveTrait("lubu_redhare_armoured"))
            {
                target.removeTrait("lubu_redhare_armoured");
                ActorStatus data = Reflection.GetField(target.GetType(), target, "data") as ActorStatus;
                data.health += 150;
                if (TextureLoader.TextureLoader_Main.traitReplacedActors.Contains(target))
                {
                    TextureLoader.TextureLoader_Main.traitReplacedActors.Remove(target);
                }
                target.addTrait("lubu_unmounted");
            }
            Actor actor = MapBox.instance.createNewUnit(target.stats.id, lastTile, null, 0f, null);
            actor.addTrait("lubu_rdhare");
            dedicatedMount = actor;
        }

        public static void detectCustomActorTexture_Postfix(Actor target)
        {
            // need more logic other than just the replaced actor list.. sometimes the sprite resets (like transforming) and theyre only added to the list once, so never replaced again
            if (TextureLoader.TextureLoader_Main.traitReplacedActors != null && TextureLoader.TextureLoader_Main.traitReplacedActors.Contains(target) == false)
            {
                string name = target.stats.id;
                List<string> skinVariationsAvailable = new List<string>();
                if (target.haveTrait("lubu_redhare_armoured"))  // or other unit-reskin-traits
                {
                    skinVariationsAvailable = TextureLoader.TextureLoader_Main.variationsOfCustomTexture("lubu_redhare_armoured"); // can rename the variations and pick specifics out
                }
                else if (target.haveTrait("lubu_unmounted"))  // or other unit-reskin-traits
                {
                    skinVariationsAvailable = TextureLoader.TextureLoader_Main.variationsOfCustomTexture("lubu_unmounted"); // can rename the variations and pick specifics out
                }
                else if (target.haveTrait("lubu_rdhare"))  // or other unit-reskin-traits
                {
                    skinVariationsAvailable = TextureLoader.TextureLoader_Main.variationsOfCustomTexture("lubu_rdhare"); // can rename the variations and pick specifics out
                }
                else
                {
                    if (TextureLoader.TextureLoader_Main.traitReplacedActors.Contains(target))
                    {
                        TextureLoader.TextureLoader_Main.traitReplacedActors.Remove(target);
                    }
                }
                SpriteRenderer spriteRendererBody = Reflection.GetField(target.GetType(), target, "spriteRenderer") as SpriteRenderer;
                //Debug.Log("Attempted getting skins for unit, count: " + skinVariationsAvailable.Count);
                if (skinVariationsAvailable != null && skinVariationsAvailable.Count >= 1)
                {
                    name = skinVariationsAvailable.GetRandom();
                    TextureLoader.TextureLoader_Main.customTextures.TryGetValue(name, out Sprite replacement);
                    if (replacement == null)
                    {
                    }
                    if (replacement != null)
                    {
                        spriteRendererBody.sprite = replacement;
                        TextureLoader.TextureLoader_Main.traitReplacedActors.Add(target);
                        //target.addTrait(""); unnesseary since we're checking for the trait first
                        // Debug.Log("Replaced actor " + target.stats.id + " sprite: " + name);
                    }
                }
            }
        }
        public static bool stopMovement_Prefix(ActorBase __instance) // stop controlled actor canceling movement from AI behaviour
        {
            if (controlledActor != null && __instance == controlledActor)
            {
                if (directionMoving != null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static class Reflection
    {
        // found on https://stackoverflow.com/questions/135443/how-do-i-use-reflection-to-invoke-a-private-method
        public static object CallMethod(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(o, args);
            }
            return null;
        }
        // found on: https://stackoverflow.com/questions/3303126/how-to-get-the-value-of-private-field-in-c/3303182
        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = originalObject.GetType().GetField(fieldName, bindFlags);
            field.SetValue(originalObject, newValue);
        }
    }

    public static class GetObjectProperties
    {

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static List<string> GetProps(GameObject obj)
        {
            List<string> PropertiesList = new List<string>();
            GameObject Object = obj;
            FieldInfo[] fields;
            Component[] Components = obj.GetComponents(typeof(Component));
            foreach (var comp in Components)
            {
                fields = comp.GetType().GetFields();
                foreach (var fi in fields)
                {
                    PropertiesList.Add(fi.ToString());
                }
            }
            return PropertiesList;
        }
    }
}
