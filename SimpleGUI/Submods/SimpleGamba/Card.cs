namespace SimpleGUI.Submods.SimpleGamba
{
    public class Card
    {
        public int value;
        public string stringValue;
        public int suit;
        public string stringSuit;

        public Card(int value, int suit)
        {
            this.value = value;
            stringValue = StringValue(values[value]);
            this.suit = suit;
            stringSuit = StringSuit(suit);
        }

        public static readonly string[] values = {
            "0", "A", "2", "3", "4", "5",
            "6", "7", "8", "9", "10",
            "J", "Q", "K"
        };

        public static readonly string[] suits = {
            "C","S","H","D" // "Clubs","Spades","Hearts","Diamonds"
        };

        public static readonly string[] suitSymbols = {
            "♣","♠","♥","♦" // "Clubs","Spades","Hearts","Diamonds"
        };


        public string StringSuit(int suit)
        {
            if (suit == 0)
            {
                return "Clubs";
            }
            if (suit == 1)
            {
                return "Spades";
            }
            if (suit == 2)
            {
                return "Hearts";
            }
            if (suit == 3)
            {
                return "Diamonds";
            }
            return "StringSuit not found: " + suit;
        }
        public string StringValue(string value)
        {
            if (value == "A")
            {
                return "Ace";
            }
            if (value == "2")
            {
                return "Two";
            }
            if (value == "3")
            {
                return "Three";
            }
            if (value == "4")
            {
                return "Four";
            }
            if (value == "5")
            {
                return "Five";
            }
            if (value == "6")
            {
                return "Six";
            }
            if (value == "7")
            {
                return "Seven";
            }
            if (value == "8")
            {
                return "Eight";
            }
            if (value == "9")
            {
                return "Nine";
            }
            if (value == "10")
            {
                return "Ten";
            }
            if (value == "J")
            {
                return "Jack";
            }
            if (value == "Q")
            {
                return "Queen";
            }
            if (value == "K")
            {
                return "King";
            }
            return "StringValue not found: " + value;
        }


        public override string ToString() // loads as AS, QS, etc
        {
            return suits[suit] + values[value];
        }
        public string ToStringReverse() // loads as AS, QS, etc
        {
            return values[value] + suits[suit];
        }
        public string DisplayString() // loads as Ace of Spades, Queen of Spades, etc
        {
            return stringValue + " of " + stringSuit;
        }

    }

}
