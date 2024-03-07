using SimplerGUI.Submods.SimpleGamba.LargeNumbers;
using UnityEngine;

namespace SimplerGUI.Submods.SimpleGamba {
	public class Clicker {

        public static BlackjackPlayer humanPlayer => Main.humanPlayer; // main player


        public void ClickerWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button("Money: " + humanPlayer.money);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Color original = GUI.backgroundColor;
            bool plusOneActive = Time.realtimeSinceStartup > plusSlotOneTime + plusSlotOneDelay;
            bool plusTwoActive = Time.realtimeSinceStartup > plusSlotTwoTime + plusSlotTwoDelay;
            bool plusThreeActive = Time.realtimeSinceStartup > plusSlotThreeTime + plusSlotThreeDelay;

            if(plusOneActive) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("+" + plusSlotOne) && plusOneActive) {
                humanPlayer.money += plusSlotOne;
                Main.totalIncomeClicker.Value = Main.totalIncomeClicker.Value + plusSlotOne;
                plusSlotOneTime = Time.realtimeSinceStartup;
            }
            if(plusTwoActive) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("+" + plusSlotTwo) && plusTwoActive) {
                humanPlayer.money += plusSlotTwo;
                Main.totalIncomeClicker.Value = Main.totalIncomeClicker.Value + plusSlotTwo;

                plusSlotTwoTime = Time.realtimeSinceStartup;
            }
            if(plusThreeActive) {
                GUI.backgroundColor = Color.green;
            }
            else {
                GUI.backgroundColor = Color.red;
            }
            if(GUILayout.Button("+" + plusSlotThree) && plusThreeActive) {
                humanPlayer.money += plusSlotThree;
                Main.totalIncomeClicker.Value = Main.totalIncomeClicker.Value + plusSlotThree;

                plusSlotThreeTime = Time.realtimeSinceStartup;
            }
            GUI.backgroundColor = original;
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Buy level 1 money printer for 500")) {
                humanPlayer.money -= 500f;
                Main.printersLevelOne.Value++;
            }
            GUILayout.Label("50 per 10 sec. You have " + Main.printersLevelOne.Value);
            if(GUILayout.Button("Buy level 2 money printer for 2500")) {
                humanPlayer.money -= 2500f;
                Main.printersLevelTwo.Value++;
            }
            GUILayout.Label("375 per 10 sec. You have " + Main.printersLevelTwo.Value);
            if(GUILayout.Button("Buy level 3 money printer for 10000")) {
                humanPlayer.money -= 10000f;
                Main.printersLevelThree.Value++;
            }
            GUILayout.Label("2% total per 10 sec. You have " + Main.printersLevelThree.Value);
            GUILayout.Button("Total printing amount: " + ClickerPurchaseReward());
            GUILayout.Button("Amount in stash: " + printedStash);
            if(GUILayout.Button("Collect")) {
                Main.totalIncomeIdle.Value = Main.totalIncomeIdle.Value + printedStash;
                humanPlayer.money += printedStash;
                printedStash = new LargeNumber(0f);
                Main.SaveMoney();
            }
            GUI.DragWindow();
        }

        public double levelOnePrinterReward = 50f;
        public double levelTwoPrinterReward = 375f;

        public LargeNumber ClickerPurchaseReward()
        {
            LargeNumber totalAmount = new LargeNumber(0f);
            LargeNumber amount = new LargeNumber(0f);
            for(int i = 0; i < Main.printersLevelOne.Value; i++) {
                totalAmount += levelOnePrinterReward;
            }
            for(int i = 0; i < Main.printersLevelTwo.Value; i++) {
                totalAmount += levelTwoPrinterReward;
            }
            amount = (printedStash + humanPlayer.money) * .02f; // 2% increase
            for(int i = 0; i < Main.printersLevelThree.Value; i++) {
                totalAmount += amount;
            }
            return totalAmount;
        }

        public void CheckClickerPurchases()
        {
            printedStash += ClickerPurchaseReward();
        }

        public float plusSlotOne = 1f;
        public float plusSlotTwo = 10f;
        public float plusSlotThree => plusSlotTwo * 5f;

        public float plusSlotOneMult = 1.0f;
        public float plusSlotTwoMult = 1.0f;
        public float plusSlotThreeMult = 1.0f;

        public float plusSlotOneDelay = 1f;
        public float plusSlotTwoDelay = 3f;
        public float plusSlotThreeDelay => plusSlotTwoDelay * 2.5f; // 2.5x the delay


        public float plusSlotOneTime = Time.realtimeSinceStartup;
        public float plusSlotTwoTime = Time.realtimeSinceStartup;
        public float plusSlotThreeTime = Time.realtimeSinceStartup;


        public LargeNumber printedStash = new LargeNumber(0f);

    }
}
