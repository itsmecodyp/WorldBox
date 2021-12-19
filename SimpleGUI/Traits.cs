using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ai;
using ai.behaviours;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace SimpleGUI
{
    class GuiTraits
    {
        public static bool show_Prefix(GameObject pObject, string pType, string pTitle = null, string pDescription = null)
        {
            if (pTitle != null)
            {
                if (traitNamesAndDescriptions.ContainsKey(pTitle)) // should only be true for stuff like trait_modded_giant
                {
                    pDescription = traitNamesAndDescriptions[pTitle];
                }
            }
            return true;
        }

        public static bool load_Prefix(string pTrait)
        {
            ActorTrait loadedTrait = AssetManager.traits.get(pTrait);
            if (loadedTrait.icon == null)
            {
                loadedTrait.icon = "iconVermin";
                return true;
            }
            return true;
        }

        public static string StringWithFirstUpper(string targetstring)
        {
            return char.ToUpper(targetstring[0]) + targetstring.Substring(1);
        }

        // adding tooltip stuff
        public static void loadLocalizedText_Postfix(string pLocaleID)
        {
            string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
            Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
            if (language == "en")
            {
                // text tips
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
        /*
        actorTrait.action_special_effect += actionTest;
        public static bool turnIntoSkeleton(BaseSimObject pTarget, WorldTile pTile = null)
        {
            Actor a = pTarget.a;
            if (a.gameObject == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(a.stats.skeletonID))
            {
                return false;
            }
            if (a == null)
            {
                return false;
            }
            if (!a.haveTrait("cursed"))
            {
                return false;
            }
            if (!a.inMapBorder())
            {
                return false;
            }
            string skeletonID = a.stats.skeletonID;
            a.removeTrait("cursed");
            a.removeTrait("infected");
            Actor actor = MapBox.instance.createNewUnit(skeletonID, a.currentTile, null, 0f, null);
            actor.currentPosition = a.currentPosition;
            actor.transform.position = a.transform.position;
            actor.curAngle = a.transform.localEulerAngles;
            actor.transform.localEulerAngles = actor.curAngle;
            actor.targetAngle = default(Vector3);
            a.spriteRenderer.enabled = false;
            foreach (SpriteRenderer spriteRenderer in a.bodyParts)
            {
                spriteRenderer.GetComponent<SpriteRenderer>().enabled = false;
            }
            actor.data.firstName = "Un" + Toolbox.LowerCaseFirst(a.data.firstName);
            actor.data.age = a.data.age;
            actor.data.kills = a.data.kills;
            actor.data.children = a.data.children;
            actor.data.favorite = a.data.favorite;
            actor.checkFavoriteIcon();
            actor.takeItems(a, true);
            foreach (string text in a.data.traits)
            {
                if (!(text == "peaceful"))
                {
                    actor.addTrait(text);
                }
            }
            actor.statsDirty = true;
            if (!MapBox.instance.qualityChanger.lowRes)
            {
                MapBox.instance.stackEffects.startSpawnEffect(actor.currentTile, "spawn");
            }
            if (Config.spectatorMode && MoveCamera.focusUnit == pTarget)
            {
                MoveCamera.focusUnit = actor;
            }
            MapBox.instance.destroyActor((Actor)pTarget);
            return true;
        }
        */
        public static void GetObjects(WorldTile pTile, int pRadius, MapObjectType pObjectType)
        {
            MapBox.instance.CallMethod("getObjects", new object[] { pTile, pRadius, pObjectType });
        }

        public static void ActorRadiusthing(Actor centerActor, int radius)
        {
            WorldTile currentTile = Reflection.GetField(centerActor.GetType(), centerActor, "currentTile") as WorldTile;
            GetObjects(currentTile, radius, MapObjectType.Actor);
            List<BaseMapObject> tempUnits = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "temp_units") as List<BaseMapObject>;
            foreach (BaseMapObject baseMapObject in tempUnits)
            {
                Actor actor = (Actor)baseMapObject;
                bool flag = !(actor == centerActor);
                if (flag)
                {
                    /* targeted actor stuff
                    actor.restoreHealth(pVal);
                    actor.spawnParticle(Toolbox.color_heal);
                    actor.removeTrait("plague");
                    */
                }
            }
        }

        public static void BuildingRadiusthing(Actor centerActor, int radius)
        {
            WorldTile currentTile = Reflection.GetField(centerActor.GetType(), centerActor, "currentTile") as WorldTile;
            GetObjects(currentTile, radius, MapObjectType.Building);
            List<BaseMapObject> tempUnits = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "temp_units") as List<BaseMapObject>;
            foreach (BaseMapObject baseMapObject in tempUnits)
            {
                Building building = (Building)baseMapObject;

                /* targeted building stuff
               
                */

            }
        }

        public static void drawDivineLight_Postfix(WorldTile pCenterTile, string pPowerID, MapBox __instance)
        {
            if (divineLight)
                foreach (Actor actor in pCenterTile.units)
                {
                    if (divineLightFunction)
                    {
                        foreach (ActorTrait trait in activeTraits)
                        {
                            actor.addTrait(trait.id);
                        }
                        if(addingShieldToActor) {
                            AddShieldToActor(actor);
                        }
                      
                    }
                    else if (!divineLightFunction)
                    {
                        foreach (ActorTrait trait in activeTraits)
                        {
                            actor.removeTrait(trait.id);
                        }
                    }
                }
        }

        public static void AddShieldToActor(Actor target)
        {
            target.CallMethod("addStatusEffect", new object[] { "shield", 5000f });
        }

        public static void RemoveShieldFromActor(Actor target)
        {
            if (addingShieldToActor)
            {
                Dictionary<string, ActiveStatusEffect> activeStatus_dict = Reflection.GetField(target.GetType(), target, "activeStatus_dict") as Dictionary<string, ActiveStatusEffect>;
                if (activeStatus_dict.ContainsKey("shield"))
                {
                    target.CallMethod("removeStatusEffect", new object[] { "shield", null, -1 });
                }
            }
        }

        public void traitWindowUpdate()
        {
            if (GuiMain.showWindowMinimizeButtons.Value)
            {
                string buttontext = "T";
                if (GuiMain.showHideTraitsWindowConfig.Value)
                {
                    buttontext = "-";
                }
                if (GUI.Button(new Rect(traitWindowRect.x + traitWindowRect.width - 25f, traitWindowRect.y - 25, 25, 25), buttontext))
                {
                    GuiMain.showHideTraitsWindowConfig.Value = !GuiMain.showHideTraitsWindowConfig.Value;
                }
            }

            if (GuiMain.showHideTraitsWindowConfig.Value)
            {
                traitWindowRect = GUILayout.Window(1006, traitWindowRect, new GUI.WindowFunction(TraitWindow), "Traits", new GUILayoutOption[]
                {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                });

                if(lastSelectedActor == null || (lastSelectedActor != null && lastSelectedActor != Config.selectedUnit)) {
                    lastSelectedActor = Config.selectedUnit;
                }
            }
        }

        public static Actor lastSelectedActor;

        public static void TraitWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            original = GUI.backgroundColor;
            if (AssetManager.traits == null)
            {
                return;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset list"))
            {
                activeTraits = new List<ActorTrait>();
            }
            int i = 0;
            foreach (ActorTrait trait in AssetManager.traits.list)
            {
                if (!trait.id.Contains("stats") && !trait.id.Contains("customTrait"))
                {
                    GUI.backgroundColor = Color.red;
                    if (activeTraits.Contains(trait))
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    if (GUILayout.Button(trait.id))
                    {
                        if (activeTraits.Contains(trait))
                        {
                            activeTraits.Remove(trait);
                        }
                        else
                        {
                            activeTraits.Add(trait);
                        }
                    }
                    if (i != 1 && i % 5 == 0) // split buttons into vertical rows of 5
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                    i++;
                }
            }
            if (addingShieldToActor)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button("Shield"))
            {
                addingShieldToActor = !addingShieldToActor;
            }
            GUI.backgroundColor = original;
            GUILayout.EndHorizontal();
            if (activeTraits != null && (activeTraits.Count >= 1 || addingShieldToActor))
            {

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add traits to last selected") && lastSelectedActor != null)
                {
                    foreach (ActorTrait trait in activeTraits)
                    {
                        lastSelectedActor.addTrait(trait.id);
                    }
                    if(addingShieldToActor)
                        AddShieldToActor(lastSelectedActor);
                    bool statsDirty = (bool)Reflection.GetField(lastSelectedActor.GetType(), lastSelectedActor, "statsDirty");
                    statsDirty = true;
                   
                }
                if (GUILayout.Button("Remove traits to last selected") && lastSelectedActor != null)
                {
                    foreach (ActorTrait trait in activeTraits)
                    {
                        lastSelectedActor.removeTrait(trait.id);
                    }
                    RemoveShieldFromActor(lastSelectedActor);
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (divineLight)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;

                }
                if (GUILayout.Button("Divine light: "))
                {
                    divineLight = !divineLight;
                }
                string button;
                if (divineLightFunction)
                {
                    GUI.backgroundColor = Color.green;
                    button = "adds";
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                    button = "removes";
                }
                if (GUILayout.Button(button))
                {
                    divineLightFunction = !divineLightFunction;
                }

                GUILayout.EndHorizontal();

            }

            GUI.DragWindow();
        }

        public static bool addingShieldToActor;
        public static List<ActorTrait> traits => AssetManager.traits.list;
        public bool showHideTraitsWindow;
        public Rect traitWindowRect = new Rect(126f, 1f, 1f, 1f);
        public static List<ActorTrait> activeTraits = new List<ActorTrait>();
        private static Color original;
        public static bool divineLight;
        public static bool divineLightFunction;
        public static Dictionary<string, string> traitNamesAndDescriptions = new Dictionary<string, string>();
    }
}
