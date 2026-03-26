using System.Collections.Generic;

namespace Macao_Game_V2.Abstractions
{
    public interface IPlayer
    {
        string Name { get; }
        int Id { get; }
        List<ICard> Hand { get; }
        
        void AddCardToHand(ICard card);
        bool RemoveCardFromHand(ICard card);
        ICard FindCardInHand(ICard cardToFind);
        bool HasWon();
        bool HasCardWithValue(string value);
        List<ICard> GetCardsWithValue(string value);
    }
}
