using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReflectionUtility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace PowerBox
{
    partial class WorldBoxMod
    {
        private void initWindows()
        {
            #region aboutPowerBoxWindow
            //var aboutPowerBoxWindow = Helper.Windows.createNewWindow("aboutPowerBox");
            var aboutPowerBoxWindow = NCMS.Utils.Windows.CreateNewWindow("aboutPowerBox", "PowerBox!");
            //aboutPowerBoxWindow.titleText.text = "PowerBox!";
            aboutPowerBoxWindow.transform.Find("Background").Find("Scroll View").gameObject.SetActive(true);
            var aboutPowerBoxContent = GameObject.Find("/Canvas Container Main/Canvas - Windows/windows/aboutPowerBox/Background/Scroll View/Viewport/Content");
            //scrollReact.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            #region var description

            string hlColor = "#65BD00FF";

            var description =
@"This mod adding <color='" + hlColor + @"'>25 new powers</color> to the new god's powers tab.


There is <color='" + hlColor + @"'>11</color> new spawn powers:
    <color='" + hlColor + @"'>- Greg</color>
    <color='" + hlColor + @"'>- Tumor monster unit and animal</color>
    <color='" + hlColor + @"'>- Mush unit and animal</color>
    <color='" + hlColor + @"'>- Boats</color>
with <color='" + hlColor + @"'>3</color> new creatures:
    <color='" + hlColor + @"'>- Maxim</color>
    <color='" + hlColor + @"'>- Mastef</color>
    <color='" + hlColor + @"'>- Burger-spider</color>(<color='yellow'>pug bread#2872</color> and <color='yellow'>community</color> - idea, <color='yellow'>Arson Eel#0808</color>  - art concept, <color='yellow'>QuickLast#9791</color> - sprites)!

<color='" + hlColor + @"'>4</color> powers for creatures:
    <color='" + hlColor + @"'>- Editing creatures traits and items!</color> Massively by using new god powers, or specific by clicking new buttons in creature inspection window.

<color='" + hlColor + @"'>8</color> new powers and improvements for kingdoms and cities:
    <color='" + hlColor + @"'>- Not random friendship and spite!</color> Now you can choose which kingdom will be a friend/enemy with other specific kingdom!
    <color='" + hlColor + @"'>- Upgrade and downgrade buildings</color>
    <color='" + hlColor + @"'>- Make colony power!</color> Now you can drag creatures(using native Divine Magnet power), which are already citizens of one of kingdoms, and then drop them to free ground and use this power on them. Done! New colony founded! Also, you can transfer citizens between cities using this power, all what you need is just put them to new city and then use Make Colony power on them, and they will join new city as citizens!
    <color='" + hlColor + @"'>- Random kingdom color</color>
    <color='" + hlColor + @"'>- Editing kingdoms banners</color>
    <color='" + hlColor + @"'>- Editing colony resources</color>

<color='" + hlColor + @"'>10</color> new world laws:
    <color='" + hlColor + @"'>- Upgrade Buildings</color>
    <color='" + hlColor + @"'>- Shooting trough the mountains</color>
    <color='" + hlColor + @"'>- Imperial Thinking!</color> If enabled, different races will capture other cities instead of destroying them.
    <color='" + hlColor + @"'>- Villagers Reproduction</color>
    <color='" + hlColor + @"'>- Virus Apocalypse</color>
    <color='" + hlColor + @"'>- Mad Animals</color>
    <color='" + hlColor + @"'>- More disasters!</color>
    <color='" + hlColor + @"'>- Insect Spawn</color>
    <color='" + hlColor + @"'>- Animal Reproduction</color>
    <color='" + hlColor + @"'>- Regeneration</color>

<color='" + hlColor + @"'>6</color> new disasters:
    <color='" + hlColor + @"'>- 4 random infections:</color> plague, zombie, tumor and mush!
    <color='" + hlColor + @"'>- Burger-spider rain clouds!</color>
    <color='" + hlColor + @"'>- Blood rain clouds!</color>

<color='" + hlColor + @"'>2</color> new fan powers:
    <color='" + hlColor + @"'>- Blood rain cloud spawning!</color>
    <color='" + hlColor + @"'>- Burger-spider cloud spawning!</color>


Special thanks to <color='yellow'>QuickLast#9791</color> for making sprites for new creatures and god powers buttons.

Special thanks to beta-testers: 
    <color='yellow'>QuickLast#9791</color>
    <color='yellow'>OzzDeni#9151</color>
    <color='yellow'>Unununium#0666</color>
    <color='yellow'>Purple Haze#2477</color>
    <color='yellow'>Ares#0303</color>
    <color='yellow'>juanchiz#8905</color>
    <color='yellow'>Санёк 🐌#1503</color>
    <color='yellow'>BrisketandChad#6740</color>

Mod author: <color='yellow'>Nikon#7777</color>
";
            #endregion

            var name = aboutPowerBoxWindow.transform.Find("Background").Find("Name").gameObject;
            //name.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 300);

            var nameText = name.GetComponent<Text>();
            nameText.text = description;
            nameText.color = new Color(0, 0.74f, 0.55f, 1);
            nameText.fontSize = 7;
            nameText.alignment = TextAnchor.UpperLeft;
            nameText.supportRichText = true;
            name.transform.SetParent(aboutPowerBoxWindow.transform.Find("Background").Find("Scroll View").Find("Viewport").Find("Content"));


            name.SetActive(true);

            var nameRect = name.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.offsetMin = new Vector2(-90f, nameText.preferredHeight * -1);
            nameRect.offsetMax = new Vector2(90f, -17);
            nameRect.sizeDelta = new Vector2(180, nameText.preferredHeight + 50);
            aboutPowerBoxContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, nameText.preferredHeight + 50);

            name.transform.localPosition = new Vector2(name.transform.localPosition.x, ((nameText.preferredHeight / 2) + 30) * -1);

            #endregion


            #region editTraitsWindow AddRemoveTraitsWindow EditItemsWindow

            Reflection.CallStaticMethod(typeof(ScrollWindow), "checkWindowExist", "inspect_unit");
            var inspect_unit = GameObject.Find("/Canvas Container Main/Canvas - Windows/windows/inspect_unit");
            var inspect_unitContent = inspect_unit.transform.Find("/Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background/Scroll View/Viewport/Content");
            inspect_unit.SetActive(false);
            initEditTraitsWindow(inspect_unitContent);
            initAddRemoveTraitsWindow();
            initEditItemsWindow(inspect_unitContent);

            #endregion


            #region EditResoucesWindow

            Reflection.CallStaticMethod(typeof(ScrollWindow), "checkWindowExist", "village");
            var inspect_village = NCMS.Utils.GameObjects.FindEvenInactive("village");
            inspect_village.SetActive(false);
            //var inspect_village = Helper.Utils.FindEvenInactive("village");
            Debug.Log(inspect_village);
            var inspect_villageBackground = inspect_village.transform.Find("Background");
            initEditResoucesWindow(inspect_villageBackground);

            #endregion


            #region EditBannerWindow

            Reflection.CallStaticMethod(typeof(ScrollWindow), "checkWindowExist", "kingdom");
            var inspect_kingdom = GameObject.Find("/Canvas Container Main/Canvas - Windows/windows/kingdom");
            var inspect_kingdomBackground = inspect_unit.transform.Find("/Canvas Container Main/Canvas - Windows/windows/kingdom/Background");

            inspect_kingdom.SetActive(false);

            initEditBannerWindow(inspect_kingdomBackground);

            #endregion


            #region PowerBoxLawsWindow

            Reflection.CallStaticMethod(typeof(ScrollWindow), "checkWindowExist", "world_laws");
            var worldLaws = GameObject.Find("/Canvas Container Main/Canvas - Windows/windows/world_laws");
            var worldLaws_unitContent = inspect_unit.transform.Find("/Canvas Container Main/Canvas - Windows/windows/world_laws/Background/Scroll View/Viewport/Content");
            worldLaws.SetActive(false);

            initPowerBoxLawsWindow();

            #endregion
        }
    }
}
