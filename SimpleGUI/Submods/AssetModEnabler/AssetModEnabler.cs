using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.Runtime.Internal.Transform;
using BepInEx;
using HarmonyLib;
using SimplerGUI.Menus;
using SimplerGUI.Submods.SimpleMessages;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimplerGUI.Submods.AssetModEnabler
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class AsssetModEnabler_Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.asset_mod.enabler";
        public const string pluginName = "Asset Mod Loader Enabler";
        public const string pluginVersion = "0.0.0.1";

        public void Awake()
        {
            HarmonyPatchSetup();
        }

        public void HarmonyPatchSetup()
        {
            Harmony harmony = new Harmony(pluginName);
            MethodInfo original;
            MethodInfo patch;

            original = AccessTools.Method(typeof(AssetModLoader), "loadTexture");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "loadTexture_prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            original = AccessTools.Method(typeof(AssetModLoader), "loadFileJson");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "loadFileJson_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            original = AccessTools.Method(typeof(MoodAsset), "getSprite");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "getSprite_prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            original = AccessTools.Method(typeof(AssetModLoader), "checkModFolder");
            patch = AccessTools.Method(typeof(AsssetModEnabler_Main), "checkModFolder_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
        }




        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {

            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                showSubMod = true;
            }
            mainWindowRect.height = 0f;
        }
        public static bool showHideTileTypeWindow;
        public static Rect tileTypeWindowRect = new Rect(0f, 1f, 1f, 1f);


        public static bool showHideMainWindow;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public bool showSubMod = true;

        public void OnGUI()
        {

            if (showSubMod)
            {
                if (GUI.Button(new Rect(Screen.width - 120, 100, 95, 20), "Assets"))
                {
                    showHideMainWindow = !showHideMainWindow;
                    //showHideTileTypeWindow = !showHideTileTypeWindow;
                }
                if (GUI.Button(new Rect(Screen.width - 25, 100, 25, 20), "x"))
                {
                    showHideMainWindow = false;
                    showSubMod = false;
                }
                if (showHideMainWindow)
                {
                    mainWindowRect = GUILayout.Window(410401, mainWindowRect, AssetModEnablerWindow, "Asset Stuff", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }
                if (showHideTileTypeWindow)
                {
                    tileTypeWindowRect = GUILayout.Window(410402, tileTypeWindowRect, TileTypeSetWindow, "TileType Stuff", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                }
            }
        }
     
        public void AssetModEnablerWindow(int windowID)
        {
            if (GUILayout.Button("Load assets"))
            {
                AssetModLoader.load();
                AssetManager.months.post_init(); // reload months that got added
                List<string> moodList = new List<string>();
                foreach(MoodAsset mood in AssetManager.moods.list)
                {
                    moodList.Add(mood.id);
                }
                GuiMain.ActorInteraction.moods = moodList;
                AssetManager.powers.linkAssets();
                GuiMain.postAssetInitStuff();
            }
            if (GUILayout.Button("Load bonfire2"))
            {
                BuildingAsset b2 = AssetManager.buildings.get("bonfire2");
                if (b2 != null)
                {
                    AssetManager.buildings.loadSprites(b2);
                }
            }
            if (GUILayout.Button("Export bonfire"))
            {
                string assetID = "bonfire";
                //setup folders
                string path12 = Application.streamingAssetsPath + "/mods";
                if (Directory.Exists(path12 + "/BonfireExport") == false)
                {
                    Directory.CreateDirectory(path12 + "/BonfireExport");
                }
                if (Directory.Exists(path12 + "/BonfireExport/buildings") == false)
                {
                    Directory.CreateDirectory(path12 + "/BonfireExport/buildings");
                }
                if (Directory.Exists(path12 + "/BonfireExport/sprites") == false)
                {
                    Directory.CreateDirectory(path12 + "/BonfireExport/sprites");
                }
                Sprite[] spriteList2 = SpriteTextureLoader.getSpriteList("buildings/" + assetID);
                if (spriteList2 != null && spriteList2.Length > 0)
                {
                    foreach (Sprite sprite in spriteList2)
                    {
                        Texture2D itemBGTex = sprite.texture;
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/BonfireExport/sprites/" + assetID + "_" + sprite.name + ".png", itemBGBytes);
                    }
                }

                string dataToSave = JsonUtility.ToJson(AssetManager.buildings.get(assetID), true);
                File.WriteAllText(path12 + "/BonfireExport/buildings/" + assetID + ".json", dataToSave);

            }
            if (GUILayout.Button("Export ALL buildings"))
            {
                foreach (BuildingAsset ba in AssetManager.buildings.list)
                {
                    string assetID = ba.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/buildings") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/buildings");
                    }
                    if (Directory.Exists(path12 + "/Export/building_sprites") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/building_sprites");
                    }
                    Sprite[] spriteList2 = SpriteTextureLoader.getSpriteList("buildings/" + assetID);
                    if (spriteList2 != null && spriteList2.Length > 0)
                    {
                        foreach (Sprite sprite in spriteList2)
                        {
                            Texture2D itemBGTex = sprite.texture;
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/building_sprites/" + assetID + "_" + sprite.name + ".png", itemBGBytes);
                        }
                    }

                    string dataToSave = JsonUtility.ToJson(ba, true);
                    File.WriteAllText(path12 + "/Export/buildings/" + assetID + ".json", dataToSave);

                }
            }
            //export traits for reuse
            if (GUILayout.Button("Export ALL traits"))
            {
                foreach (ActorTrait ta in AssetManager.traits.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/traits") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/traits");
                    }
                    if (Directory.Exists(path12 + "/Export/trait_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/trait_icon");
                    }
                    Sprite[] spriteList2 = SpriteTextureLoader.getSpriteList("buildings/" + assetID);
                    if (spriteList2 != null && spriteList2.Length > 0)
                    {
                        foreach (Sprite sprite in spriteList2)
                        {
                            Texture2D itemBGTex = sprite.texture;
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/trait_icon/" + sprite.name + ".png", itemBGBytes);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ta.path_icon) == false)
                        {
                            Sprite sprite = SpriteTextureLoader.getSprite(ta.path_icon);
                            if (sprite != null)
                            {
                                Texture2D itemBGTex = duplicateTexture(sprite.texture);//new Texture2D(sprite.texture.width, sprite.texture.height);
                                //itemBGTex.LoadRawTextureData(sprite.texture.GetRawTextureData());
                                byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                                File.WriteAllBytes(path12 + "/Export/trait_icon/" + assetID + "_" + sprite.name + ".png", itemBGBytes);
                            }
                        }
                    }

                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/traits/" + assetID + ".json", dataToSave);

                }
            }
            if (GUILayout.Button("Export ALL resources"))
            {
                foreach (ResourceAsset ta in AssetManager.resources.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/resources") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/resources");
                    }
                    if (Directory.Exists(path12 + "/Export/resources_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/resources_icon");
                    }
                    Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/" + ta.path_icon);
                    if (sprite != null)
                    {

                        Texture2D itemBGTex = duplicateTexture(sprite.texture);//sprite.texture;
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/Export/resources_icon/" + sprite.name + ".png", itemBGBytes);
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/resources/" + assetID + ".json", dataToSave);

                }
            }
            if (GUILayout.Button("Export ALL eras"))
            {
                foreach (EraAsset ta in AssetManager.era_library.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/eras") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/eras");
                    }
                    if (Directory.Exists(path12 + "/Export/era_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/era_icon");
                    }
                    Sprite sprite = SpriteTextureLoader.getSprite(ta.path_icon);
                    if (sprite != null)
                    {

                        Texture2D itemBGTex = sprite.texture;
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/Export/era_icon/" + sprite.name + ".png", itemBGBytes);
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/eras/" + assetID + ".json", dataToSave);

                }
            }
            if (GUILayout.Button("Export ALL months"))
            {
                foreach (MonthAsset ta in AssetManager.months.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/months") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/months");
                    }
                  
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/months/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL moods"))
            {
                foreach (MoodAsset ta in AssetManager.moods.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/moods") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/moods");
                    }
                    if (Directory.Exists(path12 + "/Export/mood_icon") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/mood_icon");
                    }
                    Sprite sprite = (Sprite)Resources.Load("ui/Icons/" + ta.icon, typeof(Sprite));
                    if (sprite != null)
                    {
                        Texture2D itemBGTex = duplicateTexture(sprite.texture);
                        byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                        File.WriteAllBytes(path12 + "/Export/mood_icon/" + sprite.name + ".png", itemBGBytes);
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/moods/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL tileTypes"))
            {
                foreach (TileType ta in AssetManager.tiles.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/tiles") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/tiles");
                    }
                    if (Directory.Exists(path12 + "/Export/tile_icons") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/tile_icons");
                    }
                    Sprite[] array = Resources.LoadAll<Sprite>("tiles/" + ta.id);
                    if (array != null && array.Length > 0)
                    {
                        foreach(Sprite sprite in array)
                        {
                            Texture2D itemBGTex = duplicateTexture(sprite.texture);
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/tile_icons/" + sprite.name + ".png", itemBGBytes);
                        }
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/tiles/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL topTileTypes"))
            {
                foreach (TopTileType ta in AssetManager.topTiles.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/topTiles") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/topTiles");
                    }
                    if (Directory.Exists(path12 + "/Export/topTile_icons") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/topTile_icons");
                    }
                    Sprite[] array = Resources.LoadAll<Sprite>("topTiles/" + ta.id);
                    if (array != null && array.Length > 0)
                    {
                        foreach (Sprite sprite in array)
                        {
                            Texture2D itemBGTex = duplicateTexture(sprite.texture);
                            byte[] itemBGBytes = itemBGTex.EncodeToPNG();
                            File.WriteAllBytes(path12 + "/Export/topTile_icons/" + sprite.name + ".png", itemBGBytes);
                        }
                    }
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/topTiles/" + assetID + ".json", dataToSave);
                }
            }
            if (GUILayout.Button("Export ALL biomes"))
            {
                foreach (BiomeAsset ta in AssetManager.biome_library.list)
                {
                    string assetID = ta.id;

                    //setup folders
                    string path12 = Application.streamingAssetsPath + "/mods";
                    if (Directory.Exists(path12 + "/Export") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export");
                    }
                    if (Directory.Exists(path12 + "/Export/biomes") == false)
                    {
                        Directory.CreateDirectory(path12 + "/Export/biomes");
                    }
                    //biomes dont have sprites?
                    string dataToSave = JsonUtility.ToJson(ta, true);
                    File.WriteAllText(path12 + "/Export/biomes/" + assetID + ".json", dataToSave);
                }
            }

            GUI.DragWindow();
        }
        public void TileTypeSetWindow(int windowID)
        {
            if(selectedTileTypeMap != null)
            {
                if(selectedTileTypeMap.mainsheetFull != null)
                {
                    //?
                    GUILayout.Box(selectedTileTypeMap.mainsheetFull);
                }
                /*
                for (int y = 0; y < selectedTileTypeMap.height; y++)
                {
                    for (int x = 0; x < selectedTileTypeMap.width; x++)
                    {
                        
                        //GUI.Button(new Rect(x, y, 5, 5), x.ToString() + "," + y.ToString());

                        //GUILayout.Button(x.ToString() + "," + y.ToString());
                    }
                }
                */
            }
            GUILayout.Button("t");
            //make background the full sheet/image
            GUI.DragWindow();
        }

        public TileTypeMap selectedTileTypeMap;

        public class TileTypeMap
        {
            //name of tilemap/spritesheet/theme/etc
            public string id;
            public Texture2D mainsheetFull;
            //dimension of sprites on sheet
            public int width = 8;
            public int height = 8;
            //map of sub-sprites/textures, accessable like spriteMap[0,1] for first from bottom left
            public Dictionary<Vector2, Sprite> spriteMap;
            public Dictionary<Vector2, Texture2D> texture2dMap;

        }
        //split image into many 8x8 images

        public static bool checkModFolder_Prefix(string pPath, string pType)
        {
            AssetModLoader.log("CMF pType: " + pType);
            List<string> files = AssetModLoader.getFiles(pPath);
            string[] array = pPath.Split(new char[] { Path.DirectorySeparatorChar });
            AssetModLoader.log("");
            AssetModLoader.log("# CHECKING PATH... " + array[array.Length - 1]);
            AssetModLoader.log("FILES: " + files.Count.ToString());
            AssetModLoader.log("");
            List<Sprite> multiSpritesForAsset = null;
            string assetName = null;
            if (pType == "buildings" || pType == "tiles" || pType == "topTiles")
            {
                multiSpritesForAsset = new List<Sprite>();
            }
            foreach (string text in files)
            {
                AssetModLoader.log(text);
                if (text.Contains("json"))
                {
                    if(assetName == null)
                    {
                        string[] array2 = pPath.Split(new char[] { Path.DirectorySeparatorChar });
                        string text2 = array[array.Length - 1];
                        AssetModLoader.log("# FOUND NAME: " + text2);
                        assetName = text2.Replace(".json", "");
                    }
                    AssetModLoader.loadFileJson(text, pType);
                }
                if (text.Contains("png"))
                {
                    //check if image is inside folder for multi-sprite assets
                    if(pType == "buildings" && multiSpritesForAsset != null)
                    {
                        loadTextureBuilding(text, multiSpritesForAsset);
                    }
                    else if (pType == "tiles" || pType == "topTiles" && multiSpritesForAsset != null)
                    {
                        loadTextureTiles(text, multiSpritesForAsset);
                    }
                    else
                    {
                        AssetModLoader.loadTexture(text);
                    }
                }
            }
            if(pType == "buildings" && multiSpritesForAsset != null && multiSpritesForAsset.Count > 0)
            {
                Sprite[] spriteArray = multiSpritesForAsset.ToArray();
                Debug.Log("building sprite count: " + spriteArray.Length);
                SpriteTextureLoader.cached_sprite_list.Add("buildings/" + assetName, spriteArray);
                Debug.Log("added sprites to cached_sprite_list");
                BuildingAsset buildingAsset = AssetManager.buildings.get(assetName);
                if(buildingAsset != null)
                {
                    Debug.Log("asset found, loading sprites");
                    //buildingAsset.spritePath should probably equal the building name/id
                    AssetManager.buildings.loadSprites(buildingAsset);
                }
            }
            if (pType == "tiles")
            {
                TileType asset = AssetManager.tiles.get(assetName);
                if (asset != null)
                {
                    loadSpritesForTile(asset, multiSpritesForAsset);
                }

                GodPower powerForTile = AssetManager.powers.clone(assetName, "tileDeepOcean");
                powerForTile.tileType = assetName;
                powerForTile.name = assetName;
                AssetManager.powers.add(powerForTile);
                //icon needs changed and reloaded, GodPower.getIconSprite

            }
            if (pType == "topTiles")
            {
                TopTileType asset = AssetManager.topTiles.get(assetName);
                if (asset != null)
                {
                    loadSpritesForTile(asset, multiSpritesForAsset);
                }

                GodPower powerForTile = AssetManager.powers.clone(assetName, "tileDeepOcean");
                powerForTile.topTileType = assetName;
                powerForTile.name = assetName;
                AssetManager.powers.add(powerForTile);
            }
            return false;
        }

        private static void loadTextureTiles(string pPath, List<Sprite> multiSpritesForAsset)
        {
            string[] array = pPath.Split(new char[] { Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD TEXTURE TILE: " + text);
            byte[] array2 = File.ReadAllBytes(pPath);
            //string text2 = "@wb_" + text;
            text = text.Replace(".png", "");
            AssetModLoader.log("ADDING TEXTURE TILE... " + text);
            //SpriteTextureLoader.addSprite(text, array2);

            //addSprite edit
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.filterMode = FilterMode.Point;
            if (texture2D.LoadImage(array2))
            {
                Rect rect = new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height);
                Vector2 vector = new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(texture2D, rect, vector, 1f);
                sprite.name = text;
                //SpriteTextureLoader.cached_sprites.Add(pPathID, sprite);
                multiSpritesForAsset.Add(sprite);
            }
        }

        //copy paste with smol edit. lil ugly, just like you
        private static void loadTextureBuilding(string pPath, List<Sprite> listToAddTo)
        {
            string[] array = pPath.Split(new char[] { Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD TEXTURE BUILDING: " + text);
            byte[] array2 = File.ReadAllBytes(pPath);
            //string text2 = "@wb_" + text;
            text = text.Replace(".png", "");
            AssetModLoader.log("ADDING TEXTURE BUILDING... " + text);
            //SpriteTextureLoader.addSprite(text, array2);

            //addSprite edit
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.filterMode = FilterMode.Point;
            if (texture2D.LoadImage(array2))
            {
                Rect rect = new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height);
                Vector2 vector = new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(texture2D, rect, vector, 1f);
                sprite.name = text;
                //SpriteTextureLoader.cached_sprites.Add(pPathID, sprite);
                listToAddTo.Add(sprite);
            }
        }

        //copy paste and edit
        public static void loadSpritesForTile(TileType pType, List<Sprite> listToAddTo)
        {
            Sprite[] array = listToAddTo.ToArray();//Resources.LoadAll<Sprite>("tiles/" + pType.id);
            if (array == null || array.Length == 0)
            {
                return;
            }
            pType.sprites = new TileSprites();
            foreach (Sprite sprite in array)
            {
                pType.sprites.addVariation(sprite);
            }
        }

        public static void loadSpritesForTile(TopTileType pType, List<Sprite> listToAddTo)
        {
            Sprite[] array = listToAddTo.ToArray();//Resources.LoadAll<Sprite>("tiles/" + pType.id);
            if (array == null || array.Length == 0)
            {
                return;
            }
            pType.sprites = new TileSprites();
            foreach (Sprite sprite in array)
            {
                pType.sprites.addVariation(sprite);
            }
        }

        //remove @wb_ prefix from textures..
        //replace @ in filename with /, allows nested path identical to what game might expect ex: civ/icons/minimap_boat_small
        public static bool loadTexture_prefix(string pPath)
        {
            string[] array = pPath.Split(new char[] { Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD TEXTURE: " + text);
            byte[] array2 = File.ReadAllBytes(pPath);
            text = text.Replace('@', '/');
            text = text.Replace(".png", "");
            AssetModLoader.log("(p)ADDING TEXTURE... " + text);
            SpriteTextureLoader.addSprite(text, array2);
            return false;
        }

        public static bool getSprite_prefix(ref Sprite __result, MoodAsset __instance)
        {
            if (__instance.sprite == null)
            {
                __instance.sprite = (Sprite)Resources.Load("ui/Icons/" + __instance.icon, typeof(Sprite));
            }
            //above is original, below is in case above isnt found, default to normal AML behaviour
            if (__instance.sprite == null)
            {
                __instance.sprite = SpriteTextureLoader.getSprite(__instance.icon);
            }
            __result = __instance.sprite;
            return false;
        }

        //load new asset types
        //prefix to just go ahead and replace with a clean switch statement
        public static void loadFileJson_Prefix(string pPath, string pType)
        {
            string[] array = pPath.Split(new char[] { Path.DirectorySeparatorChar });
            string text = array[array.Length - 1];
            AssetModLoader.log("# LOAD ASSET: " + text);
            string fileText = File.ReadAllText(pPath);
            if (pType != null)
            {
                switch (pType)
                {
                    case "tiles":
                        LoadAssetTileType(fileText);
                        break;
                    case "topTiles":
                        LoadAssetTopTileType(fileText);
                        break;
                    case "months":
                        //months need MANY patches to make work: formatDate, getDate, getCurrentMonth, getCurrentYear, etc
                        //hard coded for 12 months and "inflected" names which will null if not found
                        //not worth it for now, revisit later
                        LoadAssetMonth(fileText);
                        break;
                    case "moods":
                        LoadAssetMood(fileText);
                        break;
                    case "eras":
                        LoadAssetEra(fileText);
                        break;
                    case "resources":
                        LoadAssetResource(fileText);
                        break;
                    case "traits":
                        LoadAssetTrait(fileText);
                        break;
                    case "powers":
                        AssetModLoader.loadAssetPowers(fileText);
                        break;
                    case "buildings":
                        //building textures are not supported yet
                        AssetModLoader.loadAssetBuilding(fileText);
                        break;
                    default:
                        break;
                }
            }
        }

        public static void LoadAssetTileType(string tileData)
        {
            TileType tileAsset = JsonUtility.FromJson<TileType>(tileData);
            AssetManager.tiles.add(tileAsset);
        }

        public static void LoadAssetTopTileType(string tileData)
        {
            TopTileType tileAsset = JsonUtility.FromJson<TopTileType>(tileData);
            AssetManager.topTiles.add(tileAsset);
        }

        public static void LoadAssetTrait(string traitData)
        {
            ActorTrait traitAsset = JsonUtility.FromJson<ActorTrait>(traitData);
            AssetManager.traits.add(traitAsset);
        }

        public static void LoadAssetEra(string eraData)
        {
            EraAsset eraAsset = JsonUtility.FromJson<EraAsset>(eraData);
            AssetManager.era_library.add(eraAsset);
        }

        public static void LoadAssetResource(string resourceData)
        {
            ResourceAsset resourceAsset = JsonUtility.FromJson<ResourceAsset>(resourceData);
            AssetManager.resources.add(resourceAsset);
        }
        public static void LoadAssetMonth(string monthData)
        {
            MonthAsset monthAsset = JsonUtility.FromJson<MonthAsset>(monthData);
            AssetManager.months.add(monthAsset);
        }
        public static void LoadAssetMood(string moodData)
        {
            MoodAsset moodAsset = JsonUtility.FromJson<MoodAsset>(moodData);
            AssetManager.moods.add(moodAsset);
        }
        Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

    }

}