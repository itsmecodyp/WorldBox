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
using static UnitClipboard.UnitClipboard_Main;

namespace UnitClipboard
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class UnitClipboard_Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.unit.clipboard";
        public const string pluginName = "Unit Clipboard";
        public const string pluginVersion = "0.0.0.2";

        public void Awake()
        {
            HarmonyPatchSetup();
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.C)))
            {
                CopyUnit(ClosestActorToTile(MapBox.instance.getMouseTilePos(), 3f));
            }
            if (Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.V)))
            {
                PasteUnit(MapBox.instance.getMouseTilePos(), selectedUnitToPaste);
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
            {
                showHideMainWindow = !showHideMainWindow;
            }
        }

        private void PasteUnit(WorldTile targetTile, UnitData unitData)
        {
            if (targetTile != null && unitData != null)
            {
                Actor pastedUnit = MapBox.instance.createNewUnit(unitData.statsID, targetTile, null, 0f, null);
                ActorStatus data = Reflection.GetField(pastedUnit.GetType(), pastedUnit, "data") as ActorStatus;
                data.firstName = unitData.dataFirstName;
                if (data.traits != null && data.traits.Count >= 1)
                {
                    pastedUnit.resetTraits();
                }
                if (unitData.equipment != null)
                {
                    pastedUnit.equipment = unitData.equipment;
                }
                
                ActorTrait pasted = new ActorTrait();
                pasted.id = "pasted" + selectedUnitToPaste.dataFirstName; // might need to change to be unique
                if (addedTraits.Contains(pasted.id))
                {
                    pastedUnit.addTrait(pasted.id); // refresh stats
                    pastedUnit.removeTrait(pasted.id); // remove because unnecessary
                }
                else
                {
                    AssetManager.traits.add(pasted);
                    addedTraits.Add(pasted.id);
                    pastedUnit.addTrait(pasted.id); // refresh stats
                    pastedUnit.removeTrait(pasted.id); // remove because unnecessary
                }
                foreach (string trait in unitData.traits)
                {
                    pastedUnit.addTrait(trait);
                }
                pastedUnit.restoreHealth(10^9); //lazy
                Debug.Log("Pasted " + unitData.dataFirstName);
            }
        }
        public List<string> addedTraits = new List<string>(); // lazy
        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public void OnGUI()
        {
            if (showHideMainWindow)
            {
                mainWindowRect = GUILayout.Window(500401, mainWindowRect, new GUI.WindowFunction(UnitClipboardWindow), "Unit Clipboard", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
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


        public Dictionary<string, UnitData> unitClipboardDict = new Dictionary<string, UnitData>(); // id, data

        public void UnitClipboardWindow(int windowID)
        {
            if (unitClipboardDict.Count >= 1)
            {
                for (int i = 0; i < unitClipboardDict.Count(); i++)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(unitClipboardDict[i.ToString()].dataFirstName))
                    {
                        selectedUnitToPaste = unitClipboardDict[i.ToString()];
                    }

                    GUILayout.EndHorizontal();
                }
            }
            GUI.DragWindow();
        }

        private void CopyUnit(Actor targetActor)
        {
            if (targetActor != null)
            {
                ActorStatus data = Reflection.GetField(targetActor.GetType(), targetActor, "data") as ActorStatus;
                BaseStats curStats = Reflection.GetField(targetActor.GetType(), targetActor, "curStats") as BaseStats;

                UnitData newSavedUnit = new UnitData();
                foreach (string trait in data.traits)
                {
                    newSavedUnit.traits.Add(trait);
                }
                newSavedUnit.dataFirstName = data.firstName;
                newSavedUnit.statsID = data.statsID;
                newSavedUnit.equipment = targetActor.equipment;
                unitClipboardDict.Add(unitClipboardDictNum.ToString(), newSavedUnit);
                unitClipboardDictNum++;
                selectedUnitToPaste = newSavedUnit;
                Debug.Log("Copied " + newSavedUnit.dataFirstName);
            }

        }

        int unitClipboardDictNum = 0;

        public UnitData selectedUnitToPaste;

        public class UnitData
        {
            public string dataFirstName = "";
            public string statsID = "";
            public List<string> traits = new List<string>();
            public ActorEquipment equipment;
        }

        public void HarmonyPatchSetup()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;
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
