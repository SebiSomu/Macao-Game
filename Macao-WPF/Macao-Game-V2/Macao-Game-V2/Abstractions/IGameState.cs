using System.Collections.Generic;

namespace Macao_Game_V2.Abstractions
{
    public interface IGameState
    {
        ICard TopCard { get; }
        int CardsToDraw { get; set; }
        bool SkipNextTurn { get; set; }
        bool IsGameOver { get; set; }
        bool IsHumanTurn { get; set; }
        string CurrentTurnCardValue { get; set; }
        
        IPlayer HumanPlayer { get; }
        IPlayer ComputerPlayer { get; }
        IDeck Deck { get; }
        Stack<ICard> DiscardPile { get; }
        
        void NotifyStateChanged();
        void NotifyMessage(string message);
        void NotifyGameOver(string message);
    }
}
