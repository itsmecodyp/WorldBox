using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace SimpleGUI.Menus
{
    public static class SimpleSettings
    {
        public enum MenuType{
            Main,
            Timescale,
            Items,
            Traits,
            Diplomacy,
            World,
            Construction,
            Interaction,
            StatSetting,
            Other,
            Settings,
            Control,
            Messages
        }

        public static void CloseAllWindows()
        {
            foreach (MenuType menu in Enum.GetValues(typeof(MenuType)))
            {
                ToggleMenu(menu, true);
            }
        }

        public static void ToggleMenu(MenuType menu, bool forceClose = false)
        {
            Debug.Log("switching menu id:" + menu.ToString());
            switch (menu)
            {
                case MenuType.Main:
                    if (forceClose)
                    {
                        showHideMainWindowConfig = false;
                    }
                    else
                    {
                        showHideMainWindowConfig = !showHideMainWindowConfig;
                    }
                    break;
                case MenuType.Timescale:
                    if (forceClose)
                    {
                        showHideTimescaleWindowConfig = false;
                    }
                    else
                    {
                        showHideTimescaleWindowConfig = !showHideTimescaleWindowConfig;
                    }
                    break;
                case MenuType.Items:
                    if (forceClose)
                    {
                        showHideItemGenerationConfig = false;
                    }
                    else
                    {
                        showHideItemGenerationConfig = !showHideItemGenerationConfig;
                    }
                    break;
                case MenuType.Traits:
                    if (forceClose)
                    {
                        showHideTraitsWindowConfig = false;
                    }
                    else
                    {
                        showHideTraitsWindowConfig = !showHideTraitsWindowConfig;
                    }
                    break;
                case MenuType.Diplomacy:
                    if (forceClose)
                    {
                        showHideDiplomacyConfig = false;
                    }
                    else
                    {
                        showHideDiplomacyConfig = !showHideDiplomacyConfig;
                    }
                    break;
                case MenuType.World:
                    if (forceClose)
                    {
                        showHideWorldOptionsConfig = false;
                    }
                    else
                    {
                        showHideWorldOptionsConfig = !showHideWorldOptionsConfig;
                    }
                    break;
                case MenuType.Construction:
                    if (forceClose)
                    {
                        showHideConstructionConfig = false;
                    }
                    else
                    {
                        showHideConstructionConfig = !showHideConstructionConfig;
                    }
                    break;
                case MenuType.Interaction:
                    if (forceClose)
                    {
                        showHideActorInteractConfig = false;
                    }
                    else
                    {
                        showHideActorInteractConfig = !showHideActorInteractConfig;
                    }
                    break;
                case MenuType.StatSetting:
                    if (forceClose)
                    {
                        showHideStatSettingConfig = false;
                    }
                    else
                    {
                        showHideStatSettingConfig = !showHideStatSettingConfig;
                    }
                    break;
                case MenuType.Other:
                    if (forceClose)
                    {
                        showHideOtherConfig = false;
                    }
                    else
                    {
                        showHideOtherConfig = !showHideOtherConfig;
                    }
                    break;
                case MenuType.Settings:
                    if (forceClose)
                    {
                        //showHideSettingsWindowConfig = false;
                    }
                    else
                    {
                        //showHideSettingsWindowConfig = !showHideSettingsWindowConfig;
                    }
                    break;
                case MenuType.Control:
                    if (forceClose)
                    {
                        showHideActorControlConfig = false;
                    }
                    else
                    {
                        showHideActorControlConfig = !showHideActorControlConfig;
                    }
                    break;
                default:
                    break;
            }
        }

        #region retardedBoolTogglesNeedChanged
        public static bool showPatreonWindow;
        public static bool showHideSettingsWindowConfig;
        public static bool showHideMainWindowConfig;
        public static bool showHideTimescaleWindowConfig;
        public static bool showHideItemGenerationConfig;
        public static bool showHideTraitsWindowConfig;
        public static bool showHideDiplomacyConfig;
        public static bool showHideWorldOptionsConfig;
        public static bool showHideConstructionConfig;
        public static bool showHideOtherConfig;
        public static bool showHidePatreonConfig;
        public static bool showHideActorInteractConfig;
        public static bool showHideStatSettingConfig;
        public static bool showHideActorControlConfig;
        #endregion

        public static ConfigEntry<float> farmsNewRange;
        public static ConfigEntry<int> fillToolIterations;
        public static ConfigEntry<float> timerBetweenFill;
        public static ConfigEntry<float> maxTimeToWait;
        public static ConfigEntry<int> fillTileCount;
        public static ConfigEntry<string> fillByLines;
        public static ConfigEntry<float> zoneAlpha;
    }
}
