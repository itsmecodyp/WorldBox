using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using LargeNumbers;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomBlackjack
{

    // worldbox version with firebase support

    [BepInPlugin("cody.gamba", "SimpleGambling", "0.0.0.2")]
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

            crashLargestWinC = Config.AddSetting("Money", "Crash largest winC", 500d, "Money money (don't cheat please)");
            crashLargestWinM = Config.AddSetting("Money", "Crash largest winM", 0, "Money money (don't cheat please)");
            crashLargestWinMult = Config.AddSetting("Money", "Crash largest win mult", 0f, "The multiplier when the largest win was achieved");


            humanMoneyC = Config.AddSetting("Money", "Human MoneyC", 500d, "Money money (don't cheat please)");
            humanMoneyM = Config.AddSetting("Money", "Human MoneyM", 0, "Money money (don't cheat please)");

            aiMoney1 = Config.AddSetting("General", "AI 1 Money", 500f, "Money money");
            aiMoney2 = Config.AddSetting("General", "AI 2 Money", 500f, "Money money");
            aiMoney3 = Config.AddSetting("General", "AI 3 Money", 500f, "Money money");

            printersLevelOne = Config.AddSetting("Money", "Level 1 printers", 0d, "Number of level one printers you have");
            printersLevelTwo = Config.AddSetting("Money", "Level 2 printers", 0d, "Number of level two printers you have");
            printersLevelThree = Config.AddSetting("Money", "Level 3 printers", 0d, "Number of level three printers you have");


            blackJackLosses = Config.AddSetting("Stats", "Blackjack losses", 0d, "Times you've lost a round of blackjack");
			blackJackWins = Config.AddSetting("Stats", "Blackjack wins", 0d, "Times you've won a round of blackjack");
            blackjackBlackjacks = Config.AddSetting("Stats", "Blackjack blackjacks", 0d, "Times you've had blackjack");

            blackJackPushes = Config.AddSetting("Stats", "Blackjack pushes", 0d, "Times you've pushed in a round of blackjack");

            blackjackGames = Config.AddSetting("Stats", "Blackjack Games", 0d, "Total times you've played a round of blackjack");

            totalIncomeClicker = Config.AddSetting("Stats", "Total clicker income", 0d, "Total money youve made from clicker minigame");
            totalIncomeIdle = Config.AddSetting("Stats", "Total idle income", 0d, "Total money youve made from idle minigame");
            totalIncomeBlackjack = Config.AddSetting("Stats", "Total blackjack income", 0d, "Total money youve made from blackjack minigame");
            totalIncomeStocks = Config.AddSetting("Stats", "Total stock income", 0d, "Total money youve made from stocks");

            playerName = Config.AddSetting("General", "Player name", "Human", "The name displayed for player's window");

            Blackjack.currentPlayers.Clear();
            activePlayer = 0;
            humanPlayer = new BlackjackPlayer(playerName.Value);
            humanPlayer.money = new LargeNumber(humanMoneyC.Value, humanMoneyM.Value);
            Blackjack.currentPlayers.Add(humanPlayer);
            InvokeRepeating("CheckClickerPurchases", 10f, 10f);
        }

        public void CheckClickerPurchases()
        {
            idleClicker.CheckClickerPurchases();
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

        public static ConfigEntry<double> crashLargestWinC { get; set; }
        public static ConfigEntry<int> crashLargestWinM { get; set; }

        public static LargeNumber crashLargestWin => new LargeNumber(crashLargestWinC.Value, crashLargestWinM.Value);

        public static ConfigEntry<float> crashLargestWinMult { get; set; }


        public static ConfigEntry<string> cardTexturesPath{get; set;}

        public static ConfigEntry<string> playerName { get; set; }


        public static ConfigEntry<double> humanMoneyC // coefficient
        {
            get; set;
        }
        public static ConfigEntry<int> humanMoneyM { //magnitude
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
        public static ConfigEntry<double> blackjackBlackjacks {
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
        public static ConfigEntry<double> blackJackPushes {
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

        
        //resize windows constantly, 1 frame update
        public void Update()
		{
            foreach(BlackjackPlayer player in Blackjack.currentPlayers) {
                player.personalWindowRect.height = 1f;
            }
            MainWindowRect.height = 1f;
            Blackjack.dealerWindowRect.height = 1f;
            WorkWindowRect.height = 1f;
			if(Input.GetKey(KeyCode.H)) {
                foreach(BlackjackPlayer player in Blackjack.currentPlayers) {
                    player.money *= 10;
                }
			}
            if(Input.GetKeyDown(KeyCode.J)) {
                foreach(BlackjackPlayer player in Blackjack.currentPlayers) {
                    player.money *= 10;
                }
            }
        }

        public Clicker idleClicker = new Clicker();

        public void OnGUI()
        {

            foreach (BlackjackPlayer player in Blackjack.currentPlayers)
            {
                player.personalWindowRect = GUILayout.Window(player.windowID, player.personalWindowRect, player.PlayerWindow, "Player: " + player.playername, GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f));
            }

            if(showHideMainWindow)
            {
                //main window = add ai window
                //MainWindowRect = GUILayout.Window(4300, MainWindowRect, new GUI.WindowFunction(MainWindow), "Blackjack main", new GUILayoutOption[] { GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f) });
                Blackjack.dealerWindowRect = GUILayout.Window(4303, Blackjack.dealerWindowRect, Blackjack.DealerWindow, "Dealer", GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f));

                // idle game
                WorkWindowRect = GUILayout.Window(43011, WorkWindowRect, idleClicker.ClickerWindow, "SimpleIdleClicker", GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f));

            }

           
            GUILayout.BeginArea(new Rect(Screen.width - 120, 150, 120, 50));

            if (GUILayout.Button("CustomBlackjack"))
            {
                showHideMainWindow = !showHideMainWindow;
            }
           
            /*
            if (GUILayout.Button("Stocks"))
            {
                Stocks.showHideStocks = !Stocks.showHideStocks;
            }
            */
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(Screen.width - 120, 175, 120, 50));

            if(GUILayout.Button("CustomCrash")) {
                showHideCrashWindow = !showHideCrashWindow;
            }
			if(showHideCrashWindow) {
                crashWindowRect = GUILayout.Window(4605, crashWindowRect, crashGame.CrashWindow, "SimpleCrash", GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f));
                //crashGame.CrashWindow(4605);
            }
            GUILayout.EndArea();

        }

        public Rect crashWindowRect;
        public bool showHideCrashWindow;

        public Crash crashGame = new Crash();

        
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
                /* no need, and it gets confusing, most of the code is designed for 1v1
                if (GUILayout.Button("Add AI Player to the game"))
                {
                    Blackjack.currentPlayers.Add(new BlackjackPlayer());
                }
                */
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
        public int activePlayer;

    }


}
