using System.Collections.Generic;

namespace Macao_Game_V2.Abstractions
{
    public interface ICardsMemory
    {
        IReadOnlyList<ICard> PlayedCards { get; }

        void AddCard(ICard card);
        void Reset();
        int SeenCountBySuit(char suit);
        double ProbabilityOpponentLacksSuit(char suit, List<ICard> aiHand, int opponentCardCount);
    }
}
