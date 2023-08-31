using System.Collections.Generic;
using UnityEngine;

namespace SimpleGUI.Submods.SimpleGamba {
    public class Blackjack {

        public enum DealerStatus {
            Waiting,
            Playing
        }
        public void PlayRound()
        {
            //Debug.Log("Count: " + currentPlayers.Count);
            //Debug.Log("target: " + activePlayer);
            if(activePlayer < currentPlayers.Count) {
                BlackjackPlayer target = currentPlayers[activePlayer];
                if(target.playername == "Human") {
                    activePlayer++;
                    Debug.Log("Moved active player:" + (activePlayer - 1) + "->" + activePlayer + "count:" + currentPlayers.Count);
                }
                else {
                    target.DrawCard();
                    midRoundEndTime = Time.realtimeSinceStartup;
                    if(target.waiting) // will become "waiting for turn to end" during the draw
                    {
                        activePlayer++;
                        Debug.Log("Moved active player:" + (activePlayer - 1) + "->" + activePlayer + "count:" + currentPlayers.Count);
                    }
                }
            }
            else {
                DealersTurn();
            }


        }

        public void NewRound()
        {
            foreach(BlackjackPlayer player in currentPlayers) {
                //player.HandlePayout(player.handStatus);
                player.waiting = false;
            }

            roundEndTime = Time.realtimeSinceStartup;
            readyToResetHands = true;
            if(blackjackDeck.cards.Count <= 7) {
                readyToResetDeck = true;
            }
            Main.blackjackGames.Value++;
        }
        public float roundEndTime;
        public float roundEndDelay = 5f;
        public float midRoundEndTime;
        public float midRoundEndDelay = 3f;

        public float moneySaveTime;
        public float moneySaveDelay = 3f;

        public bool readyToResetHands;
        public bool readyToResetDeck;

        public DealerStatus currentDealerStatus = DealerStatus.Waiting;

        public Rect dealerWindowRect = new Rect(0f, 1f, 1f, 1f);

        public void DealerWindow(int windowID)
        {
            GUILayout.Button("Dealer status: " + currentDealerStatus);
            CardDisplay(dealersHand); // neat
            GUILayout.Button("Current hand total: " + HandTotalValue(dealersHand));
            GUI.DragWindow();
        }

        public List<Card> dealersHand = new List<Card>();

        public Deck blackjackDeck = new Deck();

        public List<BlackjackPlayer> currentPlayers = new List<BlackjackPlayer>(); // AI players

