using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649

namespace SimpleGUI.Menus {
	class ActorControl {
		public void actorControlWindow(int windowID)
		{
			GuiMain.SetWindowInUse(windowID);
            Color originalColor = GUI.backgroundColor;


        
            GUI.backgroundColor = originalColor;
            GUI.DragWindow();
		}

     


        public Rect controlWindowRect = new Rect(126f, 1f, 1f, 1f);
		public bool showHideControlWindow;
	}
}
