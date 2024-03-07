using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace SimplerGUI.Submods.SimpleGamba
{
    public class Deck
    {
        public List<Card> cards = new List<Card>();

        private Random random;

        public Deck()
        {
            random = new Random();
            cards = new List<Card>();
            SetupDeck();
            Shuffle();
        }

        public void SetupDeck()
        {
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
        }

        public Card DrawCard()
        {
            if (cards != null && cards.Count > 0) {
                Card card = cards.First();
                return card;
            }
            else
            {
                Debug.Log("cards null, cant draw! resetting deck");
                SetupDeck();
                Debug.Log("Deck reset, now shuffling");
                Shuffle();
                return null;
            }
        }

        public Blackjack man;

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
