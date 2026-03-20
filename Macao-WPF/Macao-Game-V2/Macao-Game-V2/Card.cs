using System;
using System.Collections.Generic;
using System.Text;

namespace Macao_Game_V2
{
    public class Card
    {
        private string cardValue;
        private char cardSuit;
        private bool cardIsJoker;

        public Card(string value, char suit, bool joker)
        {
            cardValue = value;
            cardSuit = suit;
            cardIsJoker = joker;
        }

        public string Value
        {
            get => cardValue;
            set => cardValue = value;
        }

        public char Suit
        {
            get => cardSuit;
            set => cardSuit = value;
        }

        public bool IsJoker
        {
            get => cardIsJoker;
            set => cardIsJoker = value;
        }

        public bool IsCardJoker()
        {
            return cardIsJoker;
        }

        public bool IsCardValid(Card topCard, int cardsToDraw)
        {
            if (cardsToDraw > 0)
                return (cardValue == "2" || cardValue == "3" || cardIsJoker);

            if (cardIsJoker || cardValue == "7")
                return true;

            if (topCard.cardIsJoker)
                return true;

            return (cardSuit == topCard.cardSuit || cardValue == topCard.cardValue);
        }

        public static bool operator ==(Card card1, Card card2)
        {
            if (ReferenceEquals(card1, null) && ReferenceEquals(card2, null))
                return true;
            if (ReferenceEquals(card1, null) || ReferenceEquals(card2, null))
                return false;
            return card1.cardValue == card2.cardValue && card1.cardSuit == card2.cardSuit && card1.cardIsJoker == card2.cardIsJoker;
        }

        public static bool operator !=(Card card1, Card card2)
        {
            return !(card1 == card2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Card c)
                return this == c;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(cardValue, cardSuit, cardIsJoker);
        }

        public override string ToString()
        {
            if (cardIsJoker) return "Joker";
            return $"{cardValue}{cardSuit}";
        }
    }
}
