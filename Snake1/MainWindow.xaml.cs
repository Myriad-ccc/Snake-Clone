using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Snake1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValueToImage = new()
        {
            { GridValue.Empty, Images.Empty},
            {GridValue.Snake, Images.Body },
            {GridValue.Food, Images.Food }
        };

        private readonly Dictionary<Direction, int> directionToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        private readonly int rows = 16;
        private readonly int cols = 16;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning = false;

        private int topScore = 0;
        private int gamesPlayed = 0;
        private readonly string statsFile = "Stats.txt";

        public MainWindow()
        {
            InitializeComponent();

            gridImages = BuildGrid();
            gameState = new GameState(rows, cols);
            GetGameStats(false);
        }

        private Image[,] BuildGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = (GameGrid.Height * cols) / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new()
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE: {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridValue = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValueToImage[gridValue];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            var headPos = gameState.HeadPosition();
            var image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = directionToRotation[gameState.Direction];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            var snakePositions = new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < snakePositions.Count; i++)
            {
                var pos = snakePositions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(35);
            }
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();

            gameState = new GameState(rows, cols);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
                return;

            switch (e.Key)
            {
                case Key.A: gameState.ChangeDirection(Direction.Left); break;
                case Key.D: gameState.ChangeDirection(Direction.Right); break;
                case Key.W: gameState.ChangeDirection(Direction.Up); break;
                case Key.S: gameState.ChangeDirection(Direction.Down); break;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(90);
                gameState.Move();
                Draw();
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i > 0; i--)
            {
                OverlayText.Text = $"{i}";
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            GetGameStats(true);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = $"ggwphfglntlybbg\nScore: {gameState.Score}\n{(double)gameState.Score / topScore * 100:F1}% of high score";
            await Task.Delay(500);
        }

        private void GetGameStats(bool update)
        {
            if (File.Exists(statsFile))
            {
                var lines = File.ReadAllLines(statsFile);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Top Score:"))
                        int.TryParse(line["Top Score:".Length..].Trim(), out topScore);

                    if (line.StartsWith("Games played:"))
                        int.TryParse(line["Games played:".Length..].Trim(), out gamesPlayed);
                }
            }

            TopScore.Text = $"BEST:{topScore}";
            GamesPlayed.Text = $"GAMES:{gamesPlayed}";

            if (update)
            {
                topScore = Math.Max(topScore, gameState.Score);
                gamesPlayed++;
                File.WriteAllTextAsync(statsFile, $"Top Score: {topScore}\nGames played: {gamesPlayed}");
            }
        }
    }
}