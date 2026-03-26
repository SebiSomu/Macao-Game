using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Macao_Game_V2.Domain;

namespace Macao_Game_V2
{
    /// <summary>
    /// UserControl that renders a realistic Bicycle-style playing card using WPF shapes.
    /// Fully programmatic — no image assets required.
    /// </summary>
    public partial class CardVisual : UserControl
    {
        // ── Dependency Properties ────────────────────────────────────────────────

        public static readonly DependencyProperty CardProperty =
            DependencyProperty.Register("Card", typeof(Card), typeof(CardVisual),
                new PropertyMetadata(null, OnCardChanged));

        public static readonly DependencyProperty IsFaceDownProperty =
            DependencyProperty.Register("IsFaceDown", typeof(bool), typeof(CardVisual),
                new PropertyMetadata(false, OnIsFaceDownChanged));

        public static readonly DependencyProperty IsDarkModeProperty =
            DependencyProperty.Register("IsDarkMode", typeof(bool), typeof(CardVisual),
                new PropertyMetadata(false, OnDarkModeChanged));

        public Card Card
        {
            get => (Card)GetValue(CardProperty);
            set => SetValue(CardProperty, value);
        }

        public bool IsFaceDown
        {
            get => (bool)GetValue(IsFaceDownProperty);
            set => SetValue(IsFaceDownProperty, value);
        }

        public bool IsDarkMode
        {
            get => (bool)GetValue(IsDarkModeProperty);
            set => SetValue(IsDarkModeProperty, value);
        }

        // ── Card dimensions ──────────────────────────────────────────────────────

        private const double CardW = 80;
        private const double CardH = 120;
        private const double Radius = 6;
        private const double Margin = 5;

        // ── Colors ───────────────────────────────────────────────────────────────

        // Light Mode Colors
        private static readonly Brush LightRedBrush = new SolidColorBrush(Color.FromRgb(185, 20, 20));
        private static readonly Brush LightBlackBrush = new SolidColorBrush(Color.FromRgb(15, 15, 15));
        private static readonly Brush LightWhiteBrush = Brushes.White;
        private static readonly Brush LightPaperColor = new SolidColorBrush(Color.FromRgb(253, 251, 244));

        // Dark Mode Colors
        private static readonly Brush DarkRedBrush = new SolidColorBrush(Color.FromRgb(220, 50, 50));
        private static readonly Brush DarkWhiteBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        private static readonly Brush DarkPaperColor = new SolidColorBrush(Color.FromRgb(20, 20, 20));
        private static readonly Brush DarkBorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        // Card Back Colors
        private static readonly Brush BackBlue = new SolidColorBrush(Color.FromRgb(12, 36, 97));
        private static readonly Brush BackAccent = new SolidColorBrush(Color.FromRgb(30, 70, 180));
        private static readonly Brush BackBorder = new SolidColorBrush(Color.FromRgb(200, 160, 60));

        // ── Color getters based on mode ───────────────────────────────────────────

        private Brush GetColor(bool isRed, bool isDarkMode)
        {
            if (isDarkMode)
                return isRed ? DarkRedBrush : DarkWhiteBrush;
            else
                return isRed ? LightRedBrush : LightBlackBrush;
        }

        private Brush GetPaperColor(bool isDarkMode)
        {
            return isDarkMode ? DarkPaperColor : LightPaperColor;
        }

        private Brush GetBorderColor(bool isDarkMode)
        {
            return isDarkMode ? DarkBorderBrush : Brushes.Transparent;
        }

        // ── Font ─────────────────────────────────────────────────────────────────

        private static readonly FontFamily SerifFont = new FontFamily("Georgia, Times New Roman, serif");

        // ── Container ────────────────────────────────────────────────────────────

        private Grid _container;

        public CardVisual()
        {
            InitializeComponent();
            Width = CardW;
            Height = CardH;
            Background = Brushes.Transparent;

            _container = new Grid();
            Content = _container;

            UpdateVisual();
        }

        // ── Change callbacks ─────────────────────────────────────────────────────

        private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((CardVisual)d).UpdateVisual();

