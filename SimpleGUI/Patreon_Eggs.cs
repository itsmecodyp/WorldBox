using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleGUI
{
    class GuiPatreon
    {
        public void patreonWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            if (GUILayout.Button("Click to visit my patreon"))
            {
                Application.OpenURL("https://www.patreon.com/codysmods");
            }
            if (GUILayout.Button("Click to visit my discord"))
            {
                Application.OpenURL("https://discord.gg/fQFAZPV");
            }
            GUI.DragWindow();
        }

        public void patreonWindowUpdate()
        {
            if (GuiMain.showHidePatreonConfig.Value)
            {
                patreonWindowRect = GUILayout.Window(1009, patreonWindowRect, new GUI.WindowFunction(patreonWindow), "Patreon", new GUILayoutOption[]
                {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                });
            }
            if (nameCount != setCount)
            {
                Debug.Log("Easter egg unit: " + SpawnedNames.Last().Key);
                setCount = nameCount;
            }
        }
      
        public static void generatePersonality_Postfix(ActorBase __instance)
        {
            ActorStatus data = Reflection.GetField(__instance.GetType(), __instance, "data") as ActorStatus;
            Race race = Reflection.GetField(__instance.GetType(), __instance, "race") as Race;
            ActorStats stats = Reflection.GetField(__instance.GetType(), __instance, "stats") as ActorStats;
            string name = "null";
            if (race.id == "dragon")
            {
                name = "Ismoehr Traving";
                if (!SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    if (UnityEngine.Random.Range(1, 100) > 90)
                    {
                        __instance.addTrait("burning_feet");
                    }
                    SpawnedNames.Add(name, true);
                }
            }
            if (race.civilization)
            {
                if (race.id == "human" || race.id == "dwarf")
                {
                    name = "Juanchiz";
                    if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                    {
                        data.firstName = name;
                        __instance.addTrait("fast");
                        __instance.addTrait("attractive");
                        ActorTrait actorTrait = new ActorTrait();
                        actorTrait.baseStats.intelligence = 50;
                        actorTrait.baseStats.health = 400;
                        actorTrait.baseStats.damage = 100;
                        actorTrait.baseStats.diplomacy = 50;
                        actorTrait.baseStats.personality_administration = 50f;
                        actorTrait.inherit = 0f;
                        actorTrait.id = "customTraitJuan";
                        AssetManager.traits.add(actorTrait);
                        __instance.addTrait(actorTrait.id);
                        SpawnedNames.Add(name, true);
                        return;
                    }
                }
                name = "Apex";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name)) // if name hasnt been used yet
                {
                    data.firstName = name; // override the name
                    __instance.addTrait("stupid"); // hehe apex is stupid
                    __instance.addTrait("attractive"); // hehe apex is attractive
                    SpawnedNames.Add(name, true); // add it to the "used" list
                    return;
                }
                name = "Nicholas";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "Hayes";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("fast");
                    __instance.addTrait("lucky");
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "Styderr";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("fast");
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "Ruma";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    ItemGenerator.generateItem(AssetManager.items.get("sword"), "silver", __instance.equipment.weapon, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("ring"), "silver", __instance.equipment.ring, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("amulet"), "silver", __instance.equipment.amulet, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("armor"), "silver", __instance.equipment.armor, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("boots"), "silver", __instance.equipment.boots, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("helmet"), "silver", __instance.equipment.helmet, MapBox.instance.mapStats.year, "The Void", name, 1);
                    SpawnedNames.Add(name, true);
                    return;
                }
                name = "Bill Dipperly";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    SpawnedNames.Add(name, true);
                    return;
                }
            }
            if (race.id == "human")
            {
                name = "Amon";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("veteran");
                    __instance.addTrait("tough");
                    __instance.addTrait("wise");
                    __instance.addTrait("paranoid");
                    __instance.addTrait("greedy");
                    __instance.addTrait("slow");
                    __instance.addTrait("eyepatch");
                    SpawnedNames.Add(name, true);
                    return;
                }
            }
            if (stats.race == "cat")
            {
                name = "PolyMorphik's Lynx";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("fast");
                    __instance.addTrait("strong");
                    ActorTrait actorTrait = new ActorTrait();
                    actorTrait.baseStats.speed = 50f;
                    actorTrait.inherit = 0f;
                    actorTrait.id = "customTraitPoly";
                    AssetManager.traits.add(actorTrait);
                    __instance.addTrait(actorTrait.id);
                    __instance.transform.localScale *= 1.5f;
                    SpawnedNames.Add(name, true);
                    return;
                }
                name = "Floppa";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    SpawnedNames.Add(name, true);
                    return;
                }
            }
        }

        public static string getKingdomName_Postfix(string __result)
        {
            if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey("Supra Empire"))
            {
                SpawnedNames.Add("Supra Empire", true);
                return "Supra Empire";
            }
            if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey("Styderr's Empire"))
            {
                SpawnedNames.Add("Styderr's Empire", true);
                return "Styderr's Empire";
            }
            if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey("Misty's Empire"))
            {
                SpawnedNames.Add("Misty's Empire", true);
                return "Misty's Empire";
            }
            else
            {
                return __result;
            }

        }

        public static void getTipID_Postfix(LoadingScreen __instance)
        {
            int num = Toolbox.randomInt(1, 11); // highest number == null string, the rest are valid rolls
            string text = "null";
            if (num == 1)
            {
                text = "Styderr makes awesome maps, check them out!";
            }
            if (num == 2)
            {
                text = "Nothing to see here guys - KJYhere";
            }
            if (num == 3)
            {
                text = "Call up Rajit at 1(800)-911-SCAM   - Ramlord";
            }
            if (num == 4)
            {
                text = "10/10 would recommend - boopahead08";
            }
            if (num == 5)
            {
                // need replacement message
            }
            if (num == 6)
            {
                text = "This mod is sponsored by Raid: Shadow Legends - Slime";
            }
            if (num == 7)
            {
                text = "The four nations lived in harmony, until the orc nation attacked";
            }
            if (num == 8)
            {
                text = "Modificating and customizating the game...";
            }
            if (num == 9)
            {
                text = "Now with raytracing!";
            }
            if (num == 10)
            {
                text = "Tiempo con Juan Diego makes amazing worldbox videos! - Juanchiz";
            }
            if (num == 11)
            {
                text = "null";
            }
            __instance.topText.key = text;
            __instance.topText.CallMethod("updateText", new object[] { true });
        }

        public int nameCount => SpawnedNames.Count;
        public int setCount = 0;
        public static Dictionary<string, bool> SpawnedNames = new Dictionary<string, bool>();
        public bool showHidePatreon;
        public Rect patreonWindowRect;
    }
}
