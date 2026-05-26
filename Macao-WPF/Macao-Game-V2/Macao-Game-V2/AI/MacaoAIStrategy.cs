using System;
using System.Collections.Generic;
using System.Linq;
using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.AI
{
    public class MacaoAIStrategy : IAIStrategy
    {
        private readonly CardsMemory _memory = new CardsMemory();
        public ICardsMemory Memory => _memory;
        private static readonly char[] Suits = { '\u2660', '\u2665', '\u2666', '\u2663' };

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
                // Normal play 
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
                    double probLacks = EstimateOpponentLacksPenaltyCards(gameState, hand);
                    double roll = new Random().NextDouble();

                    if (roll > probLacks)
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
                foreach (var c in hand)
                    if (c.IsJoker)
                        return new List<ICard> { c };
            }

            return bestSelection;
        }

        private double EstimateOpponentLacksPenaltyCards(IGameState gameState, List<ICard> hand)
        {
            int opponentCardCount = gameState.HumanPlayer.Hand.Count;
            if (opponentCardCount <= 0) return 1.0;

            const int totalPenaltiesInDeck = 10; 
            
            int seenPenalties = _memory.PlayedCards.Count(c => c.Value == "2" || c.Value == "3" || c.IsJoker);
            int aiPenalties = hand.Count(c => c.Value == "2" || c.Value == "3" || c.IsJoker);

            int totalKnown = _memory.PlayedCards.Count + hand.Count;
            int remainingPenalties = Math.Max(0, totalPenaltiesInDeck - seenPenalties - aiPenalties);
            int totalUnknown = Math.Max(1, 54 - totalKnown);

            double fractionPenalties = (double)remainingPenalties / totalUnknown;
            
            double probLacks = Math.Pow(1.0 - fractionPenalties, opponentCardCount);
            return Math.Min(1.0, Math.Max(0.0, probLacks));
        }

        private List<ICard> ReorderSelection(List<ICard> selection, IGameState gameState, List<ICard> hand)
        {
            if (selection == null || selection.Count <= 1)
                return selection;

            int opponentCardCount = gameState.HumanPlayer.Hand.Count;

            double GetCardScoreForLast(ICard card)
            {
                return _memory.ProbabilityOpponentLacksSuit(card.Suit, hand, opponentCardCount);
            }

            var validator = new Domain.MacaoCardValidator();
            var validStarters = selection
                .Where(card => validator.IsCardValid(card, gameState.TopCard, gameState.CardsToDraw, null))
                .ToList();

            ICard firstCard;
            if (validStarters.Count > 0)
            {
                firstCard = validStarters.OrderBy(GetCardScoreForLast).First();
            }
            else
            {
                firstCard = selection[0];
            }

            List<ICard> reordered = new List<ICard> { firstCard };

            var remaining = selection.Where(card => card != firstCard)
                                     .OrderBy(GetCardScoreForLast)
                                     .ToList();

            reordered.AddRange(remaining);

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
            int opponentCardCount = gameState.HumanPlayer.Hand.Count;
            List<ICard> handList = hand;

            char bestSuit = ' ';
            double bestScore = -1;

            foreach (char suit in Suits)
            {
                int greedyCount = hand.Count(c => !c.IsJoker && !(excludeSpecials && c.Value == "7") && c.Suit == suit);
                double memoryBonus = _memory.ProbabilityOpponentLacksSuit(suit, handList, opponentCardCount);
                double score = greedyCount * 2.0 + memoryBonus * 1.0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestSuit = suit;
                }
            }

            if (bestSuit == ' ')
                bestSuit = Suits[new Random().Next(Suits.Length)];

            return bestSuit;
        }
    }
}