        private static void OnIsFaceDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((CardVisual)d).UpdateVisual();

        private static void OnDarkModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((CardVisual)d).UpdateVisual();

        // ── Main render dispatcher ───────────────────────────────────────────────

        private void UpdateVisual()
        {
            _container.Children.Clear();

            if (IsFaceDown)
                RenderCardBack();
            else
                RenderCardFace();
        }

        // ════════════════════════════════════════════════════════════════════════
        // CARD BACK — classic Bicycle-style crosshatch + gold border
        // ════════════════════════════════════════════════════════════════════════

        private void RenderCardBack()
        {
            var root = new Canvas { Width = CardW, Height = CardH };

            // Card base with shadow
            root.Children.Add(MakeCardRect(GetPaperColor(IsDarkMode)));

            // Outer gold border (thin frame inset)
            root.Children.Add(MakeRect(4, 4, CardW - 8, CardH - 8, Brushes.Transparent, BackBorder, 1.5));

            // Blue fill area
            root.Children.Add(MakeRect(7, 7, CardW - 14, CardH - 14, BackBlue, Brushes.Transparent, 0));

            // Crosshatch diagonal pattern inside blue area
            var patternClip = new RectangleGeometry(new Rect(7, 7, CardW - 14, CardH - 14));
            var patternCanvas = new Canvas { Width = CardW, Height = CardH, Clip = patternClip };
            for (int i = -20; i < 30; i++)
            {
                patternCanvas.Children.Add(MakeLine(i * 8, 7, i * 8 + (CardH - 14), CardH - 7,
                    BackAccent, 0.7));
                patternCanvas.Children.Add(MakeLine(i * 8 + (CardH - 14), 7, i * 8, CardH - 7,
                    BackAccent, 0.7));
            }
            root.Children.Add(patternCanvas);

            // Inner gold border
            root.Children.Add(MakeRect(10, 10, CardW - 20, CardH - 20, Brushes.Transparent, BackBorder, 1));

            // Center diamond ornament
            var diamond = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(CardW / 2, CardH / 2 - 9),
                    new Point(CardW / 2 + 6, CardH / 2),
                    new Point(CardW / 2, CardH / 2 + 9),
                    new Point(CardW / 2 - 6, CardH / 2)
                },
                Fill = BackBorder,
                Stroke = Brushes.Transparent
            };
            root.Children.Add(diamond);

