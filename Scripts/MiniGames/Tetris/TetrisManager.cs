using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using InputSystem;
using System.Threading.Tasks;

namespace Tetris
{
    public partial class TetrisManager : ColorRect
    {
        public struct Cell
        {
            public bool IsOccupied;
            public Color Color;

            public Cell(bool isOccupied, Color color)
            {
                IsOccupied = isOccupied;
                Color = color;
            }
        }

        [Export]
        public Godot.Collections.Array<PieceData> Piece = new();

        [Export]
        public ColorRect BackGround;

        [Export]
        private float fallSpeed = 1.0f;

        [Export]
        private float minFallSpeed = 0.1f;

        [Export]
        private float speedDecreaseStep = 0.05f;

        [Export]
        private float speedIncreaseInterval = 10f;

        private float totalGameTime = 0.0f;
        private float timeSinceLastFall = 0.0f;

        private Node2D currentPiece;
        private Vector2 currentPivot;
        private Vector2 direction;
        private Vector2? pivotOverride = null;

        private Cell[,] board;
        private List<ColorRect> activeVisualBlocks = new();

        private ObjectPulling<ColorRect> blockPool;

        public override void _Ready()
        {
            blockPool = new ObjectPulling<ColorRect>(() =>
            {
                var rect = new ColorRect();
                return rect;
            }, 50);

            int width = (int)BackGround.Size.X;
            int height = (int)BackGround.Size.Y;
            board = new Cell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    board[x, y] = new Cell(false, new Color(0, 0, 0, 0));
                }
            }
            SpawnPiece();
        }

        public override void _Process(double delta)
        {
            timeSinceLastFall += (float)delta;
            totalGameTime += (float)delta;

            if (totalGameTime >= speedIncreaseInterval)
            {
                if (fallSpeed > minFallSpeed)
                    fallSpeed = Math.Max(fallSpeed - speedDecreaseStep, minFallSpeed);

                totalGameTime = 0f;
            }

            if (timeSinceLastFall >= fallSpeed)
            {
                timeSinceLastFall = 0.0f;
                bool canMoveDown = true;
                Vector2 dir = new Vector2(0, 1);

                foreach (Node child in currentPiece.GetChildren())
                {
                    if (child is ColorRect colorRect)
                    {
                        Vector2 next = colorRect.GlobalPosition + dir;
                        int x = Mathf.RoundToInt(next.X);
                        int y = Mathf.RoundToInt(next.Y);

                        if (y >= board.GetLength(1) || x < 0 || x >= board.GetLength(0) || board[x, y].IsOccupied)
                        {
                            canMoveDown = false;
                            break;
                        }
                    }
                }

                if (canMoveDown)
                {
                    currentPiece.Position += dir;
                }
                else
                {
                    foreach (ColorRect block in currentPiece.GetChildren().OfType<ColorRect>())
                    {
                        int x = Mathf.RoundToInt(block.GlobalPosition.X);
                        int y = Mathf.RoundToInt(block.GlobalPosition.Y);

                        if (x >= 0 && x < board.GetLength(0) && y >= 0 && y < board.GetLength(1))
                        {
                            board[x, y] = new Cell(true, block.Color);
                            ColorRect visual = new ColorRect
                            {
                                Color = block.Color,
                                Size = new Vector2(1, 1),
                                Position = new Vector2(x, y)
                            };
                            AddChild(visual);
                            activeVisualBlocks.Add(visual);
                        }
                    }

                    currentPiece.QueueFree();
                    ClearLines(); // <--- LLAMADA AQUÍ
                    SpawnPiece();
                }
            }
        }

        public void ClearLines()
        {
            int width = board.GetLength(0);
            int height = board.GetLength(1);

            for (int y = height - 1; y >= 0; y--)
            {
                bool isFull = true;
                for (int x = 0; x < width; x++)
                {
                    if (!board[x, y].IsOccupied)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    for (int x = 0; x < width; x++)
                    {
                        board[x, y] = new Cell(false, new Color(0, 0, 0, 0));

                        var blockToRemove = activeVisualBlocks.FirstOrDefault(b =>
                            Mathf.RoundToInt(b.Position.X) == x &&
                            Mathf.RoundToInt(b.Position.Y) == y);

                        if (blockToRemove != null)
                        {
                            StartFadeToWhiteThenReturn(blockToRemove);
                        }
                    }

                    // Bajar todo lo que está por encima
                    for (int yy = y - 1; yy >= 0; yy--)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            board[x, yy + 1] = board[x, yy];

                            var blockToMove = activeVisualBlocks.FirstOrDefault(b =>
                                Mathf.RoundToInt(b.Position.X) == x &&
                                Mathf.RoundToInt(b.Position.Y) == yy);

                            if (blockToMove != null)
                            {
                                blockToMove.Position += Vector2.Down;
                            }
                        }
                    }

                    // Limpiar fila superior
                    for (int x = 0; x < width; x++)
                    {
                        board[x, 0] = new Cell(false, new Color(0, 0, 0, 0));
                    }

                    y++; // Rechequear esta fila tras mover bloques
                }
            }
        }

        private void StartFadeToWhiteThenReturn(ColorRect block)
        {
            _ = FadeCoroutine(block);
        }

        private async Task FadeCoroutine(ColorRect block)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Color startColor = block.Color;
            Color targetColor = Colors.White;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                block.Color = LerpColor(startColor, targetColor, t);
                await ToSignal(GetTree().CreateTimer(0.016f), "timeout");
                elapsed += 0.016f;
            }

            block.Color = targetColor;

            block.GetParent()?.RemoveChild(block); // mantener esto como pediste
            activeVisualBlocks.Remove(block);
            blockPool.Return(block); // esto ya hace Visible = false
        }

        private Color LerpColor(Color a, Color b, float t)
        {
            return new Color(
                Mathf.Lerp(a.R, b.R, t),
                Mathf.Lerp(a.G, b.G, t),
                Mathf.Lerp(a.B, b.B, t),
                Mathf.Lerp(a.A, b.A, t)
            );
        }

        public void HardDrop(InputActionState state)
        {
            if (state.state == InputActionState.PressState.JustPressed)
            {
                List<ColorRect> blocks = currentPiece.GetChildren().OfType<ColorRect>().ToList();
                int dropDistance = 0;
                bool canMove = true;

                while (canMove)
                {
                    dropDistance++;
                    foreach (ColorRect block in blocks)
                    {
                        Vector2 nextGlobal = block.GlobalPosition + new Vector2(0, dropDistance);
                        int x = Mathf.RoundToInt(nextGlobal.X);
                        int y = Mathf.RoundToInt(nextGlobal.Y);

                        if (y >= board.GetLength(1) || x < 0 || x >= board.GetLength(0) || board[x, y].IsOccupied)
                        {
                            dropDistance--;
                            canMove = false;
                            break;
                        }
                    }
                }

                currentPiece.Position += new Vector2(0, dropDistance);

                foreach (ColorRect block in blocks)
                {
                    int x = Mathf.RoundToInt(block.GlobalPosition.X);
                    int y = Mathf.RoundToInt(block.GlobalPosition.Y);

                    if (x >= 0 && x < board.GetLength(0) && y >= 0 && y < board.GetLength(1))
                    {
                        board[x, y] = new Cell(true, block.Color);
                        ColorRect visual = new ColorRect
                        {
                            Color = block.Color,
                            Size = new Vector2(1, 1),
                            Position = new Vector2(x, y)
                        };
                        AddChild(visual);
                        activeVisualBlocks.Add(visual);
                    }
                }

                currentPiece.QueueFree();
                ClearLines(); // <--- LLAMADA TAMBIÉN AQUÍ SI QUIERES QUE FUNCIONE CON HARD DROP
                SpawnPiece();
            }
        }

        public void SetPivot(Vector2 pivot)
        {
            pivotOverride = pivot;
        }

        public void Rotate(InputActionState state)
        {
            if (state.state != InputActionState.PressState.JustPressed || currentPiece == null)
                return;

            List<ColorRect> blocks = currentPiece.GetChildren().OfType<ColorRect>().ToList();
            List<Vector2> originalLocal = blocks.Select(b => b.Position).ToList();

            Vector2 pivot = currentPivot;

            List<Vector2> rotatedLocal = new List<Vector2>(blocks.Count);
            foreach (Vector2 pos in originalLocal)
            {
                Vector2 d = pos - pivot;
                Vector2 r = new Vector2(-d.Y, d.X) + pivot;
                rotatedLocal.Add(r);
            }

            float rotatedMinX = rotatedLocal.Min(p => p.X);
            float rotatedMaxX = rotatedLocal.Max(p => p.X);
            float correctionX = 0;
            if (currentPiece.Position.X + rotatedMinX < 0)
                correctionX = -(currentPiece.Position.X + rotatedMinX);
            else if (currentPiece.Position.X + rotatedMaxX >= board.GetLength(0))
                correctionX = (board.GetLength(0) - 1) - (currentPiece.Position.X + rotatedMaxX);

            for (int i = 0; i < rotatedLocal.Count; i++)
                rotatedLocal[i] += new Vector2(correctionX, 0);

            bool valid = true;
            HashSet<Vector2> uniquePositions = new();
            for (int i = 0; i < blocks.Count; i++)
            {
                Vector2 global = currentPiece.Position + rotatedLocal[i];
                int x = Mathf.RoundToInt(global.X);
                int y = Mathf.RoundToInt(global.Y);

                if (!uniquePositions.Add(global) ||
                    x < 0 || x >= board.GetLength(0) ||
                    y < 0 || y >= board.GetLength(1) ||
                    board[x, y].IsOccupied)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                for (int i = 0; i < blocks.Count; i++)
                    blocks[i].Position = rotatedLocal[i];
            }
            else
            {
                for (int i = 0; i < blocks.Count; i++)
                    blocks[i].Position = originalLocal[i];
            }
        }

        private void TryMovePiece(InputActionState state)
        {
            Vector2 rawDirection = (Vector2)state.strength;
            direction = Mathf.Abs(rawDirection.X) > Mathf.Abs(rawDirection.Y)
                ? new Vector2(Mathf.Sign(rawDirection.X), 0)
                : new Vector2(0, Mathf.Sign(rawDirection.Y));

            bool canMove = true;
            foreach (Node child in currentPiece.GetChildren())
            {
                if (child is ColorRect colorRect)
                {
                    Vector2 nextPos = colorRect.GlobalPosition + direction;
                    int x = Mathf.RoundToInt(nextPos.X);
                    int y = Mathf.RoundToInt(nextPos.Y);

                    if (x < 0 || x >= BackGround.Size.X || y < 0 || y >= BackGround.Size.Y || board[x, y].IsOccupied)
                    {
                        canMove = false;
                        break;
                    }
                }
            }

            if (canMove)
                currentPiece.Position += direction;
        }

        public void SpawnPiece()
        {
            var random = new Random();
            PieceData randomPiece = Piece[random.Next(Piece.Count)];

            currentPiece = new Node2D();
            AddChild(currentPiece);

            int randomX = (int)GD.RandRange(0, BackGround.Size.X - 1);
            currentPiece.Position = new Vector2(randomX, 0);

            foreach (var pos in randomPiece.Pos)
            {
                ColorRect block = blockPool != null ? blockPool.Get() : null;

                block.Color = randomPiece.col;
                block.Size = new Vector2(1, 1);
                block.Position = pos;
                currentPiece.AddChild(block);
            }

            currentPivot = randomPiece.Pivot;

            HashSet<int> xOverflow = new();
            foreach (ColorRect block in currentPiece.GetChildren().OfType<ColorRect>())
            {
                if (block.GlobalPosition.X >= BackGround.Position.X + BackGround.Size.X)
                    xOverflow.Add((int)block.GlobalPosition.X);
            }

            currentPiece.Position -= new Vector2(xOverflow.Count, 0);
        }
    }
}