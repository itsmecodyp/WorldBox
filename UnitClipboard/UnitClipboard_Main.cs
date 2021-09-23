using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using static UnityEngine.UI.Image;
using UnityEngine;
using System.Reflection;

namespace UnitClipboard
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class UnitClipboard_Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.unit.clipboard";
        public const string pluginName = "Unit Clipboard";
        public const string pluginVersion = "0.0.0.1";

        public void Awake()
        {
            HarmonyPatchSetup();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && MapBox.instance.getMouseTilePos() != null)
            {
                lastInteractionActor = ClosestActorToTile(MapBox.instance.getMouseTilePos(), 3f);
            }
            if (Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.V)))
            {
                if (MapBox.instance.getMouseTilePos() != null && selectedUnitToPaste != null)
                {
                    Actor pastedUnit = MapBox.instance.createNewUnit(selectedUnitToPaste.statsID, MapBox.instance.getMouseTilePos(), null, 0f, null);
                    ActorStatus data = Reflection.GetField(pastedUnit.GetType(), pastedUnit, "data") as ActorStatus;
                    data.firstName = selectedUnitToPaste.dataFirstName;
                    ActorTrait statCatchup = new ActorTrait();
                    statCatchup.id = "stats" + selectedUnitToPaste.dataFirstName; // might need to change to be unique
                    statCatchup.baseStats = selectedUnitToPaste.statDifference;
                    if (AssetManager.traits.list.Contains(statCatchup))
                    {
                        Debug.Log("e");
                        pastedUnit.addTrait(statCatchup.id);
                    }
                    else
                    {
                        addTraitToLocalizedLibrary(statCatchup.id, "Unit was copy pasted");
                        AssetManager.traits.add(statCatchup);
                        pastedUnit.addTrait(statCatchup.id);
                    }
                    foreach (string trait in selectedUnitToPaste.traits)
                    {
                        pastedUnit.addTrait(trait);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                showHideMainWindow = !showHideMainWindow;
            }
        }

        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public void OnGUI()
        {
            if (showHideMainWindow)
            {
                mainWindowRect = GUILayout.Window(500401, mainWindowRect, new GUI.WindowFunction(UnitClipboardWindow), "Clipboard", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
        }

        public static Actor ClosestActorToTile(WorldTile pTarget, float range)
        {
            Actor returnActor = null;
            foreach (Actor actorToCheck in MapBox.instance.units)
            {
                float actorDistanceFromTile = Toolbox.Dist(actorToCheck.currentPosition.x, actorToCheck.currentPosition.y, (float)pTarget.pos.x, (float)pTarget.pos.y);
                if (actorDistanceFromTile < range)
                {
                    range = actorDistanceFromTile;
                    returnActor = actorToCheck;
                }
            }
            return returnActor;
        }


        public Actor lastInteractionActor;
        public Dictionary<string, UnitData> unitClipboardDict = new Dictionary<string, UnitData>(); // id, data

        public void UnitClipboardWindow(int windowID)
        {
            if (unitClipboardDict.Count >= 1)
            {
                for (int i = 0; i < unitClipboardDict.Count(); i++)
                {
                    if (GUILayout.Button(unitClipboardDict[i.ToString()].dataFirstName))
                    {
                        selectedUnitToPaste = unitClipboardDict[i.ToString()];
                    }
                }
            }
            
            if (lastInteractionActor != null && GUILayout.Button("Add selected unit"))
            {
                ActorStatus data = Reflection.GetField(lastInteractionActor.GetType(), lastInteractionActor, "data") as ActorStatus;
                BaseStats curStats = Reflection.GetField(lastInteractionActor.GetType(), lastInteractionActor, "curStats") as BaseStats;
                
                UnitData newSavedUnit = new UnitData();
                foreach (string trait in data.traits)
                {
                    newSavedUnit.traits.Add(trait);
                }
                newSavedUnit.dataFirstName = data.firstName;
                newSavedUnit.statsID = data.statsID;
                BaseStats originalStats = AssetManager.unitStats.get(data.statsID).baseStats;
                BaseStats currentStats = curStats;
                BaseStats statsDifference = new BaseStats();
                #region statDifference
                statsDifference.accuracy = currentStats.accuracy - originalStats.accuracy;
                statsDifference.areaOfEffect = currentStats.accuracy - originalStats.accuracy;
                statsDifference.armor = currentStats.armor - originalStats.armor;
                statsDifference.army = currentStats.army - originalStats.army;
                statsDifference.attackSpeed = currentStats.attackSpeed - originalStats.attackSpeed;
                statsDifference.bonus_towers = currentStats.bonus_towers - originalStats.bonus_towers;
                statsDifference.cities = currentStats.cities - originalStats.cities;
                statsDifference.crit = currentStats.crit - originalStats.crit;
                statsDifference.damage = currentStats.damage - originalStats.damage;
                statsDifference.damageCritMod = currentStats.damageCritMod - originalStats.damageCritMod;
                statsDifference.diplomacy = currentStats.diplomacy - originalStats.diplomacy;
                statsDifference.health = currentStats.health - originalStats.health;
                statsDifference.intelligence = currentStats.intelligence - originalStats.intelligence;
                statsDifference.knockback = currentStats.knockback - originalStats.knockback;
                statsDifference.knockbackReduction = currentStats.knockbackReduction - originalStats.knockbackReduction;
                statsDifference.loyalty_mood = currentStats.loyalty_mood - originalStats.loyalty_mood;
                statsDifference.loyalty_traits = currentStats.loyalty_traits - originalStats.loyalty_traits;
                statsDifference.mod_armor = currentStats.mod_armor - originalStats.mod_armor;
                statsDifference.mod_attackSpeed = currentStats.mod_attackSpeed - originalStats.mod_attackSpeed;
                statsDifference.mod_crit = currentStats.mod_crit - originalStats.mod_crit;
                statsDifference.mod_damage = currentStats.mod_damage - originalStats.mod_damage;
                statsDifference.mod_diplomacy = currentStats.mod_diplomacy - originalStats.mod_diplomacy;
                statsDifference.mod_health = currentStats.mod_health - originalStats.mod_health;
                statsDifference.mod_speed = currentStats.mod_speed - originalStats.mod_speed;
                statsDifference.mod_supply_timer = currentStats.mod_supply_timer - originalStats.mod_supply_timer;
                statsDifference.opinion = currentStats.opinion - originalStats.opinion;
                statsDifference.personality_administration = currentStats.personality_administration - originalStats.personality_administration;
                statsDifference.personality_aggression = currentStats.personality_aggression - originalStats.personality_aggression;
                statsDifference.personality_diplomatic = currentStats.personality_diplomatic - originalStats.personality_diplomatic;
                statsDifference.personality_rationality = currentStats.personality_rationality - originalStats.personality_rationality;
                statsDifference.projectiles = currentStats.projectiles - originalStats.projectiles;
                statsDifference.range = currentStats.range - originalStats.range;
                statsDifference.scale = currentStats.scale - originalStats.scale;
                statsDifference.speed = currentStats.speed - originalStats.speed;
                statsDifference.stewardship = currentStats.stewardship - originalStats.stewardship;
                statsDifference.s_crit_chance = currentStats.s_crit_chance - originalStats.s_crit_chance;
                statsDifference.targets = currentStats.targets - originalStats.targets;
                statsDifference.warfare = currentStats.warfare - originalStats.warfare;
                statsDifference.zones = currentStats.zones - originalStats.zones;
                #endregion
                newSavedUnit.statDifference = statsDifference;

                unitClipboardDict.Add(unitClipboardDictNum.ToString(), newSavedUnit);
                unitClipboardDictNum++;
            }
            GUI.DragWindow();
        }
        int unitClipboardDictNum = 0;

        public UnitData selectedUnitToPaste;

        public class UnitData
        {
            public string dataFirstName = "";
            public string statsID = "";
            public BaseStats statDifference;
            public List<string> traits = new List<string>();
        }

        public void HarmonyPatchSetup()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(pluginName);
            original = AccessTools.Method(typeof(ActorEquipmentSlot), "setItem");
            patch = AccessTools.Method(typeof(UnitClipboard_Main), "setItem_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);

        }
        // save equipment data in unitData
        public static bool setItem_Prefix(ItemData pData, ActorEquipmentSlot __instance)
        {
            if (manualGeneration)
            {
                pData.prefix = itemGenerationPrefix;
                pData.suffix = itemGenerationSuffix;
                __instance.data = pData;
                manualGeneration = false;
                return false;
            }
            return true;
        }
        public static string itemGenerationPrefix;
        public static string itemGenerationSuffix;
        public static bool manualGeneration;
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
        public static object GetField(System.Type type, object instance, string fieldName)
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

}
