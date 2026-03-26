using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Macao_Game_V2.Domain;

namespace Macao_Game_V2
{
    public partial class MainWindow : Window
    {
        private Game _game;
        private Card _pendingSevenCard;
        private int _humanWins = 0;
        private int _aiWins = 0;
        private bool _isDarkMode = false;
        private LinearGradientBrush _backgroundBrush;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                UpdateCardDarkMode();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _game = new Game();
            
            _game.OnGameStateChanged += UpdateUI;
            _game.OnGameMessage += ShowMessage;
            _game.OnSuitSelectionRequired += AskForSuit;
            _game.OnGameOver += HandleGameOver;

            // Initialize dark mode toggle
            UpdateDarkModeToggle();
            
            // Initialize background
            UpdateAppBackground();

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
                TopCardVisual.IsDarkMode = _isDarkMode;
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
                
                // Check if card is playable using validator
                var validator = new MacaoCardValidator();
                bool isValid = validator.IsCardValid(card, _game.TopCard, _game.CardsToDraw, _game.CurrentTurnCardValue);
                cardBtn.Cursor = isValid ? Cursors.Hand : Cursors.No;
                cardBtn.IsEnabled = isValid;
                
                // Use CardVisual for the card face
                var cardVisual = new CardVisual();
                cardVisual.Card = (Card)card;
                cardVisual.IsFaceDown = false;
                cardVisual.IsDarkMode = _isDarkMode;
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

        private void UpdateCardDarkMode()
        {
            // Update top card
            if (TopCardVisual != null)
            {
                TopCardVisual.IsDarkMode = _isDarkMode;
            }

            // Update all player cards
            foreach (UIElement element in PlayerHandPanel.Children)
            {
                if (element is Button btn && btn.Content is CardVisual cardVisual)
                {
                    cardVisual.IsDarkMode = _isDarkMode;
                }
            }

            // Update app background
            UpdateAppBackground();

            // Update toggle button icon
            UpdateDarkModeToggle();
        }

        private void UpdateAppBackground()
        {
            if (_isDarkMode)
            {
                // Dark mode background
                var darkBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(15, 15, 25), 0.0),
                        new GradientStop(Color.FromRgb(25, 25, 45), 1.0)
                    }
                };
                this.Background = darkBrush;
            }
            else
            {
                // Light mode background
                var lightBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(44, 62, 80), 0.0),
                        new GradientStop(Color.FromRgb(76, 161, 175), 1.0)
                    }
                };
                this.Background = lightBrush;
            }
        }

        private void DarkModeToggle_Click(object sender, RoutedEventArgs e)
        {
            IsDarkMode = !_isDarkMode;
        }

        private void UpdateDarkModeToggle()
        {
            if (DarkModeToggle != null)
            {
                var icon = DarkModeToggle.Template.FindName("ToggleIcon", DarkModeToggle) as TextBlock;
                if (icon != null)
                {
                    icon.Text = _isDarkMode ? "🌙" : "☀️";
                }
            }
        }
    }
}