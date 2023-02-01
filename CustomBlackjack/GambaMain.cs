using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Proyecto26;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomBlackjack
{

    // worldbox version with firebase support

    [BepInPlugin("cody.blackjack", "SimpleBlackjack", "0.0.0.1")]
    public class Main : BaseUnityPlugin
    {

        public static ConfigEntry<double> stockAmountWLD
        {
            get; set;
        }
        public static ConfigEntry<double> stockAmountDON
        {
            get; set;
        }
        public static ConfigEntry<double> stockAmountMAS
        {
            get; set;
        }
        public static ConfigEntry<double> stockAmountMAX
        {
            get; set;
        }
        public static ConfigEntry<double> stockAmountMST
        {
            get; set;
        }
        public static ConfigEntry<double> stockAmountSOR
        {
            get; set;
        }

        public static ConfigEntry<double> stockBoughtTotal
        {
            get; set;
        }
        public static ConfigEntry<double> stockSoldTotal
        {
            get; set;
        }
        public static ConfigEntry<double> stockBoughtTotalValue
        {
            get; set;
        }
        public static ConfigEntry<double> stockSoldTotalValue
        {
            get; set;
        }
        public static Dictionary<string, double> userStockDict = new Dictionary<string, double>();

        public void Awake()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            stockAmountSOR = Config.AddSetting("Stock", "SOR Stock", 0d, "The amount of stock you're holding (don't cheat please)");
#pragma warning restore CS0618 // Type or member is obsolete
            stockAmountMST = Config.AddSetting("Stock", "MST Stock", 0d, "The amount of stock you're holding (don't cheat please)");
            stockAmountMAX = Config.AddSetting("Stock", "MAX Stock", 0d, "The amount of stock you're holding (don't cheat please)");
            stockAmountMAS = Config.AddSetting("Stock", "MAS Stock", 0d, "The amount of stock you're holding (don't cheat please)");
            stockAmountDON = Config.AddSetting("Stock", "DON Stock", 0d, "The amount of stock you're holding (don't cheat please)");
            stockAmountWLD = Config.AddSetting("Stock", "WLD Stock", 0d, "The amount of stock you're holding (don't cheat please)");
            userStockDict.Add("SOR", stockAmountSOR.Value);
            userStockDict.Add("WLD", stockAmountWLD.Value);
            userStockDict.Add("MAS", stockAmountMAS.Value);
            userStockDict.Add("MAX", stockAmountMAX.Value);
            userStockDict.Add("DON", stockAmountDON.Value);
            userStockDict.Add("MST", stockAmountMST.Value);

            stockBoughtTotal = Config.AddSetting("Stock", "Stock bought", 0d, "Total number of stocks ever bought");
            stockSoldTotal = Config.AddSetting("Stock", "Stock sold", 0d, "Total number stocks ever sold");


            stockBoughtTotalValue = Config.AddSetting("Stock", "Stock bought value", 0d, "Total value of all stocks bought");
            stockSoldTotalValue = Config.AddSetting("Stock", "Stock sold value", 0d, "Total value of all stocks sold");


            humanMoney = Config.AddSetting("Money", "Human Money", (double)500f, "Money money (don't cheat please)");
            aiMoney1 = Config.AddSetting("General", "AI 1 Money", 500f, "Money money");
            aiMoney2 = Config.AddSetting("General", "AI 2 Money", 500f, "Money money");
            aiMoney3 = Config.AddSetting("General", "AI 3 Money", 500f, "Money money");

            printersLevelOne = Config.AddSetting("Money", "Level 1 printers", 0d, "Number of level one printers you have");
            printersLevelTwo = Config.AddSetting("Money", "Level 2 printers", 0d, "Number of level two printers you have");
            printersLevelThree = Config.AddSetting("Money", "Level 3 printers", 0d, "Number of level three printers you have");


            blackJackLosses = Config.AddSetting("Stats", "Blackjack losses", 0d, "Times you've lost a round of blackjack");
            blackJackWins = Config.AddSetting("Stats", "Blackjack wins", 0d, "Times you've won a round of blackjack");
            blackjackGames = Config.AddSetting("Stats", "Blackjack Games", 0d, "Total times you've played a round of blackjack");

            totalIncomeClicker = Config.AddSetting("Stats", "Total clicker income", 0d, "Total money youve made from clicker minigame");
            totalIncomeIdle = Config.AddSetting("Stats", "Total idle income", 0d, "Total money youve made from idle minigame");
            totalIncomeBlackjack = Config.AddSetting("Stats", "Total blackjack income", 0d, "Total money youve made from blackjack minigame");
            totalIncomeStocks = Config.AddSetting("Stats", "Total stock income", 0d, "Total money youve made from stocks");


            Blackjack.currentPlayers.Clear();
            activePlayer = 0;
            humanPlayer = new BlackjackPlayer("Human");
            humanPlayer.money = humanMoney.Value;
            Blackjack.currentPlayers.Add(humanPlayer);
            InvokeRepeating("CheckClickerPurchases", 10f, 10f);
        }

        IEnumerator GetText(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.error != null && www.error != "")
            {
                Debug.LogError(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);

                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
            }
        }


        public static ConfigEntry<string> cardTexturesPath{get; set;}

        public static ConfigEntry<double> humanMoney
        {
            get; set;
        }

        public static ConfigEntry<double> printersLevelOne
        {
            get; set;
        }
        public static ConfigEntry<double> printersLevelTwo
        {
            get; set;
        }
        public static ConfigEntry<double> printersLevelThree
        {
            get; set;
        }

        public static ConfigEntry<double> totalIncomeClicker
        {
            get; set;
        }
        public static ConfigEntry<double> totalIncomeIdle
        {
            get; set;
        }
        public static ConfigEntry<double> totalIncomeBlackjack
        {
            get; set;
        }
        public static ConfigEntry<double> totalIncomeStocks
        {
            get; set;
        }

        public static ConfigEntry<double> blackjackGames
        {
            get; set;
        }
        public static ConfigEntry<double> blackJackWins
        {
            get; set;
        }
        public static ConfigEntry<double> blackJackLosses
        {
            get; set;
        }

        //kinda useless
        public static ConfigEntry<float> aiMoney1
        {
            get; set;
        }
        public static ConfigEntry<float> aiMoney2
        {
            get; set;
        }
        public static ConfigEntry<float> aiMoney3
        {
            get; set;
        }

        public static BlackjackPlayer humanPlayer; // main player

        public bool showHideMainWindow;

        public static Rect MainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public Rect WorkWindowRect = new Rect(0f, 1f, 1f, 1f);
        public bool showHideWorkWindow;

        public static Rect AI1WindowRect = new Rect(0f, 1f, 1f, 1f);

        public static Main instance = new Main();

        public double levelOnePrinterReward = 50f;
        public double levelTwoPrinterReward = 375f;

        public double ClickerPurchaseReward()
        {
            double totalAmount = 0f;
            double amount = 0f;
            for (int i = 0; i < printersLevelOne.Value; i++)
            {
                totalAmount += levelOnePrinterReward;
            }
            for (int i = 0; i < printersLevelTwo.Value; i++)
            {
                totalAmount += levelTwoPrinterReward;
            }
            amount = (double)((printedStash + humanPlayer.money) * .02f); // 2% increase
            for (int i = 0; i < printersLevelThree.Value; i++)
            {
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

        public void ClickerWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button("Money: " + Stocks.moneyString(humanPlayer.money));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            UnityEngine.Color original = GUI.backgroundColor;
            bool plusOneActive = Time.realtimeSinceStartup > plusSlotOneTime + plusSlotOneDelay;
            bool plusTwoActive = Time.realtimeSinceStartup > plusSlotTwoTime + plusSlotTwoDelay;
            bool plusThreeActive = Time.realtimeSinceStartup > plusSlotThreeTime + plusSlotThreeDelay;

            if (plusOneActive)
            {
                GUI.backgroundColor = UnityEngine.Color.green;
            }
            else
            {
                GUI.backgroundColor = UnityEngine.Color.red;
            }
            if (GUILayout.Button("+" + plusSlotOne) && plusOneActive)
            {
                humanPlayer.money += plusSlotOne;
                totalIncomeClicker.Value = totalIncomeClicker.Value + plusSlotOne;
                plusSlotOneTime = Time.realtimeSinceStartup;
            }
            if (plusTwoActive)
            {
                GUI.backgroundColor = UnityEngine.Color.green;
            }
            else
            {
                GUI.backgroundColor = UnityEngine.Color.red;
            }
            if (GUILayout.Button("+" + plusSlotTwo) && plusTwoActive)
            {
                humanPlayer.money += plusSlotTwo;
                totalIncomeClicker.Value = totalIncomeClicker.Value + plusSlotTwo;

                plusSlotTwoTime = Time.realtimeSinceStartup;
            }
            if (plusThreeActive)
            {
                GUI.backgroundColor = UnityEngine.Color.green;
            }
            else
            {
                GUI.backgroundColor = UnityEngine.Color.red;
            }
            if (GUILayout.Button("+" + plusSlotThree) && plusThreeActive)
            {
                humanPlayer.money += plusSlotThree;
                totalIncomeClicker.Value = totalIncomeClicker.Value + plusSlotThree;

                plusSlotThreeTime = Time.realtimeSinceStartup;
            }
            GUI.backgroundColor = original;
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Buy level 1 money printer for 500"))
            {
                humanPlayer.money -= 500f;
                printersLevelOne.Value++;
            }
            GUILayout.Label("50 per 10 sec. You have " + printersLevelOne.Value);
            if (GUILayout.Button("Buy level 2 money printer for 2500"))
            {
                humanPlayer.money -= 2500f;
                printersLevelTwo.Value++;
            }
            GUILayout.Label("375 per 10 sec. You have " + printersLevelTwo.Value);
            if (GUILayout.Button("Buy level 3 money printer for 10000"))
            {
                humanPlayer.money -= 10000f;
                printersLevelThree.Value++;
            }
            GUILayout.Label("2% total per 10 sec. You have " + printersLevelThree.Value);
            GUILayout.Button("Total printing amount: " + ClickerPurchaseReward());
            GUILayout.Button("Amount in stash: " + printedStash);
            if (GUILayout.Button("Collect"))
            {
                totalIncomeIdle.Value = totalIncomeIdle.Value + printedStash;
                humanPlayer.money += printedStash;
                printedStash = 0f;
            }
            GUI.DragWindow();
        }

        public double printedStash = 0f;

        //resize windows constantly, 1 frame update
        public void Update()
		{
            foreach(BlackjackPlayer player in Blackjack.currentPlayers) {
                player.personalWindowRect.height = 1f;
            }
            MainWindowRect.height = 1f;
            Blackjack.dealerWindowRect.height = 1f;
            WorkWindowRect.height = 1f;
        }

        public void OnGUI()
        {

            foreach (BlackjackPlayer player in Blackjack.currentPlayers)
            {
                player.personalWindowRect = GUILayout.Window(player.windowID, player.personalWindowRect, new GUI.WindowFunction(player.PlayerWindow), "Player: " + player.playername, new GUILayoutOption[] { GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f) });
            }

            if(showHideMainWindow)
            {
                MainWindowRect = GUILayout.Window(4300, MainWindowRect, new GUI.WindowFunction(MainWindow), "Blackjack main", new GUILayoutOption[] { GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f) });
                Blackjack.dealerWindowRect = GUILayout.Window(4303, Blackjack.dealerWindowRect, new GUI.WindowFunction(Blackjack.DealerWindow), "Dealer", new GUILayoutOption[] { GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f) });

                // idle game
                WorkWindowRect = GUILayout.Window(43011, WorkWindowRect, new GUI.WindowFunction(ClickerWindow), "SimpleIdleClicker", new GUILayoutOption[] { GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f) });

            }

           
            GUILayout.BeginArea(new Rect(Screen.width - 120, 150, 120, 50));

            if (GUILayout.Button("CustomBlackjack"))
            {
                showHideMainWindow = !showHideMainWindow;
            }
            if (GUILayout.Button("Stocks"))
            {
                Stocks.showHideStocks = !Stocks.showHideStocks;
            }

            GUILayout.EndArea();
        }

        
        public Card lastCard;
        public Texture lastTexture;

        public void MainWindow(int windowID)
        {
            if (!Blackjack.currentPlayers.Contains(humanPlayer))
            {
                if (GUILayout.Button("Sit to play blackjack"))
                {
                }
            }
            if (Blackjack.currentPlayers.Contains(humanPlayer))
            {
                if (GUILayout.Button("Add AI Player to the game"))
                {
                    Blackjack.currentPlayers.Add(new BlackjackPlayer());
                }
            }
            /*
            if (lastCard != null)
            {
                
                GUILayout.Box("-------\n" +
                              "|     |\n" +
                              "|     |\n" +
                              "|     |\n" +
                              "-------");
                GUILayout.Button("Last card tostring: " + lastCard.ToString());
                GUILayout.Button("Last card display: " + lastCard.DisplayString());
            }
          */
            GUI.DragWindow();
        }

        public int playerCount = 2;
        public int activePlayer = 0;

    }


}
