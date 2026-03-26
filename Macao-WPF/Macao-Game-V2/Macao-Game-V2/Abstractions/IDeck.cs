using System.Collections.Generic;

namespace Macao_Game_V2.Abstractions
{
    public interface IDeck
    {
        void Reshuffle(IEnumerable<ICard> discardPileCards);
        ICard DrawCard();
        int DeckSize();
        bool IsEmpty();
    }
}
