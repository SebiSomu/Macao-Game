using System.Collections.Generic;

namespace Macao_Game_V2.Abstractions
{
    public interface IAIStrategy
    {
        List<ICard> SelectCardsToPlay(IGameState gameState, List<ICard> hand);
        char ChooseSuit(IGameState gameState, List<ICard> hand, bool excludeSpecials);
    }
}
