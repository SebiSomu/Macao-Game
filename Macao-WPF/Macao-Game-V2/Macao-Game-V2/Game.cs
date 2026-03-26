using System;
using System.Collections.Generic;
using Macao_Game_V2.Abstractions;
using Macao_Game_V2.Domain;
using Macao_Game_V2.Effects;
using Macao_Game_V2.AI;

namespace Macao_Game_V2
{
    public class Game : IGameState
    {
        private IDeck _deck;
        private IPlayer _humanPlayer;
        private IPlayer _computerPlayer;
        private Stack<ICard> _discardPile;
        
        private int _cardsToDraw;
        private bool _skipNextTurn;
        private bool _isGameOver;
        private bool _isHumanTurn;

        private readonly ICardValidator _cardValidator;
        private readonly IAIStrategy _aiStrategy;
        private readonly CardEffectResolver _effectResolver;

        public event Action<string> OnGameMessage;
        public event Action OnGameStateChanged;
        public event Action<Card> OnSuitSelectionRequired; 
        public event Action<string> OnGameOver;

        // Public properties for backward compatibility
        public Player HumanPlayer => (Player)_humanPlayer;
        public Player ComputerPlayer => (Player)_computerPlayer;
        public Card TopCard => _discardPile.Count > 0 ? (Card)_discardPile.Peek() : null;
        public int CardsToDraw => _cardsToDraw;
        public bool IsHumanTurn => _isHumanTurn;
        public bool IsGameOver => _isGameOver;
        public string CurrentTurnCardValue { get; private set; }

        // IGameState implementation
        ICard IGameState.TopCard => _discardPile.Count > 0 ? _discardPile.Peek() : null;
        int IGameState.CardsToDraw { get => _cardsToDraw; set => _cardsToDraw = value; }
        bool IGameState.SkipNextTurn { get => _skipNextTurn; set => _skipNextTurn = value; }
        bool IGameState.IsGameOver { get => _isGameOver; set => _isGameOver = value; }
        bool IGameState.IsHumanTurn { get => _isHumanTurn; set => _isHumanTurn = value; }
        string IGameState.CurrentTurnCardValue { get => CurrentTurnCardValue; set => CurrentTurnCardValue = value; }
        IPlayer IGameState.HumanPlayer => _humanPlayer;
        IPlayer IGameState.ComputerPlayer => _computerPlayer;
        IDeck IGameState.Deck => _deck;
        Stack<ICard> IGameState.DiscardPile => _discardPile;

        public Game(ICardValidator cardValidator = null, IAIStrategy aiStrategy = null)
        {
            _humanPlayer = new Player("You", 0);
            _computerPlayer = new Player("AI", 1);
            _discardPile = new Stack<ICard>();
            
            _cardValidator = cardValidator ?? new MacaoCardValidator();
            _aiStrategy = aiStrategy ?? new MacaoAIStrategy();
            _effectResolver = new CardEffectResolver();
        }

        public void StartGame()
        {
            _deck = new Deck();
            _humanPlayer.Hand.Clear();
            _computerPlayer.Hand.Clear();
            _discardPile.Clear();
            _cardsToDraw = 0;
            _skipNextTurn = false;
            _isGameOver = false;
            CurrentTurnCardValue = null;

            // Deal 5 cards each
            for (int i = 0; i < 5; i++)
            {
                _humanPlayer.AddCardToHand(_deck.DrawCard());
                _computerPlayer.AddCardToHand(_deck.DrawCard());
            }

            // Start discard pile with a non-action card if possible, or just standard play
            ICard firstCard = _deck.DrawCard();
            List<ICard> invalidCards = new List<ICard>();
            while (firstCard != null && (firstCard.Value == "2" || firstCard.Value == "3" || 
                   firstCard.Value == "7" || firstCard.Value == "A" || 
                   firstCard.IsJoker))
            {
                invalidCards.Add(firstCard);
                firstCard = _deck.DrawCard();
            }
            
            if (invalidCards.Count > 0)
            {
                _deck.Reshuffle(invalidCards);
            }
            
            _discardPile.Push(firstCard);
            _isHumanTurn = true;

            NotifyMessage("Game started! Your turn.");
            NotifyStateChanged();
        }

        public void HumanDrawCards()
        {
            if (!_isHumanTurn || _isGameOver) return;

            int drawCount = Math.Max(1, _cardsToDraw);
            for (int i = 0; i < drawCount; i++)
            {
                ICard drawn = DrawFromDeck();
                if (drawn != null)
                {
                    _humanPlayer.AddCardToHand(drawn);
                }
            }

            if (_cardsToDraw > 0)
            {
                NotifyMessage($"You drew {drawCount} cards as penalty.");
                _cardsToDraw = 0;
            }
            else
            {
                NotifyMessage("You drew a card.");
            }

            EndHumanTurn();
        }

        public bool TryPlayHumanCard(Card card)
        {
            if (!_isHumanTurn || _isGameOver) return false;

            if (CurrentTurnCardValue != null)
            {
                if (card.Value != CurrentTurnCardValue)
                {
                    NotifyMessage($"You must play a {CurrentTurnCardValue} or click END TURN to finish.");
                    return false;
                }
            }
            else
            {
                if (!_cardValidator.IsCardValid(card, TopCard, _cardsToDraw, null))
                {
                    NotifyMessage("Invalid card! Choose another.");
                    return false;
                }
            }

            _humanPlayer.RemoveCardFromHand(card);
            CurrentTurnCardValue = card.Value;
            
            if (card.Value == "7")
            {
                // Trigger UI to ask for suit
                OnSuitSelectionRequired?.Invoke(card);
                return true; // Wait for suit selection callback to finish turn
            }

            FinishHumanCardPlay(card);
            return true;
        }

