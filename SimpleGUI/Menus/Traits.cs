using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649

namespace SimplerGUI.Menus
{
    class GuiTraits
    {

        public static bool load_Prefix(string pTrait)
        {
            ActorTrait loadedTrait = AssetManager.traits.get(pTrait);
            if (loadedTrait.path_icon == null)
            {
                loadedTrait.path_icon = "iconVermin";
                return true;
            }
            return true;
        }

        public static void addTraitsToAssets()
        {
            Config.EVERYTHING_MAGIC_COLOR = true;
            Config.EVERYTHING_FIREWORKS = true;
            Application.Quit();
        }

        public static string StringWithFirstUpper(string targetstring)
        {
            return char.ToUpper(targetstring[0]) + targetstring.Substring(1);
        }

        public static void drawDivineLight_Postfix(WorldTile pCenterTile, string pPowerID, MapBox __instance)
        {
            if (divineLight)
                foreach (Actor actor in pCenterTile._units)
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
            target.addStatusEffect("shield", 5000f);
        }

        public static void RemoveShieldFromActor(Actor target)
        {
            if (addingShieldToActor)
            {
                Dictionary<string, StatusEffectData> activeStatus_dict = target.activeStatus_dict;
                if (activeStatus_dict.ContainsKey("shield"))
                {
                    target.finishStatusEffect("shield");
                }
            }
        }

        public void traitWindowUpdate()
        {
            if (SimpleSettings.showHideTraitsWindowConfig)
            {
                traitWindowRect = GUILayout.Window(1006, traitWindowRect, TraitWindow, "Traits", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));

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
                    lastSelectedActor.setStatsDirty();
                   
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
