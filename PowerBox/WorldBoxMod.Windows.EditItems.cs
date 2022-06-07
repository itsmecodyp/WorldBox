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
        private static GameObject changeItemType;
        private static GameObject changeItemPrefix;
        private static ScrollWindow editItemsWindow;
        private static void initEditItemsWindow(Transform inspect_unitContent)
        {
            initAddRemoveChoosen();

            editItemsWindow = NCMS.Utils.Windows.CreateNewWindow("editItems", "Edit Items");
            //editItemsWindow.titleText.text = "Edit Items";

            editItemsWindow.transform.Find("Background").Find("Scroll View").gameObject.SetActive(true);

            var editItems = Helper.GodPowerTab.createButton(
                "EditItems",
                Mod.EmbededResources.LoadSprite(resources + ".powers.items.png", 0, 0),
                inspect_unitContent,
                Edit_Items_Button_Click,
                "Edit items",
                "Edit unit's items");

            var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{editItemsWindow.name}/Background/Scroll View/Viewport");
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.sizeDelta = new Vector2(0, 17);


            editItems.transform.localPosition = new Vector3(245.50f, -42.50f, editItems.transform.localPosition.z);
            editItems.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta = new Vector2(65f, 65f);
            var editItemsRect = editItems.GetComponent<RectTransform>();
            editItemsRect.sizeDelta = new Vector2(100f, 100f);
            editItems.GetComponent<Image>().sprite = Mod.EmbededResources.LoadSprite(resources + ".other.backgroundBackButtonRev.png", 0, 0);
            editItems.GetComponent<Button>().transition = Selectable.Transition.None;

            var bg = editItemsWindow.transform.Find("Background");


            var saveButton = Helper.GodPowerTab.createButton(
                "DoneItems",
                Resources.Load<Sprite>("ui/icons/iconsavelocal"),
                bg.transform,
                Items_Save_Button_Click,
                "Save selected items",
                "");
            saveButton.transform.localPosition = new Vector2(70.00f, -100.00f);

            changeItemType = Helper.GodPowerTab.createButton(
                "ChangeType",
                Mod.EmbededResources.LoadSprite(resources + ".powers.armor.png", 0, 0),
                bg,
                Change_Type_Button_Click,
                "Armors",
                "");
            changeItemType.transform.localPosition = new Vector2(30.00f, -100.00f);


            changeItemPrefix = Helper.GodPowerTab.createButton(
                "ChangePrefix",
                Mod.EmbededResources.LoadSprite(resources + ".powers.prefix.png", 0, 0),
                bg,
                Change_Prefix_Button_Click,
                "Prefix",
                "");
            changeItemPrefix.transform.localPosition = new Vector2(-10.00f, -100.00f);
        }


        private static string choosenType = "weapon";
        public static void Change_Type_Button_Click()
        {
            if(choosenType == "weapon")
            {
                choosenType = "other";
                changeItemType.transform.Find("Icon").GetComponent<Image>().sprite = Mod.EmbededResources.LoadSprite(resources + ".powers.weapons.png", 0, 0);
                NCMS.Utils.Localization.setLocalization("ChangeType", "Weapons");
                //Helper.Localization.setLocalization("ChangeType", "Weapons");
            }
            else
            {
                choosenType = "weapon";
                changeItemType.transform.Find("Icon").GetComponent<Image>().sprite = Mod.EmbededResources.LoadSprite(resources + ".powers.armor.png", 0, 0);
                NCMS.Utils.Localization.setLocalization("ChangeType", "Armors");
                //Helper.Localization.setLocalization("ChangeType", "Armors");
            }

            choosenPrefix = "0";

            if (TType == PowerType.unset)
            {
                initEditItems(editItemsWindow, editItemsButtonCallBack);
            }
            else
            {
                initEditItems(editItemsWindow, null, TType);
            }
        }

        private static string choosenPrefix = "0";
        public static void Change_Prefix_Button_Click()
        {
            List<string> prefixList = new List<string>();

            var list = AssetManager.items_prefix.list;

            for (int i = 0; i < list.Count; i++)
            {
                var nextIndex = i + 1;

                if (i == list.Count - 1)
                    nextIndex = 0;


                bool condition = choosenType == "weapon" ? (list[i].pool.Contains("melee") && list[i].pool.Contains("range")) : (list[i].pool.Contains("equipment"));

                if(condition)
                {
                    prefixList.Add(list[i].id);
                }
            }

            var currentIndex = prefixList.FindIndex(c => c == choosenPrefix);

            if(currentIndex == -1)
                currentIndex = 0;

            if (currentIndex == prefixList.Count - 1)
                choosenPrefix = prefixList[0];
            else
                choosenPrefix = prefixList[currentIndex + 1];


            if (TType == PowerType.unset)
            {
                initEditItems(editItemsWindow, editItemsButtonCallBack);
            } 
            else
            {
                initEditItems(editItemsWindow, null, TType);
            }
        }

        public static void Edit_Items_Button_Click()
        {
            if (UNIT.stats.use_items)
            {
                initEditItems(editItemsWindow, editItemsButtonCallBack);
                editItemsWindow.clickShow();
            }
        }

        private static void initEditItems(ScrollWindow window, Action<EquipmentButton> callback, PowerType Ttype = PowerType.unset)
        {
            TType = Ttype;

            if(TType != PowerType.unset)
            {
                callback = addRemoveItemsButtonCallBack;
            }

            var rt = spriteHighlighter.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60f, 60f);

            var Content = GameObject.Find("/Canvas Container Main/Canvas - Windows/windows/" + window.name + "/Background/Scroll View/Viewport/Content");
            for (int i = 0; i < Content.transform.childCount; i++)
            {
                GameObject.Destroy(Content.transform.GetChild(i).gameObject);
            }

            var itemButton = NCMS.Utils.GameObjects.FindEvenInactive("EquipmentButton").GetComponent<EquipmentButton>();

            var itemsArray = AssetManager.items.list;

            var prepared = new List<itemObj>();

            for (int i = 0; i < itemsArray.Count; i++)
            {
                for (int j = 0; j < itemsArray[i].materials.Count; j++)
                {
                    var path = "ui/Icons/items/icon_" + itemsArray[i].id;
                    if (itemsArray[i].materials[j] != "base")
                        path = path + "_" + itemsArray[i].materials[j];

                    if(Resources.Load<Sprite>(path) != null)
                    {
                        if (itemsArray[i].suffixes.Count > 0)
                        {
                            for (int k = 0; k < itemsArray[i].suffixes.Count; k++)
                            {
                                //for (int l = 0; l < AssetManager.items_prefix.list.Count; l++)
                                //{
                                //    prepared.Add(new itemObj(itemsArray[i], itemsArray[i].materials[j], AssetManager.items_prefix.list[l].id, itemsArray[i].suffixes[k]));
                                //}
                                var newItemObj = new itemObj(itemsArray[i], itemsArray[i].materials[j], choosenPrefix, itemsArray[i].suffixes[k]);
                                if (!prepared.Contains(newItemObj))
                                {
                                    prepared.Add(newItemObj);
                                }
                            }
                        }
                        else
                        {
                            var newItemObj = new itemObj(itemsArray[i], itemsArray[i].materials[j], choosenPrefix, "0");
                            if (!prepared.Contains(newItemObj))
                            {
                                prepared.Add(newItemObj);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < prepared.Count; i++)
            {
                if (prepared[i].Asset.id.Contains("amulet"))
                {
                    prepared[i].TType = "amulet";
                    prepared[i].EqTType = EquipmentType.Amulet;
                }
                else if (prepared[i].Asset.id.Contains("armor"))
                {
                    prepared[i].TType = "armor";
                    prepared[i].EqTType = EquipmentType.Armor;
                }
                else if (prepared[i].Asset.id.Contains("boots"))
                {
                    prepared[i].TType = "boots";
                    prepared[i].EqTType = EquipmentType.Boots;
                }
                else if (prepared[i].Asset.id.Contains("helmet"))
                {
                    prepared[i].TType = "helmet";
                    prepared[i].EqTType = EquipmentType.Helmet;
                }
                else if (prepared[i].Asset.id.Contains("ring"))
                {
                    prepared[i].TType = "ring";
                    prepared[i].EqTType = EquipmentType.Ring;
                }
                else if (prepared[i].Asset.id.Contains("axe") || prepared[i].Asset.id.Contains("sword") || prepared[i].Asset.id.Contains("spear") || prepared[i].Asset.id.Contains("hammer") || prepared[i].Asset.id.Contains("staff") || prepared[i].Asset.id.Contains("bow") || prepared[i].Asset.id.Contains("blaster"))
                {
                    prepared[i].TType = "weapon";
                    prepared[i].EqTType = EquipmentType.Weapon;
                }
            }

            var prePrepared = prepared.FindAll((c) => { return choosenType == "weapon" ? c.TType == "weapon" : c.TType != "weapon"; }).Distinct(new ItemsComparer()).ToList();

            if (choosenType == "other")
            {
                var comp = new Comparison<itemObj>((c, z) =>
                {
                    return c.TType.CompareTo(z.TType);
                });

                var helmets = prePrepared.FindAll(c => c.TType == "helmet");
                helmets.Sort(comp);

                var armors = prePrepared.FindAll(c => c.TType == "armor");
                armors.Sort(comp);

                var boots = prePrepared.FindAll(c => c.TType == "boots");
                boots.Sort(comp);

                var amulets = prePrepared.FindAll(c => c.TType == "amulet");
                amulets.Sort(comp);

                var rings = prePrepared.FindAll(c => c.TType == "ring");
                rings.Sort(comp);


                prePrepared = new List<itemObj>();

                prePrepared.AddRange(helmets);
                prePrepared.AddRange(armors);
                prePrepared.AddRange(boots);
                prePrepared.AddRange(amulets);
                prePrepared.AddRange(rings);
            }

            XStep = 24.75f;
            countInRow = 8;


            var rect = Content.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(0, Mathf.Abs(GetPosByIndex(prePrepared.Count).y) + 100);

            for (int i = 0; i < prePrepared.Count; i++)
            {
                GameObject hl;
                if(TType == PowerType.unset)
                {
                    hl = AddHighLight(i, Content, ActorEquipment.getList(UNIT.equipment).Find(c => c.data.id == prePrepared[i].Asset.id && c.data.suffix == prePrepared[i].Suffix && c.data.material == prePrepared[i].Material && c.data.prefix == prePrepared[i].Prefix) != null);
                }
                else
                {
                    var addCond = false;
                    var removeCond = false;


                    if (choosenForAddSlots[prePrepared[i].EqTType] != null)
                    {
                        //Debug.Log(choosenForAddSlots[prePrepared[i].EqTType].data.id + choosenForAddSlots[prePrepared[i].EqTType].data.material + choosenForAddSlots[prePrepared[i].EqTType].data.suffix + " | " + prePrepared[i].Asset.id + prePrepared[i].Material + prePrepared[i].Suffix);
                        addCond = choosenForAddSlots[prePrepared[i].EqTType].data.id == prePrepared[i].Asset.id && choosenForAddSlots[prePrepared[i].EqTType].data.material == prePrepared[i].Material && choosenForAddSlots[prePrepared[i].EqTType].data.suffix == prePrepared[i].Suffix && choosenForAddSlots[prePrepared[i].EqTType].data.prefix == prePrepared[i].Prefix;
                        //addCond = choosenForAddSlots[prePrepared[i].EqTType].data.id + choosenForAddSlots[prePrepared[i].EqTType].data.material + choosenForAddSlots[prePrepared[i].EqTType].data.suffix == prePrepared[i].Asset.id + prePrepared[i].Material + prePrepared[i].Suffix;
                    }

                    if (choosenForRemoveSlots[prePrepared[i].EqTType] != null)
                    {
                        removeCond = choosenForRemoveSlots[prePrepared[i].EqTType].data.id == prePrepared[i].Asset.id && choosenForRemoveSlots[prePrepared[i].EqTType].data.material == prePrepared[i].Material && choosenForRemoveSlots[prePrepared[i].EqTType].data.suffix == prePrepared[i].Suffix && choosenForAddSlots[prePrepared[i].EqTType].data.prefix == prePrepared[i].Prefix;
                    }

                    hl = AddHighLight(i, Content, TType == PowerType.add ? addCond : removeCond);
                }
                //TType == PowerType.add ? SelectedToAdd.Contains(traitsArray[i].id) : SelectedToRemove.Contains(traitsArray[i].id)

                loadItemButton(prePrepared[i], i, prePrepared.Count, itemButton, hl.transform, callback);
            }

            ResetWrapVals();
        }

        public class itemObj
        {

            public ItemAsset Asset;
            public string Material;
            public string Prefix;
            public string Suffix;
            public string TType;
            public EquipmentType EqTType;
            public itemObj(ItemAsset asset, string material, string prefix = "", string suffix = "", string type = "", EquipmentType eqTType = EquipmentType.Amulet)
            {
                Asset = asset;
                Material = material;
                Prefix = prefix;
                Suffix = suffix;
                TType = type;
                EqTType = eqTType;
            }
        }
        private static void loadItemButton(itemObj item, int pIndex, int pTotal, EquipmentButton itemButtonPref, Transform parent, Action<EquipmentButton> callback)
        {
            EquipmentButton itemButton = GameObject.Instantiate<EquipmentButton>(itemButtonPref, parent);
            var path = "ui/Icons/items/icon_" + item.Asset.id;
            if (item.Material != "base" && item.Material != "")
                path = path + "_" + item.Material;

            itemButton.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);

            itemButton.transform.localPosition = Vector3.zero;
            ActorEquipmentSlot slot = new ActorEquipmentSlot();
            slot.data = new ItemData();
            slot.data.by = "Nikon";
            slot.data.from = "Nikon";
            slot.data.id = item.Asset.id;
            slot.data.prefix = choosenPrefix;
            slot.data.suffix = item.Suffix;
            slot.data.material = item.Material;
            slot.data.year = 0;

            if (item.Asset.id.Contains("amulet"))
            {
                slot.data.type = EquipmentType.Amulet;
            }
            else if(item.Asset.id.Contains("armor"))
            {
                slot.data.type = EquipmentType.Armor;
            }
            else if (item.Asset.id.Contains("boots"))
            {
                slot.data.type = EquipmentType.Boots;
            }
            else if (item.Asset.id.Contains("helmet"))
            {
                slot.data.type = EquipmentType.Helmet;
            }
            else if (item.Asset.id.Contains("ring"))
            {
                slot.data.type = EquipmentType.Ring;
            }
            else if (item.Asset.id.Contains("axe") || item.Asset.id.Contains("sword") || item.Asset.id.Contains("spear") || item.Asset.id.Contains("hammer") || item.Asset.id.Contains("staff"))
            {
                slot.data.type = EquipmentType.Weapon;
            }

            if (slot.data.prefix == "" || !new string[] { "sword", "axe", "spear", "hammer", "bow", "armor", "boots", "helmet", "ring", "amulet" }.Contains(item.Asset.id))
            {
                slot.data.prefix = "0";
            }

            if (slot.data.suffix == "")
            {
                slot.data.suffix = "0";
            }


            Reflection.SetField(itemButton, "slot", slot);

            var button = itemButton.gameObject.GetComponent<Button>();
            button.onClick.AddListener(() => callback(itemButton));
        }

        private static void editItemsButtonCallBack(EquipmentButton buttonPressed)
        {
            var slot = Reflection.GetField(buttonPressed.GetType(), buttonPressed, "slot") as ActorEquipmentSlot;
            var unitSlot = UNIT.equipment.getSlot(slot.data.type);

            if(unitSlot.data != null)
            {
                if (slot.data.id == unitSlot.data.id && slot.data.material == unitSlot.data.material && slot.data.suffix == unitSlot.data.suffix && slot.data.prefix == unitSlot.data.prefix)
                {
                    unitSlot.emptySlot();
                    //unitSlot.CallMethod("setItem", null);
                }
                else
                { 
                    unitSlot.CallMethod("setItem", slot.data);
                }
            }
            else
            {
                unitSlot.CallMethod("setItem", slot.data);
            }

            Reflection.SetField(UNIT, "statsDirty", true);


            var window = NCMS.Utils.Windows.GetWindow("editItems");
            initEditItems(window, editItemsButtonCallBack);
        }

        private static Dictionary<EquipmentType, ActorEquipmentSlot> choosenForAddSlots = new Dictionary<EquipmentType, ActorEquipmentSlot>();
        private static Dictionary<EquipmentType, ActorEquipmentSlot> choosenForRemoveSlots = new Dictionary<EquipmentType, ActorEquipmentSlot>();

        private static void addRemoveItemsButtonCallBack(EquipmentButton buttonPressed)
        {
            var slot = Reflection.GetField(buttonPressed.GetType(), buttonPressed, "slot") as ActorEquipmentSlot;

            if(TType == PowerType.add)
            {
                choosenForAddSlots[slot.data.type] = slot;
            }
            else if (TType == PowerType.remove)
            {
                choosenForRemoveSlots[slot.data.type] = slot;
            }

            var window = NCMS.Utils.Windows.GetWindow("editItems");
            initEditItems(window, null, TType);
        }

        private static void initAddRemoveChoosen()
        {
            choosenForAddSlots.Add(EquipmentType.Amulet, null);
            choosenForAddSlots.Add(EquipmentType.Armor, null);
            choosenForAddSlots.Add(EquipmentType.Boots, null);
            choosenForAddSlots.Add(EquipmentType.Helmet, null);
            choosenForAddSlots.Add(EquipmentType.Ring, null);
            choosenForAddSlots.Add(EquipmentType.Weapon, null);
            choosenForRemoveSlots.Add(EquipmentType.Amulet, null);
            choosenForRemoveSlots.Add(EquipmentType.Armor, null);
            choosenForRemoveSlots.Add(EquipmentType.Boots, null);
            choosenForRemoveSlots.Add(EquipmentType.Helmet, null);
            choosenForRemoveSlots.Add(EquipmentType.Ring, null);
            choosenForRemoveSlots.Add(EquipmentType.Weapon, null);
        }

        public static void Items_Save_Button_Click()
        {
            var addRemoveTraitsWindow = NCMS.Utils.Windows.GetWindow("editItems");
            addRemoveTraitsWindow.clickHide();

            if ((TType == PowerType.add && choosenForAddSlots.Count > 0) || (TType == PowerType.remove && choosenForRemoveSlots.Count > 0))
            {
                var pbsInstance = Reflection.GetField(typeof(PowerButtonSelector), null, "instance") as PowerButtonSelector;
                var pButton = NCMS.Utils.GameObjects.FindEvenInactive(TType == PowerType.add ? "addItems" : "removeItems");
                pbsInstance.clickPowerButton(pButton.GetComponent<PowerButton>());
            }
        }
        public class ItemsComparer : IEqualityComparer<itemObj>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(itemObj x, itemObj y)
            {

                //Check whether the compared objects reference the same data.
                if (System.Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (System.Object.ReferenceEquals(x, null) || System.Object.ReferenceEquals(y, null))
                    return false;

                return x.Asset.id + x.Material + x.Prefix + x.Suffix == y.Asset.id + y.Material + y.Prefix + y.Suffix;
            }

            // Products are equal if their names and product numbers are equal.
            public int GetHashCode(itemObj x)
            {
                int Asset = x.Asset.GetHashCode();
                int Material = x.Material.GetHashCode();
                int Prefix = x.Prefix.GetHashCode();
                int Suffix = x.Suffix.GetHashCode();

                return Asset ^ Material ^ Prefix ^ Suffix;
            }
        }
    }
}
