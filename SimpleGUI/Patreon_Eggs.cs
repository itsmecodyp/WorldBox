using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
                patreonWindowRect = GUILayout.Window(1009, patreonWindowRect, patreonWindow, "Patreon", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
            if (nameCount != setCount)
            {
                Debug.Log("Easter egg unit: " + SpawnedNames.Last().Key);
                setCount = nameCount;
            }
        }
      
        public static void generatePersonality_Postfix(ActorBase __instance)
        {
            //overwrite unit names after they are created
            ActorData data =__instance.data;
            Race race = __instance.race; //for people who want a certain type of creature to take their name
            ActorAsset stats = __instance.asset;
            string name = "null";
            if (race.id == "dragon")
            {
                name = "Ismoehr Traving";
                if (!SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    if (Random.Range(1, 100) > 90)
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
                    if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                    {
                        data.setName(name);
                        __instance.addTrait("fast");
                        __instance.addTrait("attractive");
                        ActorTrait actorTrait = new ActorTrait();
                        actorTrait.base_stats["intelligence"] = 50;
                        actorTrait.base_stats["health"] = 400;
                        actorTrait.base_stats["damage"] = 100;
                        actorTrait.base_stats["diplomacy"] = 50;
                        actorTrait.base_stats["personality_administration"] = 50f;
                        actorTrait.inherit = 0f;
                        actorTrait.id = "customTraitJuan";
                        AssetManager.traits.add(actorTrait);
                        __instance.addTrait(actorTrait.id);
                        SpawnedNames.Add(name, true);
                        return;
                    }
                }
                name = "Apex";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name)) // if name hasnt been used yet
                {
                    data.setName(name); // override the name
                    __instance.addTrait("stupid"); // hehe apex is stupid
                    __instance.addTrait("attractive"); // hehe apex is attractive
                    SpawnedNames.Add(name, true); // add it to the "used" list
                    return;
                }
                name = "Nicholas";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "Hayes";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    __instance.addTrait("fast");
                    __instance.addTrait("lucky");
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "Styderr";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    __instance.addTrait("fast");
                    SpawnedNames.Add(name, true);

                    return;
                }
                name = "Ruma";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    //ItemData pData = ItemGenerator.generateItem(itemAsset, materialForItem.id, MapBox.instance.mapStats.year, this.kingdom.name, pCreatorName, pTries, pActor);
                    ItemGenerator.generateItem(AssetManager.items.get("sword"), "silver", MapBox.instance.mapStats.year, null, "The Void", 1, __instance);
                    ItemGenerator.generateItem(AssetManager.items.get("ring"), "silver"  , MapBox.instance.mapStats.year, null, "The Void", 1, __instance);
                    ItemGenerator.generateItem(AssetManager.items.get("amulet"), "silver", MapBox.instance.mapStats.year, null, "The Void", 1, __instance);
                    ItemGenerator.generateItem(AssetManager.items.get("armor"), "silver", MapBox.instance.mapStats.year, null, "The Void", 1, __instance);
                    ItemGenerator.generateItem(AssetManager.items.get("boots"), "silver", MapBox.instance.mapStats.year, null, "The Void", 1, __instance);
                    ItemGenerator.generateItem(AssetManager.items.get("helmet"), "silver", MapBox.instance.mapStats.year, null, "The Void", 1, __instance);
                    SpawnedNames.Add(name, true);
                    return;
                }
                name = "Bill Dipperly";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    SpawnedNames.Add(name, true);
                    return;
                }
            }
            if (race.id == "human")
            {
                name = "Amon";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
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
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    __instance.addTrait("fast");
                    __instance.addTrait("strong");
                    ActorTrait actorTrait = new ActorTrait();
                    actorTrait.base_stats["speed"] = 50f;
                    actorTrait.inherit = 0f;
                    actorTrait.id = "customTraitPoly";
                    AssetManager.traits.add(actorTrait);
                    __instance.addTrait(actorTrait.id);
                    __instance.transform.localScale *= 1.5f;
                    SpawnedNames.Add(name, true);
                    return;
                }
                name = "Floppa";
                if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey(name))
                {
                    data.setName(name);
                    SpawnedNames.Add(name, true);
                }
            }
        }

        public static string getKingdomName_Postfix(string __result)
        {
            //same as above but for kingdom names
            if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey("Supra Empire"))
            {
                SpawnedNames.Add("Supra Empire", true);
                return "Supra Empire";
            }
            if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey("Styderr's Empire"))
            {
                SpawnedNames.Add("Styderr's Empire", true);
                return "Styderr's Empire";
            }
            if (Random.Range(1, 100) > 90 && !SpawnedNames.ContainsKey("Misty's Empire"))
            {
                SpawnedNames.Add("Misty's Empire", true);
                return "Misty's Empire";
            }

            return __result;

        }

        public static void OnEnable_Postfix(LoadingScreen __instance)
        {
            Debug.Log("1");
            int num = Toolbox.randomInt(1, 13); // highest number == null string, the rest are valid rolls
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
                text = "Tiempo con Juan Diego makes amazing worldbox videos! - Juanchiz";
            }
            if (num == 11)
            {
                text = "Have you heard the legend of Greg?";
            }
            if(num == 12) {
                text = "Want your message here? Support me on Patreon!";
            }
            if(num == 13) {
                text = "664187111083212804";
            }
            //time stuff
            DateTime timeNow = DateTime.Now;
            foreach(KeyValuePair<string, DateTime> birthdayToCheck in birthdays) {
                if(timeNow.Month == birthdayToCheck.Value.Month) {
                    if(timeNow.Day == birthdayToCheck.Value.Day) {
                        // Complete override for single day
                        text = "Happy birthday to " + birthdayToCheck.Key + "!!";
                    }
                }
            }
            if(__instance.tipText != null) {
                // add simplegui tag on new line so people arent thinking these messages came from maxim
                __instance.tipText.text.text = text + " \n (SimpleGUI)";
            }
        }

        // years dont matter, i only check month/day
        public static Dictionary<string, DateTime> birthdays = new Dictionary<string, DateTime>
        {
            {"Cody", new DateTime(1, 1, 17)},
            //{"Adiniz", new DateTime(1, 9, 11)},
        };

        public int nameCount => SpawnedNames.Count;
        public int setCount;
        public static Dictionary<string, bool> SpawnedNames = new Dictionary<string, bool>();
        public bool showHidePatreon;
        public Rect patreonWindowRect;
    }
}
