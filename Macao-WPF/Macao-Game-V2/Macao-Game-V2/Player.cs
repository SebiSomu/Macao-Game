using System;
using System.Collections.Generic;
using System.Linq;

namespace Macao_Game_V2
{
    public class Player
    {
        private string name;
        private int id;
        private List<Card> hand;

        public string Name => name;
        public int Id => id;
        public List<Card> Hand => hand;

        public Player(string playerName, int playerId)
        {
            name = playerName;
            id = playerId;
            hand = new List<Card>();
        }

        public void AddCardToHand(Card card)
        {
            hand.Add(card);
        }

        public bool RemoveCardFromHand(Card card)
        {
            return hand.Remove(card);
        }

        public Card FindCardInHand(Card cardToFind)
        {
            return hand.FirstOrDefault(c => c == cardToFind);
        }

        public bool HasWon()
        {
            return hand.Count == 0;
        }

        public bool HasCardWithValue(string value)
        {
            return hand.Exists(c => c.Value == value);
        }

        public List<Card> GetCardsWithValue(string value)
        {
            return hand.Where(c => c.Value == value).ToList();
        }
    }
}
