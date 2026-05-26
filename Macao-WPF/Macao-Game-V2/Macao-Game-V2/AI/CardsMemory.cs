using System;
using System.Collections.Generic;
using System.Linq;
using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.AI
{
    public class CardsMemory : ICardsMemory
    {
        public const int Capacity = 54;
        private const int CardsPerSuit = 13;
        private const int TotalDeckSize = 54;
        private readonly List<ICard> _buffer = new List<ICard>(Capacity);

        public IReadOnlyList<ICard> PlayedCards => _buffer.AsReadOnly();

        public void AddCard(ICard card)
        {
            if (_buffer.Count >= Capacity)
                return;

            _buffer.Add(card);
        }

        public void Reset() => _buffer.Clear();
        public int SeenCountBySuit(char suit) => _buffer.Count(c => !c.IsJoker && c.Suit == suit);

        public double ProbabilityOpponentLacksSuit(char suit, List<ICard> aiHand, int opponentCardCount)
        {
            if (opponentCardCount <= 0)
                return 1.0;

            int seenOfSuit = SeenCountBySuit(suit);
            int aiHasOfSuit = aiHand.Count(c => !c.IsJoker && c.Suit == suit);
            int totalKnown = _buffer.Count + aiHand.Count;
            int remainingOfSuit = Math.Max(0, CardsPerSuit - seenOfSuit - aiHasOfSuit);
            int totalUnknown = Math.Max(1, TotalDeckSize - totalKnown);

            double fractionOfSuit = (double)remainingOfSuit / totalUnknown;

            double probLacks = Math.Pow(1.0 - fractionOfSuit, opponentCardCount);
            return probLacks;
        }
    }
}
