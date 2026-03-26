using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.Effects
{
    public class DrawTwoEffect : ICardEffect
    {
        public string TargetValue => "2";
        public bool IsJokerEffect => false;

        public void Apply(IGameState gameState)
        {
            gameState.CardsToDraw += 2;
        }
    }
}
