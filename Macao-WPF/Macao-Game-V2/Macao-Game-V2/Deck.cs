using System;
using System.Collections.Generic;

namespace Macao_Game_V2
{
    public class Deck
    {
        private List<Card> cards;
        private Stack<Card> drawPile;

        public Deck()
        {
            cards = new List<Card>();
            drawPile = new Stack<Card>();
            InitializeStandardDeck();
            ShuffleArray();
            CutDeck();

            // Push to draw pile
            foreach (var card in cards)
            {
                drawPile.Push(card);
            }
        }

        private void InitializeStandardDeck()
        {
            char[] suits = { '♠', '♥', '♦', '♣' };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (char suit in suits)
            {
                foreach (string val in values)
                {
                    cards.Add(new Card(val, suit, false));
                }
            }
            // Add Jokers
            cards.Add(new Card("Joker", ' ', true));
            cards.Add(new Card("Joker", ' ', true));
        }

        public void Reshuffle(IEnumerable<Card> discardPileCards)
        {
            List<Card> tempCards = new List<Card>(discardPileCards);
            Random random = new Random();
            int n = tempCards.Count;
            while (n > 1)
            {
                int k = random.Next(n--);
                Card temp = tempCards[n];
                tempCards[n] = tempCards[k];
                tempCards[k] = temp;
            }

            foreach (var c in tempCards)
            {
                drawPile.Push(c);
            }
        }

        private void ShuffleArray()
        {
            Random random = new Random();
            int n = cards.Count;
            while (n > 1)
            {
                int k = random.Next(n--);
                Card temp = cards[n];
                cards[n] = cards[k];
                cards[k] = temp;
            }
        }

        private void CutDeck()
        {
            Random random = new Random();
            int cutPoint = random.Next(1, cards.Count - 1);
            List<Card> cut1 = cards.GetRange(0, cutPoint);
            List<Card> cut2 = cards.GetRange(cutPoint, cards.Count - cutPoint);
            
            cards.Clear();
            cards.AddRange(cut2);
            cards.AddRange(cut1);
        }

        public Card DrawCard()
        {
            if (drawPile.Count == 0)
            {
                return null;
            }
            return drawPile.Pop();
        }

        public int DeckSize()
        {
            return drawPile.Count;
        }

        public bool IsEmpty()
        {
            return drawPile.Count == 0;
        }
    }
}
