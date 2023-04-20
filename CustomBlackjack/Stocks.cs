using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Discord;
using Firebase.Auth;
using LargeNumbers;
using Proyecto26;
using SimpleJSON;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomBlackjack
{
    [Serializable]
    public class ModStock
    {
        public string Name = "null";
        public string FullName = "null";
        public int Price; // to int
    }

    [Serializable]
    public class ModUserEcon
    {
        public string Id = "null";
        public string DiscordName = "null";
        public string DiscordAccountID = "null";
        public string money = "0";
        public string stocksAmount = "0";
        public string stocksCurrentValue = "0";
        public string stocksTotalBoughtValue = "0";
        public string stocksTotalBuyTrades = "0";
        public string stocksTotalSoldValue = "0";
        public string stocksTotalSellTrades = "0";
        public string blackjackGames = "0";
        public string blackjackWins = "0";
        public string blackjackLosses = "0";
        public string idlePrintersLevelOne = "0";
        public string idlePrintersLevelTwo = "0";
        public string idlePrintersLevelThree = "0";
        public string totalIncomeBlackjack = "0";
        public string totalIncomeStocks = "0";
        public string totalIncomeIdle = "0";
        public string totalIncomeClicker = "0";
    }

    //[BepInPlugin("cody.stocks", "SimpleStocks", "0.0.0.1")]
    public class Stocks : BaseUnityPlugin
    {

        public static ModUserEcon myUser = new ModUserEcon();

        public void Awake()
        {
            Debug.Log("stocks awake");
            myUser.Id = SystemInfo.deviceUniqueIdentifier;
            InvokeRepeating("CheckStockPrices", 10f, 60f);
            InvokeRepeating("DiscordStuff", 15, 10); // checks for discord every 10 sec until its found, updates
                                                     //InvokeRepeating("UploadUserData", 30, 60); // done after stock price check now

            // do this better, probably just method call, not repeating
            //InvokeRepeating("CheckUserData", 5, 60000); // need to grab stored stock data from user

                                                     //UpdateAllStocks(); // added to auto-rotate, need to comment out on release versions
            InvokeRepeating("UpdateAllStocks", 30, 60); // checks for discord every 10 sec until its found, updates


        }
        string discordUser;

        public FirebaseUser myFireUser;
        public FirebaseAuth myFireAuth;

        public void DiscordStuff()
        {
            // use discord to ID users and do stuff
            if (discordUser == null)
            {
                Discord.Discord discord = DiscordTracker.discord;
                // discord.GetActivityManager().
                // discord.GetUserManager().GetCurrentUser().
                UserManager userManager;
                User user;
                if (discord != null)
                {
                    userManager = discord.GetUserManager();
                    user = userManager.GetCurrentUser();
                    discordUser = user.Username + "#" + user.Discriminator;
                    myUser.DiscordName = discordUser;
                    myUser.DiscordAccountID = user.Id.ToString();
                }
            }
        }
        public void OnGUI()
        {
            if (showHideStocks)
            {
                stockWindowRect = GUILayout.Window(43031, stockWindowRect, StockWindow, "SimpleStocks", GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f));
            }
        }
        public static bool showHideStocks;

        public void UploadUserData()
        {
            //myUser.money = moneyString(Main.humanMoney.Value);
            myUser.totalIncomeBlackjack = moneyString(Main.totalIncomeBlackjack.Value);
            myUser.totalIncomeClicker = moneyString(Main.totalIncomeClicker.Value);
            myUser.totalIncomeIdle = moneyString(Main.totalIncomeIdle.Value);
            myUser.idlePrintersLevelOne = Main.printersLevelOne.Value.ToString();
            myUser.idlePrintersLevelTwo = Main.printersLevelTwo.Value.ToString();
            myUser.idlePrintersLevelThree = Main.printersLevelThree.Value.ToString();
            myUser.blackjackGames = Main.blackjackGames.Value.ToString();
            myUser.blackjackWins = Main.blackJackWins.Value.ToString();
            myUser.blackjackLosses = Main.blackJackLosses.Value.ToString();
            myUser.totalIncomeBlackjack = moneyString(Main.totalIncomeBlackjack.Value);
            myUser.stocksAmount = moneyString((Main.stockAmountWLD.Value + Main.stockAmountDON.Value + Main.stockAmountMAS.Value + Main.stockAmountMAX.Value + Main.stockAmountMST.Value + Main.stockAmountSOR.Value));
            myUser.stocksCurrentValue = moneyString(((Main.stockAmountWLD.Value * currentStocks["WLD"].Price) + (Main.stockAmountDON.Value * currentStocks["DON"].Price) + (Main.stockAmountMAS.Value * currentStocks["MAS"].Price) + (Main.stockAmountMAX.Value * currentStocks["MAX"].Price) + (Main.stockAmountMST.Value * currentStocks["MST"].Price) + (Main.stockAmountSOR.Value * currentStocks["SOR"].Price)));
            myUser.stocksTotalBoughtValue = moneyString(Main.stockBoughtTotalValue.Value);
            myUser.stocksTotalSoldValue = moneyString(Main.stockSoldTotalValue.Value);
            myUser.stocksTotalBuyTrades = moneyString(Main.stockBoughtTotal.Value);
            myUser.stocksTotalSellTrades = moneyString(Main.stockSoldTotal.Value);
            var vURL = "https://mymods-2-default-rtdb.firebaseio.com/users/" + myUser.Id + "/.json";
            RestClient.Put(vURL, myUser);

            /* second method!
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                myUser
            });
            // user = "UserNameValue",
            //message = "MessageValue"
            var request = WebRequest.CreateHttp("https://mymods-2-default-rtdb.firebaseio.com/users.json?access_token=kOzjpLzFPpOfr6h7DrPOiqs8CQ73");
            request.Method = "PUT";
            request.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(json);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            var response = request.GetResponse();
            json = (new StreamReader(response.GetResponseStream())).ReadToEnd();
            */
        }

        public bool showStockWindow;
        public Rect stockWindowRect;

        public ModStock selectedStock;
        public void StockWindow(int windowID)
        {
            if (selectedStock != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.EndHorizontal();
                GUILayout.Button(selectedStock.Name);
                GUILayout.Button(selectedStock.FullName);
                GUILayout.Button("Price: " + selectedStock.Price);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                {
                    double amountChange = 1;
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift))
                    {
                        amountChange = 10;
                        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                        {
                            amountChange = 100;
                            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
                            {
                                amountChange = Main.userStockDict[selectedStock.Name];
                            }
                        }
                    }
                    if (Main.userStockDict[selectedStock.Name] >= amountChange)
                    {
                        Main.userStockDict[selectedStock.Name]-= amountChange;
                        Main.humanPlayer.money += selectedStock.Price * amountChange;
                        Main.stockSoldTotal.Value+= amountChange;
                        Main.stockSoldTotalValue.Value += selectedStock.Price * amountChange;
                    }
                }
                GUILayout.Button("held: " + moneyString(Main.userStockDict[selectedStock.Name]));
                GUILayout.Button("value: " + moneyString(Main.userStockDict[selectedStock.Name] * selectedStock.Price));
                if (GUILayout.Button("+"))
                {
                    double amountChange = 1d;
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift))
                    {
                        amountChange = 10;
                        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                        {
                            amountChange = 100;
                            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
                            {
                                amountChange = 1000d;
                            }
                            if ((double)selectedStock.Price * 10000 < Main.humanPlayer.money)
                            {
                                amountChange = 10000;
                            }
                            if ((double)selectedStock.Price * 100000 < Main.humanPlayer.money)
                            {
                                amountChange = 100000;
                            }
                            if ((double)selectedStock.Price * 1000000 < Main.humanPlayer.money)
                            {
                                amountChange = 1000000;
                            }
                            if ((double)selectedStock.Price * 10000000 < Main.humanPlayer.money)
                            {
                                amountChange = 10000000;
                            }
                            if ((double)selectedStock.Price * 100000000 < Main.humanPlayer.money)
                            {
                                amountChange = 100000000;
                            }
                            if ((double)selectedStock.Price * 1000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 1000000000;
                            }
                            if ((double)selectedStock.Price * 10000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 10000000000;
                            }
                            if ((double)selectedStock.Price * 100000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 100000000000;
                            }
                            if ((double)selectedStock.Price * 1000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 1000000000000;
                            }
                            if ((double)selectedStock.Price * 10000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 10000000000000;
                            }
                            if ((double)selectedStock.Price * 100000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 100000000000000;
                            }
                            if ((double)selectedStock.Price * 1000000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 1000000000000000;
                            }
                            if ((double)selectedStock.Price * 10000000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 10000000000000000;
                            }
                            if ((double)selectedStock.Price * 100000000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 100000000000000000;
                            }
                            if ((double)selectedStock.Price * 1000000000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 1000000000000000000;
                            }
                            if ((double)selectedStock.Price * 10000000000000000000 < Main.humanPlayer.money)
                            {
                                amountChange = 10000000000000000000;
                            }
                            if (selectedStock.Price * 100000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 100000000000000000000d;
                            }
                            if (selectedStock.Price * 1000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 1000000000000000000000d;
                            }
                            if (selectedStock.Price * 10000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 10000000000000000000000d;
                            }
                            if (selectedStock.Price * 100000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 100000000000000000000000d;
                            }
                            if (selectedStock.Price * 1000000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 1000000000000000000000000d;
                            }
                            if (selectedStock.Price * 10000000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 10000000000000000000000000d;
                            }
                            if (selectedStock.Price * 100000000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 100000000000000000000000000d;
                            }
                            if (selectedStock.Price * 1000000000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 1000000000000000000000000000d;
                            }
                            if (selectedStock.Price * 10000000000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 10000000000000000000000000000d;
                            }
                            if (selectedStock.Price * 100000000000000000000000000000d < Main.humanPlayer.money)
                            {
                                amountChange = 100000000000000000000000000000d;
                            }
                        }
                    }
                    if (Main.humanPlayer.money > (selectedStock.Price * amountChange))
                    {
                        Main.userStockDict[selectedStock.Name]+= amountChange;
                        Main.humanPlayer.money -= selectedStock.Price * amountChange;
                        Main.stockBoughtTotal.Value+= amountChange;
                        Main.stockBoughtTotalValue.Value += selectedStock.Price * amountChange;
                    }
                }
                GUILayout.EndHorizontal();


            }
            GUILayout.BeginHorizontal();
            foreach (ModStock stock in currentStocks.Values)
            {
                if (GUILayout.Button(stock.Name))
                {
                    selectedStock = stock;
                }
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        public void UpdateUserSave()
        {
            // updates user saved data
            Main.stockAmountSOR.Value = Main.userStockDict["SOR"];
            Main.stockAmountWLD.Value = Main.userStockDict["WLD"];
            Main.stockAmountMAX.Value = Main.userStockDict["MAX"];
            Main.stockAmountMAS.Value = Main.userStockDict["MAS"];
            Main.stockAmountDON.Value = Main.userStockDict["DON"];
            Main.stockAmountMST.Value = Main.userStockDict["MST"];
           // Main.humanMoney.Value = new LargeNumber(Main.humanPlayer.money);
        }

    
        public void UpdateStockPrice(ModStock targetstock)
        {
            Debug.Log("updatestock start: " + targetstock.Name);
            if (stockPriceHistory.ContainsKey(targetstock.Name))
            {
                stockPriceHistory[targetstock.Name].Add(targetstock.Price);
            }
            else
            {
                List<int> newStockHistory = new List<int>();
                newStockHistory.Add(targetstock.Price);
                stockPriceHistory.Add(targetstock.Name, newStockHistory);
            }
            //Debug.Log("updatestock price now: " + targetstock.Price.ToString());

            int adjustment = Random.Range(-stockAdjustmentMinMax, stockAdjustmentMinMax);
            targetstock.Price += adjustment;
            if (targetstock.Price < Random.Range(0, 100))
            {
                targetstock.Price = Random.Range(100, stockAdjustmentMinMax);
            }
            //Debug.Log("updatestock price after: " + targetstock.Price.ToString());

        }
        public int stockAdjustmentMinMax = 1500;

        public Dictionary<string, List<int>> stockPriceHistory = new Dictionary<string, List<int>>();

        public void UpdateAllStocks()
        {
            if (currentStocks != null && currentStocks.Count >= 1)
            {
                List<ModStock> workingList = currentStocks.Values.ToList();
                for (int i = 0; i < workingList.Count; i++)
                {
                    UpdateStockPrice(workingList[i]);
                    // updates even client side with no new database stock prices
                    // but can be reset as the client restarts.. for now
                }
                // if its me, update the database prices too
                if (myUser.Id == "7bd9f13c2f6f9d059ff0f67c78717839e3ed812a")
                {
                    var vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/DON/.json";
                    RestClient.Put(vURL, currentStocks["DON"]);

                    vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/MST/.json";
                    RestClient.Put(vURL, currentStocks["MST"]);

                    vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/MAX/.json";
                    RestClient.Put(vURL, currentStocks["MAX"]);

                    vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/MAS/.json";
                    RestClient.Put(vURL, currentStocks["MAS"]);

                    vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/WLD/.json";
                    RestClient.Put(vURL, currentStocks["WLD"]);

                    vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/SOR/.json";
                    RestClient.Put(vURL, currentStocks["SOR"]);
                }
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                CheckUserData();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                //CheckStockPrices();
                //UpdateAllStocks();

                // if debug/auth
                //UpdateAllStocks();


                //var vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/.json";
                /*
                Firebase.Auth.FirebaseAuth newAuth = new FirebaseAuth();
                newAuth.SignInWithEmailAndPasswordAsync("email", "pass").ContinueWithOnMainThread(delegate (Task<FirebaseUser> task)
                {
                    if (task.IsCanceled)
                    {
                        this.errorStatus("Canceled");
                        Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                        this.userLoginWindow.setLogin();
                        return;
                    }
                    if (task.IsFaulted)
                    {
                    }
                }
                */



                // needs redone

                // user stocks, why not
                //stock creation
                ModStock nikonStock = new ModStock
                {
                    Name = "DON",
                    FullName = "Don Nikon - Nikon",
                    Price = Random.Range(0, 1000),
                };
                var vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/" + nikonStock.Name + "/.json";
                RestClient.Put(vURL, nikonStock);

                ModStock mystStock = new ModStock
                {
                    Name = "MST",
                    FullName = "Misty - Myst Colors",
                    Price = Random.Range(0, 1000),
                };
                vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/" + mystStock.Name + "/.json";
                RestClient.Put(vURL, mystStock);

                ModStock maximStock = new ModStock
                {
                    Name = "MAX",
                    FullName = "Maxim - Developer of WorldBox",
                    Price = Random.Range(0, 1000),
                };
                vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/" + maximStock.Name + "/.json";
                RestClient.Put(vURL, maximStock);

                ModStock mastefStock = new ModStock
                {
                    Name = "MAS",
                    FullName = "Mastef - Developer of WorldBox",
                    Price = Random.Range(0, 1000),
                };
                vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/" + mastefStock.Name + "/.json";
                RestClient.Put(vURL, mastefStock);


                // game stocks, why not
                ModStock worldStock = new ModStock
                {
                    Name = "WLD",
                    FullName = "WorldBox - Official stock for Super WorldBox",
                    Price = Random.Range(0, 1000)
                };
                vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/" + worldStock.Name + "/.json";
                RestClient.Put(vURL, worldStock);

                ModStock sorStock = new ModStock
                {
                    Name = "SOR",
                    FullName = "Streets of Rogue - Official stock for Streets of Rogue",
                    Price = Random.Range(0, 1000)
                };
                vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/" + sorStock.Name + "/.json";
                RestClient.Put(vURL, sorStock);



                //StartCoroutine(GetText("https://pastebin.com/raw/Jh1a54rf"));
            }
        }

        public void CheckUserData() // unnecessary right now
        {
            //Debug.Log("shcecking user data");

            //var vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/.json";
            /*
            var vURL = "https://mymods-2-default-rtdb.firebaseio.com/users.json"; // " + myUser.Id + "
            RestClient.Get(vURL).Then(delegate (ResponseHelper response)
            {
                JSONNode node = JSON.Parse(response.Text);
                Debug.Log("node: " + node.Value);
                foreach (JSONNode nodeChild in node.Children)
                {
                    Debug.Log("nodechild: " + nodeChild.Value);
                    if (nodeChild.Children.ToList().Count >= 1)
                    {
                        Debug.Log("userschild: " + nodeChild.Value);
                        foreach (JSONNode nodeGrandchild in nodeChild.Children)
                        {
                            Debug.Log("usersgrandchild: " + nodeGrandchild.Value);
                        }

                    }
                }
            });
            */

            var vURLMoney = "https://mymods-2-default-rtdb.firebaseio.com/users/" + myUser.Id + "/money.json"; // " + myUser.Id + "
            RestClient.Get(vURLMoney).Then(delegate (ResponseHelper response)
            {
                JSONNode node = JSON.Parse(response.Text);
                Debug.Log("node: " + node.Value);
                double loadedMoney = double.Parse(node.Value);
                if (loadedMoney != null && loadedMoney != 0d)
                {

                    Debug.Log("old money: " + new LargeNumber(Main.humanMoneyC.Value, Main.humanMoneyM.Value));
                    Main.humanPlayer.money = new LargeNumber(loadedMoney);
                   // Main.humanMoney.Value = new LargeNumber(loadedMoney);
                   // myUser.money = Main.humanMoney.Value.ToString();

                    Debug.Log("node parsed: " + node.Value);
                   // Debug.Log("new money: " + Main.humanMoney.Value);


                }
            });
            Debug.Log("shcecking user data end");



        }

        public Dictionary<string, ModStock> currentStocks = new Dictionary<string, ModStock>();
        public void CheckStockPrices()
        {
            string time = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
            Debug.Log("shcecking schtosks at time: " + time);
            var vURL = "https://mymods-2-default-rtdb.firebaseio.com/stocks/.json";
            RestClient.Get(vURL).Then(delegate (ResponseHelper response)
            {
                JSONNode node = JSON.Parse(response.Text);
                foreach (JSONNode nodeChild in node.Children)
                {
                    if (nodeChild.Children.ToList().Count >= 1)
                    {
                        ModStock newStock = new ModStock();
                        foreach (JSONNode nodeGrandchild in nodeChild.Children)
                        {
                            Debug.Log("grandchild: " + nodeGrandchild.Value);
                            if (nodeGrandchild.Value.Length == 3)
                            {
                                if (int.TryParse(nodeGrandchild.Value, out int newPrice) == false)
                                {
                                    newStock.Name = nodeGrandchild.Value;
                                }
                                else
                                {
                                    newStock.Price = newPrice;
                                }
                            }
                            if (nodeGrandchild.Value.Length != 3)
                            {
                                if (int.TryParse(nodeGrandchild.Value, out int newPrice) == false)
                                {
                                    newStock.FullName = nodeGrandchild.Value;
                                }
                                else
                                {
                                    newStock.Price = newPrice;
                                }
                            }
                        }
                        if (newStock.Name != null & newStock.FullName != null && newStock.Price != null)
                        {
                            
                            Debug.Log("New stock: " + newStock.Name + " + " + newStock.Price + " + " + newStock.FullName);
                            currentStocks.Add(newStock.Name, newStock);
                        }
                    }
                }
            });
            UpdateUserSave(); // update saved data
            UploadUserData(); // send saved data
        }
        public static string moneyString(double money)
        {
            // possibly parse by number of 0s or sets of 3
            string returnString = money.ToString();

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + "Googol";
                return returnString;

            }

            if (money > 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Duotrigintillion";
                return returnString;

            }

            if (money > 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Untrigintillion";
                return returnString;

            }

            if (money > 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Trigintillion";
                return returnString;

            }

            if (money > 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Nonvigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Octovigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Septenvigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Sexvigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Quinvigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Quattuorvigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Trevigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Duovigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Unvigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Vigintillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 100000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Novemdecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Novemdecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Octodecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Septendecillion";
                return returnString;
                // wake me up when septemdecillions
            }

            if (money > 1000000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000000000000000d).ToString("F") + " Sexdecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000000000000d).ToString("F") + " Quindecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000000000d).ToString("F") + " Quattuordecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000000d).ToString("F") + " Tredecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000000d).ToString("F") + " Duodecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000000d).ToString("F") + " Undecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000000d)
            {
                returnString = (money / 1000000000000000000000000000000000000000d).ToString("F") + " Undecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000000f)
            {
                returnString = (money / 1000000000000000000000000000000000000f).ToString("F") + " Undecillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000000f)
            {
                returnString = (money / 1000000000000000000000000000000000f).ToString("F") + " Decillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000000f)
            {
                returnString = (money / 1000000000000000000000000000000f).ToString("F") + " Nonillion";
                return returnString;

            }

            if (money > 1000000000000000000000000000f)
            {
                returnString = (money / 1000000000000000000000000000f).ToString("F") + " Octillion";
                return returnString;

            }

            if (money > 1000000000000000000000000f)
            {
                returnString = (money / 1000000000000000000000000f).ToString("F") + " Septillion";
                return returnString;

            }

            if (money > 1000000000000000000000f)
            {
                returnString = (money / 1000000000000000000000f).ToString("F") + " Sextillion";
                return returnString;

            }

            if (money > 1000000000000000000f)
            {
                returnString = (money / 1000000000000000000f).ToString("F") + " Quintillion";
                return returnString;

            }

            if (money > 1000000000000000f)
            {
                returnString = (money / 1000000000000000f).ToString("F") + " Quadrillion";
                return returnString;

            }

            if (money > 1000000000000f)
            {
                returnString = (money / 1000000000000f).ToString("F") + " Trillion";
                return returnString;

            }

            if (money > 1000000000f)
            {
                returnString = (money / 1000000000f).ToString("F") + " Billion";
                return returnString;

            }

            if (money > 1000000f)
            {
                returnString = (money / 1000000f).ToString("F") + " Million";
                return returnString;

            }

            if (money > 1000f)
            {
                returnString = (money / 1000f).ToString("F") + " Thousand";
                return returnString;
            }
            return returnString;
        }

    }
}
