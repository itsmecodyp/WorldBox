using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWrecks_RPG;
using UnityEngine;
using UnityEngine.UI;

namespace z_com_TWrecksMod
{
    class RPGActorWindow
    {
        Actor controlled => TWrecks_RPG.TWrecks_Main.controlledActor;
        public void OpenControlledWindow()
        {
            ScrollWindow window = LoadControlledWindow();
            LoadWindowDetails(controlled, window);
            window.clickShow();
        }

        public bool hasInit;

        public void doInit()
        {
            if (!hasInit)
            {
                Windows.init();
                hasInit = true;
            }
        }

        public StatBar health;//= new StatBar();
        public NameInput NameInput;

        public static Slider AddSliderToCanvas(string textString, GameObject canvasGameObject, float min, float max, float value)
        { // doesnt work?
            GameObject textObject = new GameObject("rpgSlider_" + textString); // create new object to contain menu element
            textObject.transform.SetParent(canvasGameObject.transform); // canvas content
            Slider slider = textObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.SetDirection(Slider.Direction.LeftToRight, false);
            return slider;
        }

        public static InputField AddInputFieldToCanvas(string textString, GameObject canvasGameObject)
        { // doesnt work?
            GameObject textObject = new GameObject("rpgInputField_" + textString); // create new object to contain menu element
            textObject.transform.SetParent(canvasGameObject.transform); // canvas content
            InputField inputField = textObject.AddComponent<InputField>();
            inputField.textComponent = AddTextToCanvas(textString, canvasGameObject);
            inputField.text = textString;
            inputField.characterLimit = 99;
            inputField.inputType = InputField.InputType.Standard;
            return inputField;
        }

        public static Text AddTextToCanvas(string textString, GameObject canvasGameObject)
        {
            GameObject textObject = new GameObject("rpgText_"+textString); // create new object to contain menu element
            textObject.transform.SetParent(canvasGameObject.transform); // canvas content
            Text text = textObject.AddComponent<Text>();
            text.text = textString;
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            text.font = ArialFont;
            text.material = ArialFont.material;
            return text;
        }
        public Image AddImageToCanvas(string textString, GameObject canvasGameObject, Sprite iconSprite)
        {
            var iconObj = new GameObject("rpgImage_" + textString);
            iconObj.transform.SetParent(canvasGameObject.transform);
            iconObj.transform.localPosition = Vector3.zero;
            Image icon = iconObj.AddComponent<Image>();
            icon.sprite = iconSprite;
            return icon;
        }

        public Button AddButtonToCanvas(string textString, GameObject canvasGameObject, Sprite iconSprite = null)
        {
            // add onclick/onhover/etc to the button after
            GameObject rpgButton = new GameObject("rpgButton_" + textString); // create new object to contain menu element
            rpgButton.transform.SetParent(canvasGameObject.transform); // canvas content
            Button button = rpgButton.AddComponent<Button>();

            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(button.transform);
            iconObj.transform.localPosition = Vector3.zero;
            Image icon = iconObj.AddComponent<Image>();

            if (iconSprite == null)
            {
                Sprite iconPause = Resources.Load<Sprite>("ui/icons/iconpause"); // temp
                icon.sprite = iconPause;
            }
            else
            {
                icon.sprite = iconSprite;
            }
            return button;
        }



        public void LoadWindowDetails(Actor target, ScrollWindow window)
        {

            ActorStats stats = Reflection.GetField(target.GetType(), target, "stats") as ActorStats;
            ActorStatus data = Reflection.GetField(target.GetType(), target, "data") as ActorStatus;

            GameObject canvasObject = GameObject.Find("/Canvas/CanvasWindows/windows/" + window.name + "/Background/Scroll View/Viewport/Content");
            for (int i = 0; i < canvasObject.transform.childCount; i++)
            {
                UnityEngine.Object.Destroy(canvasObject.transform.GetChild(i).gameObject);
            }

            if (target != null)
            {
                if (data.alive)
                {
                    Text nameText1 = AddTextToCanvas(data.firstName, canvasObject);
                    nameText1.fontSize = 20;
                    nameText1.transform.localPosition = new Vector3(50, -20, 0); 

                    SpriteRenderer spriteRendererBody = Reflection.GetField(target.GetType(), target, "spriteRenderer") as SpriteRenderer;
                    Image bodySprite = AddImageToCanvas("body", canvasObject, spriteRendererBody.sprite);
                    bodySprite.transform.localPosition = new Vector3(50, -25, 0); // button/element position

                    // traits stuff

                    TraitButton originalTraitButton = Utils.FindEvenInactive("TraitButton").GetComponent<TraitButton>();

                    if (data.traits.Count >= 1)
                    {
                        for (int i = 0; i < data.traits.Count; i++)
                        {
                            string traitname = AssetManager.traits.get(data.traits[i]).id;
                            TraitButton traitButton = UnityEngine.Object.Instantiate<TraitButton>(originalTraitButton, canvasObject.transform);
                            traitButton.CallMethod("load", new object[] { traitname });
                            traitButton.transform.localPosition = bodySprite.transform.localPosition; //chan

                            RectTransform component = traitButton.GetComponent<RectTransform>();
                            float num = 10f;
                            float num2 = 136f - num * 1.5f;
                            float num3 = 15.6799994f;
                            bool flag = (float)data.traits.Count * num3 >= num2;
                            if (flag)
                            {
                                num3 = num2 / (float)data.traits.Count;
                            }
                            float x = num + num3 * (float)i;
                            float y = -11f;

                            component.transform.SetParent(canvasObject.transform); // parent this new object to the canvas, positions to center of canvas
                            component.localPosition = (Vector2)bodySprite.transform.localPosition + new Vector2(x - 24, -14 + y); //  + new Vector3(0, 30, 0); // position of whole 

                            Button traitActualButton = traitButton.gameObject.GetComponent<Button>();
                            traitActualButton.onClick.AddListener(delegate ()
                            {
                                Debug.Log("traitbutton click:" + traitname);
                                //callback(traitButton);
                            });
                        }
                    }

                    //


                }

            }
            window.transform.Find("Background").Find("Scroll View").gameObject.SetActive(true);



        }

        public ScrollWindow LoadControlledWindow()
        {
            doInit();
            ScrollWindow activeWindow = null;
            if (Windows.getWindow("rpgcontrolled") != null)
            {
                activeWindow = Windows.getWindow("rpgcontrolled");
            }
            else
            {
                ScrollWindow newWindow = Windows.createNewWindow("rpgcontrolled");
                newWindow.titleText.text = "Controlled";
                activeWindow = newWindow;
            }

            return activeWindow;
        }

    }
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

    internal class Utils
    {
        public static GameObject FindEvenInactive(string Name)
        {
            GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < array.Length; i++)
            {
                bool flag = array[i].gameObject.gameObject.name == Name;
                if (flag)
                {
                    return array[i];
                }
            }
            return null;
        }
    }

}
