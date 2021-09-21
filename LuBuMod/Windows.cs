using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LuBuMod
{
    class Windows
    {
        public static Dictionary<string, ScrollWindow> allWindows;

        public static void init()
        {
            allWindows = getAllWindows();
        }

        public static ScrollWindow createNewWindow(string windowId)
        {
            if (allWindows.ContainsKey(windowId))
                return allWindows[windowId];

            ScrollWindow original = (ScrollWindow)Resources.Load("windows/empty", typeof(ScrollWindow));

            ScrollWindow scrollWindow = GameObject.Instantiate<ScrollWindow>(original, CanvasMain.instance.transformWindows);

            GameObject.Destroy(scrollWindow.titleText.GetComponent<LocalizedText>());

            scrollWindow.screen_id = windowId;
            scrollWindow.name = windowId;
            scrollWindow.titleText.text = windowId;

            Reflection.CallMethod(scrollWindow, "create", false);


            if (allWindows.ContainsKey(windowId))
                return allWindows[windowId];

            allWindows.Add(windowId, scrollWindow);



            return allWindows[windowId];
        }


        public static ScrollWindow getWindow(string windowId)
        {
            if (allWindows.ContainsKey(windowId))
                return allWindows[windowId];

            return null;
        }

        public static void showWindow(string windowId)
        {
            if (allWindows.ContainsKey(windowId))
                allWindows[windowId].clickShow();
        }

        public static Dictionary<string, ScrollWindow> getAllWindows()
        {
            return Reflection.GetField(typeof(ScrollWindow), null, "allWindows") as Dictionary<string, ScrollWindow>;
        }
    }

}
