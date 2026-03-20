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
            
            _game.HumanDrawCards();
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