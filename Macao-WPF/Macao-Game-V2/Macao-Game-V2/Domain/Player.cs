using System.Collections.Generic;
using System.Linq;
using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.Domain
{
    public class Player : IPlayer
    {
        private string name;
        private int id;
        private List<ICard> hand;

        public string Name => name;
        public int Id => id;
        public List<ICard> Hand => hand;

        public Player(string playerName, int playerId)
        {
            name = playerName;
            id = playerId;
            hand = new List<ICard>();
        }

        public void AddCardToHand(ICard card)
        {
            hand.Add(card);
        }

        public bool RemoveCardFromHand(ICard card)
        {
            // Find matching card (uses Card's equality operator)
            var cardToRemove = hand.FirstOrDefault(c => 
                c.Value == card.Value && 
                c.Suit == card.Suit && 
                c.IsJoker == card.IsJoker);
            
            if (cardToRemove != null)
            {
                return hand.Remove(cardToRemove);
            }
            return false;
        }

        public ICard FindCardInHand(ICard cardToFind)
        {
            return hand.FirstOrDefault(c => 
                c.Value == cardToFind.Value && 
                c.Suit == cardToFind.Suit && 
                c.IsJoker == cardToFind.IsJoker);
        }

        public bool HasWon()
        {
            return hand.Count == 0;
        }

        public bool HasCardWithValue(string value)
        {
            return hand.Exists(c => c.Value == value);
        }

        public List<ICard> GetCardsWithValue(string value)
        {
            return hand.Where(c => c.Value == value).ToList();
        }
    }
}
