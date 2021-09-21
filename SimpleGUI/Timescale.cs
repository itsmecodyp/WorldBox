using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleGUI
{
    class GuiTimescale
    {
		public void timescaleWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
			if (testNormalTimescale)
			{
				GUI.backgroundColor = Color.green;
			}
			else
			{
				GUI.backgroundColor = Color.red;
			}
			if (GUILayout.Button("Test: normal timescale instead"))
			{
				testNormalTimescale = !testNormalTimescale;
			}
			GUI.backgroundColor = Color.grey;
			if (GUILayout.Button("Set to 1 / Reset"))
			{
				if (testNormalTimescale)
				{
					Time.timeScale = 1f;
				}
				else
				{
					Config.timeScale = 1f;
				}
			}
			if (GUILayout.Button("Set to 5"))
			{
				if (testNormalTimescale)
				{
					Time.timeScale = 5f;
				}
				else
				{
					Config.timeScale = 5f;
				}
			}
			if (GUILayout.Button("Set to 10"))
			{
				if (testNormalTimescale)
				{
					Time.timeScale = 10f;
				}
				else
				{
					Config.timeScale = 10f;
				}
			}
			if (GUILayout.Button("Set to 15"))
			{
				if (testNormalTimescale)
				{
					Time.timeScale = 15f;
				}
				else
				{
					Config.timeScale = 15f;
				}
			}
			if (GUILayout.Button("Set to 25"))
			{
				if (testNormalTimescale)
				{
					Time.timeScale = 25f;
				}
				else
				{
					Config.timeScale = 25f;
				}
			}
			if (GUILayout.Button("Set to 50"))
			{
				if (testNormalTimescale)
				{
					Time.timeScale = 50f;
				}
				else
				{
					Config.timeScale = 50f;
				}
			}
			if (GUILayout.Button("Set to 100"))
			{
				if (testNormalTimescale)
				{
					Time.timeScale = 100f;
				}
				else
				{
					Config.timeScale = 100f;
				}
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
			if (GuiMain.showWindowMinimizeButtons.Value)
			{
				string buttontext = "T";
				if (GuiMain.showHideTimescaleWindowConfig.Value)
				{
					buttontext = "-";
				}
				if (GUI.Button(new Rect(timescaleWindowRect.x + timescaleWindowRect.width - 25f, timescaleWindowRect.y - 25, 25, 25), buttontext))
				{
					GuiMain.showHideTimescaleWindowConfig.Value = !GuiMain.showHideTimescaleWindowConfig.Value;
				}
			}
			
			//
			if (GuiMain.showHideTimescaleWindowConfig.Value)
			{
				timescaleWindowRect = GUILayout.Window(1002, timescaleWindowRect, new GUI.WindowFunction(timescaleWindow), "Timescale", new GUILayoutOption[]
				{
				GUILayout.MaxWidth(300f),
				GUILayout.MinWidth(200f)
				});
			}
		}

		public string configTimescaleInput = "1";
		public bool showHideTimescaleWindow;
		public Rect timescaleWindowRect = new Rect(126f, 1f, 1f, 1f);
		public bool testNormalTimescale;
	}
}
