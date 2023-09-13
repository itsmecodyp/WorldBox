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
            Settings
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
                        showHideMainWindowConfig.Value = false;
                    }
                    else
                    {
                        showHideMainWindowConfig.Value = !showHideMainWindowConfig.Value;
                    }
                    break;
                case MenuType.Timescale:
                    if (forceClose)
                    {
                        showHideTimescaleWindowConfig.Value = false;
                    }
                    else
                    {
                        showHideTimescaleWindowConfig.Value = !showHideTimescaleWindowConfig.Value;
                    }
                    break;
                case MenuType.Items:
                    if (forceClose)
                    {
                        showHideItemGenerationConfig.Value = false;
                    }
                    else
                    {
                        showHideItemGenerationConfig.Value = !showHideItemGenerationConfig.Value;
                    }
                    break;
                case MenuType.Traits:
                    if (forceClose)
                    {
                        showHideTraitsWindowConfig.Value = false;
                    }
                    else
                    {
                        showHideTraitsWindowConfig.Value = !showHideTraitsWindowConfig.Value;
                    }
                    break;
                case MenuType.Diplomacy:
                    if (forceClose)
                    {
                        showHideDiplomacyConfig.Value = false;
                    }
                    else
                    {
                        showHideDiplomacyConfig.Value = !showHideDiplomacyConfig.Value;
                    }
                    break;
                case MenuType.World:
                    if (forceClose)
                    {
                        showHideWorldOptionsConfig.Value = false;
                    }
                    else
                    {
                        showHideWorldOptionsConfig.Value = !showHideWorldOptionsConfig.Value;
                    }
                    break;
                case MenuType.Construction:
                    if (forceClose)
                    {
                        showHideConstructionConfig.Value = false;
                    }
                    else
                    {
                        showHideConstructionConfig.Value = !showHideConstructionConfig.Value;
                    }
                    break;
                case MenuType.Interaction:
                    if (forceClose)
                    {
                        showHideActorInteractConfig.Value = false;
                    }
                    else
                    {
                        showHideActorInteractConfig.Value = !showHideActorInteractConfig.Value;
                    }
                    break;
                case MenuType.StatSetting:
                    if (forceClose)
                    {
                        showHideStatSettingConfig.Value = false;
                    }
                    else
                    {
                        showHideStatSettingConfig.Value = !showHideStatSettingConfig.Value;
                    }
                    break;
                case MenuType.Other:
                    if (forceClose)
                    {
                        showHideOtherConfig.Value = false;
                    }
                    else
                    {
                        showHideOtherConfig.Value = !showHideOtherConfig.Value;
                    }
                    break;
                case MenuType.Settings:
                    if (forceClose)
                    {
                        //showHideSettingsWindowConfig.Value = false;
                    }
                    else
                    {
                        //showHideSettingsWindowConfig.Value = !showHideSettingsWindowConfig.Value;
                    }
                    break;
                default:
                    break;
            }
        }

        public static ConfigEntry<bool> showPatreonWindow;
        public static ConfigEntry<bool> showHideSettingsWindowConfig;
        public static ConfigEntry<bool> showHideMainWindowConfig;
        public static ConfigEntry<bool> showHideTimescaleWindowConfig;
        public static ConfigEntry<bool> showHideItemGenerationConfig;
        public static ConfigEntry<bool> showHideTraitsWindowConfig;
        public static ConfigEntry<bool> showHideDiplomacyConfig;
        public static ConfigEntry<bool> showHideWorldOptionsConfig;
        public static ConfigEntry<bool> showHideConstructionConfig;
        public static ConfigEntry<bool> showHideOtherConfig;
        public static ConfigEntry<bool> showHidePatreonConfig;
        public static ConfigEntry<bool> showHideActorInteractConfig;
        public static ConfigEntry<bool> showHideStatSettingConfig;
    }
}
