using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CustomBlackjack.Blackjack;

namespace CustomBlackjack
{
    public class BlackjackPlayer
    {

        public BlackjackPlayer(string name = null)
        {
            playername = name;
            if (playername == "Human")
            {
                money = Main.humanMoney.Value; // or wherever else value is stored / default start value
            }
            else
            {
                money = UnityEngine.Random.Range(1f, 1000f);
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
            if (roundEndTime + roundEndDelay < Time.realtimeSinceStartup) // UnityEngine.Random.Range(3f, roundEndDelay) 
            {
                if (readyToResetDeck)
                {
                    Blackjack.ResetDeck();
                    readyToResetDeck = false;
                }
            }
            if (Time.realtimeSinceStartup > midRoundEndTime + UnityEngine.Random.Range(1f, midRoundEndDelay) && currentPlayers[0].waiting)
            {
                Blackjack.PlayRound();
            }
            GUILayout.BeginHorizontal();
            if (handStatus != HandStatus.None)
            {
                GUILayout.Button("Status: " + handStatus.ToString());
            }
            if (lastRoundStatus != HandStatus.None)
            {
                GUILayout.Button("LastRound: " + lastRoundStatus.ToString());
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
            GUILayout.Label("Money: " + moneyString(money));
            GUILayout.Label("CurrentBet: " + betAmountDouble);
            GUILayout.EndHorizontal();
            if (playername == "Human")
            {
                // force round to continue despite other checks not finding anything
                // if(GUILayout.Button("PlayRound")) { Blackjack.PlayRound(); }
                if (betting)
                {
                    GUILayout.BeginHorizontal();
                    if (betting)
                    {
                        if (GUILayout.Button("Bet"))
                        {
                            if (!float.TryParse(betAmount, out float bet))
                            {
                                return;
                            }
                            betAmountDouble = bet;
                            betting = false;
                            money -= betAmountDouble;
                            handStatus = HandStatus.Playing;
                            //Debug.Log("Round started");
                            if (dealersHand.Count == 0)
                            {
                                DealerDrawCard(); // draw first card, maybe before this round
                            }
                            foreach (BlackjackPlayer player in currentPlayers)
                            {
                                player.DrawCard();
                                player.DrawCard();
                            }
                        }
                        betAmount = GUILayout.TextField(betAmount);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                switch (handStatus)
                {
                    case HandStatus.Playing:
                        if (GUILayout.Button("Hit"))
                        {
                            DrawCard();
                        }
                        if (hand.Count == 2 && PlayerBlackjack() == false && Player21() == false)
                        {
                            if (GUILayout.Button("Double"))
                            {

                                DrawCard();
                                money -= betAmountDouble;
                                betAmountDouble *= 2;
                                TurnEnd();
                            }
                        }

                        if (GUILayout.Button("Stand"))
                        {
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
        public double betAmountDouble = 0f;

        public double money = 0;
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
            if (betting)
            {
                if (playername != "Human") // ai bet set, sloppy
                {
                    double bet = (double)UnityEngine.Random.Range(1f, 1500f);
                    betAmountDouble = bet;
                    betAmount = bet.ToString();
                    money -= betAmountDouble;
                    betting = false;
                }
                else
                {
                    Debug.Log("Player hasn't bet/started round, cancelling draw");
                    return;
                }
            }
            if (waiting)
            {
                Debug.Log("Player already finished the round, cancelling draw");
                return;
            }
            Card drawnCard = blackjackDeck.DrawCard();
            this.hand.Add(drawnCard);
            Debug.Log("Player drew card: " + drawnCard.DisplayString() + ". New total: " + HandTotalValue(this.hand)); //  + ". Cards left in deck: " + officialDeck.cards.Count

            if (PlayerBlackjack())
            {
                Debug.Log("Player Blackjack!");
                handStatus = HandStatus.Blackjack;
                TurnEnd();
            }
            if (!PlayerBlackjack() && PlayerBust())
            {
                Debug.Log("Bust!");
                handStatus = HandStatus.Bust;
                TurnEnd();
            }
            if (!PlayerBlackjack() && !PlayerBust())
            {
                if (activePlayer > 0 && handTotalValue >= 17) // auto stand at 17 for AI
                {
                    handStatus = HandStatus.Standing;
                    TurnEnd();
                }
                handStatus = HandStatus.Playing;
            }

        }

        public void TurnEnd()
        {
            waiting = true;
            if (playername == "Human")
            {
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

        public void HandlePayout(HandStatus status)
        {
            switch (status)
            {
                case HandStatus.Win:
                    double winAmount = (betAmountDouble * 2f) + ((float)streak * streakMultiplierAdditional);
                    money += winAmount;
                    streak++;
                    if (playername == "Human")
                    {
                        Main.totalIncomeBlackjack.Value += winAmount;
                        Main.blackjackGames.Value ++;
                        Main.blackJackWins.Value += winAmount;

                    }
                    break;
                case HandStatus.Loss:
                    streak = 0;
                    //money += (int)betAmountDouble;
                    if (playername == "Human")
                    {
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
            betting = true;
            waiting = false;
        }
        public int streak;
        public float streakMultiplierAdditional = 0.1f;
    }

}
