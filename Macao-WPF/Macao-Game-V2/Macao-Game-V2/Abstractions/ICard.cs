namespace Macao_Game_V2.Abstractions
{
    public interface ICard
    {
        string Value { get; set; }
        char Suit { get; set; }
        bool IsJoker { get; set; }
        bool IsCardJoker();
        string ToString();
    }
}
