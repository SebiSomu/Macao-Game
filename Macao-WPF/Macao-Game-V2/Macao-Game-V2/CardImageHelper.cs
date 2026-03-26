using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Macao_Game_V2
{
    /// <summary>
    /// Helper class for mapping cards to image file paths.
    /// Provides a clean API for resolving card images from Assets folder.
    /// </summary>
    public static class CardImageHelper
    {
        // Base path to Assets/Cards folder (relative to executable)
        private static readonly string AssetsPath = "Assets/Cards";
        
        // Image file extension (png recommended for transparency)
        private const string ImageExtension = ".png";

        // Cache for card-to-filename mapping
        private static readonly Dictionary<string, string> ValueMapping = new Dictionary<string, string>
        {
            { "2", "2" },
            { "3", "3" },
            { "4", "4" },
            { "5", "5" },
            { "6", "6" },
            { "7", "7" },
            { "8", "8" },
            { "9", "9" },
            { "10", "10" },
            { "J", "jack" },
            { "Q", "queen" },
            { "K", "king" },
            { "A", "ace" }
        };

        private static readonly Dictionary<char, string> SuitMapping = new Dictionary<char, string>
        {
            { '♠', "spades" },
            { '♥', "hearts" },
            { '♦', "diamonds" },
            { '♣', "clubs" }
        };

        private static readonly Dictionary<char, string> SuitLetterMapping = new Dictionary<char, string>
        {
            { 'S', "spades" },
            { 'H', "hearts" },
            { 'D', "diamonds" },
            { 'C', "clubs" }
        };

        /// <summary>
        /// Gets the absolute path to a card image file.
        /// Returns null if image doesn't exist.
        /// </summary>
        public static string GetCardImagePath(Card card)
        {
            if (card == null)
                return null;

            if (card.IsJoker)
                return GetJokerPath();

            string value = GetValueFileName(card.Value);
            string suit = GetSuitFileName(card.Suit);

            string fileName = $"{value}_of_{suit}{ImageExtension}";
            string fullPath = GetFullPath(fileName);

            return File.Exists(fullPath) ? fullPath : null;
        }

        /// <summary>
        /// Gets path using alternative naming: {value}{suit_letter}.png (e.g., "2S.png")
        /// </summary>
        public static string GetCardImagePathSimple(Card card)
        {
            if (card == null || card.IsJoker)
                return GetJokerPath();

            string suitLetter = card.Suit switch
            {
                '♠' => "S",
                '♥' => "H",
                '♦' => "D",
                '♣' => "C",
                _ => ""
            };

            string fileName = $"{card.Value}{suitLetter}{ImageExtension}";
            string fullPath = GetFullPath(fileName);

            return File.Exists(fullPath) ? fullPath : null;
        }

        /// <summary>
        /// Gets the card back image path.
        /// </summary>
        public static string GetCardBackPath()
        {
            string[] backNames = { "back", "card_back", "back_blue", "back_red" };
            
            foreach (var name in backNames)
            {
                string path = GetFullPath($"{name}{ImageExtension}");
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        /// <summary>
        /// Gets the joker image path.
        /// </summary>
        public static string GetJokerPath()
        {
            string[] jokerNames = { "joker", "Joker", "joker_red", "joker_black" };
            
            foreach (var name in jokerNames)
            {
                string path = GetFullPath($"{name}{ImageExtension}");
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        /// <summary>
        /// Gets the full absolute path to the assets folder.
        /// </summary>
        private static string GetFullPath(string fileName)
        {
            // Get the directory where the executable is running
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, AssetsPath, fileName);
        }

        private static string GetValueFileName(string cardValue)
        {
            return ValueMapping.TryGetValue(cardValue, out string mapped) ? mapped : cardValue.ToLower();
        }

        private static string GetSuitFileName(char suit)
        {
            return SuitMapping.TryGetValue(suit, out string mapped) ? mapped : "";
        }

        /// <summary>
        /// Checks if all required card images exist in the assets folder.
        /// Useful for debugging missing assets.
        /// </summary>
        public static List<string> CheckMissingImages()
        {
            var missing = new List<string>();
            char[] suits = { '♠', '♥', '♦', '♣' };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (char suit in suits)
            {
                foreach (string val in values)
                {
                    var card = new Card(val, suit, false);
                    if (GetCardImagePath(card) == null)
                        missing.Add($"{val}{suit}");
                }
            }

            if (GetJokerPath() == null)
                missing.Add("Joker");

            if (GetCardBackPath() == null)
                missing.Add("Card Back");

            return missing;
        }

        /// <summary>
        /// Shows a message box with missing images (for debugging).
        /// </summary>
        public static void ShowMissingImagesDialog()
        {
            var missing = CheckMissingImages();
            if (missing.Count > 0)
            {
                string msg = $"Missing {missing.Count} card images:\n" + string.Join(", ", missing);
                MessageBox.Show(msg, "Missing Card Images", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("All card images found!", "Assets Check", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
