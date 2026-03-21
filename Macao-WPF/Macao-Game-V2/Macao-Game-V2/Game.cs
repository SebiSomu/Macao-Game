using System;
using System.Collections.Generic;
using System.Linq;

namespace Macao_Game_V2
{
    public class Game
    {
        private Deck _deck;
        private Player _humanPlayer;
        private Player _computerPlayer;
        private Stack<Card> _discardPile;
        
        private int _cardsToDraw;
        private bool _skipNextTurn;
        private bool _isGameOver;
        private bool _isHumanTurn;

        public event Action<string> OnGameMessage;
        public event Action OnGameStateChanged;
        public event Action<Card> OnSuitSelectionRequired; 
        public event Action<string> OnGameOver;

        public Player HumanPlayer => _humanPlayer;
        public Player ComputerPlayer => _computerPlayer;
        public Card TopCard => _discardPile.Count > 0 ? _discardPile.Peek() : null;
        public int CardsToDraw => _cardsToDraw;
        public bool IsHumanTurn => _isHumanTurn;
        public bool IsGameOver => _isGameOver;
        public string CurrentTurnCardValue { get; private set; }

        public Game()
        {
            _humanPlayer = new Player("You", 0);
            _computerPlayer = new Player("AI", 1);
            _discardPile = new Stack<Card>();
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
            Card firstCard = _deck.DrawCard();
            List<Card> invalidCards = new List<Card>();
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

            OnGameMessage?.Invoke("Game started! Your turn.");
            OnGameStateChanged?.Invoke();
        }

        public void HumanDrawCards()
        {
            if (!_isHumanTurn || _isGameOver) return;

            int drawCount = Math.Max(1, _cardsToDraw);
            for (int i = 0; i < drawCount; i++)
            {
                Card drawn = DrawFromDeck();
                if (drawn != null)
                {
                    _humanPlayer.AddCardToHand(drawn);
                }
            }

            if (_cardsToDraw > 0)
            {
                OnGameMessage?.Invoke($"You drew {drawCount} cards as penalty.");
                _cardsToDraw = 0;
            }
            else
            {
                OnGameMessage?.Invoke("You drew a card.");
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
                    OnGameMessage?.Invoke($"You must play a {CurrentTurnCardValue} or click END TURN to finish.");
                    return false;
                }
            }
            else
            {
                if (!card.IsCardValid(TopCard, _cardsToDraw))
                {
                    OnGameMessage?.Invoke("Invalid card! Choose another.");
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
            ApplyCardEffects(card);

            if (_humanPlayer.HasWon())
            {
                _isGameOver = true;
                OnGameOver?.Invoke("You won!");
                OnGameStateChanged?.Invoke();
                return;
            }

            if (_humanPlayer.HasCardWithValue(CurrentTurnCardValue))
            {
                OnGameMessage?.Invoke($"You can play another {CurrentTurnCardValue} or click END TURN.");
                OnGameStateChanged?.Invoke();
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
            OnGameStateChanged?.Invoke();

            if (_skipNextTurn)
            {
                _skipNextTurn = false;
                OnGameMessage?.Invoke("AI's turn is skipped due to an Ace!");
                _isHumanTurn = true;
                OnGameMessage?.Invoke("Your turn again.");
                OnGameStateChanged?.Invoke();
                return;
            }

            AITurn();
        }

        private void AITurn()
        {
            if (_isGameOver) return;
            CurrentTurnCardValue = null;

            OnGameMessage?.Invoke("AI is thinking...");

            Card firstCard = DetermineAICard();

            if (firstCard != null)
            {
                List<Card> cardsToPlay = new List<Card> { firstCard };
                
                var moreCards = _computerPlayer.GetCardsWithValue(firstCard.Value);
                foreach (var c in moreCards)
                {
                    if (c != firstCard) cardsToPlay.Add(c);
                }

                foreach (var cardToPlay in cardsToPlay)
                {
                    _computerPlayer.RemoveCardFromHand(cardToPlay);

                    if (cardToPlay.Value == "7")
                    {
                        cardToPlay.Suit = AIChooseSuit();
                    }

                    _discardPile.Push(cardToPlay);
                    ApplyCardEffects(cardToPlay);

                    if (_computerPlayer.HasWon())
                    {
                        _isGameOver = true;
                        OnGameOver?.Invoke("AI won!");
                        OnGameStateChanged?.Invoke();
                        return;
                    }
                }

                string cardNames = string.Join(", ", cardsToPlay.Select(c => c.ToString()));
                if (cardsToPlay[0].Value == "7") {
                    OnGameMessage?.Invoke($"AI played 7s: {cardNames} and chose suit {cardsToPlay.Last().Suit}");
                } else {
                    OnGameMessage?.Invoke($"AI played: {cardNames}");
                }
            }
            else
            {
                int drawCount = Math.Max(1, _cardsToDraw);
                for (int i = 0; i < drawCount; i++)
                {
                    Card drawn = DrawFromDeck();
                    if (drawn != null)
                        _computerPlayer.AddCardToHand(drawn);
                }

                if (_cardsToDraw > 0)
                {
                    OnGameMessage?.Invoke($"AI drew {drawCount} cards as penalty.");
                    _cardsToDraw = 0;
                }
                else
                {
                    OnGameMessage?.Invoke("AI drew a card.");
                }
            }

            _isHumanTurn = true;
            OnGameStateChanged?.Invoke();

            if (_skipNextTurn)
            {
                _skipNextTurn = false;
                OnGameMessage?.Invoke("Your turn is skipped due to an Ace!");
                EndHumanTurn();
            }
            else
            {
                OnGameMessage?.Invoke("Your turn.");
            }
        }

        private Card DetermineAICard()
        {
            foreach (var card in _computerPlayer.Hand)
            {
                if (card.IsCardValid(TopCard, _cardsToDraw))
                {
                    return card;
                }
            }
            return null;
        }

        private char AIChooseSuit()
        {
            var suits = _computerPlayer.Hand.Where(c => !c.IsJoker).GroupBy(c => c.Suit).OrderByDescending(g => g.Count()).FirstOrDefault();
            if (suits != null && suits.Key != ' ') return suits.Key;

            char[] possibleSuits = { '♠', '♥', '♦', '♣' };
            return possibleSuits[new Random().Next(4)];
        }

        private void ApplyCardEffects(Card card)
        {
            if (card.Value == "2") _cardsToDraw += 2;
            else if (card.Value == "3") _cardsToDraw += 3;
            else if (card.IsJoker) _cardsToDraw += 5;
            else if (card.Value == "A") _skipNextTurn = true;
        }

        private Card DrawFromDeck()
        {
            if (_deck.IsEmpty())
            {
                if (_discardPile.Count <= 1) return null; // No cards left anywhere!

                Card top = _discardPile.Pop();
                List<Card> toReshuffle = new List<Card>(_discardPile);
                _discardPile.Clear();
                _discardPile.Push(top);
                _deck.Reshuffle(toReshuffle);
            }
            return _deck.DrawCard();
        }
    }
}
