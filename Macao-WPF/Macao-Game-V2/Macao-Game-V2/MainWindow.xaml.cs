using System;
using System.Windows;
using System.Windows.Controls;
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

            // Render AI Hand (Debug/Testing)
            /*
            AiHandPanel.Children.Clear();
            foreach (var card in _game.ComputerPlayer.Hand)
            {
                Border cardBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Margin = new Thickness(2),
                    Padding = new Thickness(3),
                    Width = 40,
                    Height = 60
                };
                TextBlock aiTb = new TextBlock
                {
                    Text = card.ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = (card.Suit == '♥' || card.Suit == '♦') ? Brushes.Red : Brushes.Black
                };
                cardBorder.Child = aiTb;
                AiHandPanel.Children.Add(cardBorder);
            }
            */

            // Penalty
            if (_game.CardsToDraw > 0)
            {
                CardsToDrawText.Text = $"Penalty: {_game.CardsToDraw} card(s)";
                CardsToDrawText.Visibility = Visibility.Visible;
            }
            else
            {
                CardsToDrawText.Visibility = Visibility.Hidden;
            }

            // Top Card
            if (_game.TopCard != null)
            {
                TopCardText.Text = _game.TopCard.ToString();
                TopCardText.Foreground = (_game.TopCard.Suit == '♥' || _game.TopCard.Suit == '♦') ? Brushes.Red : Brushes.Black;
            }

            // Draw Pile / End Turn button text
            if (DrawPileButton.Content is TextBlock tb)
            {
                tb.Text = _game.CurrentTurnCardValue != null ? "END TURN" : "DRAW";
            }

            // Render Player Hand
            PlayerHandPanel.Children.Clear();
            foreach (var card in _game.HumanPlayer.Hand)
            {
                Button cardBtn = new Button();
                cardBtn.Style = (Style)FindResource("CardButtonStyle");
                cardBtn.Content = card.ToString();
                cardBtn.Tag = card;
                
                if (card.Suit == '♥' || card.Suit == '♦')
                {
                    cardBtn.Foreground = Brushes.Red;
                }

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