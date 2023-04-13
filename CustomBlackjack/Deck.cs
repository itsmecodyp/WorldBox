using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomBlackjack
{
    public class Deck
    {
        public List<Card> cards;

        private System.Random random;

        public Deck()
        {
            random = new System.Random();
            cards = new List<Card>();

            // add cards to deck
            for (int c = 0; c < Card.values.Length; c++)
            {
                if (c != 0) // 0 is placeholder/empty in array (so count stays a proper 52)
                {
                    for (int t = 0; t < Card.suits.Length; t++)
                    {
                        cards.Add(new Card(c, t));
                    }
                }
            }

            Debug.Log("Deck reset, now shuffling");
            Shuffle();
        }

        public Card DrawCard()
        {
            var card = cards.First();
            cards.Remove(card);
            Blackjack.currentCardNumber++;
            return card;
        }

        public void Shuffle()
        {
            var list = cards.ToList();

            for (int i = 0; i < cards.Count; i++)
            {
                var index = random.Next(list.Count);

                cards[i] = list[index];
                list.RemoveAt(index);
            }
        }


    }
}
