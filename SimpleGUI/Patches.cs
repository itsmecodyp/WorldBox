using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SimplerGUI.Menus;
using SimplerGUI.Submods;
using SimplerGUI.Submods.SimpleMessages;
using UnityEngine;
using UnityEngine.UI;

namespace SimplerGUI {
    internal class Patches {
        public static void ApplyHarmonyPatches()
        {
            Harmony harmony = new Harmony(GuiMain.pluginName);
            MethodInfo original;
            MethodInfo patch;

            harmony.PatchAll();

            original = AccessTools.Method(typeof(CityPlaceFinder), nameof(CityPlaceFinder.check));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.check_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(CityZoneGrowth), nameof(CityZoneGrowth.checkGrowBorder));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.checkGrowBorder_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(TooltipLibrary), nameof(TooltipLibrary.showActor));
            patch = AccessTools.Method(typeof(ActorInteraction), nameof(ActorInteraction.showActor_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(CrabArm), nameof(CrabArm.damageWorld));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.damageWorld_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(Giantzilla), nameof(Giantzilla.followCamera));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.followCamera_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(Actor), nameof(Actor.checkEnemyTargets));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.checkEnemyTargets_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(ActorBase), nameof(ActorBase.checkAnimationContainer));
            patch = AccessTools.Method(typeof(ActorInteraction), nameof(ActorInteraction.checkAnimationContainer_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(InitLibraries), nameof(InitLibraries.initLibs));
            patch = AccessTools.Method(typeof(GuiMain), nameof(GuiMain.postAssetInitStuff));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorBase), nameof(ActorBase.checkSpriteHead));
            patch = AccessTools.Method(typeof(ActorInteraction), nameof(ActorInteraction.checkSpriteHead_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            /*
            original = AccessTools.Method(typeof(ActorBase), "nextJobActor");
            patch = AccessTools.Method(typeof(SimpleCultists), "nextJobActor_postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(PowerLibrary), nameof(PowerLibrary.drawDivineLight));
            patch = AccessTools.Method(typeof(GuiTraits), nameof(GuiTraits.drawDivineLight_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), nameof(MapBox.checkEmptyClick));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.checkEmptyClick_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorBase), nameof(ActorBase.generatePersonality));
            patch = AccessTools.Method(typeof(GuiPatreon), nameof(GuiPatreon.generatePersonality_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(LoadingScreen), nameof(LoadingScreen.OnEnable));
            patch = AccessTools.Method(typeof(GuiPatreon), nameof(GuiPatreon.OnEnable_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(LocalizedTextManager), nameof(LocalizedTextManager.loadLocalizedText));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.loadLocalizedText_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            /* disable cloud patch? broken in 0.15+
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(CloudController), "spawn");
            patch = AccessTools.Method(typeof(GUIWorld), "spawn_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(QualityChanger), nameof(QualityChanger.update));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.update_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(TraitButton), nameof(TraitButton.load));
            patch = AccessTools.Method(typeof(GuiTraits), nameof(GuiTraits.load_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);


            //example patch extending actorsay functionality
            original = AccessTools.Method(typeof(Submods.SimpleMessages.Messages), nameof(Messages.ActorSay));
            patch = AccessTools.Method(typeof(Submods.SimpleCultists), nameof(SimpleCultists.ActorSay_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(BuildingActions), nameof(BuildingActions.spawnResource));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.spawnResource_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), nameof(MapBox.updateControls));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.updateControls_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), nameof(MapBox.isActionHappening));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.isActionHappening_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Building), nameof(Building.startDestroyBuilding));
            patch = AccessTools.Method(typeof(GUIConstruction), nameof(GUIConstruction.startDestroyBuilding_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MoveCamera), nameof(MoveCamera.updateMouseCameraDrag));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.updateMouseCameraDrag_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            /*
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(MapBox), "spawnAndLoadUnit");
            patch = AccessTools.Method(typeof(GuiMain), "spawnAndLoadUnit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(SaveWorldButton), nameof(SaveWorldButton.saveWorld));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.saveWorld_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Harmony), nameof(Harmony.PatchAll));
            patch = AccessTools.Method(typeof(GUIConstruction), nameof(GUIConstruction.addBuilding_Prefix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Building), nameof(Building.startRemove));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.startRemove_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            //original = AccessTools.Method(typeof(ActorEquipmentSlot), "setItem");
            //patch = AccessTools.Method(typeof(GuiItemGeneration), "setItem_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));
            //Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), nameof(Actor.addExperience));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.addExperience_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorBase), nameof(ActorBase.addTrait));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.addTrait_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorAnimationLoader), nameof(ActorAnimationLoader.getItem));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.getItem_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(LocalizedTextManager), nameof(LocalizedTextManager.getText));
            patch = AccessTools.Method(typeof(GuiMain), nameof(SimplerGUI.Patches.getText_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), nameof(ActorManager.createNewUnit));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.createNewUnit_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), nameof(ActorManager.spawnNewUnit));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.spawnNewUnit_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), nameof(ActorManager.destroyObject));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.destroyObject_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapAction), nameof(MapAction.terraformTile));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.terraformTile_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapAction), nameof(MapAction.applyTileDamage));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.applyTileDamage_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(WorldTile), nameof(WorldTile.setBurned));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.setBurned_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);


            original = AccessTools.Method(typeof(Heat), nameof(Heat.addTile));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.addTile_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(WorldTile), nameof(WorldTile.setFireData));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.setFireData_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            /*
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Building), "setSpriteRuin");
            patch = AccessTools.Method(typeof(GuiOther), "setSpriteRuin_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

            original = AccessTools.Method(typeof(ActorBase), nameof(ActorBase.updateDeadBlackAnimation));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.updateDeadBlackAnimation_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(City), nameof(City.addZone));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.addZone_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(City), nameof(City.removeZone));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.removeZone_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(CityBehCheckFarms), nameof(CityBehCheckFarms.checkZone));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.checkZone_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(City), nameof(City.getLimitOfBuildingsType));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.getLimitOfBuildingsType_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(BaseSimObject), nameof(BaseSimObject.updateStats));
            patch = AccessTools.Method(typeof(GuiStatSetting), nameof(GuiStatSetting.updateStats_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Docks), nameof(Docks.docksAtBoatLimit));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.docksAtBoatLimit_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(PowerButtonSelector), nameof(PowerButtonSelector.isBottomBarShowing));
            patch = AccessTools.Method(typeof(GuiOther), nameof(GuiOther.isBottomBarShowing_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), nameof(ActorManager.finalizeActor));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.finalizeActor_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), nameof(Actor.killHimself));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.killHimself_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), nameof(Actor.killHimself));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.killHimself_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(Actor), nameof(Actor.newKillAction));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.newKillAction_Postfix));
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(BattleKeeperManager), nameof(BattleKeeperManager.unitKilled));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.unitKilled_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(ActorManager), nameof(ActorManager.createNewUnit));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.createNewUnit_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(PowerLibrary), nameof(PowerLibrary.spawnUnit));
            patch = AccessTools.Method(typeof(GUIWorld), nameof(GUIWorld.spawnUnit_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), nameof(MapBox.checkClickTouchInspect));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.checkClickTouchInspect_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);

            original = AccessTools.Method(typeof(MapBox), nameof(MapBox.checkEmptyClick));
            patch = AccessTools.Method(typeof(ActorControlMain), nameof(ActorControlMain.checkEmptyClick_Prefix));
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(GuiMain.pluginName + ": Harmony patch finished: " + patch.Name);
            

            /* tired of messing with this
            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(Kingdom), "createColors");
            patch = AccessTools.Method(typeof(GuiOther), "createColors_Postfix");
            harmony.Patch(original, new HarmonyMethod(patch));
            UnityEngine.Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

        }
        
        public static void loadLocalizedText_Postfix(string pLocaleID)
        {
            GuiMain.timesLocalizedRan++;
            Debug.Log("localizedText postfix ran " + GuiMain.timesLocalizedRan.ToString() + " times");
            string language = LocalizedTextManager.instance.language;
            Dictionary<string, string> localizedText = LocalizedTextManager.instance.localizedText;
            if (language == "en")
            {
                // text tips
                if (localizedText != null)
                {
                    localizedText.Add("Styderr makes awesome maps, check them out!", "Styderr makes awesome maps, check them out!");
                    localizedText.Add("Nothing to see here guys - KJYhere", "Nothing to see here guys - KJYhere");
                    localizedText.Add("Call up Rajit at 1(800)-911-SCAM   - Ramlord", "Call up Rajit at 1(800)-911-SCAM   - Ramlord");
                    localizedText.Add("10/10 would recommend - boopahead08", "10/10 would recommend - boopahead08");
                    localizedText.Add("Kosovo je srbija!", "Kosovo je srbija!");
                    localizedText.Add("This mod is sponsored by Raid: Shadow Legends - Slime", "This mod is sponsored by Raid: Shadow Legends - Slime");
                    localizedText.Add("The four nations lived in harmony, until the orc nation attacked", "The four nations lived in harmony, until the orc nation attacked");
                    localizedText.Add("Now with raytracing!", "Now with raytracing!");
                    localizedText.Add("Modificating and customizating the game...", "Modificating and customizating the game...");
                    localizedText.Add("Tiempo con Juan Diego makes amazing worldbox videos! - Juanchiz", "Tiempo con Juan Diego makes amazing worldbox videos! - Juanchiz");
                    localizedText.Add("null", "null");
                }
            }
            else if (language == "es") // Just an example
            {
                Debug.Log("Using language: Spanish");
            }
            else
            {
                Debug.Log("English/Spanish not in use");
            }
            //localizedText.Add("en", "Lays Eggs");
        }

        public static bool getItem_Prefix(string pID)
        {
            ActorAnimationLoader.dictItems.TryGetValue(pID, out Sprite sprite);
            if(sprite == null) {
                return false; // prevent error from item gen with null textures
            }

            return true;
        }
        public static bool getText_Prefix(string pKey, Text text, ref string __result)
        {
            if (pKey == null)
            {
                __result = "_placeholder?";
                return false; // prevent error from random localized texts
            }
            else
            {
                return true;
            }
        }
        public static bool addTrait_Prefix(string pTrait, bool pRemoveOpposites, ActorBase __instance)
        {
            ActorData data = __instance.data; //Reflection.GetField(__instance.GetType(), __instance, "data") as ActorStatus;
            if(__instance.hasTrait(pTrait) && GuiMain.Other.allowMultipleSameTrait == false) {
                return false;
            }

            if(AssetManager.traits.get(pTrait) == null) {
                return false;
            }
            if(pRemoveOpposites) {
                __instance.removeOppositeTraits(pTrait);
            }
            if(__instance.hasOppositeTrait(pTrait)) {
                return false;
            }
            __instance.data.traits.Add(pTrait);
            __instance.setStatsDirty();
            return false;
        }
        public static void saveWorld_Postfix()
        {
            foreach(Actor actor in MapBox.instance.units) {
                ActorData data = actor.data; //Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;
                if(data.traits.Contains("stats" + data.name)) {
                    actor.removeTrait("stats" + data.name);
                }
            }
        }
        public static void isActionHappening_Postfix(ref bool __result)
        {
            if(GuiMain.windowInUse != -1) {
                __result = true; // "menu in use" is the action happening
            }
        }
        public static bool updateControls_Prefix()
        {
            if(GuiMain.windowInUse != -1) {
                return false; // cancel all control input if a window is in use
            }
            return true;
        }
        public static bool checkEmptyClick_Prefix()
        {
            if(GuiMain.windowInUse != -1) {
                return false; // cancel empty click usage when windows in use
            }
            return true;
        }
        public static bool addExperience_Prefix(int pValue, Actor __instance)
        {
            if(GuiMain.Other.disableLevelCap) {
                //ActorStats stats = __instance.stats; //Reflection.GetField(__instance.GetType(), __instance, "stats") as ActorStats;
                ActorData data = __instance.data; //Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;
                if(__instance.asset.canLevelUp) {
                    if(__instance.data.alive) {
                        int expToLevelup = __instance.getExpToLevelup();
                        data.experience += pValue;
                        bool readyToLevelUp = data.experience >= expToLevelup;
                        if(readyToLevelUp) {
                            data.experience = 0;
                            data.level++;
                            __instance.setStatsDirty();
                            __instance.event_full_heal = true;
                        }
                    }
                }
                return false;
            }

            return true;
        }
    }

}