        public void CardDisplay(List<Card> targetHand)
        {
            GUILayout.Button("Total value: " + HandTotalValue(targetHand));
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(200f), GUILayout.MinWidth(200f));
            Color original = GUI.contentColor;
            Color original2 = GUI.backgroundColor;
            foreach(Card heldCard in targetHand) {
                string value = heldCard.value.ToString();
                if(heldCard.stringValue == "Ace") {
                    value = "A";
                }
                else if(heldCard.stringValue == "King") {
                    value = "K";
                }
                else if(heldCard.stringValue == "Queen") {
                    value = "Q";
                }
                else if(heldCard.stringValue == "Jack") {
                    value = "J";
                }
                string color = "";
                if(heldCard.stringSuit == "Hearts" || heldCard.stringSuit == "Diamonds") {
                    GUI.contentColor = Color.red;
                    color = "red";
                }
                else {
                    GUI.contentColor = Color.black;
                    color = "black";
                }
                GUI.backgroundColor = Color.white;
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

        public List<Card> currentPlayerHand => currentPlayers[activePlayer].hand;

        /*
        public static Card PlayerDrawCard(List<Card> targetHand)
        {
            Card drawnCard = blackjackDeck.DrawCard();
            targetHand.Add(drawnCard);
            Debug.Log("Player drew card: " + drawnCard.DisplayString() + ". New total: " + HandTotalValue(targetHand)); //  + ". Cards left in deck: " + officialDeck.cards.Count
            // below removed later

            if(Player21(targetHand)) {
                Debug.Log("Player 21! Standing.");
                Main.humanPlayer.TurnEnd();
                //ResetDeck();
            }
            if(PlayerBlackjack(targetHand)) {
                Debug.Log("Blackjack! Player won. Resetting.");
                Main.blackJackWins.Value += 1;
            }
            if(PlayerBust(targetHand)) {
                Debug.Log("Player busted! Resetting.");
                Main.blackJackLosses.Value += 1;
                ResetDeck();
            }
            return drawnCard;
        }
        */

        public void DealersTurn()
        {
            Debug.Log("Players finished. Starting dealers turn.");
            if(currentPlayers[0].handStatus == HandStatus.Bust && currentPlayers.Count == 1) {
                Debug.Log("Player lost, dealer showing unflipped card");
                DealerDrawCard();
                NewRound();
            }
			else {
                Debug.Log("Dealer drawing");
                DealerDrawCard();
                if(DealerBlackjack()) {
                    Debug.Log("Dealer blackjack");

                    foreach(BlackjackPlayer player in currentPlayers) {
                        if(player.Player21() || player.PlayerBlackjack()) // both end 21
                        {
                            player.handStatus = HandStatus.Push;
                        }
                        if(player.handTotalValue < HandTotalValue(dealersHand)) {
                            player.handStatus = HandStatus.Loss;
                        }
                    }
                    NewRound();
                }
                if(!DealerBlackjack() && Dealer21()) {
                    Debug.Log("Dealer 21");
                    // something went wrong here 2/16
                    // dealer total was off for entire round, ended 20push with dealer count 21, player lost
                    foreach(BlackjackPlayer player in currentPlayers) {
                        if(player.Player21()) {
                            player.handStatus = HandStatus.Push;
                        }
                        if(!player.Player21() && player.handTotalValue < 21) {
                            player.handStatus = HandStatus.Loss;
                        }
                    }
                    NewRound();
                }
                if(!DealerBlackjack() && !Dealer21() && DealerBust()) {
                    Debug.Log("Dealer bust");

                    foreach(BlackjackPlayer player in currentPlayers) {
                        if(player.handStatus == HandStatus.Standing || player.handStatus == HandStatus.Playing) {
                            player.handStatus = HandStatus.Win;
                        }
                    }
                    NewRound();
                }
                if(!DealerBlackjack() && !Dealer21() && !DealerBust() && DealerStands()) {
                    Debug.Log("Dealer stands");

                    foreach(BlackjackPlayer player in currentPlayers) {
                        if(HandTotalValue(dealersHand) > player.handTotalValue) {
                            player.handStatus = HandStatus.Loss;
                        }
                        else if(HandTotalValue(dealersHand) == player.handTotalValue) {
                            player.handStatus = HandStatus.Push;
                        }
                        else if(player.PlayerBust()) {
                            player.handStatus = HandStatus.Loss;
                        }
                        else {
                            player.handStatus = HandStatus.Win;
                        }
                    }
                    NewRound();
                }
            }
        }
        public int activePlayer;
        public void DealerDrawCard()
        {
            midRoundEndTime = Time.realtimeSinceStartup;
            Card drawnCard = blackjackDeck.DrawCard();
            Debug.Log("Dealer drew: " + drawnCard.DisplayString());
            Debug.Log("Dealer new total: " + HandTotalValue(dealersHand));
            blackjackDeck.cards.Remove(drawnCard);
            dealersHand.Add(drawnCard);
            foreach(BlackjackPlayer player in currentPlayers) {
                if(player.playername == "Human") {
                    Main.humanMoneyC.Value = player.money.coefficient;
                    Main.humanMoneyM.Value = player.money.magnitude;

                }
            } // money saving
        }

        // need to make individual card references for the displays
        public int HandTotalValue(List<Card> targetHand)
        {
            if(targetHand.Count == 1 && targetHand[0].stringValue == "Ace") {
                return 1;
            }
            int totalValue = 0;
            foreach(Card heldCard in targetHand) {
                if(heldCard.stringValue != "Ace" && heldCard.stringValue != "King" && heldCard.stringValue != "Queen" && heldCard.stringValue != "Jack") {
                    totalValue += heldCard.value; // adding non-face cards
                }
                if(heldCard.stringValue != "Ace" && heldCard.stringValue == "King" || heldCard.stringValue == "Queen" || heldCard.stringValue == "Jack") {
                    totalValue += 10; // adding non-ace face cards
                }
            }
            foreach(Card heldCard in targetHand) // second loop to make sure aces are done last
            {
                if(heldCard.stringValue == "Ace")  // need more logic for "soft X" hands
                {
                    if(totalValue >= 11) {
                        totalValue += 1;
                    }
                    else {
                        totalValue += 11;
                    }
                }
            }
            return totalValue;
        }

        public void ResetDeck()
        {
            blackjackDeck = new Deck();
            Debug.Log("Deck reset");
        }
        public bool PlayerBlackjack(List<Card> targetHand)
        {
            return HandTotalValue(targetHand) == 21 && targetHand.Count == 2 && (targetHand[0].stringValue == "Ace" || targetHand[1].stringValue == "Ace");
        }
        public bool PlayerBust(List<Card> targetHand)
        {
            return HandTotalValue(targetHand) > 21;
        }
        public bool Player21(List<Card> targetHand)
        {
            return HandTotalValue(targetHand) == 21;
        }

        public bool DealerBlackjack()
        {
            return HandTotalValue(dealersHand) == 21 && dealersHand.Count == 2 && (dealersHand[0].stringValue == "Ace" || dealersHand[1].stringValue == "Ace");
        }
        public bool DealerBust()
        {
            return HandTotalValue(dealersHand) > 21;
        }
        public bool DealerStands()
        {
            return HandTotalValue(dealersHand) > 16; // dealer stands on 17+
        }
        public bool Dealer21()
        {
            return HandTotalValue(dealersHand) == 21;
        }

        public enum HandStatus {
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
