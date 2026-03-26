namespace Macao_Game_V2.Abstractions
{
    public interface ICardEffect
    {
        string TargetValue { get; }
        bool IsJokerEffect { get; }
        void Apply(IGameState gameState);
    }
}
