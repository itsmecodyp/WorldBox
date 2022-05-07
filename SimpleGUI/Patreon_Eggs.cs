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
                    __instance.addTrait("shadow summoner"); // hehe apex is stupid
                    __instance.addTrait("madness"); // hehe apex is attractive
                    SpawnedNames.Add(name, true); // add it to the "used" list
                    return;
                }
                name = "ben";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "patrick";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("fast");
                    __instance.addTrait("lucky");
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "pab";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("fast");
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "Ninda";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    ItemGenerator.generateItem(AssetManager.items.get("sword"), "adamantine", __instance.equipment.weapon, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("ring"), "adamantine", __instance.equipment.ring, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("amulet"), "adamantine", __instance.equipment.amulet, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("armor"), "adamantine", __instance.equipment.armor, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("boots"), "adamantine", __instance.equipment.boots, MapBox.instance.mapStats.year, "The Void", name, 1);
                    ItemGenerator.generateItem(AssetManager.items.get("helmet"), "adamantine", __instance.equipment.helmet, MapBox.instance.mapStats.year, "The Void", name, 1);
                    SpawnedNames.Add(name, true);
                    return;
                }
                name = "Bob";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    SpawnedNames.Add(name, true);
                    return;
                }
            }
            if (race.id == "human")
            {
                name = "William afton";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("evil");
                    __instance.addTrait("tough");
                    __instance.addTrait("mageslayer");
                    __instance.addTrait("paranoid");
                    __instance.addTrait("greedy");
                    __instance.addTrait("fast");
                    __instance.addTrait("immortal");
                    SpawnedNames.Add(name, true);
                    return;
                }
            }
            if (stats.race == "cat")
            {
                name = "Ben";
                if (UnityEngine.Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.firstName = name;
                    __instance.addTrait("fat");
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
                name = "garfield";
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
            int num = Toolbox.randomInt(1, 13); // highest number == null string, the rest are valid rolls
            string text = "null";
            if (num == 1)
            {
                text = "Cody helped me make my own unit name generator -Plague";
            }
            if (num == 2)
            {
                text = "I used codys Simple gui to make My mod - Plague";
            }
            if (num == 3)
            {
                text = "I always come back";
            }
            if (num == 4)
            {
                text = "Amogus";
            }
            if (num == 5)
            {
                string platform = Application.platform.ToString().ToLower();
                if(platform.Contains("window")) {
                    text = "When was your last blue screen of death?";
                }
                else if(platform.Contains("osx")) {
                    text = "Too good for Windows or something?";
                }
                else if(platform.Contains("linux")) {
                    text = "What distro you running? Have you heard of-";
                }
                else {
                    text = "What kind of system are you running here?!";
                }
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
                text = "Now with raytracing! And 3d!";
            }
            if (num == 10)
            {
                text = "cody makes awesome mods!";
            }
            if (num == 11)
            {
                text = "you can use asteroids to make maps it makes dirt tiles on mountains its perfect!";
            }
            if(num == 12) {
                text = "Tornados can be grown bigger with cursed trait even though there not tomatos";
            }
            if(num == 13) {
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
