using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.Effects
{
    public class JokerDrawEffect : ICardEffect
    {
        public string TargetValue => "Joker";
        public bool IsJokerEffect => true;

        public void Apply(IGameState gameState)
        {
            gameState.CardsToDraw += 5;
        }
    }
}
