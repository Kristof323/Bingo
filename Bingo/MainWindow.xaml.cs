using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;


namespace BingoGUI
{
    public partial class MainWindow : Window
    {
        private readonly TextBox[,] _cells = new TextBox[5, 5];
        private readonly Random _rng = new Random();
        private readonly List<string> _playerFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            BuildCardInputs();
            SetCenterX();
        }

        private void BuildCardInputs()
        {
            CardGrid.Children.Clear();

            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    var tb = new TextBox
                    {
                        Width = 50,
                        Height = 36,
                        Margin = new Thickness(3),
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FontSize = 16,
                        FontWeight = FontWeights.SemiBold,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                        BorderThickness = new Thickness(1)
                    };

                    _cells[r, c] = tb;
                    CardGrid.Children.Add(tb);
                }
            }
        }

        private void SetCenterX()
        {
            var center = _cells[2, 2];
            center.Text = "X";
            center.IsReadOnly = true;
            center.Focusable = false;
            center.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            for (int c = 0; c < 5; c++)
            {
                int start = c * 15 + 1;
                var values = Enumerable.Range(start, 15)
                    .OrderBy(_ => _rng.Next())
                    .Take(5)
                    .ToArray();

                for (int r = 0; r < 5; r++)
                {
                    if (r == 2 && c == 2) continue;

                    _cells[r, c].IsReadOnly = false;
                    _cells[r, c].Background = Brushes.White;
                    _cells[r, c].Text = values[r].ToString(CultureInfo.InvariantCulture);
                }
            }

            SetCenterX();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SetCenterX();

            if (!ValidateFilled())
            {
                MessageBox.Show("Minden mező legyen 1..75 közötti szám (a középső X kivétel)!",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var filename = (FileNameTextBox.Text ?? "bingo.txt").Trim();
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            if (string.IsNullOrWhiteSpace(filename))
            {
                MessageBox.Show("Adj meg fájlnevet (pl. bingo.txt)!",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sb = new StringBuilder();

            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    var t = (_cells[r, c].Text ?? "").Trim();
                    if (r == 2 && c == 2) t = "X";
                    if (c > 0) sb.Append(';');
                    sb.Append(t);
                }
                if (r < 4) sb.AppendLine();
            }

            try
            {
                System.IO.File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
                MessageBox.Show($"Mentve: {path}", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mentési hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool ValidateFilled()
        {
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    if (r == 2 && c == 2) continue;

                    var t = (_cells[r, c].Text ?? "").Trim();
                    if (!int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
                        return false;

                    if (n < 1 || n > 75)
                        return false;
                }
            }
            return true;
        }

        private void LoadPlayerListButton_Click(object sender, RoutedEventArgs e)
        {
            var filename = (PlayerNameTextBox.Text ?? "").Trim();
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            if (string.IsNullOrWhiteSpace(filename))
            {
                MessageBox.Show("Adj meg nevek fájlt (pl. nevek.text)!",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show($"A fájl nem található: {path}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _playerFiles.Clear();
                _playerFiles.AddRange(LoadNameList(path));

                PlayerComboBox.Items.Clear();
                foreach (var file in _playerFiles)
                {
                    PlayerComboBox.Items.Add(file);
                }

                MessageBox.Show($"{_playerFiles.Count} játékos betöltve.",
                    "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Betöltési hiba: {ex.Message}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void PlayerComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PlayerComboBox.SelectedItem == null) return;

            var fileName = PlayerComboBox.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(fileName)) return;

            if (!File.Exists(fileName))
            {
                MessageBox.Show($"A fájl nem található: {fileName}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                LoadCardFromFile(fileName);
                MessageBox.Show($"Betöltve: {fileName}", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Betöltési hiba: {ex.Message}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string[] LoadNameList(string path)
        {
            return System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToArray();
        }


        private void LoadCardFromFile(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length != 5)
                throw new InvalidDataException("A fájlnak pontosan 5 sora kell legyen.");

            for (int r = 0; r < 5; r++)
            {
                var parts = lines[r].Split(';');
                if (parts.Length != 5)
                    throw new InvalidDataException($"A(z) {r + 1}. sornak pontosan 5 mezőt kell tartalmaznia.");

                for (int c = 0; c < 5; c++)
                {
                    var t = parts[c].Trim();
                    _cells[r, c].IsReadOnly = false;
                    _cells[r, c].Background = Brushes.White;
                    _cells[r, c].Text = t;
                }
            }

            SetCenterX();
        }
    }
}
