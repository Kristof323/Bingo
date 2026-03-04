using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BingoGUI
{
    public partial class MainWindow : Window
    {
        private readonly TextBox[,] _cells = new TextBox[5, 5];
        private readonly Random _rng = new Random();

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
                for (int c = 0; c < 5; c++)
                {
                    var tb = new TextBox
                    {
                        Width = 44,
                        Height = 28,
                        Margin = new Thickness(4),
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };
                    _cells[r, c] = tb;
                    CardGrid.Children.Add(tb);
                }
        }

        private void SetCenterX()
        {
            var center = _cells[2, 2];
            center.Text = "X";
            center.IsReadOnly = true;
            center.Focusable = false;
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
                    _cells[r, c].Text = values[r].ToString(CultureInfo.InvariantCulture);
                }
            }
            SetCenterX();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SetCenterX();

            var path = (FileNameTextBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Adj meg fájlnevet (pl. bingo.txt)!");
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

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Mentve: " + path);
        }

        private void LoadCardFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            if (lines.Length != 5) throw new InvalidDataException("A fájlnak 5 sora kell legyen.");

            for (int r = 0; r < 5; r++)
            {
                var parts = lines[r].Split(';');
                if (parts.Length != 5) throw new InvalidDataException("Minden sornak 5 mezőt kell tartalmaznia.");

                for (int c = 0; c < 5; c++)
                {
                    var t = parts[c].Trim();
                    _cells[r, c].IsReadOnly = false;
                    _cells[r, c].Text = t;
                }
            }

            SetCenterX();
        }



    }
}


