using System.Collections.Generic;
using Macao_Game_V2.Abstractions;

namespace Macao_Game_V2.Effects
{
    public class CardEffectResolver
    {
        private readonly List<ICardEffect> _effects;

        public CardEffectResolver()
        {
            _effects = new List<ICardEffect>
            {
                new DrawTwoEffect(),
                new DrawThreeEffect(),
                new JokerDrawEffect(),
                new AceSkipEffect()
            };
        }

        public void ApplyEffects(ICard card, IGameState gameState)
        {
            foreach (var effect in _effects)
            {
                if (effect.IsJokerEffect && card.IsJoker)
                {
                    effect.Apply(gameState);
                    return;
                }
                else if (!effect.IsJokerEffect && card.Value == effect.TargetValue)
                {
                    effect.Apply(gameState);
                    return;
                }
            }
        }
    }
}
