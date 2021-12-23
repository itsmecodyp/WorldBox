using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using NCMS;

namespace MapSizes
{
    [ModEntry]
    class Main : MonoBehaviour
    {
        public const string pluginGuid = "cody.worldbox.map.sizes";
        public const string pluginName = "MapSizes";
        public const string pluginVersion = "0.0.0.3";
        public int mapSizeX = 4;
        public int mapSizeY = 4;
        public bool showHideMapSizeWindow;
        public Rect mapSizeWindowRect;
        public static string filename = "picture";
        public void Awake()
        {

            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(GeneratorTool), "applyTemplate");
            MethodInfo patch = AccessTools.Method(typeof(Main), "applyTemplate_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Pre patch: GeneratorTool.applyTemplate");
            Debug.Log("MapSizes loaded");
        }
        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 25, 120, 30));
            if (GUILayout.Button("Map Sizes"))
            {
                showHideMapSizeWindow = !showHideMapSizeWindow;
            }
            if (showHideMapSizeWindow)
            {
                mapSizeWindowRect = GUILayout.Window(102, mapSizeWindowRect, new GUI.WindowFunction(mapSizesWindow), "Map Stuff", new GUILayoutOption[]
                    {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                    });
            }
            GUILayout.EndArea();
        }
        public bool syncResize;
        public void mapSizesWindow(int windowID)
        {
			GUILayout.BeginHorizontal();
			GUILayout.Button("Map size x: " + mapSizeX.ToString());
			GUILayout.Button("Map size y: " + mapSizeY.ToString());
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("-"))
			{
				mapSizeX--;
                if (mapSizeX < 0)
                {
                    mapSizeX = 0;
                }
			}
			if (GUILayout.Button("+"))
			{
				mapSizeX++;
			}
			if (GUILayout.Button("-"))
			{
				mapSizeY--;
                if (mapSizeY < 0)
                {
                    mapSizeY = 0;
                }
            }
			if (GUILayout.Button("+"))
			{
				mapSizeY++;
			}
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Regenerate map"))
			{
                MapBox.instance.setMapSize(mapSizeX, mapSizeY);
                MapBox.instance.CallMethod("GenerateMap", new object[] { "custom" });
                MapBox.instance.finishMakingWorld();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Button("PicSizeX");
            pictureSizeX = (int)GUILayout.HorizontalSlider((float)pictureSizeX, 1f, 2000f);
            if (syncResize)
            {
                pictureSizeY = pictureSizeX;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Button("PicSizeY");
            pictureSizeY = (int)GUILayout.HorizontalSlider((float)pictureSizeY, 1f, 2000f);
            if (syncResize)
            {
                pictureSizeX = pictureSizeY;
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Sync buttons: " + syncResize.ToString()))
            {
                syncResize = !syncResize;
            }
            filename = GUILayout.TextField(filename);
            if (File.Exists(imagePath))
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;

            }
            if (GUILayout.Button("Regenerate " + filename + ".png"))
            {
                MapBox.instance.setMapSize(mapSizeX, mapSizeY);
                imageToMap(filename); // why does this run twice?
                startingPicture = true;
                MapBox.instance.CallMethod("GenerateMap", new object[] { "earth" });
                MapBox.instance.finishMakingWorld();
            }
            GUI.DragWindow();
        }
        public static string imagePath => Directory.GetCurrentDirectory() + "\\WorldBox_Data//images//" + filename + ".png";
        public static bool startingPicture;
        public static bool applyTemplate_Prefix(string pTexture, float pMod = 1f)
        {
            if (startingPicture && File.Exists(imagePath))
            {
                imageToMap(filename);
                startingPicture = false;
                return false;
            }
            return true;
        }

        // for later: public static Dictionary<WorldTile, Color> customTileColors = new Dictionary<WorldTile, Color>();
        public static int pictureSizeX = 100;
        public static int pictureSizeY = 100;
        public static void imageToMap(string imageName)
        {
            // ModTest.isMapLoadedFromPicture = true;
            // Texture2D texture2D = (Texture2D)Resources.Load("mapTemplates/earth"); // default earth picture
            string path = Directory.GetCurrentDirectory() + "\\WorldBox_Data//images//" + imageName + ".png"; // picture to convert
            Texture2D texture2D2 = null;
            byte[] data = File.ReadAllBytes(path);
            texture2D2 = new Texture2D(2, 2);
            texture2D2.LoadImage(data);
            TextureScale.Bilinear(texture2D2, pictureSizeX, pictureSizeY); 
            for (int i = 0; i < texture2D2.width; i++)
            {
                for (int j = 0; j < texture2D2.height; j++)
                {
                    WorldTile tile = MapBox.instance.GetTile(i, j);
                    if (tile != null)
                    {
                        int num2 = (int)((1f - texture2D2.GetPixel(i, j).g) * 255f); // change tile according to pixel color
                        tile.Height += num2;
                    }
                }
            }
            // tile.data.tileMinimapColor = texture2D2.GetPixel(i, j); // for when minimap showing pic is important
        }
    }
}
