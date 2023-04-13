using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CustomBlackjack.Blackjack;
using LargeNumbers;

namespace CustomBlackjack {
    public class BlackjackPlayer {

        public HandStatus lastRoundStatus = HandStatus.None;

        public BlackjackPlayer(string name = null)
        {
            playername = name;
            if(playername == Main.playerName.Value) {
                money = new LargeNumber(Main.humanMoneyC.Value, Main.humanMoneyM.Value); // or wherever else value is stored / default start value
            }
            else {
                money = new LargeNumber(UnityEngine.Random.Range(1f, 1000f));
            }
        }

        public static string moneyString(double money)
        {
            return Stocks.moneyString(money); // lazy af
        }

        public Color originalColor = GUI.backgroundColor;

        public void PlayerWindow(int windowID)
        {
            // change window color for last game, maybe do text instead?
            if(handStatus != HandStatus.None) {
                if(lastHandResult == HandStatus.Loss) {
                    GUI.backgroundColor = Color.red;
                }
                else if(lastHandResult == HandStatus.Win) {
                    GUI.backgroundColor = Color.green;
                }
            }

            // wait before end of round, then reset
            if(roundEndTime + roundEndDelay < Time.realtimeSinceStartup) // UnityEngine.Random.Range(3f, roundEndDelay) 
            {
				// wipe hand at the end of round
				if(readyToResetHands) {
                    foreach(BlackjackPlayer player in currentPlayers) // hands reset
                    {
                        //payouts
                        //player.hand = new List<Card>();
                        player.lastRoundStatus = player.handStatus;
                        switch(player.lastRoundStatus) {
                            case HandStatus.None:
                                break;
                            case HandStatus.Blackjack:
                                player.money += (player.betAmountDouble * 2.5); // 2.5x bet for blackjack
                                Main.blackjackBlackjacks.Value += 1;
                                // should these be counted as normal wins too?
                                break;
                            case HandStatus.Win:
                                player.money += (player.betAmountDouble * 2); // 2x bet for win
                                Main.blackJackWins.Value += 1;
                                break;
                            case HandStatus.Loss:
                                Main.blackJackLosses.Value += 1;
                                break;
                            case HandStatus.Playing:
                                break;
                            case HandStatus.Standing:
                                break;
                            case HandStatus.Bust:
                                Main.blackJackLosses.Value += 1;
                                break;
                            case HandStatus.Push:
                                player.money += player.betAmountDouble; // pay back original bet
                                Main.blackJackPushes.Value += 1;
                                break;
                            default:
                                break;
                        }
                        player.betAmountDouble = new LargeNumber(0f); // reset bet amount (pointless since overridden by bet button?)
                        player.handStatus = HandStatus.None;
                    }

                    hand.Clear();
                    dealersHand.Clear();
                    betting = true;
                    readyToResetHands = false;
                    //log win stats
                    double winNumber = Main.blackJackWins.Value + Main.blackjackBlackjacks.Value;
                    Debug.Log("Blackjack win/blackjacks: " + Main.blackJackWins.Value.ToString() + "/" + Main.blackjackBlackjacks.Value.ToString());
                    Debug.Log("Blackjack win/loss/push: " + winNumber.ToString() + "/" + Main.blackJackLosses.Value.ToString() + "/" + Main.blackJackPushes.Value.ToString());
                    Debug.Log("Blackjack win/totalGames: " + winNumber.ToString() + "/" + Main.blackjackGames.Value.ToString());
                    Debug.Log("Blackjack win %: " + ((winNumber / Main.blackjackGames.Value) * 100f).ToString() + "%");
                }

                if(readyToResetDeck) {
                    Blackjack.ResetDeck();
                    readyToResetDeck = false;
                }
            }
            if(Time.realtimeSinceStartup > midRoundEndTime + UnityEngine.Random.Range(1f, midRoundEndDelay) && currentPlayers[0].waiting) {
                Blackjack.PlayRound();
            }
            GUILayout.BeginHorizontal();
            if(handStatus != HandStatus.None) {
                GUILayout.Button("Status: " + handStatus.ToString());
            }
            if(lastRoundStatus != HandStatus.None) {
                if(lastHandResult == HandStatus.Loss) {
                    GUI.backgroundColor = Color.red;
                }
                else if(lastHandResult == HandStatus.Win) {
                    GUI.backgroundColor = Color.green;
                }
                GUILayout.Button("LastRound: " + lastRoundStatus.ToString());
                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();
            /*
            if (GUILayout.Button("+ money/10"))
            {
                money += (money / 10);
            }
            if (GUILayout.Button("+ money*10"))
            {
                money += (money * 10);
            }
            if (GUILayout.Button("+ money*100"))
            {
                money += (money * 100);
            }
            */
            GUILayout.BeginHorizontal();
            GUILayout.Label("Money: " + money.ToString());
            GUILayout.Label("CurrentBet: " + betAmountDouble);
            GUILayout.EndHorizontal();
            if(playername == "Human") {
                // force round to continue despite other checks not finding anything
                // if(GUILayout.Button("PlayRound")) { Blackjack.PlayRound(); }
                if(betting) {
                    GUILayout.BeginHorizontal();
                    if(betting) {
                        if(GUILayout.Button("Bet")) {
                            if(!double.TryParse(betAmount, out double bet)) {
                                return;
                            }
                            betAmountDouble = new LargeNumber(bet);
                            betting = false;
                            money -= betAmountDouble;
                            handStatus = HandStatus.Playing;
                            //Debug.Log("Round started");
                            if(dealersHand.Count == 0) {
                                DealerDrawCard(); // draw first card, maybe before this round
                            }
                            foreach(BlackjackPlayer player in currentPlayers) {
                                player.DrawCard();
                                player.DrawCard();
                            }
                        }
                        betAmount = GUILayout.TextField(betAmount);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                switch(handStatus) {
                    case HandStatus.Playing:
                        if(GUILayout.Button("Hit")) {
                            DrawCard();
                        }
                        if(hand.Count == 2 && PlayerBlackjack() == false && Player21() == false) {
                            if(GUILayout.Button("Double")) {

                                DrawCard();
                                money -= betAmountDouble;
                                betAmountDouble *= 2;
                                handStatus = HandStatus.Standing;
                                TurnEnd();
                            }
                        }

                        if(GUILayout.Button("Stand")) {
                            handStatus = HandStatus.Standing;
                            TurnEnd();
                        }
                        break;
                    default:
                        break;
                }


                GUILayout.EndHorizontal();

            }
            GUILayout.Button("Current hand total: " + Blackjack.HandTotalValue(this.hand));
            Blackjack.CardDisplay(this.hand); // neat
            GUI.DragWindow();
        }
        public string betAmount = "0";
        public LargeNumber betAmountDouble = new LargeNumber(0f);

        public LargeNumber money = new LargeNumber(0);
        public string playername = "";
        public int windowID = UnityEngine.Random.Range(31334, 99999);
        public Rect personalWindowRect = new Rect(0f, 1f, 1f, 1f);
        public List<Card> hand = new List<Card>();
        public int handTotalValue => Blackjack.HandTotalValue(hand);

        //dict maybe
        public bool betting = true;
        public bool waiting; // waiting for dealer/round end
        public HandStatus lastHandResult = HandStatus.None;
        public HandStatus handStatus = HandStatus.None;

        public void DrawCard()
        {
            if(betting) {
                if(playername != "Human") // ai bet set, sloppy
                {
                    LargeNumber bet = new LargeNumber(UnityEngine.Random.Range(1f, 1500f));
                    betAmountDouble = bet;
                    betAmount = bet.ToString();
                    money -= betAmountDouble;
                    betting = false;
                }
                else {
                    Debug.Log("Player hasn't bet/started round, cancelling draw");
                    return;
                }
            }
            if(waiting) {
                Debug.Log("Player already finished the round, cancelling draw");
                return;
            }
            if(activePlayer > 1 && handTotalValue >= 17) // auto stand at 17 for AI
               {
                handStatus = HandStatus.Standing;
                TurnEnd();
            }
            else {
                Card drawnCard = blackjackDeck.DrawCard();
                this.hand.Add(drawnCard);
                Debug.Log("Player drew card: " + drawnCard.DisplayString() + ". New total: " + HandTotalValue(this.hand)); //  + ". Cards left in deck: " + officialDeck.cards.Count

                if(PlayerBlackjack()) {
                    Debug.Log("Player Blackjack!");
                    handStatus = HandStatus.Blackjack;
                    TurnEnd();
                }
                if(!PlayerBlackjack() && Player21()) {
                    Debug.Log("Player 21! Standing.");
                    handStatus = HandStatus.Standing;
                    TurnEnd();
                }
                if(!PlayerBlackjack() && PlayerBust()) {
                    Debug.Log("Bust!");
                    handStatus = HandStatus.Bust;
                    TurnEnd();
                }
                if(!PlayerBlackjack() && !PlayerBust()) {
                    if(activePlayer > 1 && handTotalValue >= 17) // auto stand at 17 for AI
                    {
                        handStatus = HandStatus.Standing;
                        TurnEnd();
                    }
                    handStatus = HandStatus.Playing;
                }
            }
        }

        public void TurnEnd()
        {
            waiting = true;
            if(playername == "Human") {
                midRoundEndTime = Time.realtimeSinceStartup;
            }
        }

        public bool PlayerBlackjack()
        {
            return HandTotalValue(this.hand) == 21 && this.hand.Count == 2 && (this.hand[0].stringValue == "Ace" || this.hand[1].stringValue == "Ace");
        }
        public bool PlayerBust()
        {
            return HandTotalValue(this.hand) > 21;
        }
        public bool Player21()
        {
            return HandTotalValue(this.hand) == 21;
        }
        /*
        public void HandlePayout(HandStatus status)
        {
            switch(status) {
                case HandStatus.Win:
                    double winAmount = (betAmountDouble * 2f) + ((float)streak * streakMultiplierAdditional);
                    money += winAmount;
                    streak++;
                    if(playername == "Human") {
                        Main.totalIncomeBlackjack.Value += winAmount;
                        Main.blackjackGames.Value++;
                        Main.blackJackWins.Value += winAmount;

                    }
                    break;
                case HandStatus.Loss:
                    streak = 0;
                    //money += (int)betAmountDouble;
                    if(playername == "Human") {
                        Main.totalIncomeBlackjack.Value -= betAmountDouble;
                        Main.blackjackGames.Value++;
                        Main.blackJackLosses.Value++;

                    }
                    break;
                case HandStatus.Bust:
                    streak = 0;
                    //money += (int)betAmountDouble;
                    break;
                case HandStatus.Push:
                    streak = 0;
                    //money += (int)betAmountDouble;
                    break;
                case HandStatus.Blackjack:
                    streak++;
                    money += (betAmountDouble * 2.5f) + ((float)streak * streakMultiplierAdditional);
                    break;
                default:
                    break;
            }
            waiting = false;
        }
        */
        public int streak;
        public float streakMultiplierAdditional = 0.1f;
    }

}
