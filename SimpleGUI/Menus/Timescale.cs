using UnityEngine;

#pragma warning disable CS0649

namespace SimplerGUI.Menus
{
    class GuiTimescale
    {
        public void timescaleWindow(int windowID)
        {
            GuiMain.SetWindowInUse(windowID);
            GUI.backgroundColor = Color.grey;
            if (GUILayout.Button("Set to 1 / Reset"))
            {
                Config.timeScale = 1f;
            }
            if (GUILayout.Button("Set to 5"))
            {
                Config.timeScale = 5f;
            }
            if (GUILayout.Button("Set to 10"))
            {
                Config.timeScale = 10f;
            }
            if (GUILayout.Button("Set to 15"))
            {
                Config.timeScale = 15f;
            }
            if (GUILayout.Button("Set to 25"))
            {
                Config.timeScale = 25f;

            }
            if (GUILayout.Button("Set to 50"))
            {
                Config.timeScale = 50f;
            }
            if (GUILayout.Button("Set to 100"))
            {
                Config.timeScale = 100f;
            }
            if (GUILayout.Button("Set to custom input") && float.TryParse(configTimescaleInput, out float newTime))
            {
                Config.timeScale = newTime;
            }
            configTimescaleInput = GUILayout.TextField(configTimescaleInput);
            GUI.DragWindow();
        }

        public void timescaleWindowUpdate()
        {
            if (SimpleSettings.showHideTimescaleWindowConfig)
            {
                timescaleWindowRect = GUILayout.Window(1002, timescaleWindowRect, timescaleWindow, "Timescale", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
            }
        }

        public string configTimescaleInput = "1";
        public bool showHideTimescaleWindow;
        public Rect timescaleWindowRect = new Rect(126f, 1f, 1f, 1f);
    }
}
