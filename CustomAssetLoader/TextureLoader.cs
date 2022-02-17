using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace TextureLoader
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]

    public class TextureLoader_Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.simple.texture.loader";
        public const string pluginName = "Simple Texture Loader";
        public const string pluginVersion = "0.0.0.3";
        public void Awake()
        {
            HarmonyPatchSetup();
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 125, 120, 30));
            if (GUILayout.Button("Textures"))
            {
                showHideMainMenu = !showHideMainMenu;
            }
            if (showHideMainMenu)
            {
                mainWindowRect = GUILayout.Window(50001, mainWindowRect, new GUI.WindowFunction(mainWindow), "Textures", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            GUILayout.EndArea();
        }

        public void HarmonyPatchSetup()
        {
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(ItemGenerator), "init");
            MethodInfo patch = AccessTools.Method(typeof(TextureLoader_Main), "init_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Actor), "updateStats");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "updateStats_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("Post patch: Actor.updateStats");

            original = AccessTools.Method(typeof(ActorBase), "updateAnimation");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "updateAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Post patch: ActorBase.updateAnimation");

            original = AccessTools.Method(typeof(Building), "setSpriteMain");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "setSpriteMain_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("Post patch: Building.setSpriteMain");

            original = AccessTools.Method(typeof(Building), "checkSpriteConstructor");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "checkSpriteConstructor_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Pre patch: Building.checkShadow");


            original = AccessTools.Method(typeof(LocalizedTextManager), "loadLocalizedText");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "loadLocalizedText_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("Post patch: LocalizedTextManager.loadLocalizedText");

            original = AccessTools.Method(typeof(Building), "setSpriteRuin");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "setSpriteRuin_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("setSpriteRuin_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(TilemapExtended), "setTile");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "setTile_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("setTile_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(PowerButton), "OnEnable");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "OnEnable_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("OnEnable_Postfix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(LoadingScreen), "setupBg");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "setupBg_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("setupBg_Postfix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(UnitAvatarLoader), "load");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "load_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("load_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(TraitButton), "load");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "tload_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("tload_Postfix");


            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Actor), "checkSpriteRenderer");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "checkSpriteRenderer_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("checkSpriteRenderer_Postfix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Actor), "forceAnimation");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "forceAnimation_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("forceAnimation_Prefix");

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(ResourceAsset), "getSprite");
            patch = AccessTools.Method(typeof(TextureLoader_Main), "getSprite_Postfix");
            harmony.Patch(original,null, new HarmonyMethod(patch));
            Debug.Log("getSprite_Postfix");

        }

        public static List<Building> spriteReplacedBuildings = new List<Building>();
        public static Dictionary<string, Sprite> customTextures;
        public static List<Sprite> customSprites = new List<Sprite>();
        public static bool useCustomBuildingTextures = true;
        public static bool useCustomTextureHeads = true;
        public static bool useCustomTextureWeapons = true;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);
        public bool showHideMainMenu;
        public static Dictionary<BaseSimObject, bool> objectHasTexture = new Dictionary<BaseSimObject, bool>();

        public static List<Tile> spriteReplacedTiles = new List<Tile>();
        public static List<WorldTile> spriteReplacedWorldTiles = new List<WorldTile>();

        /*
        public static bool addVariation_Prefix(string pSpriteName, TileLib __instance)
        {
            Tile tile = (Tile)Resources.Load("tilemap/" + pSpriteName, typeof(Tile));
            List<Tile> tiles = Reflection.GetField(__instance.GetType(), __instance, "tiles") as List<Tile>;
            Dictionary<string, Tile> tiles_dict = Reflection.GetField(__instance.GetType(), __instance, "tiles_dict") as Dictionary<string, Tile>;

            tiles_dict.Add(pSpriteName, tile);  
            tiles.Add(tile);
        }
        */

        public static void getSprite_Postfix(ResourceAsset __instance)
        {
            List<string> resourceVariations = variationsOfCustomTexture(__instance.icon);

            if (resourceVariations.Count >= 1)
            {
                if (__instance != null)
                {
                    string newTraitIcon = resourceVariations.GetRandom();

                    customTextures.TryGetValue(newTraitIcon, out Sprite replacement);

                    Sprite sprite = replacement;

                    __instance.sprite = sprite;
                }
            }
            else
            {
                if (__instance.sprite == null)
                {
                    __instance.sprite = (Sprite)Resources.Load("ui/Icons/" + __instance.icon, typeof(Sprite));
                }
            }
        }
        public static void tload_Postfix(string pTrait, TraitButton __instance)
        {
            ActorTrait loadedTrait = AssetManager.traits.get(pTrait);
            Debug.Log("checking trait id");
            Debug.Log("trait: " + loadedTrait.id);
            List<string> traitIconVariations = variationsOfCustomTexture(loadedTrait.icon);

            if (traitIconVariations.Count >= 1)
            {
                if (loadedTrait != null)
                {
                    string newTraitIcon = traitIconVariations.GetRandom();

                    customTextures.TryGetValue(newTraitIcon, out Sprite replacement);

                    Sprite sprite = replacement;

                    __instance.GetComponent<Image>().sprite = sprite;
                }
            }
            else
            {
                Sprite sprite = (Sprite)Resources.Load("ui/Icons/" + loadedTrait.icon.Replace(" ", ""), typeof(Sprite)); // replace
                __instance.GetComponent<Image>().sprite = sprite;
            }
        }

        public static void setupBg_Postfix(LoadingScreen __instance)
        {
            List<string> loadingScreenBackgroundVariationsAvailable = variationsOfCustomTexture("background");
            if (loadingScreenBackgroundVariationsAvailable != null && loadingScreenBackgroundVariationsAvailable.Count >= 1)
            {
                string newBackground = loadingScreenBackgroundVariationsAvailable.GetRandom();
                customTextures.TryGetValue(newBackground, out Sprite replacement);
                __instance.background.sprite = replacement;
            }

            List<string> barVariationsAvailable = variationsOfCustomTexture("loadingbar");
            if (barVariationsAvailable != null && barVariationsAvailable.Count >= 1)
            {
                string newBar = barVariationsAvailable.GetRandom();
                customTextures.TryGetValue(newBar, out Sprite replacement);
                __instance.bar.sprite = replacement;
            }
            //Camera.main.backgroundColor = TileType.get("deep_ocean").color;

        }

        public static void OnEnable_Postfix(PowerButton __instance)
        {
            // Debug.Log(__instance.gameObject.name);
            //Debug.Log("CTCount: " + customTextures.Count);
            List<string> buttonVariationsAvailable = variationsOfCustomTexture(__instance.gameObject.name);
            if (buttonVariationsAvailable != null && buttonVariationsAvailable.Count >= 1)
            {
                string newButtonSprite = buttonVariationsAvailable.GetRandom();
                customTextures.TryGetValue(newButtonSprite, out Sprite replacement);
                __instance.icon.sprite = replacement;
            }
        }

        public static bool tileSkinsEnabled = true;
        //Better off done from TileLib and related, this method skips game logic and creates buggy tile textures
        // worldtilemap.getVariation sets edge variations
        public static bool setTile_Prefix(WorldTile pWorldTile, Vector3Int pVec, Tile pGraphic, TilemapExtended __instance)
        {
            if (tileSkinsEnabled)
            {
                string tiletype = pWorldTile.Type.id;
                pVec.z = 0;
                //Debug.Log("tile type: " + tiletype);
                List<string> tileVariationsAvailable = variationsOfCustomTexture(tiletype);

                if (tileVariationsAvailable != null && tileVariationsAvailable.Count >= 1)
                {

                    List<Vector3Int> vec = Reflection.GetField(__instance.GetType(), __instance, "vec") as List<Vector3Int>;
                    List<Tile> tiles = Reflection.GetField(__instance.GetType(), __instance, "tiles") as List<Tile>;
                    tiletype = tileVariationsAvailable.GetRandom(); // new name

                    customTextures.TryGetValue(tiletype, out Sprite replacement);

                    if (replacement == null)
                    {
                        return false;
                    }
                    Tile curGraphics = pGraphic;
                    if (curGraphics != null)
                    {
                        curGraphics.sprite = replacement;
                    }
                    vec.Add(pVec);
                    tiles.Add(pGraphic);
                    return false;
                }
                else
                {
                    {
                        List<Vector3Int> vec = Reflection.GetField(__instance.GetType(), __instance, "vec") as List<Vector3Int>;
                        List<Tile> tiles = Reflection.GetField(__instance.GetType(), __instance, "tiles") as List<Tile>;
                        Tile curGraphics = Reflection.GetField(pWorldTile.GetType(), pWorldTile, "curGraphics") as Tile;
                        curGraphics = pGraphic;
                        vec.Add(pVec);
                        tiles.Add(pGraphic);
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        public TileType selectedCustomType;
        public bool drawCustomTypeOnEmptyClick;
        public void mainWindow(int windowID)
        {
            string[] textureSets = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\WorldBox_Data//");
            if (textureSets != null && textureSets.Length >= 1)
            {
                for (int i = 0; i < textureSets.Length; i++)
                {
                    string set = textureSets[i].Replace(Directory.GetCurrentDirectory() + "\\WorldBox_Data//", "");
                    if (set.ToLower().Contains("assets") == false && set.ToLower().Contains("resources") == false && set.ToLower().Contains("plugins") == false && set.ToLower().Contains("managed") == false)
                    {
                        if (GUILayout.Button("Textures: " + set))
                        {
                            customTextures = new Dictionary<string, Sprite>();
                            loadCustomTextures(set); // change entire sets by folder 
                        }
                    }
                }
            }
            
            if (CustomAssetLoader.AssetLoader_Main.customSoundControllers != null && CustomAssetLoader.AssetLoader_Main.customSoundControllers.Count >= 1)
            {
                if (GUILayout.Button("Change_NF"))
                {
                    CustomAssetLoader.AssetLoader_Main.PlaySound("Change_NF");
                }
                if (GUILayout.Button("Change_NF.wav"))
                {
                    CustomAssetLoader.AssetLoader_Main.PlaySound("Change_NF.wav");
                }
                if (GUILayout.Button("Test sounds2"))
                {
                    if (Directory.Exists(Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//custom//" + "sounds" + "//"))
                    {
                        FileInfo[] files = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\WorldBox_Data//assets//custom//" + "sounds" + "//").GetFiles("*.wav");

                        for (int i = 0; i < files.Count<FileInfo>(); i++)
                        {
                            StartCoroutine(CustomAssetLoader.AssetLoader_Main.LoadAudio(files[i].FullName));

                        }
                    }
                }
            }

            if (CustomAssetLoader.AssetLoader_Main.addedTileTypes != null && CustomAssetLoader.AssetLoader_Main.addedTileTypes.Count >= 1)
            {
                foreach (KeyValuePair<string, TileType> customTileType in CustomAssetLoader.AssetLoader_Main.addedTileTypes)
                {
                    if (GUILayout.Button(customTileType.Key))
                    {
                        selectedCustomType = customTileType.Value;
                    }
                }
                if (GUILayout.Button("drawCustomTypeInConstructionMenu"))
                {
                   drawCustomTypeOnEmptyClick = !drawCustomTypeOnEmptyClick;
                }
            }
                GUI.DragWindow();
        }

        public void Update()
        {
            if (drawCustomTypeOnEmptyClick)
            {
                if (selectedCustomType != null)
                {
                    if (Input.GetMouseButtonDown(0) && MapBox.instance.getMouseTilePos() != null)
                    {
                        if (MapBox.instance.getMouseTilePos().zone.tileTypes.ContainsKey(selectedCustomType) == false)
                        {
                            MapBox.instance.getMouseTilePos().zone.tileTypes.Add(selectedCustomType, new HashSet<WorldTile>() as HashSetWorldTile);
                        }
                        MapAction.terraformMain(MapBox.instance.getMouseTilePos(), selectedCustomType, AssetManager.terraform.get("flash"));

                    }
                }
            }
        }

        public static Sprite LoadSprite(string path, float offsetx, float offsety)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.anisoLevel = 0;
                texture2D.LoadImage(data);
                texture2D.filterMode = FilterMode.Point;
                // TextureScale.Point(texture2D, resizeX, resizeY);
                Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(offsetx, offsety), 1f);
                return sprite;
            }
            return null;
        }
        public static List<string> variationsOfCustomTexture(string originalName)
        {
            if (customTextures == null)
            {
                // automatic one time loading
                readTileColorFile();
                customTextures = new Dictionary<string, Sprite>();
                loadCustomTextures("images"); // change entire sets by folder 
            }
            List<string> variations = new List<string>();
            foreach (string key in customTextures.Keys)
            {
                if (key.StartsWith(originalName)) // STARTSWITH NOT EQUALS
                {
                    if (!key.Contains("head"))
                    {
                        variations.Add(key);
                    }
                }
            }
            return variations;
        }
        public static void detectCustomBuildingTexture(Building target)
        {
            BuildingData data = Reflection.GetField(target.GetType(), target, "data") as BuildingData;
            BuildingAsset stats = Reflection.GetField(target.GetType(), target, "stats") as BuildingAsset;
            SpriteRenderer spriteRendererBody = Reflection.GetField(target.GetType(), target, "spriteRenderer") as SpriteRenderer;
            SpriteAnimation targetAnimator = Reflection.GetField(target.GetType(), target, "spriteAnimation") as SpriteAnimation;
            if (!data.underConstruction)
            {
                string name = stats.id;
                List<string> variationsAvailable = variationsOfCustomTexture(stats.id);
                if (variationsAvailable != null && variationsAvailable.Count >= 1)
                {
                    name = variationsAvailable.GetRandom();
                }
                customTextures.TryGetValue(name, out Sprite replacement);
                if (replacement != null)
                {
                    //target.hasHadBodyTextureChange = true; // required likely
                    spriteRendererBody.sprite = replacement;
                    spriteReplacedBuildings.Add(target);
                    List<Sprite> newAnimationOfSingles = new List<Sprite>();
                    foreach (Sprite sprite in targetAnimator.frames)
                    {
                        newAnimationOfSingles.Add(replacement);
                    }
                    if (newAnimationOfSingles != null && newAnimationOfSingles.Count >= 1)
                    {
                        targetAnimator.CallMethod("setFrames", new object[] { newAnimationOfSingles.ToArray() });
                        targetAnimator.frames = newAnimationOfSingles.ToArray();
                    }

                    //Debug.Log("Replaced building: " + stats.id + " sprite with custom texture");
                }
            }
        }
        public static Dictionary<Actor, bool> spriteActors = new Dictionary<Actor, bool>();
        public static void updateSpecialTraitEffects_Postfix(Actor __instance)
        {

        }

        public static bool setHeadSprite_Prefix(int pID, ActorBase __instance)
        {
            if (professionReplacedActors.Contains(__instance) || traitReplacedActors.Contains(__instance) || bodyReplacedActors.Contains(__instance))
            {
                return false;
            }
            return true;
        }
        public static List<Actor> professionReplacedActors = new List<Actor>();
        public static List<Actor> traitReplacedActors = new List<Actor>();
        public static List<Actor> bodyReplacedActors = new List<Actor>();

        public static Dictionary<Actor, Sprite> professionReplacedActorsAndSprite = new Dictionary<Actor, Sprite>();
        public static Dictionary<Actor, Sprite> traitReplacedActorsAndSprite = new Dictionary<Actor, Sprite>();
        public static Dictionary<Actor, Sprite> bodyReplacedActorsAndSprite = new Dictionary<Actor, Sprite>();

        
        public static void checkSpriteRenderer_Postfix(Actor __instance)
        {
            detectCustomActorTexture(__instance);
        }
        
        public static bool forceAnimation_Prefix()
        {
                return false;
            // needs morew work, this is where the unit bounce comes from
        }

        public static void detectCustomActorTexture(Actor target)
        {
            ActorStatus data = target.data;
            if (target.data.alive)
            {
                SpriteRenderer spriteRendererBody = Reflection.GetField(target.GetType(), target, "spriteRenderer") as SpriteRenderer;

                // this needs cleaned up incase something is in multiple
             
                string name = target.stats.id;
                if (bodyReplacedActors.Contains(target) == false)
                {
                    List<string> bodyVariationsAvailable = variationsOfCustomTexture(name);
                    if (bodyVariationsAvailable != null && bodyVariationsAvailable.Count >= 1)
                    {
                        name = bodyVariationsAvailable.GetRandom();
                        customTextures.TryGetValue(name, out Sprite replacement);
                        if (replacement == null)
                        {
                        }
                        if (replacement != null)
                        {
                            spriteRendererBody.sprite = replacement;
                            bodyReplacedActors.Add(target);
                            bodyReplacedActorsAndSprite.Add(target, replacement);
                            // Debug.Log("Replaced actor " + target.stats.id + " sprite: " + name);
                        }
                    }
                }
                if (professionReplacedActors.Contains(target) == false)
                {
                    if (data.profession != UnitProfession.Null)
                    {
                        string profession = data.profession.ToString().ToLower();
                        List<string> professionVariationsAvailable = variationsOfCustomTexture(profession + "_" + name);
                        if (professionVariationsAvailable != null && professionVariationsAvailable.Count >= 1)
                        {
                            name = professionVariationsAvailable.GetRandom();
                            customTextures.TryGetValue(name, out Sprite replacement);
                            if (replacement == null)
                            {
                            }
                            if (replacement != null)
                            {
                                spriteRendererBody.sprite = replacement;
                                professionReplacedActors.Add(target);
                                professionReplacedActorsAndSprite.Add(target, replacement);

                                // Debug.Log("Replaced actor " + target.stats.id + " sprite: " + name);
                            }
                        }
                    }
                }
                if (traitReplacedActors.Contains(target) == false)
                {
                    if (data.traits != null && data.traits.Count >= 1)
                    {

                        foreach (string trait in data.traits)
                        {
                            if (CustomAssetLoader.AssetLoader_Main.addedTraits.Contains(trait)) // detect custom traits
                            {
                                ActorTrait foundTrait = CustomAssetLoader.AssetLoader_Main.addedActorTraits.Find(y => y.id == trait); // get actual trait
                                if (foundTrait != null)
                                {
                                    if (foundTrait.icon.Contains("skin"))
                                    {
                                        List<string> bodyVariationsAvailableFromIcon = variationsOfCustomTexture(foundTrait.icon);
                                        if (bodyVariationsAvailableFromIcon != null && bodyVariationsAvailableFromIcon.Count >= 1)
                                        {
                                            name = bodyVariationsAvailableFromIcon.GetRandom();
                                            customTextures.TryGetValue(name, out Sprite replacement);
                                            if (replacement == null)
                                            {
                                            }
                                            if (replacement != null)
                                            {
                                                traitReplacedActors.Add(target);
                                                traitReplacedActorsAndSprite.Add(target, replacement);

                                                spriteRendererBody.sprite = replacement;
                                                // Debug.Log("Replaced actor " + target.stats.id + " sprite: " + name);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        public static void upgradeBulding_Postfix(Building __instance)
        {
            if (useCustomBuildingTextures && customTextures != null)
            {
                detectCustomBuildingTexture(__instance);
            }
        }

        // ActorBase patch: cancel animations, forcing the 1 replaced sprite to always remain
        public static bool updateAnimation_Prefix(float pElapsed, bool pForce, ActorBase __instance)
        {
            if (bodyReplacedActors.Contains(__instance) || traitReplacedActors.Contains(__instance) || professionReplacedActors.Contains(__instance))
            {
                return false;
            }
            return true;
        }

        // Actor patch: detect/replace sprite every time the actors sprite would otherwise update
        public static void updateStats_Postfix(Actor __instance)
        {
            detectCustomActorTexture(__instance);
        }

        // LocalizedText patch: add text tip for the custom trait
        public static void loadLocalizedText_Postfix(string pLocaleID)
        {
            string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
            Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
            if (language == "en")
            {
                localizedText.Add("trait_spriteReplaced", "Sprite Replaced");
                localizedText.Add("trait_spriteReplaced_info", "This unit has a custom sprite");
            }
            if (language == "es")
            {
                localizedText.Add("trait_spriteReplaced", "Sprite sustituido");
                localizedText.Add("trait_spriteReplaced_info", "Esta unidad tiene un sprite personalizado");
            }
        }

        // Traits init patch: add a custom trait
        public static void init_Postfix()
        {
            AssetManager.traits.add(new ActorTrait { id = "spriteReplaced", icon = "iconVermin" });
            //Debug.Log("Added trait: spriteReplaced");
        }

        // Building setSprite patch: replacing the building texture
        public static void setSpriteMain_Postfix(bool pTween, Building __instance)
        {
            detectCustomBuildingTexture(__instance);
        }
        // used to be checkShadow
        public static bool checkSpriteConstructor_Prefix(Building __instance)
        {
            BuildingData data = Reflection.GetField(__instance.GetType(), __instance, "data") as BuildingData;

            if (!data.underConstruction && spriteReplacedBuildings.Contains(__instance)) //  
            {
                Dictionary<string, Sprite> dict_sprites = Reflection.GetField(BuildingAnimationLoader.shadows.GetType(), BuildingAnimationLoader.shadows, "dict_sprites") as Dictionary<string, Sprite>;
                SpriteRenderer spriteRenderer = Reflection.GetField(__instance.GetType(), __instance, "spriteRenderer") as SpriteRenderer;
                if (!dict_sprites.ContainsKey(spriteRenderer.sprite.name))
                {
                    Sprite newTest = spriteRenderer.sprite;
                    dict_sprites.Add(spriteRenderer.sprite.name, newTest);
                }
                return true;
            }
            return true;
        }

        // error fix, need to detect which buildings are actually sprite replaced

        public static bool setSpriteRuin_Prefix(Building __instance)
        {
            if (spriteReplacedBuildings.Contains(__instance))
            {
                return false;
                BuildingAsset stats = Reflection.GetField(__instance.GetType(), __instance, "stats") as BuildingAsset;
                List<TileZone> zones = Reflection.GetField(__instance.GetType(), __instance, "zones") as List<TileZone>;
                if (string.IsNullOrEmpty(stats.ruins))
                {
                    return false;
                }
                __instance.CallMethod("clearZoneBuilding");
                zones.Clear();
                __instance.CallMethod("clearTiles");
                __instance.CallMethod("fillTiles");
            }
            return true;
        }

        // fixes error when trying to load avatar for skinned unit
        public static bool load_Prefix(Actor pActor, UnitAvatarLoader __instance)
        {
            if (bodyReplacedActors.Contains(pActor) || traitReplacedActors.Contains(pActor) || professionReplacedActors.Contains(pActor))
            {
                while (__instance.transform.childCount > 0)
                {
                    Transform child = __instance.transform.GetChild(0);
                    child.SetParent(null);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                SpriteRenderer spriteRendererBody = Reflection.GetField(pActor.GetType(), pActor, "spriteRenderer") as SpriteRenderer;
                __instance.transform.localScale = new Vector3(pActor.stats.inspectAvatarScale * __instance.avatarSize, pActor.stats.inspectAvatarScale * __instance.avatarSize, pActor.stats.inspectAvatarScale);
                __instance.CallMethod("showSpritePart", new object[] { spriteRendererBody.sprite, pActor, new Vector3(0f, 0f) });
                if (pActor.isItemRendered())
                {
                    AnimationFrameData animationFrameData2 = pActor.getAnimationFrameData();
                    Vector3 pPos2 = default(Vector3);
                    if (animationFrameData2 != null)
                    {
                        Vector2 posItem = (Vector2)Reflection.GetField(animationFrameData2.GetType(), animationFrameData2, "posItem");
                        if (posItem != null)
                        {
                            pPos2.x = posItem.x;
                            pPos2.y = posItem.y;
                            string textureToRenderInHand = pActor.getTextureToRenderInHand();
                            __instance.CallMethod("showSpritePart", new object[] { ActorAnimationLoader.getItem(textureToRenderInHand), pActor, pPos2 });
                        }
                    }
                }
                return false;
            }
            return true;
        }




        public static void loadCustomTextures(string parentFolder)
        {
            string path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//" + parentFolder + "//";
            string fullPath;
            if (!Directory.Exists(path))
            {
                return;
            }
            foreach (string directoryOfSet in Directory.GetDirectories(path))
            {
                if (directoryOfSet.Contains("building"))
                {
                    fullPath = path + "building//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading building textures");
                        foreach (string newTexturePath in Directory.GetFiles(fullPath))
                        {
                            Sprite buildingSprite = LoadSprite(newTexturePath, 0.5f, 0f); // x is usually 
                            buildingSprite.name = newTexturePath.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(buildingSprite.name))
                            {
                                customTextures.Add(buildingSprite.name, buildingSprite);
                            }
                            // Debug.Log("Added " + buildingSprite.name + " to custom texture library");
                        }
                    }

                }
                if (directoryOfSet.Contains("body"))
                {
                    fullPath = path + "body//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading body textures");

                        foreach (string newTexturePath in Directory.GetFiles(fullPath))
                        {
                            Sprite bodySprite = LoadSprite(newTexturePath, 0.5f, 0f);
                            bodySprite.name = newTexturePath.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(bodySprite.name))
                            {
                                customTextures.Add(bodySprite.name, bodySprite);
                            }
                            // Debug.Log("Added " + bodySprite.name + " to custom texture library");
                        }
                    }
                }
                if (directoryOfSet.Contains("tile"))
                {
                    fullPath = path + "tile//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading tile textures");
                        foreach (string newTexturePath in Directory.GetFiles(fullPath))
                        {
                            Sprite bodySprite = LoadSprite(newTexturePath, 0.5f, 0f);
                            bodySprite.name = newTexturePath.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(bodySprite.name))
                            {
                                customTextures.Add(bodySprite.name, bodySprite);
                            }
                            // Debug.Log("Added " + bodySprite.name + " to custom texture library");
                        }
                    }
                }
                if (directoryOfSet.Contains("powerbutton"))
                {
                    fullPath = path + "powerbutton//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading powerbutton textures");
                        foreach (string newTexturePath in Directory.GetFiles(fullPath))
                        {
                            Sprite bodySprite = LoadSprite(newTexturePath, 0.5f, 0f);
                            bodySprite.name = newTexturePath.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(bodySprite.name))
                            {
                                customTextures.Add(bodySprite.name, bodySprite);
                            }
                            // Debug.Log("Added " + bodySprite.name + " to custom texture library");
                        }
                    }
                }
                if (directoryOfSet.Contains("loadingscreen"))
                {
                    fullPath = path + "loadingscreen//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading loadingscreen textures");
                        foreach (string newTexturePath in Directory.GetFiles(fullPath))
                        {
                            Sprite bodySprite = LoadSprite(newTexturePath, 0.5f, 0f);
                            bodySprite.name = newTexturePath.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(bodySprite.name))
                            {
                                customTextures.Add(bodySprite.name, bodySprite);
                            }
                            // Debug.Log("Added " + bodySprite.name + " to custom texture library");
                        }
                    }
                }
                if (directoryOfSet.Contains("trait"))
                {
                    fullPath = path + "trait//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading trait icon textures");
                        foreach (string newTexturePath in Directory.GetFiles(fullPath))
                        {
                            Sprite traitIcon = LoadSprite(newTexturePath, 0.5f, 0f);
                            traitIcon.name = newTexturePath.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(traitIcon.name))
                            {
                                customTextures.Add(traitIcon.name, traitIcon);
                            }
                            Debug.Log("Added " + traitIcon.name + " to custom texture library");
                        }
                    }
                }
                if (directoryOfSet.Contains("resource"))
                {
                    fullPath = path + "resource//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading resource icon textures");
                        foreach (string newTexturePath in Directory.GetFiles(fullPath))
                        {
                            Sprite resourceIcon = LoadSprite(newTexturePath, 0.5f, 0f);
                            resourceIcon.name = newTexturePath.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(resourceIcon.name))
                            {
                                customTextures.Add(resourceIcon.name, resourceIcon);
                            }
                            Debug.Log("Added " + resourceIcon.name + " to custom texture library");
                        }
                    }
                }

                // Supports weapons almost 100%, heads are difficult to get right and should be left out
                #region Head & Weapon
                /*
                if (directoryOfSet.Contains("head"))
                {
                    fullPath = path + "head//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading head textures");
                        foreach (string newTexturePath1 in Directory.GetFiles(fullPath))
                        {
                            Sprite headSprite = LoadSprite(newTexturePath1, 0.5f, 0f);

                            headSprite.name = newTexturePath1.Replace(fullPath, "").Replace(".png", "");
                            if (!customTextures.ContainsKey(headSprite.name))
                            {
                                customTextures.Add(headSprite.name, headSprite);
                            }
                            Debug.Log("Added " + headSprite.name + " to custom texture library");

                        }
                    }
                }
                if (directoryOfSet.Contains("weapon"))
                {
                    fullPath = path + "weapon//custom//";
                    if (Directory.Exists(fullPath))
                    {
                        Debug.Log("Started loading weapon textures");
                        foreach (string newTexturePath2 in Directory.GetFiles(fullPath))
                        {
                            float xpos = 0.5f;
                            if (fullPath.Contains("bow"))
                            {
                                xpos = 0.3f;
                            }
                            Sprite weaponSprite = LoadSprite(newTexturePath2, xpos, 0.5f);
                            weaponSprite.name = newTexturePath2.Replace(fullPath, "").Replace(".png", "");

                            if (!customTextures.ContainsKey(weaponSprite.name))
                            {
                                customTextures.Add(weaponSprite.name, weaponSprite);
                            }
                            Debug.Log("Added " + weaponSprite.name + " to custom texture library");

                        }
                    }
                }
                */
                #endregion
                // Change minimap colors for each tile, no one used it
                //readTileColorFile(); 

            }
        }

        public static string ColorToHex(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        }

        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return new Color32(r, g, b, byte.MaxValue);
        }

        public static void readTileColorFile()
        {
            string path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//images//tiles.txt";
            bool flag = File.Exists(path);
            if (flag)
            {
                string text = File.ReadAllText(path);
                string[] array = text.Split(new char[]
                {
                "\n"[0]
                });
                for (int i = 0; i < array.Length; i++)
                {
                    string pID = array[i].Split(new char[]
                    {
                    ":"[0]
                    })[0];
                    string hex = array[i].Split(new char[]
                    {
                    ":"[0]
                    })[1];
                    TopTileType tileType = AssetManager.topTiles.get(pID);
                    bool flag2 = tileType != null;
                    if (flag2)
                    {
                        tileType.color = HexToColor(hex);
                    }
                }
            }
        }

    }




}

