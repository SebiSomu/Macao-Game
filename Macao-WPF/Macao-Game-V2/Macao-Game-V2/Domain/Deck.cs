using System;
using System.Collections.Generic;
using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.Domain
{
    public class Deck : IDeck
    {
        private List<ICard> cards;
        private Stack<ICard> drawPile;

        public Deck()
        {
            cards = new List<ICard>();
            drawPile = new Stack<ICard>();
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

        public void Reshuffle(IEnumerable<ICard> discardPileCards)
        {
            List<ICard> tempCards = new List<ICard>(discardPileCards);
            Random random = new Random();
            int n = tempCards.Count;
            while (n > 1)
            {
                int k = random.Next(n--);
                ICard temp = tempCards[n];
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
                ICard temp = cards[n];
                cards[n] = cards[k];
                cards[k] = temp;
            }
        }

        private void CutDeck()
        {
            Random random = new Random();
            int cutPoint = random.Next(1, cards.Count - 1);
            List<ICard> cut1 = cards.GetRange(0, cutPoint);
            List<ICard> cut2 = cards.GetRange(cutPoint, cards.Count - cutPoint);
            
            cards.Clear();
            cards.AddRange(cut2);
            cards.AddRange(cut1);
        }

        public ICard DrawCard()
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
