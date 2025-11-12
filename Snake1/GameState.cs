using System.Windows;
using System.Windows.Media.Media3D;

namespace Snake1
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Direction { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> directionChanges = [];
        public LinkedList<Position> snakePositions = [];
        private readonly Random random = new();

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Direction = Direction.Right;

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;

            for (int c = 1; c < 5; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (Grid[r, c] == GridValue.Empty)
                        yield return new Position(r, c);
        }

        private void AddFood()
        {
            List<Position> empty = [.. EmptyPositions()];

            if (empty.Count == 0)
            {
                MessageBox.Show("wtf");
                return;
            }

            var pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        public Position HeadPosition() => snakePositions.First.Value;
        public Position TailPosition() => snakePositions.Last.Value;
        public IEnumerable<Position> SnakePositions() => snakePositions;

        private void AddHead(Position position)
        {
            snakePositions.AddFirst(position);
            Grid[position.Row, position.Col] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            var tail = TailPosition();
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        public Direction GetLastDirection()
        {
            if (directionChanges.Count == 0)
                return Direction;
            return directionChanges.Last.Value;
        }

        public bool CanChangeDirection(Direction newDirection)
        {
            if (directionChanges.Count == 2)
                return false;

            var lastDirection = GetLastDirection();
            return newDirection != lastDirection && newDirection != lastDirection.Opposite();
        }

        public void ChangeDirection(Direction direction)
        {
            if (CanChangeDirection(direction))
                directionChanges.AddLast(direction);
        }

        private bool OutsideGrid(Position position)
        {
            return
                position.Row < 0 || position.Row >= Rows ||
                position.Col < 0 || position.Col >= Cols;
        }

        private GridValue NextMove(Position newHeadPosition)
        {
            if (OutsideGrid(newHeadPosition))
                return GridValue.Outside;

            if (newHeadPosition == TailPosition())
                return GridValue.Empty;

            return Grid[newHeadPosition.Row, newHeadPosition.Col];
        }

        public void Move()
        {
            if (directionChanges.Count > 0)
            {
                Direction = directionChanges.First.Value;
                directionChanges.RemoveFirst();
            }

            var newHeadPosition = HeadPosition().Translate(Direction);
            GridValue move = NextMove(newHeadPosition);

            if (move == GridValue.Outside || move == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (move == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPosition);
            }
            else if (move == GridValue.Food)
            {
                AddHead(newHeadPosition);
                Score++;
                AddFood();
            }
        }
    }
}
