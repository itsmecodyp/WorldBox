using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Discord;
using Proyecto26;
using SimpleGUI.Submods.SimpleGamba.LargeNumbers;
using SimpleJSON;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SimpleGUI.Submods.SimpleGamba
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
    public class Stocks
    {

        public static ModUserEcon myUser = new ModUserEcon();

        public void Awake()
        {
            Debug.Log("stocks awake");
            myUser.Id = SystemInfo.deviceUniqueIdentifier;
            //InvokeRepeating("CheckStockPrices", 10f, 60f); //need this!!!
            //UpdateAllStocks(); // added to auto-rotate, need to comment out on release versions
            //InvokeRepeating("UpdateAllStocks", 30, 60);
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
                GUILayout.Button("Held: " + Main.userStockDict[selectedStock.Name]);
                GUILayout.Button("Value: " + new LargeNumber(Main.userStockDict[selectedStock.Name] * selectedStock.Price));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                {
                    if (Main.userStockDict[selectedStock.Name] >= amountChange)
                    {
                        Main.userStockDict[selectedStock.Name]-= amountChange;
                        Main.humanPlayer.money += selectedStock.Price * amountChange;
                        Main.stockSoldTotal.Value+= amountChange;
                        Main.stockSoldTotalValue.Value += selectedStock.Price * amountChange;
                        Main.SaveMoney();
                    }
                }
                amountChange = (float)Convert.ToDouble(GUILayout.TextField(amountChange.ToString()));
                if (GUILayout.Button("+"))
                {
                    if (Main.humanPlayer.money > (selectedStock.Price * amountChange))
                    {
                        Main.userStockDict[selectedStock.Name]+= amountChange;
                        Main.humanPlayer.money -= selectedStock.Price * amountChange;
                        Main.stockBoughtTotal.Value += amountChange;
                        Main.stockBoughtTotalValue.Value += selectedStock.Price * amountChange;
                        Main.SaveMoney();
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

        public double amountChange;

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
        
        //init stocks, load previous prices from save later?
        public void CreateStocks()
        {
            ModStock newStock = new ModStock()
            {
                Name = "SOR",
                FullName = "Streets of Rogue",
                Price = Random.Range(100, stockAdjustmentMinMax)
            };
            currentStocks.Add(newStock.Name, newStock);
            newStock = new ModStock()
            {
                Name = "WLD",
                FullName = "WorldBox",
                Price = Random.Range(100, stockAdjustmentMinMax)
            };
            currentStocks.Add(newStock.Name, newStock);
            newStock = new ModStock()
            {
                Name = "MAX",
                FullName = "Maxim",
                Price = Random.Range(100, stockAdjustmentMinMax)
            };
            currentStocks.Add(newStock.Name, newStock);
            newStock = new ModStock()
            {
                Name = "MAS",
                FullName = "Mastef",
                Price = Random.Range(100, stockAdjustmentMinMax)
            };
            currentStocks.Add(newStock.Name, newStock);
            newStock = new ModStock()
            {
                Name = "DON",
                FullName = "Nikon",
                Price = Random.Range(100, stockAdjustmentMinMax)
            };
            currentStocks.Add(newStock.Name, newStock);
            newStock = new ModStock()
            {
                Name = "MST",
                FullName = "Myst",
                Price = Random.Range(100, stockAdjustmentMinMax)
            };
            currentStocks.Add(newStock.Name, newStock);
        }

        public void UpdateStocks()
        {
            if (currentStocks.Count == 0)
            {
                CreateStocks();
            }
            else
            {
                foreach (ModStock stock in currentStocks.Values)
                {
                    UpdateStockPrice(stock);
                }  
            }
            UpdateUserSave();
        }
        
        public void UpdateStockPrice(ModStock targetstock)
        {
            //wanted to do history for a graph of some sort.. lots of work
            /*
            //make sure history is setup for this particular stock, and update it
            if (stockPriceHistory.TryGetValue(targetstock.Name, out var value))
            {
                value.Add(targetstock.Price);
            }
            else
            {
                List<int> newStockHistory = new List<int> { targetstock.Price };
                stockPriceHistory.Add(targetstock.Name, newStockHistory);
            }
            */
            //adjust prices randomly
            int adjustment = Random.Range(-stockAdjustmentMinMax, stockAdjustmentMinMax);
            targetstock.Price += adjustment;
            //if price is below a random minimum, make it random (prevent negative priced stock)
            if (targetstock.Price < Random.Range(0, 100))
            {
                targetstock.Price = Random.Range(100, stockAdjustmentMinMax);
            }
            //Debug.Log("updatestock price after: " + targetstock.Price.ToString());

        }
        public int stockAdjustmentMinMax = 1500;

        public Dictionary<string, List<int>> stockPriceHistory = new Dictionary<string, List<int>>();
        
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                //CheckStockPrices();
                //UpdateAllStocks();

                // if debug/auth
                //UpdateAllStocks();
            }
        }
        
        public Dictionary<string, ModStock> currentStocks = new Dictionary<string, ModStock>();
    }
}
