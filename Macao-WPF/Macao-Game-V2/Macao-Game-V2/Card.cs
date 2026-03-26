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

        // Mapping from macao-v1 format letters to Unicode suit symbols
        private static readonly Dictionary<char, char> SuitLetterToSymbol = new Dictionary<char, char>
        {
            { 'S', '♠' },  // Spade
            { 'H', '♥' },  // Heart
            { 'D', '♦' },  // Diamond
            { 'C', '♣' }   // Club
        };

        public Card(string value, char suit, bool joker)
        {
            cardValue = value;
            cardSuit = suit;
            cardIsJoker = joker;
        }

        public Card(string cardString)
        {
            ParseCardString(cardString);
        }

        private void ParseCardString(string s)
        {
            if (s.Trim().Equals("Joker", StringComparison.OrdinalIgnoreCase))
            {
                cardValue = "Joker";
                cardSuit = ' ';
                cardIsJoker = true;
            }
            else if (s.Length >= 3) // Cards like "10T" (10 of Spades)
            {
                char suitLetter = s[s.Length - 1];
                cardValue = s.Substring(0, s.Length - 1);
                cardSuit = SuitLetterToSymbol.TryGetValue(suitLetter, out char symbol) ? symbol : suitLetter;
                cardIsJoker = false;
            }
            else if (s.Length == 2) // Cards like "2T" (2 of Spades)
            {
                char suitLetter = s[1];
                cardValue = s.Substring(0, 1);
                cardSuit = SuitLetterToSymbol.TryGetValue(suitLetter, out char symbol) ? symbol : suitLetter;
                cardIsJoker = false;
            }
            else
            {
                throw new ArgumentException($"Invalid card format: {s}");
            }
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

        public bool IsCardValid(Card topCard, int cardsToDraw, string currentTurnCardValue = null)
        {
            if (cardsToDraw > 0)
                return (cardValue == "2" || cardValue == "3" || cardIsJoker);

            // If a specific card value is required (e.g., after playing a 7), only allow that value
            if (!string.IsNullOrEmpty(currentTurnCardValue))
            {
                if (cardValue != currentTurnCardValue)
                    return false;
            }

            // Special cards that can always be played
            if (cardIsJoker || cardValue == "7")
                return true;

            // If top card is a Joker, any card can be played
            if (topCard.cardIsJoker)
                return true;

            // Normal matching rules
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

        public override bool Equals(object? obj)
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
