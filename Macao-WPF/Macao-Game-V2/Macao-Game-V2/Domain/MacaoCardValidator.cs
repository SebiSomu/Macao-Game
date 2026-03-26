using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.Domain
{
    public class MacaoCardValidator : ICardValidator
    {
        public bool IsCardValid(ICard card, ICard topCard, int cardsToDraw, string currentTurnCardValue = null)
        {
            // Handle penalty cards (2, 3, Joker)
            if (cardsToDraw > 0)
                return (card.Value == "2" || card.Value == "3" || card.IsJoker);

            // If a specific card value is required (e.g., after playing a 7), only allow that value
            if (!string.IsNullOrEmpty(currentTurnCardValue))
            {
                if (card.Value != currentTurnCardValue)
                    return false;
            }

            // Special cards that can always be played
            if (card.IsJoker || card.Value == "7")
                return true;

            // If top card is a Joker, any card can be played
            if (topCard?.IsJoker == true)
                return true;

            // Normal matching rules (suit or value must match)
            return (card.Suit == topCard?.Suit || card.Value == topCard?.Value);
        }
    }
}
