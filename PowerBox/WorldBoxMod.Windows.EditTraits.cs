using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReflectionUtility;
using UnityEngine;
using static Config;
using UnityEngine.UI;
using UnityEngine.Events;

namespace PowerBox
{
    partial class WorldBoxMod : MonoBehaviour
    {

        private static GameObject spriteHighlighter;
        private const float rC = 0.314f;
        private const float gC = 0.78f;
        private const float bC = 0;
        private const float aC = 0.565f;
        private static void initEditTraitsWindow(Transform inspect_unitContent)
        {
            var editTraitsWindow = NCMS.Utils.Windows.CreateNewWindow("editTraits", "Edit Traits");
            //editTraitsWindow.titleText.text = "Edit Traits";


            var editTraits = Helper.GodPowerTab.createButton(
                "EditTraits",
                Mod.EmbededResources.LoadSprite(resources + ".powers.traits_clear.png", 0, 0),
                inspect_unitContent,
                Edit_Traits_Button_Click,
                "Edit traits",
                "Edit unit's traits");

            var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{editTraitsWindow.name}/Background/Scroll View/Viewport");
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.sizeDelta = new Vector2(0, 17);

            //editTraits.transform.localPosition = new Vector3(98f, -15f, editTraits.transform.localPosition.z);
            editTraits.transform.localPosition = new Vector3(245.50f, -15f, editTraits.transform.localPosition.z);
            editTraits.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta = new Vector2(65f, 65f);
            var editTraitsRect = editTraits.GetComponent<RectTransform>();
            editTraitsRect.sizeDelta = new Vector2(100f, 100f);

            var culturesButtonCreatureInspect = GameObject.Find("/Canvas Container Main/Canvas - Windows/windows/inspect_unit/Background/ButtonCulturesContainer");
            culturesButtonCreatureInspect.transform.localPosition = new Vector2(5000f, 5000f);
            //culturesButtonCreatureInspect.gameObject.SetActive(false);
            //GameObject.Destroy(culturesButtonCreatureInspect);

            editTraits.GetComponent<Image>().sprite = Mod.EmbededResources.LoadSprite(resources + ".other.backgroundBackButtonRev.png", 0, 0);
            editTraits.GetComponent<Button>().transition = Selectable.Transition.None;


            spriteHighlighter = new GameObject("spriteHighlighter");
            spriteHighlighter.transform.localScale = new Vector2(1.0f, 1.0f);
            spriteHighlighter.layer = 5;

            var imageH = spriteHighlighter.AddComponent<Image>();
            var texture = Resources.Load<Texture2D>("ui/icons/iconbrush_circ_5");
            imageH.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, (float)texture.width, (float)texture.height), new Vector2(0.5f, 0.5f), 1f);
            imageH.color = new Color(rC, gC, bC, aC);
            imageH.raycastTarget = false;

            spriteHighlighter.SetActive(false);
        }

        public static void Edit_Traits_Button_Click()
        {
            var window = NCMS.Utils.Windows.GetWindow("editTraits");
            window.transform.Find("Background").Find("Scroll View").gameObject.SetActive(true);
            initEditTraits(window, editTraitsButtonCallBack);
            window.clickShow();
        }

        private static void initEditTraits(ScrollWindow window, Action<TraitButton> callback)
        {
            var rt = spriteHighlighter.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60f, 60f);

            var Content = GameObject.Find("/Canvas Container Main/Canvas - Windows/windows/" + window.name + "/Background/Scroll View/Viewport/Content");
            for (int i = 0; i < Content.transform.childCount; i++)
            {
                GameObject.Destroy(Content.transform.GetChild(i).gameObject);
            }


            var traitButton = NCMS.Utils.GameObjects.FindEvenInactive("TraitButton").GetComponent<TraitButton>();

            var traitsArray = AssetManager.traits.dict.Values.ToList();

            var rect = Content.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(0, Mathf.Abs(GetPosByIndex(traitsArray.Count).y) + 100);

            for (int i = 0; i < traitsArray.Count; i++)
            {
                var hl = AddHighLight(i, Content, UNIT.haveTrait(traitsArray[i].id));

                loadTraitButton(traitsArray[i].id, i, traitsArray.Count, traitButton, hl.transform, callback);
            }
        }

        private static void loadTraitButton(string pID, int pIndex, int pTotal, TraitButton traitButtonPref, Transform parent, Action<TraitButton> callback)
        {
            TraitButton traitButton = GameObject.Instantiate<TraitButton>(traitButtonPref, parent);
            Reflection.CallMethod(traitButton, "load", pID);


            traitButton.transform.localPosition = Vector3.zero;


            var button = traitButton.gameObject.GetComponent<Button>();
            button.onClick.AddListener(() => callback(traitButton));
        }

        private static void editTraitsButtonCallBack(TraitButton buttonPressed)
        {
            var trait = (ActorTrait)Reflection.GetField(buttonPressed.GetType(), buttonPressed, "trait");

            if (UNIT.haveTrait(trait.id))
            {
                UNIT.removeTrait(trait.id);
            }
            else
            {
                UNIT.addTrait(trait.id);
            }

            HighlightTrait(data.traits.Contains(trait.id), buttonPressed.transform.parent.gameObject);
        }

        private static GameObject AddHighLight(int index, GameObject Content, bool enabled = false)
        {
            var spriteHL = GameObject.Instantiate(spriteHighlighter, Content.transform);
            spriteHL.transform.localPosition = GetPosByIndex(index);
            spriteHL.SetActive(true);

            if (enabled)
            {
                spriteHL.GetComponent<Image>().color = new Color(rC, gC, bC, aC);
            }
            else
            {
                spriteHL.GetComponent<Image>().color = new Color(rC, gC, bC, 0);
            }

            return spriteHL;
        }

        private static void HighlightTrait(bool enable, GameObject HighLight)
        {
            if (enable)
            {
                HighLight.GetComponent<Image>().color = new Color(rC, gC, bC, aC);
            }
            else
            {
                HighLight.GetComponent<Image>().color = new Color(rC, gC, bC, 0);
            }
        }


        private static float startXPos = 44.4f;
        private static float XStep = 28f;
        private static int countInRow = 7;
        private static float startYPos = -22.5f;
        private static float YStep = -28.5f;
        private static Vector2 GetPosByIndex(int index)
        {
            float x = (index % countInRow) * XStep + startXPos;
            float y = (Mathf.RoundToInt(index / countInRow) * YStep) + startYPos;

            return new Vector2(x, y);
        }

        private static void ResetWrapVals()
        {
            startXPos = 40f;
            XStep = 22f;
            countInRow = 9;
            startYPos = -22.5f;
            YStep = -22.5f;
        }
    }
}
