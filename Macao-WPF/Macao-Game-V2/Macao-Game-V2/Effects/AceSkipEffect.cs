using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.Effects
{
    public class AceSkipEffect : ICardEffect
    {
        public string TargetValue => "A";
        public bool IsJokerEffect => false;

        public void Apply(IGameState gameState)
        {
            gameState.SkipNextTurn = true;
        }
    }
}