        public void CompleteHumanPlayWithSuit(Card card, char chosenSuit)
        {
            card.Suit = chosenSuit;
            FinishHumanCardPlay(card);
        }

        private void FinishHumanCardPlay(Card card)
        {
            _discardPile.Push(card);
            _effectResolver.ApplyEffects(card, this);

            if (_humanPlayer.HasWon())
            {
                _isGameOver = true;
                NotifyGameOver("You won!");
                NotifyStateChanged();
                return;
            }

            if (_humanPlayer.HasCardWithValue(CurrentTurnCardValue))
            {
                NotifyMessage($"You can play another {CurrentTurnCardValue} or click END TURN.");
                NotifyStateChanged();
            }
            else
            {
                EndHumanTurn();
            }
        }

        public void HumanEndTurnEarly()
        {
            if (!_isHumanTurn || _isGameOver || CurrentTurnCardValue == null) return;
            EndHumanTurn();
        }

        private void EndHumanTurn()
        {
            CurrentTurnCardValue = null;
            _isHumanTurn = false;
            NotifyStateChanged();

            if (_skipNextTurn)
            {
                _skipNextTurn = false;
                NotifyMessage("AI's turn is skipped due to an Ace!");
                _isHumanTurn = true;
                NotifyMessage("Your turn again.");
                NotifyStateChanged();
                return;
            }

            AITurn();
        }

        private List<ICard> EnsureValidFirstCard(List<ICard> selectedCards)
        {
            if (selectedCards == null || selectedCards.Count == 0) return new List<ICard>();

            List<ICard> reordered = new List<ICard>(selectedCards);
            for (int i = 0; i < reordered.Count; i++)
            {
                if (_cardValidator.IsCardValid(reordered[i], TopCard, _cardsToDraw, null))
                {
                    if (i != 0)
                    {
                        ICard temp = reordered[0];
                        reordered[0] = reordered[i];
                        reordered[i] = temp;
                    }
                    break;
                }
            }
            return reordered;
        }

        private void AITurn()
        {
            if (_isGameOver) return;
            CurrentTurnCardValue = null;

            NotifyMessage("AI is thinking...");

            List<ICard> cardsToPlay = _aiStrategy.SelectCardsToPlay(this, _computerPlayer.Hand);
            cardsToPlay = EnsureValidFirstCard(cardsToPlay);

            if (cardsToPlay != null && cardsToPlay.Count > 0)
            {
                foreach (var cardToPlay in cardsToPlay)
                {
                    _computerPlayer.RemoveCardFromHand(cardToPlay);

                    if (cardToPlay.Value == "7")
                    {
                        cardToPlay.Suit = _aiStrategy.ChooseSuit(this, _computerPlayer.Hand, false);
                    }

                    _discardPile.Push(cardToPlay);
                    _effectResolver.ApplyEffects(cardToPlay, this);

                    if (_computerPlayer.HasWon())
                    {
                        _isGameOver = true;
                        NotifyGameOver("AI won!");
                        NotifyStateChanged();
                        return;
                    }
                }

                string cardNames = string.Join(", ", cardsToPlay.Select(c => c.ToString()));
                if (cardsToPlay[0].Value == "7") {
                    NotifyMessage($"AI played 7s: {cardNames} and chose suit {cardsToPlay.Last().Suit}");
                } else if (cardsToPlay[0].Value == "A") {
                    NotifyMessage($"AI played Ace combo: {cardNames}");
                } else {
                    NotifyMessage($"AI played: {cardNames}");
                }
            }
            else
            {
                int drawCount = Math.Max(1, _cardsToDraw);
                for (int i = 0; i < drawCount; i++)
                {
                    ICard drawn = DrawFromDeck();
                    if (drawn != null)
                        _computerPlayer.AddCardToHand(drawn);
                }

                if (_cardsToDraw > 0)
                {
                    NotifyMessage($"AI drew {drawCount} cards as penalty.");
                    _cardsToDraw = 0;
                }
                else
                {
                    NotifyMessage("AI drew a card.");
                }
            }

            _isHumanTurn = true;
            NotifyStateChanged();

            if (_skipNextTurn)
            {
                _skipNextTurn = false;
                NotifyMessage("Your turn is skipped due to an Ace!");
                EndHumanTurn();
            }
            else
            {
                NotifyMessage("Your turn.");
            }
        }

        private ICard DrawFromDeck()
        {
            if (_deck.IsEmpty())
            {
                if (_discardPile.Count <= 1) return null;

                ICard top = _discardPile.Pop();
                List<ICard> toReshuffle = new List<ICard>(_discardPile);
                _discardPile.Clear();
                _discardPile.Push(top);
                _deck.Reshuffle(toReshuffle);
            }
            return _deck.DrawCard();
        }

        // IGameState notification methods
        public void NotifyStateChanged() => OnGameStateChanged?.Invoke();
        public void NotifyMessage(string message) => OnGameMessage?.Invoke(message);
        public void NotifyGameOver(string message) => OnGameOver?.Invoke(message);
    }
}
