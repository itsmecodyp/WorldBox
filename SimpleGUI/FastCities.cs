using UnityEngine;
using HarmonyLib;

namespace SimpleGUI
{
    class GuiFastCities
    {
        public Rect fastCitiesWindowRect;

        public void fastCitiesWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            if (GUILayout.Button("Config.fastCities: " + Config.fastCities.ToString()))
            {
                Config.fastCities = !Config.fastCities;
            }
            GUI.DragWindow();
        }

        public void fastCitiesWindowUpdate()
        {
            if (Config.fastCities)
            {
                foreach (City city in MapBox.instance.citiesList)
                {
                    CityData cityData = Reflection.GetField(city.GetType(), city, "data") as CityData;
                    foreach (ResourceAsset resource in AssetManager.resources.list)
                    {
                        cityData.storage.set(resource.id, 999);
                    }
                }
            }
            if (GuiMain.showWindowMinimizeButtons.Value)
            {
                string buttontext = "F";
                if (GuiMain.showHideFastCitiesConfig.Value)
                {
                    buttontext = "-";
                }
                if (GUI.Button(new Rect(fastCitiesWindowRect.x + fastCitiesWindowRect.width - 25f, fastCitiesWindowRect.y - 25, 25, 25), buttontext))
                {
                    GuiMain.showHideFastCitiesConfig.Value = !GuiMain.showHideFastCitiesConfig.Value;
                }
            }
            if (GuiMain.showHideFastCitiesConfig.Value)
            {
                fastCitiesWindowRect = GUILayout.Window(1003, fastCitiesWindowRect, new GUI.WindowFunction(fastCitiesWindow), "Fast Cities", new GUILayoutOption[]
                {
                GUILayout.MaxWidth(300f),
                GUILayout.MinWidth(200f)
                });
            }
        }

    }
}