using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.Runtime.Internal.Transform;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using OpenAI_API;
using UnityEngine;

namespace SimpleGUI.Submods.MapSizes {
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class MapSizes : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.map.sizes";
        public const string pluginName = "MapSizes";
        public const string pluginVersion = "0.0.0.6";
        public static int mapSizeX = 4;
        public static int mapSizeY = 4;

        public static int smallIslands = 4;
        public static int randomShapes = 4;

        public bool showHideMapSizeWindow;
        public Rect mapSizeWindowRect;
        public static string filename = "picture";
        public void Awake()
        {

            Harmony harmony = new Harmony(pluginGuid);

            MethodInfo original = AccessTools.Method(typeof(MapBox), "setMapSize");
            MethodInfo patch = AccessTools.Method(typeof(MapSizes), "setMapSize_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Pre patch: MapBox.setMapSize");

            original = AccessTools.Method(typeof(MapBox), "finishMakingWorld");
            patch = AccessTools.Method(typeof(MapSizes), "finishMakingWorld_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log("Post patch: MapBox.finishMakingWorld");

            Debug.Log("MapSizes loaded");
        }
        public void OnGUI()
        {
            if(GUI.Button(new Rect(Screen.width - 120, 20, 120, 20), "MapSizes")) {
                showHideMapSizeWindow = !showHideMapSizeWindow;
            }
            if(showHideMapSizeWindow) {
                mapSizeWindowRect = GUILayout.Window(102, mapSizeWindowRect, new GUI.WindowFunction(mapSizesWindow), "Map Stuff", new GUILayoutOption[]
                    {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                    });

                if (showCustomTemplateWindow)
                {
                    customTemplateWindowRect = GUILayout.Window(43095, customTemplateWindowRect, customTemplateWindow, "Custom Template", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
                    customTemplateWindowRect.position = new Vector2(mapSizeWindowRect.x + mapSizeWindowRect.width, (mapSizeWindowRect.y));
                }
            }
            tooltipRect.x = Input.mousePosition.x + 5f;
            tooltipRect.y = (float)Screen.height - Input.mousePosition.y + 5f;
            GUI.Label(new Rect(tooltipRect), GUI.tooltip);
        }

        //this stuff shouldnt be necessary once AssetModLoader works
        public bool showCustomTemplateWindow;
        public Rect customTemplateWindowRect;

        public Vector2 scrollPositionTemplate;

        public void customTemplateWindow(int windowID)
        {
            if (GUILayout.Button("Set 'custom' template"))
            {
                SetupCustomTemplate();
            }
            scrollPositionTemplate = GUILayout.BeginScrollView(
      scrollPositionTemplate, GUILayout.Width(300f), GUILayout.Height(mapSizeWindowRect.height - 50f));

            //quick bypass for collection modified error
            List<KeyValuePair<string, int>> valueList = customValues.ToList();
            foreach (KeyValuePair<string, int> customVal in valueList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Button(customVal.Key);
                int v = customVal.Value;
                string s = GUILayout.TextField(v.ToString());
                int.TryParse(s, out int newV);
                if(newV != v)
                {
                    customValues[customVal.Key] = newV;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

        }

        //why did i decide to do it this way?????
        public Dictionary<string, int> customValues = new Dictionary<string, int>{
            { "force_height_to", 0 },
            { "freeze_mountains", 0 },
            { "special_anthill", 0 },
            { "special_checkerboard", 0 },
            { "special_cubicles", 0 },
            { "allow_edit_size", 0 },
            { "allow_edit_random_shapes", 0 },
            { "allow_edit_random_biomes", 0 },
            { "allow_edit_perlin_scale_stage_1", 0 },
            { "allow_edit_perlin_scale_stage_2", 0 },
            { "allow_edit_perlin_scale_stage_3", 0 },
            { "allow_edit_mountain_edges", 0 },
            { "allow_edit_random_vegetation", 0 },
            { "allow_edit_random_resources", 0 },
            { "allow_edit_center_lake", 0 },
            { "allow_edit_round_edges", 0 },
            { "allow_edit_square_edges", 0 },
            { "allow_edit_ring_effect", 0 },
            { "allow_edit_center_land", 0 },
            { "allow_edit_low_ground", 0 },
            { "allow_edit_high_ground", 0 },
            { "allow_edit_remove_mountains", 0 },
            { "allow_edit_cubicles", 0 },
            { "show_reset_button", 0 },
            //.values below
            { "perlin_scale_stage_1", 0 },
            { "perlin_scale_stage_2", 0 },
            { "perlin_scale_stage_3", 0 },
            { "main_perlin_noise_stage", 0 },
            { "perlin_noise_stage_2", 0 },
            { "perlin_noise_stage_3", 0 },
            { "square_edges", 0 },
            { "gradient_round_edges", 0 },
            { "add_center_gradient_land", 0 },
            { "center_gradient_mountains", 0 },
            { "add_center_lake", 0 },
            { "ring_effect", 0 },
            { "add_vegetation", 0 },
            { "add_resources", 0 },
            { "add_mountain_edges", 0 },
            { "random_biomes", 0 },
            { "random_shapes_amount", 0 },
            { "cubicle_size", 0 },
            { "remove_mountains", 0 },
            { "low_ground", 0 },
            { "high_ground", 0 }
        };

        public void SetupCustomTemplate()
        {
            MapGenTemplate customTemplate = new MapGenTemplate();
            customTemplate.id = "custom";
            bool test = true;
            customTemplate.force_height_to = customValues["force_height_to"];
            //boolParse is custom extension to quickly assign this copy paste garbage
            customTemplate.freeze_mountains = customValues["freeze_mountains"].boolParse();
            customTemplate.special_anthill = customValues["special_anthill"].boolParse();
            customTemplate.special_checkerboard = customValues["special_checkerboard"].boolParse();
            customTemplate.special_cubicles = customValues["special_cubicles"].boolParse();
            customTemplate.allow_edit_size = customValues["allow_edit_size"].boolParse();
            customTemplate.allow_edit_random_shapes = customValues["allow_edit_random_shapes"].boolParse();
            customTemplate.allow_edit_random_biomes = customValues["allow_edit_random_biomes"].boolParse();
            customTemplate.allow_edit_mountain_edges = customValues["allow_edit_mountain_edges"].boolParse();
            customTemplate.allow_edit_random_vegetation = customValues["allow_edit_random_vegetation"].boolParse();
            customTemplate.allow_edit_random_resources = customValues["allow_edit_random_resources"].boolParse();
            customTemplate.allow_edit_center_lake = customValues["allow_edit_center_lake"].boolParse();
            customTemplate.allow_edit_round_edges = customValues["allow_edit_round_edges"].boolParse();
            customTemplate.allow_edit_square_edges = customValues["allow_edit_square_edges"].boolParse();
            customTemplate.allow_edit_ring_effect = customValues["allow_edit_ring_effect"].boolParse();
            customTemplate.allow_edit_center_land = customValues["allow_edit_center_land"].boolParse();
            customTemplate.allow_edit_low_ground = customValues["allow_edit_low_ground"].boolParse();
            customTemplate.allow_edit_high_ground = customValues["allow_edit_high_ground"].boolParse();
            customTemplate.allow_edit_remove_mountains = customValues["allow_edit_remove_mountains"].boolParse();
            customTemplate.allow_edit_cubicles = customValues["allow_edit_cubicles"].boolParse();
            customTemplate.show_reset_button = customValues["show_reset_button"].boolParse();
            customTemplate.values.perlin_scale_stage_1 = customValues["perlin_scale_stage_1"];
            customTemplate.values.perlin_scale_stage_2 = customValues["perlin_scale_stage_2"];
            customTemplate.values.perlin_scale_stage_3 = customValues["perlin_scale_stage_3"];
            customTemplate.values.main_perlin_noise_stage = customValues["main_perlin_noise_stage"].boolParse();
            customTemplate.values.perlin_noise_stage_2 = customValues["perlin_noise_stage_2"].boolParse();
            customTemplate.values.perlin_noise_stage_3 = customValues["perlin_noise_stage_3"].boolParse();
            customTemplate.values.square_edges = customValues["square_edges"].boolParse();
            customTemplate.values.gradient_round_edges = customValues["gradient_round_edges"].boolParse();
            customTemplate.values.add_center_gradient_land = customValues["add_center_gradient_land"].boolParse();
            customTemplate.values.center_gradient_mountains = customValues["center_gradient_mountains"].boolParse();
            customTemplate.values.add_center_lake = customValues["add_center_lake"].boolParse();
            customTemplate.values.ring_effect = customValues["ring_effect"].boolParse();
            customTemplate.values.add_vegetation = customValues["add_vegetation"].boolParse();
            customTemplate.values.add_resources = customValues["add_resources"].boolParse();
            customTemplate.values.add_mountain_edges = customValues["add_mountain_edges"].boolParse();
            customTemplate.values.random_biomes = customValues["random_biomes"].boolParse();
            customTemplate.values.random_shapes_amount = customValues["random_shapes_amount"];
            customTemplate.values.cubicle_size = customValues["cubicle_size"];
            customTemplate.values.remove_mountains = customValues["remove_mountains"].boolParse();
            customTemplate.values.low_ground = customValues["low_ground"].boolParse();
            customTemplate.values.high_ground = customValues["high_ground"].boolParse();

            AssetManager.map_gen_templates.add(customTemplate);
        }

        public void SetCustomToExisting(string templateID)
        {
            MapGenTemplate templateInLib = AssetManager.map_gen_templates.get(templateToUse);
            if (templateInLib != null)
            {
                customValues["force_height_to"] = templateInLib.force_height_to;
                customValues["freeze_mountains"] = templateInLib.freeze_mountains.intParse();
                customValues["special_anthill"] = templateInLib.special_anthill.intParse();
                customValues["special_checkerboard"] = templateInLib.special_checkerboard.intParse();
                customValues["special_cubicles"] = templateInLib.special_cubicles.intParse();
                customValues["allow_edit_size"] = templateInLib.allow_edit_size.intParse();
                customValues["allow_edit_random_shapes"] = templateInLib.allow_edit_random_shapes.intParse();
                customValues["allow_edit_random_biomes"] = templateInLib.allow_edit_random_biomes.intParse();
                customValues["allow_edit_perlin_scale_stage_1"] = templateInLib.allow_edit_perlin_scale_stage_1.intParse();
                customValues["allow_edit_perlin_scale_stage_2"] = templateInLib.allow_edit_perlin_scale_stage_2.intParse();
                customValues["allow_edit_perlin_scale_stage_3"] = templateInLib.allow_edit_perlin_scale_stage_3.intParse();
                customValues["allow_edit_mountain_edges"] = templateInLib.allow_edit_mountain_edges.intParse();
                customValues["allow_edit_random_vegetation"] = templateInLib.allow_edit_random_vegetation.intParse();
                customValues["allow_edit_random_resources"] = templateInLib.allow_edit_random_resources.intParse() ;
                customValues["allow_edit_center_lake"] = templateInLib.allow_edit_center_lake.intParse();
                customValues["allow_edit_round_edges"] = templateInLib.allow_edit_round_edges.intParse();
                customValues["allow_edit_square_edges"] = templateInLib.allow_edit_square_edges.intParse();
                customValues["allow_edit_ring_effect"] = templateInLib.allow_edit_ring_effect.intParse();
                customValues["allow_edit_center_land"] = templateInLib.allow_edit_center_land.intParse();
                customValues["allow_edit_low_ground"] = templateInLib.allow_edit_low_ground.intParse();
                customValues["allow_edit_high_ground"] = templateInLib.allow_edit_high_ground.intParse();
                customValues["allow_edit_remove_mountains"] = templateInLib.allow_edit_remove_mountains.intParse();
                customValues["allow_edit_cubicles"] = templateInLib.allow_edit_cubicles.intParse();
                customValues["show_reset_button"] = templateInLib.show_reset_button.intParse() ;
                customValues["perlin_scale_stage_1"] = templateInLib.values.perlin_scale_stage_1;
                customValues["perlin_scale_stage_2"] = templateInLib.values.perlin_scale_stage_2;
                customValues["perlin_scale_stage_3"] = templateInLib.values.perlin_scale_stage_3;
                customValues["main_perlin_noise_stage"] = templateInLib.values.main_perlin_noise_stage.intParse();
                customValues["perlin_noise_stage_2"] = templateInLib.values.perlin_noise_stage_2.intParse();
                customValues["perlin_noise_stage_3"] = templateInLib.values.perlin_noise_stage_3.intParse();
                customValues["square_edges"] = templateInLib.values.square_edges.intParse();
                customValues["gradient_round_edges"] = templateInLib.values.gradient_round_edges.intParse();
                customValues["add_center_gradient_land"] = templateInLib.values.add_center_gradient_land.intParse();
                customValues["center_gradient_mountains"] = templateInLib.values.center_gradient_mountains.intParse();
                customValues["add_center_lake"] = templateInLib.values.add_center_lake.intParse();
                customValues["ring_effect"] = templateInLib.values.ring_effect.intParse();
                customValues["add_vegetation"] = templateInLib.values.add_vegetation.intParse();
                customValues["add_resources"] = templateInLib.values.add_resources.intParse();
                customValues["add_mountain_edges"] = templateInLib.values.add_mountain_edges.intParse();
                customValues["random_biomes"] = templateInLib.values.random_biomes.intParse();
                customValues["cubicle_size"] = templateInLib.values.cubicle_size;
                customValues["random_shapes_amount"] = templateInLib.values.random_shapes_amount;
                customValues["remove_mountains"] = templateInLib.values.remove_mountains.intParse();
                customValues["low_ground"] = templateInLib.values.low_ground.intParse();
                customValues["high_ground"] = templateInLib.values.high_ground.intParse();
            }
        }

        public static Rect tooltipRect = new Rect();

        public bool syncResize;

        public static string templateToUse = "";

        public void mapSizesWindow(int windowID)
        {
            if (GUILayout.Button("Generate new map"))
            {
                hasFinishedLoading = false;
                intentionallyChangingMapSize = true;
                WhyDoINeedThis.ChangeConfig(smallIslands, randomShapes);
                MapBox.instance.generateNewMap(false); // was clickgenerate
            }
            GUILayout.BeginHorizontal();
            GUILayout.Button("Map size x: " + mapSizeX.ToString());
            GUILayout.Button("Map size y: " + mapSizeY.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("-")) {
                mapSizeX--;
                if(mapSizeX < 0) {
                    mapSizeX = 0;
                }
            }
            if(GUILayout.Button("+")) {
                mapSizeX++;
            }
            if(GUILayout.Button("-")) {
                mapSizeY--;
                if(mapSizeY < 0) {
                    mapSizeY = 0;
                }
            }
            if(GUILayout.Button("+")) {
                mapSizeY++;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("Islands: " + smallIslands.ToString());
            GUILayout.Button("Shapes: " + randomShapes.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            int amountChange = 1;
            if(Input.GetKeyDown(KeyCode.LeftShift)) {
                amountChange = 5;
            }
            if(GUILayout.Button("-")) {
                smallIslands -= amountChange;
                if(smallIslands < 0) {
                    smallIslands = 0;
                }
            }
            if(GUILayout.Button("+")) {
                smallIslands += amountChange;
            }
            if(GUILayout.Button("-")) {
                randomShapes -= amountChange;
                if(randomShapes < 0) {
                    randomShapes = 0;
                }
            }
            if(GUILayout.Button("+")) {
                randomShapes += amountChange;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Template to use")) {
            }
            templateToUse = GUILayout.TextField(templateToUse);
            GUILayout.EndHorizontal();
            /* im so dumb, all of this is in the normal map gen window now...
             * only advantage here is running custom delegates
            if(GUILayout.Button("Setup 'custom' with '" + templateToUse + "'"))
            {
                SetCustomToExisting(templateToUse);
            }
            //create custom map template and edit it here?
            if (GUILayout.Button("Template settings"))
            {
                showCustomTemplateWindow = !showCustomTemplateWindow;
            }
            */
            GUI.DragWindow();
        }

        public GameObject testObjectFor2Worlds;
     
        public static bool waitingForLoading = false;
        public void Update()
        {
            if(waitingForLoading) {
                if(hasFinishedLoading) {
                    waitingForLoading = false;
                    hasFinishedLoading = false;
                    intentionallyChangingMapSize = false;
                }
            }
        }

        public static bool intentionallyChangingMapSize = false;
        public static bool setMapSize_Prefix(ref int pWidth, ref int pHeight)
        {
            if(intentionallyChangingMapSize == true) {
                pWidth = mapSizeX;
                pHeight = mapSizeY;
            }

            return true;
        }

        public static void finishMakingWorld_Postfix()
        {
            hasFinishedLoading = true;
            intentionallyChangingMapSize = false;
        }
        public static bool hasFinishedLoading = true; // check if mod wanted game to load and it finished
    }

    public class WhyDoINeedThis {
        public static void ChangeConfig(int pScale, int pShapes)
        {
            if(MapSizes.templateToUse == string.Empty || AssetManager.map_gen_templates.get(MapSizes.templateToUse) == null) {
                Config.current_map_template = "islands";
            }
            else {
                Config.current_map_template = MapSizes.templateToUse;
            }
            Config.customPerlinScale = pScale;
            Config.customRandomShapes = pShapes;
        }
    }

    public static class SimpleExtension
    {
        public static bool boolParse(this int input)
        {
            if (input == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static int intParse(this bool input)
        {
            if (input == false)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

    }
}

