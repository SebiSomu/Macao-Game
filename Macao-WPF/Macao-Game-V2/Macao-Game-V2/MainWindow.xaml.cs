using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Macao_Game_V2
{
    public partial class MainWindow : Window
    {
        private Game _game;
        private Card _pendingSevenCard;
        private int _humanWins = 0;
        private int _aiWins = 0;

        public MainWindow()
        {
            InitializeComponent();
            _game = new Game();
            
            _game.OnGameStateChanged += UpdateUI;
            _game.OnGameMessage += ShowMessage;
            _game.OnSuitSelectionRequired += AskForSuit;
            _game.OnGameOver += HandleGameOver;

            StartNewGame();
        }

        private void StartNewGame()
        {
            GameOverOverlay.Visibility = Visibility.Hidden;
            SuitSelectionOverlay.Visibility = Visibility.Hidden;
            _game.StartGame();
        }

        private void UpdateUI()
        {
            // Update AI Stats
            AiStatusText.Text = $"AI Cards: {_game.ComputerPlayer.Hand.Count}";
            
            // Turn text
            TurnText.Text = _game.IsHumanTurn ? "Your Turn" : "AI is playing...";
            TurnText.Foreground = _game.IsHumanTurn ? Brushes.LightGreen : Brushes.Orange;

            if (_game.CardsToDraw > 0)
            {
                CardsToDrawText.Text = $"Penalty: {_game.CardsToDraw} card(s)";
                CardsToDrawText.Visibility = Visibility.Visible;
            }
            else
            {
                CardsToDrawText.Visibility = Visibility.Hidden;
            }

            if (_game.TopCard != null)
            {
                TopCardVisual.Card = _game.TopCard;
                TopCardVisual.IsFaceDown = false;
            }

            // Draw Pile / End Turn button text
            if (DrawPileButton.Content is TextBlock tb)
            {
                tb.Text = _game.CurrentTurnCardValue != null ? "END TURN" : "DRAW";
            }

            // Render Player Hand with CardVisual
            PlayerHandPanel.Children.Clear();
            foreach (var card in _game.HumanPlayer.Hand)
            {
                Button cardBtn = new Button();
                cardBtn.Style = (Style)FindResource("CardButtonStyle");
                cardBtn.Tag = card;
                
                // Check if card is playable
                bool isValid = card.IsCardValid(_game.TopCard, _game.CardsToDraw, _game.CurrentTurnCardValue);
                cardBtn.Cursor = isValid ? Cursors.Hand : Cursors.No;
                
                // Use CardVisual for the card face
                var cardVisual = new CardVisual();
                cardVisual.Card = card;
                cardVisual.IsFaceDown = false;
                cardBtn.Content = cardVisual;

                cardBtn.Click += CardBtn_Click;
                PlayerHandPanel.Children.Add(cardBtn);
            }
        }

        private void ShowMessage(string message)
        {
            GameMessageText.Text = message;
        }

        private void AskForSuit(Card cardPlayed)
        {
            _pendingSevenCard = cardPlayed;
            SuitSelectionOverlay.Visibility = Visibility.Visible;
        }

        private void HandleGameOver(string message)
        {
            GameOverText.Text = message;
            GameOverOverlay.Visibility = Visibility.Visible;
            if (message.Contains("You")) _humanWins++;
            else if (message.Contains("AI")) _aiWins++;
            
            HumanWinsText.Text = $"Your Wins: {_humanWins}";
            AiWinsText.Text = $"AI Wins: {_aiWins}";
        }

        private void CardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_game.IsHumanTurn || SuitSelectionOverlay.Visibility == Visibility.Visible) return;

            Button btn = sender as Button;
            Card card = btn.Tag as Card;

            _game.TryPlayHumanCard(card);
        }

        private void DrawPile_Click(object sender, RoutedEventArgs e)
        {
            if (!_game.IsHumanTurn || SuitSelectionOverlay.Visibility == Visibility.Visible) return;
            
            if (_game.CurrentTurnCardValue != null)
            {
                _game.HumanEndTurnEarly();
            }
            else
            {
                _game.HumanDrawCards();
            }
        }

        private void Suit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            char suit = btn.Content.ToString()[0];
            
            SuitSelectionOverlay.Visibility = Visibility.Hidden;
            _game.CompleteHumanPlayWithSuit(_pendingSevenCard, suit);
        }

        private void RestartGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }
    }
}