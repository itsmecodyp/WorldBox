using NCMS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWrecks_RPG;
using UnityEngine;
using UnityEngine.UI;

namespace z_com_TWrecksMod {
	class Survivor { // inspired by Vampire Survivors

		public void SetAllCreatureHealth(int pHealth = 1)
		{
			List<ActorStats> statsList = AssetManager.unitStats.list;
			for(int i = 0; i < statsList.Count; i++) {
				ActorStats statsAsset = statsList[i];
				BaseStats stats = statsAsset.baseStats;
				stats.health = pHealth;
				AssetManager.unitStats.add(statsAsset);
			}
		}
		public static int timeStarted = 0; // set to realtimeblah whenever mode/round starts
		public static int timeSinceStart => (int)Time.realtimeSinceStartup - timeStarted;

		public int startingHealth = 1;
		public static int difficultyScaling = 5;
		public int difficultyHealth = timeSinceStart / difficultyScaling; // 10 seconds = +2hp, 120seconds = +24, 600sec = +120 etc

		public void UpdateHealthFromDifficulty()
		{
			SetAllCreatureHealth(startingHealth + difficultyHealth);
		}

		public Actor controlledActor => TWrecks_Main.controlledActor;

		public void UpdateMonsterMovement() // make everyone attack player
		{
			if(controlledActor != null) {
				List<Actor> actorList = MapBox.instance.units.getSimpleList();
				actorList.Remove(controlledActor); // get controlled actor out of "monster" list
				if(TWrecks_Main.totalSquadActorList.Count > 1) { // and any squad actors too
					for(int i = 0; i < TWrecks_Main.totalSquadActorList.Count; i++) {
						actorList.Remove(TWrecks_Main.totalSquadActorList[i]);
					}
				}
				for(int i = 0; i < actorList.Count; i++) {
					Actor targetActor = actorList[i];
					targetActor.tryToAttack(controlledActor); // creatures try to attack
					TWrecks_Main.BasicMoveAndWait(targetActor, controlledActor.currentTile); // and then move closer
				}
			}
		}

		bool hasStarted;

		public void StartRound()
		{
			timeStarted = (int)Time.realtimeSinceStartup;
			hasStarted = true;
		}

		public void UpdateSurvivor()
		{

		}

		public static int levelsPerUpgrade = 1;

		public static List<string> upgradeList = new List<string>() { "fast", "boat", "strong" };
		public static Dictionary<string, int> equippedUpgrades = new Dictionary<string, int>();
		public static List<string> equippedUpgradeList => equippedUpgrades.Keys.ToList(); // or just actor.traits i guess

		public void OpenUpgradeWindow()
		{
			#region upgradeWindow
			Debug.Log("opening upgrade window");
            var upgradeWindow = NCMS.Utils.Windows.CreateNewWindow("survivor_upgrades", "Select an upgrade");
			upgradeWindow.transform.Find("Background").Find("Scroll View").gameObject.SetActive(true);

			ActorTrait upgradableTrait = AssetManager.traits.get(upgradeList.GetRandom());
			Sprite traitSprite = (Sprite)Resources.Load("ui/Icons/" + upgradableTrait.icon, typeof(Sprite));
			PowerButton upgradeButton = PowerButtons.CreateButton("upgrade_" + upgradableTrait.id, traitSprite, upgradableTrait.id, "", Vector2.zero, ButtonType.Click, null, delegate {
				if(equippedUpgrades.ContainsKey(upgradableTrait.id) == false) {
					Debug.Log("should add upgrade to player");
					equippedUpgrades.Add(upgradableTrait.id, 1);
				}
				else {
					Debug.Log("should level up upgrade");
					int currentLevel = equippedUpgrades[upgradableTrait.id];
					equippedUpgrades[upgradableTrait.id] = currentLevel + levelsPerUpgrade; 
					//update trait with new level
				}
				
			});
			#region var description

			string hlColor = "#65BD00FF";

            var description = "hi";
            #endregion

            var name = upgradeWindow.transform.Find("Background").Find("Name").gameObject;

            var nameText = name.GetComponent<Text>();
            nameText.text = description;
            nameText.color = new Color(0, 0.74f, 0.55f, 1);
            nameText.fontSize = 7;
            nameText.alignment = TextAnchor.UpperLeft;
            nameText.supportRichText = true;
            name.transform.SetParent(upgradeWindow.transform.Find("Background").Find("Scroll View").Find("Viewport").Find("Content"));


            name.SetActive(true);

            var nameRect = name.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.offsetMin = new Vector2(-90f, nameText.preferredHeight * -1);
            nameRect.offsetMax = new Vector2(90f, -17);
            nameRect.sizeDelta = new Vector2(180, nameText.preferredHeight + 50);
            //aboutPowerBoxContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, nameText.preferredHeight + 50);

            name.transform.localPosition = new Vector2(name.transform.localPosition.x, ((nameText.preferredHeight / 2) + 30) * -1);

            #endregion

        }


    }
}
