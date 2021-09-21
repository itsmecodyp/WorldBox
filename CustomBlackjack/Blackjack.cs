using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomBlackjack
{
    public static class Blackjack
    {

        public enum DealerStatus
        {
            Waiting,
            Playing
        }
        public static void PlayRound()
        {
            Debug.Log("Count: " + currentPlayers.Count);
            Debug.Log("target: " + activePlayer);
            if (activePlayer < currentPlayers.Count)
            {
                BlackjackPlayer target = currentPlayers[activePlayer];
                if (target.playername == "Human")
                {
                    activePlayer++;
                    Debug.Log("Moved active player:" + (activePlayer - 1).ToString() + "->" + activePlayer.ToString() + "count:" + currentPlayers.Count.ToString());
                }
                else
                {
                    target.DrawCard();
                    midRoundEndTime = Time.realtimeSinceStartup;
                    if (target.waiting) // will become "waiting for turn to end" during the draw
                    {
                        activePlayer++;
                        Debug.Log("Moved active player:" + (activePlayer - 1).ToString() + "->" + activePlayer.ToString() + "count:" + currentPlayers.Count.ToString());
                    }
                }
            }
            else
            {
                Blackjack.DealersTurn();
            }


        }

        public static void NewRound()
        {

            foreach (BlackjackPlayer player in currentPlayers)
            {
                player.HandlePayout(player.handStatus);
            }
            roundEndTime = Time.realtimeSinceStartup;
            readyToResetDeck = true;
        }
        public static float roundEndTime;
        public static float roundEndDelay = 3f;
        public static float midRoundEndTime;
        public static float midRoundEndDelay = 2f;

        public static float moneySaveTime;
        public static float moneySaveDelay = 3f;

        public static bool readyToResetDeck;

        public static DealerStatus currentDealerStatus = DealerStatus.Waiting;

        public static Rect dealerWindowRect = new Rect(0f, 1f, 1f, 1f);

        public static void DealerWindow(int windowID)
        {
            GUILayout.Button("Dealer status: " + currentDealerStatus);
            Blackjack.CardDisplay(dealersHand); // neat
            GUILayout.Button("Current hand total: " + Blackjack.HandTotalValue(dealersHand));
            GUI.DragWindow();
        }

        public static List<Card> dealersHand = new List<Card>();

        public static Deck blackjackDeck = new Deck();

        public static List<BlackjackPlayer> currentPlayers = new List<BlackjackPlayer>(); // AI players

        public static void CardDisplay(List<Card> targetHand)
        {
            GUILayout.Button("Total value: " + Blackjack.HandTotalValue(targetHand));
            GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f) });
            UnityEngine.Color original = GUI.contentColor;
            UnityEngine.Color original2 = GUI.backgroundColor;
            foreach (Card heldCard in targetHand)
            {
                string value = heldCard.value.ToString();
                if (heldCard.stringValue == "Ace")
                {
                    value = "A";
                }
                else if (heldCard.stringValue == "King")
                {
                    value = "K";
                }
                else if (heldCard.stringValue == "Queen")
                {
                    value = "Q";
                }
                else if (heldCard.stringValue == "Jack")
                {
                    value = "J";
                }
                string color = "";
                if (heldCard.stringSuit == "Hearts" || heldCard.stringSuit == "Diamonds")
                {
                    GUI.contentColor = UnityEngine.Color.red;
                    color = "red";
                }
                else
                {
                    GUI.contentColor = UnityEngine.Color.black;
                    color = "black";
                }
                GUI.backgroundColor = UnityEngine.Color.white;
                GUILayout.Button("-------\n" +
                                 "|    <color=white>" + value + "</color>|\n" +
                                 "|  <color=" + color + "> " + Card.suitSymbols[heldCard.suit] + "</color>  |\n" +
                                 "|<color=white>" + value + "</color>    |\n" +
                                 "-------");



            }
            GUILayout.EndHorizontal();
            /*
            GUILayout.Button("-------\n" +
                             "||   ||\n" +
                             "||   ||\n" +
                             "||   ||\n" +
                              "-------");
            */
            GUI.backgroundColor = original2;
            GUI.contentColor = original;
        }

        public static BlackjackPlayer currentPlayer => currentPlayers[activePlayer];
        public static List<Card> currentPlayerHand => currentPlayers[activePlayer].hand;

        public static Card PlayerDrawCard(List<Card> targetHand)
        {
            Card drawnCard = blackjackDeck.DrawCard();
            targetHand.Add(drawnCard);
            Debug.Log("Player drew card: " + drawnCard.DisplayString() + ". New total: " + HandTotalValue(targetHand)); //  + ". Cards left in deck: " + officialDeck.cards.Count
            // below removed later
            if (Player21(targetHand))
            {
                Debug.Log("Player won! Resetting.");
                ResetDeck();
            }
            if (PlayerBlackjack(targetHand))
            {
                Debug.Log("Blackjack! Player won. Resetting.");
            }
            if (PlayerBust(targetHand))
            {
                Debug.Log("Player busted! Resetting.");
                ResetDeck();
            }
            return drawnCard;
        }

        public static void DealersTurn()
        {
            Debug.Log("Players finished. Starting dealers turn.");
            // this should actually be done before the player makes their turn


            Debug.Log("Dealer drawing");
            DealerDrawCard();
            if (DealerBlackjack())
            {
                Debug.Log("Dealer blackjack");

                foreach (BlackjackPlayer player in currentPlayers)
                {
                    if (player.Player21() || player.PlayerBlackjack()) // both end 21
                    {
                        player.handStatus = HandStatus.Push;
                    }
                    if (player.handTotalValue < HandTotalValue(dealersHand))
                    {
                        player.handStatus = HandStatus.Loss;
                    }
                }
                NewRound();
            }
            if (!DealerBlackjack() && Dealer21())
            {
                Debug.Log("Dealer 21");

                foreach (BlackjackPlayer player in currentPlayers)
                {
                    if (player.Player21())
                    {
                        player.handStatus = HandStatus.Push;
                    }
                    if (!player.Player21() && player.handTotalValue < 21)
                    {
                        player.handStatus = HandStatus.Loss;
                    }
                }
                NewRound();
            }
            if (!DealerBlackjack() && !Dealer21() && DealerBust())
            {
                Debug.Log("Dealer bust");

                foreach (BlackjackPlayer player in currentPlayers)
                {
                    if (player.handStatus == HandStatus.Standing || player.handStatus == HandStatus.Playing)
                    {
                        player.handStatus = HandStatus.Win;
                    }
                }
                NewRound();
            }
            if (!DealerBlackjack() && !Dealer21() && !DealerBust() && DealerStands())
            {
                Debug.Log("Dealer stands");

                foreach (BlackjackPlayer player in currentPlayers)
                {
                    if (HandTotalValue(dealersHand) > player.handTotalValue)
                    {
                        player.handStatus = HandStatus.Loss;
                    }
                    else if (HandTotalValue(dealersHand) == player.handTotalValue)
                    {
                        player.handStatus = HandStatus.Push;
                    }
                    else if (player.PlayerBust())
                    {
                        player.handStatus = HandStatus.Loss;
                    }
                    else
                    {
                        player.handStatus = HandStatus.Win;
                    }
                }
                NewRound();
            }

        }
        public static int activePlayer = 0;
        public static void DealerDrawCard()
        {
            midRoundEndTime = Time.realtimeSinceStartup;
            Card drawnCard = blackjackDeck.DrawCard();
            Debug.Log("Dealer drew: " + drawnCard.DisplayString());
            Debug.Log("Dealer new total: " + HandTotalValue(dealersHand));

            dealersHand.Add(drawnCard);
            foreach (BlackjackPlayer player in currentPlayers)
            {
                if (player.playername == "Human")
                {
                    Main.humanMoney.Value = player.money;
                }
            } // money saving
        }

        // need to make individual card references for the displays
        public static int HandTotalValue(List<Card> targetHand)
        {
            if (targetHand.Count == 1 && targetHand[0].stringValue == "Ace")
            {
                return 1;
            }
            int totalValue = 0;
            foreach (Card heldCard in targetHand)
            {
                if (heldCard.stringValue != "Ace" && heldCard.stringValue != "King" && heldCard.stringValue != "Queen" && heldCard.stringValue != "Jack")
                {
                    totalValue += heldCard.value; // adding non-face cards
                }
                if (heldCard.stringValue != "Ace" && heldCard.stringValue == "King" || heldCard.stringValue == "Queen" || heldCard.stringValue == "Jack")
                {
                    totalValue += 10; // adding non-ace face cards
                }
            }
            foreach (Card heldCard in targetHand) // second loop to make sure aces are done last
            {
                if (heldCard.stringValue == "Ace")  // need more logic for "soft X" hands
                {
                    if (totalValue >= 11)
                    {
                        totalValue += 1;
                    }
                    else
                    {
                        totalValue += 11;
                    }
                }
            }
            return totalValue;
        }

        public static void ResetDeck()
        {
            blackjackDeck = new Deck();
            foreach (BlackjackPlayer player in currentPlayers) // ai hands reset
            {
                player.hand = new List<Card>();
                lastRoundStatus = player.handStatus;
                player.handStatus = HandStatus.None;
            }
            dealersHand = new List<Card>();
            activePlayer = 0;
            Debug.Log("Deck and hands reset");
        }
        public static HandStatus lastRoundStatus = HandStatus.None;
        public static bool PlayerBlackjack(List<Card> targetHand)
        {
            return HandTotalValue(targetHand) == 21 && targetHand.Count == 2 && (targetHand[0].stringValue == "Ace" || targetHand[1].stringValue == "Ace");
        }
        public static bool PlayerBust(List<Card> targetHand)
        {
            return HandTotalValue(targetHand) > 21;
        }
        public static bool Player21(List<Card> targetHand)
        {
            return HandTotalValue(targetHand) == 21;
        }

        public static bool DealerBlackjack()
        {
            return HandTotalValue(dealersHand) == 21 && dealersHand.Count == 2 && (dealersHand[0].stringValue == "Ace" || dealersHand[1].stringValue == "Ace");
        }
        public static bool DealerBust()
        {
            return HandTotalValue(dealersHand) > 21;
        }
        public static bool DealerStands()
        {
            return HandTotalValue(dealersHand) > 15; // dealer stands on 16
        }
        public static bool Dealer21()
        {
            return HandTotalValue(dealersHand) == 21;
        }

        public enum HandStatus
        {
            None,
            Blackjack,
            Win,
            Loss,
            Playing,
            Standing,
            Bust,
            Push
        }

    }

}
