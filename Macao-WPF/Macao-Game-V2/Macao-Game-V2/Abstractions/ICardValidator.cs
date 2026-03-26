namespace Macao_Game_V2.Abstractions
{
    public interface ICardValidator
    {
        bool IsCardValid(ICard card, ICard topCard, int cardsToDraw, string currentTurnCardValue = null);
    }
}