            _container.Children.Add(AddDropShadow(root));
        }

        // ════════════════════════════════════════════════════════════════════════
        // CARD FACE
        // ════════════════════════════════════════════════════════════════════════

        private void RenderCardFace()
        {
            if (Card == null) return;

            bool isRed = Card.Suit == '♥' || Card.Suit == '♦';
            Brush color = GetColor(isRed, IsDarkMode);
            string suit = Card.Suit.ToString();

            var root = new Canvas { Width = CardW, Height = CardH };
            root.Children.Add(MakeCardRect(GetPaperColor(IsDarkMode)));

            // Fine inset border line (more visible in dark mode)
            var borderBrush = GetBorderColor(IsDarkMode);
            double borderOpacity = IsDarkMode ? 0.8 : 0.3;
            root.Children.Add(MakeRect(3, 3, CardW - 6, CardH - 6, Brushes.Transparent,
                new SolidColorBrush(Color.FromArgb((byte)(255 * borderOpacity), 100, 100, 100)), 0.5));

            if (Card.IsJoker)
            {
                RenderJokerFace(root, color);
            }
            else
            {
                // Corner indices (top-left + bottom-right mirrored)
                AddCornerIndex(root, Card.Value, suit, color,
                    Margin, Margin, false);
                AddCornerIndex(root, Card.Value, suit, color,
                    CardW - Margin, CardH - Margin, true);

                if (IsFaceCard(Card.Value))
                    RenderFaceCard(root, Card.Value, suit, color, isRed);
                else if (Card.Value == "A")
                    RenderAce(root, suit, color);
                else if (int.TryParse(Card.Value, out int n))
                    RenderPips(root, n, suit, color);
            }

            _container.Children.Add(AddDropShadow(root));
        }

        // ── Corner index ─────────────────────────────────────────────────────────

        private void AddCornerIndex(Canvas root, string value, string suit,
                                    Brush color, double x, double y, bool flipped)
        {
            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            panel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = value == "10" ? 9.5 : 11,
                FontWeight = FontWeights.Bold,
                FontFamily = SerifFont,
                Foreground = color,
                LineHeight = 12,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            panel.Children.Add(new TextBlock
            {
                Text = suit,
                FontSize = 9,
                Foreground = color,
                LineHeight = 10,
                Margin = new Thickness(0, -1, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            if (flipped)
            {
                panel.RenderTransformOrigin = new Point(0.5, 0.5);
                panel.RenderTransform = new RotateTransform(180);
                // Measure approximate size to offset correctly
                Canvas.SetLeft(panel, x - 13);
                Canvas.SetTop(panel, y - 22);
            }
            else
            {
                Canvas.SetLeft(panel, x);
                Canvas.SetTop(panel, y);
            }

            root.Children.Add(panel);
        }

        // ── Ace ──────────────────────────────────────────────────────────────────

        private void RenderAce(Canvas root, string suit, Brush color)
        {
            // Large centered suit symbol — classic Bicycle ace style
            double fontSize = 62;
            var tb = new TextBlock
            {
                Text = suit,
                FontSize = fontSize,
                Foreground = color,
                FontFamily = SerifFont
            };

            // Measure roughly: suit glyphs are ~0.7 * fontSize wide
            double approxW = fontSize * 0.72;
            double approxH = fontSize * 0.9;
            Canvas.SetLeft(tb, (CardW - approxW) / 2 - 2);
            Canvas.SetTop(tb, (CardH - approxH) / 2 - 4);
            root.Children.Add(tb);
        }

        // ── Pip number cards ─────────────────────────────────────────────────────

        private void RenderPips(Canvas root, int value, string suit, Brush color)
        {
            var positions = GetPipPositions(value);
            // Pips: 19px for cards ≤6, 17px for 7–10
            double fs = value <= 6 ? 19 : 17;
            // Half-width/height of a suit glyph at given font size (empirical for Unicode symbols)
            double hw = fs * 0.42; // horizontal half
            double hh = fs * 0.50; // vertical half

            foreach (var (px, py, inv) in positions)
            {
                var tb = new TextBlock
                {
                    Text = suit,
                    FontSize = fs,
                    Foreground = color
                };

                if (inv)
                {
                    tb.RenderTransformOrigin = new Point(0.5, 0.5);
                    tb.RenderTransform = new RotateTransform(180);
                    // When rotated 180°, the anchor stays top-left of the original bbox,
                    // so shift by full glyph size to keep visual center at (px, py)
                    Canvas.SetLeft(tb, px - hw);
                    Canvas.SetTop(tb, py - hh);
                }
                else
                {
                    Canvas.SetLeft(tb, px - hw);
                    Canvas.SetTop(tb, py - hh);
                }

                root.Children.Add(tb);
            }
        }

        // Pip grid positions — (centerX, centerY, inverted)
        // Coordinates are visual centers; RenderPips offsets by half-glyph-size.
        // Card area available between corner indices: ~Y 26..94
        private (double x, double y, bool inv)[] GetPipPositions(int value)
        {
            const double L = 20;   // left column X center
            const double R = 60;   // right column X center
            const double C = 40;   // center column X

            const double T = 27;  // top row
            const double B = 93;  // bottom row
            const double M = 60;  // middle row
            const double UT = 42;  // upper-middle band
            const double UB = 78;  // lower-middle band
            const double MT = 49;  // middle-top (9/10)
            const double MB = 71;  // middle-bottom (9/10)

            return value switch
            {
                2 => new[] { (C, T, false), (C, B, true) },
                3 => new[] { (C, T, false), (C, M, false), (C, B, true) },
                4 => new[] { (L, T, false), (R, T, false), (L, B, true), (R, B, true) },
                5 => new[] { (L, T, false), (R, T, false), (C, M, false), (L, B, true), (R, B, true) },
                6 => new[] { (L, T, false), (R, T, false), (L, M, false), (R, M, false), (L, B, true), (R, B, true) },
                7 => new[] { (L, T, false), (R, T, false), (C, UT, false), (L, M, false), (R, M, false), (L, B, true), (R, B, true) },
                8 => new[] { (L, T, false), (R, T, false), (C, UT, false), (L, M, false), (R, M, false), (C, UB, true), (L, B, true), (R, B, true) },
                9 => new[] { (L, T, false), (R, T, false), (L, MT, false), (R, MT, false), (C, M, false),
                               (L, MB, true), (R, MB, true), (L, B, true), (R, B, true) },
                10 => new[] { (L, T, false), (R, T, false), (C, UT, false), (L, MT, false), (R, MT, false),
                               (L, MB, true), (R, MB, true), (C, UB, true), (L, B, true), (R, B, true) },
                _ => Array.Empty<(double, double, bool)>()
            };
        }

        // ── Face cards (J/Q/K) ───────────────────────────────────────────────────

        private void RenderFaceCard(Canvas root, string value, string suit, Brush color, bool isRed)
        {
            // Inner decorative frame — inset from corner indices
            double fx = 13, fy = 20, fw = CardW - 26, fh = CardH - 40;

            // Frame background — very subtle tint
            root.Children.Add(MakeRect(fx, fy, fw, fh,
                isRed ? new SolidColorBrush(Color.FromArgb(12, 180, 20, 20))
                      : new SolidColorBrush(Color.FromArgb(12, 0, 0, 0)),
                color, 0.8));

            // Filigree corner dots inside frame
            AddFrameCornerOrnaments(root, fx, fy, fw, fh, color);

            double cx = fx + fw / 2.0; // horizontal center of frame

            // Zone 1 — small suit top (upright)
            var topSuit = new TextBlock { Text = suit, FontSize = 11, Foreground = color };
            // Center horizontally: glyph ~0.7*11 wide ≈ 7.7px → offset -3.8
            Canvas.SetLeft(topSuit, cx - 5);
            Canvas.SetTop(topSuit, fy + 4);
            root.Children.Add(topSuit);

            // Zone 2 — large serif letter (J / Q / K)
            // Georgia Bold 32px: ascender ~24px, so top at Y 38 puts baseline ~62
            var letter = new TextBlock
            {
                Text = value,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                FontFamily = SerifFont,
                Foreground = color
            };
            // Letters are ~18px wide at 32px Georgia Bold
            Canvas.SetLeft(letter, cx - 10);
            Canvas.SetTop(letter, fy + 16);
            root.Children.Add(letter);

            // Zone 3 — thin divider at vertical midpoint
            double midY = fy + fh / 2.0 + 2;
            root.Children.Add(MakeLine(fx + 4, midY, fx + fw - 4, midY,
                new SolidColorBrush(Color.FromArgb(45, 0, 0, 0)), 0.5));

            // Zone 4 — suit symbol below divider, larger, centered
            var centerSuit = new TextBlock
            {
                Text = suit,
                FontSize = 20,
                Foreground = color
            };
            // Suit glyph ~0.7*20 = 14px wide, ~0.85*20 = 17px tall
            Canvas.SetLeft(centerSuit, cx - 8);
            Canvas.SetTop(centerSuit, midY + 4);
            root.Children.Add(centerSuit);

            // Zone 5 — small suit bottom (rotated), mirrors zone 1
            var botSuit = new TextBlock
            {
                Text = suit,
                FontSize = 11,
                Foreground = color,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(180)
            };
            Canvas.SetLeft(botSuit, cx - 5);
            Canvas.SetTop(botSuit, fy + fh - 15);
            root.Children.Add(botSuit);
        }

        private void AddFrameCornerOrnaments(Canvas root, double fx, double fy,
                                             double fw, double fh, Brush color)
        {
            double r = 2;
            double pad = 4;
            Brush faded = new SolidColorBrush(
                Color.FromArgb(90,
                    ((SolidColorBrush)color).Color.R,
                    ((SolidColorBrush)color).Color.G,
                    ((SolidColorBrush)color).Color.B));

            foreach (var (ex, ey) in new[]
            {
                (fx + pad,      fy + pad),
                (fx + fw - pad, fy + pad),
                (fx + pad,      fy + fh - pad),
                (fx + fw - pad, fy + fh - pad)
            })
            {
                root.Children.Add(new Ellipse
                {
                    Width = r * 2,
                    Height = r * 2,
                    Fill = faded,
                    Margin = new Thickness(0)
                }.Also(e => { Canvas.SetLeft(e, ex - r); Canvas.SetTop(e, ey - r); }));
            }
        }

        // ── Joker ────────────────────────────────────────────────────────────────

        private void RenderJokerFace(Canvas root, Brush color)
        {
            // Star burst background
            var starCanvas = new Canvas { Width = CardW, Height = CardH };
            for (int i = 0; i < 8; i++)
            {
                double angle = i * 22.5 * Math.PI / 180;
                double length = 18;
                double cx = CardW / 2, cy = CardH / 2;
                starCanvas.Children.Add(MakeLine(cx, cy,
                    cx + Math.Cos(angle) * length,
                    cy + Math.Sin(angle) * length,
                    new SolidColorBrush(Color.FromArgb(50,
                        ((SolidColorBrush)color).Color.R,
                        ((SolidColorBrush)color).Color.G,
                        ((SolidColorBrush)color).Color.B)), 1.5));
            }
            root.Children.Add(starCanvas);

            // "JOKER" text top
            var jokerTop = new TextBlock
            {
                Text = "JOKER",
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                FontFamily = SerifFont,
                Foreground = color
            };
            Canvas.SetLeft(jokerTop, 18);
            Canvas.SetTop(jokerTop, 16);
            root.Children.Add(jokerTop);

            // Jester hat emoji — large centered
            var jester = new TextBlock
            {
                Text = "🃏",
                FontSize = 38
            };
            Canvas.SetLeft(jester, 15);
            Canvas.SetTop(jester, 36);
            root.Children.Add(jester);

            // "JOKER" text bottom (rotated)
            var jokerBot = new TextBlock
            {
                Text = "JOKER",
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                FontFamily = SerifFont,
                Foreground = color,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(180)
            };
            Canvas.SetLeft(jokerBot, 18);
            Canvas.SetTop(jokerBot, 96);
            root.Children.Add(jokerBot);
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS — shape factories
        // ════════════════════════════════════════════════════════════════════════

        private static UIElement MakeCardRect(Brush fill)
        {
            return new Border
            {
                Width = CardW,
                Height = CardH,
                Background = fill,
                BorderBrush = new SolidColorBrush(Color.FromRgb(160, 150, 135)),
                BorderThickness = new Thickness(0.8),
                CornerRadius = new CornerRadius(Radius)
            };
        }

        private static UIElement MakeRect(double x, double y, double w, double h,
                                          Brush fill, Brush stroke, double strokeW)
        {
            var rect = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeW,
                RadiusX = 2,
                RadiusY = 2
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            return rect;
        }

        private static UIElement MakeLine(double x1, double y1, double x2, double y2,
                                          Brush stroke, double thickness)
        {
            return new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = stroke,
                StrokeThickness = thickness
            };
        }

        private Border AddDropShadow(Canvas canvas)
        {
            // Wrap canvas in a border for shadow effect
            var border = new Border
            {
                Child = canvas,
                CornerRadius = new CornerRadius(Radius)
            };
            border.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = IsDarkMode ? 2 : 3,
                Opacity = IsDarkMode ? 0.6 : 0.3,
                BlurRadius = IsDarkMode ? 4 : 5
            };
            return border;
        }

        private static bool IsFaceCard(string value)
            => value == "J" || value == "Q" || value == "K";
    }

    // ── Extension helper ─────────────────────────────────────────────────────────

    internal static class UiExtensions
    {
        public static T Also<T>(this T obj, Action<T> action) { action(obj); return obj; }
    }
}