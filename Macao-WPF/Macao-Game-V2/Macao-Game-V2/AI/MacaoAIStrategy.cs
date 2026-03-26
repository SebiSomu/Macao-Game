using System;
using System.Collections.Generic;
using System.Linq;
using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.AI
{
    public class MacaoAIStrategy : IAIStrategy
    {
        public List<ICard> SelectCardsToPlay(IGameState gameState, List<ICard> hand)
        {
            var validCardsByValue = new Dictionary<string, List<ICard>>();

            if (gameState.CardsToDraw > 0)
            {
                // Penalty situation - only 2, 3, Joker
                foreach (var card in hand)
                {
                    if (card.Value == "2" || card.Value == "3" || card.IsJoker)
                    {
                        if (!validCardsByValue.ContainsKey(card.Value)) validCardsByValue[card.Value] = new List<ICard>();
                        validCardsByValue[card.Value].Add(card);
                    }
                }
            }
            else
            {
                // Normal play - use validator to check valid cards
                var validator = new Domain.MacaoCardValidator();
                foreach (var card in hand)
                {
                    if (validator.IsCardValid(card, gameState.TopCard, gameState.CardsToDraw, null))
                    {
                        if (!validCardsByValue.ContainsKey(card.Value)) validCardsByValue[card.Value] = new List<ICard>();
                        validCardsByValue[card.Value].Add(card);
                    }
                }
            }

            // Ace combo strategy
            foreach (var ace in hand)
            {
                if (ace.Value == "A")
                {
                    var validator = new Domain.MacaoCardValidator();
                    if (validator.IsCardValid(ace, gameState.TopCard, gameState.CardsToDraw, null))
                    {
                        foreach (var other in hand)
                        {
                            if (other != ace && other.Suit == ace.Suit)
                            {
                                return new List<ICard> { ace, other };
                            }
                        }
                    }
                }
            }

            // No valid cards
            if (validCardsByValue.Count == 0)
            {
                if (gameState.CardsToDraw == 0)
                {
                    foreach (var c in hand) if (c.Value == "7") return new List<ICard> { c };
                    foreach (var c in hand) if (c.IsJoker) return new List<ICard> { c };
                }
                return new List<ICard>();
            }

            // Group all cards by value for full group plays
            var fullGroups = new Dictionary<string, List<ICard>>();
            foreach (var kvp in validCardsByValue)
            {
                string value = kvp.Key;
                var allCardsOfValue = hand.Where(c => c.Value == value).ToList();
                fullGroups[value] = allCardsOfValue;
            }

            // Find best selection
            List<ICard> bestSelection = new List<ICard>();
            int bestScore = -1;

            foreach (var kvp in fullGroups)
            {
                string value = kvp.Key;
                var cards = kvp.Value;
                if (cards.Count == 0) continue;

                bool isInflate = (value == "2" || value == "3" || cards[0].IsJoker);

                if (isInflate && cards.Count > 1)
                {
                    Random random = new Random();
                    if (random.Next(100) >= 10) // 90% chance to only play one and save the rest
                    {
                        int score = 1;
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestSelection = new List<ICard> { cards[0] };
                        }
                        continue;
                    }
                }

                int currentScore = cards.Count;
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestSelection = new List<ICard>(cards);
                }
            }

            // Reorder selection if multiple cards
            if (bestSelection.Count > 1)
            {
                bestSelection = ReorderSelection(bestSelection, gameState, hand);
            }

            // Fallback to Joker if nothing selected
            if (bestSelection.Count == 0)
            {
                foreach (var c in hand) if (c.IsJoker) return new List<ICard> { c };
            }

            return bestSelection;
        }

        private List<ICard> ReorderSelection(List<ICard> selection, IGameState gameState, List<ICard> hand)
        {
            List<ICard> reordered = new List<ICard>();
            ICard firstCard = selection[0];

            var validator = new Domain.MacaoCardValidator();

            // Find first valid card
            foreach (var card in selection)
            {
                if (validator.IsCardValid(card, gameState.TopCard, gameState.CardsToDraw, null))
                {
                    firstCard = card;
                    break;
                }
            }

            reordered.Add(firstCard);
            foreach (var card in selection)
            {
                if (card != firstCard) reordered.Add(card);
            }

            // Special handling for 7s
            if (firstCard.Value == "7")
            {
                char bestSuit = ChooseSuit(gameState, hand, false);
                for (int i = 1; i < reordered.Count; i++)
                {
                    if (reordered[i].Suit == bestSuit && !reordered[i].IsJoker)
                    {
                        ICard temp = reordered[i];
                        reordered.RemoveAt(i);
                        reordered.Add(temp);
                        break;
                    }
                }
            }

            return reordered;
        }

        public char ChooseSuit(IGameState gameState, List<ICard> hand, bool excludeSpecials)
        {
            var suitFrequency = new Dictionary<char, int>();

            foreach (var card in hand)
            {
                bool isExcluded = card.IsJoker || (excludeSpecials && card.Value == "7");
                if (!isExcluded)
                {
                    if (!suitFrequency.ContainsKey(card.Suit))
                        suitFrequency[card.Suit] = 0;
                    suitFrequency[card.Suit]++;
                }
            }

            char bestSuit = ' ';
            int maxCount = -1;
            foreach (var kvp in suitFrequency)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    bestSuit = kvp.Key;
                }
            }

            if (maxCount <= 0 || (bestSuit != '♠' && bestSuit != '♥' && bestSuit != '♦' && bestSuit != '♣'))
            {
                char[] suits = { '♠', '♥', '♦', '♣' };
                bestSuit = suits[new Random().Next(4)];
            }

            return bestSuit;
        }
    }
}
