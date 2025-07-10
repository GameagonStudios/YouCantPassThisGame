using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using InputSystem;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tetris
{
	public partial class TetrisManager : Node2D
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
		private int LinesCondition = 3;

		[Export]
		private float speedDecreaseStep = 0.05f;

		[Export]
		private float speedIncreaseInterval = 10f;

		[Export]
		public bool firstGame = true;

		[Export]
		public string SaveString = "FirstGame";


		private float totalGameTime = 0.0f;
		private float timeSinceLastFall = 0.0f;

		private Node2D currentPiece;
		private Vector2 currentPivot;
		private Vector2 direction;
		private Vector2? pivotOverride = null;

		private Cell[,] board;
		private List<ColorRect> activeVisualBlocks = new();

		private ObjectPulling<ColorRect> blockPool;

		private bool isGameOver = false;

		private PieceData lastPieceData = null;
		int TopY = 0;
		int NumLines = 0;

		int width;
		int height;


		public override void _Ready()
		{
			LoadFirstGameValue();
			blockPool = new ObjectPulling<ColorRect>(() =>
			{
				var rect = new ColorRect();
				return rect;
			}, 50);

			width = (int)BackGround.Size.X;
			height = (int)BackGround.Size.Y;
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
				if (!Godot.GodotObject.IsInstanceValid(currentPiece))
					return;

				foreach (Node child in currentPiece.GetChildren())
				{
					if (child is ColorRect colorRect)
					{
						Vector2 next = colorRect.GlobalPosition + dir;
						int x = Mathf.RoundToInt(next.X);
						int y = Mathf.RoundToInt(next.Y);
						if (y >= 0 && x >= 0 && x < board.GetLength(0) && y < board.GetLength(1) - TopY)
						{
							if (board[x, y].IsOccupied)
							{
								canMoveDown = false;
								break;
							}
						}
						else if (y >= board.GetLength(1) - TopY)
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

						if (y >= 0)
						{
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

					}

					currentPiece.QueueFree();
					_ = ClearLines().ContinueWith(_ =>
					{
						if (!isGameOver)
						{
							CallDeferred(nameof(SpawnPiece));
						}
					});
				}

				verifyPiece();
			}
		}

		public async Task ClearLines()
		{
			width = board.GetLength(0);
			height = board.GetLength(1);

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
					List<Task> fadeTasks = new List<Task>();

					for (int x = 0; x < width; x++)
					{
						board[x, y] = new Cell(false, new Color(0, 0, 0, 0));

						var blockToRemove = activeVisualBlocks.FirstOrDefault(b =>
							Mathf.RoundToInt(b.Position.X) == x &&
							Mathf.RoundToInt(b.Position.Y) == y);

						if (blockToRemove != null)
						{
							fadeTasks.Add(StartFadeToWhiteThenReturn(blockToRemove));
						}
					}

					await Task.WhenAll(fadeTasks);

					if (firstGame)
					{
						// Caso especial: subir fondo solo si eliminamos la última fila (la más baja)
						BackGround.Position += new Vector2(0, -1);

						foreach (var block in activeVisualBlocks)
						{
							block.Position += Vector2.Up;
						}

						// Subir el contenido del tablero
						for (int yy = 1; yy < height; yy++)
						{
							for (int x = 0; x < width; x++)
							{
								board[x, yy - 1] = board[x, yy];
							}
						}
						for (int x = 0; x < width; x++)
						{
							board[x, height - 1] = new Cell(false, new Color(0, 0, 0, 0));
						}

						// Ajustes de estado
						TopY += 1;
						GD.Print(TopY);
						NumLines += 1;

						if (NumLines == LinesCondition)
						{
							GameOver();
							return;
						}
					}
					// Baja todas las filas por encima (caso normal o línea intermedia en 1ra partida)
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


					y++; // Rechequear fila después de bajada o subida
				}
			}
		}

		private async Task ClearBoardWithFade()
		{
			List<Task> fadeTasks = new List<Task>();

			foreach (var block in activeVisualBlocks.ToList()) // Copia para evitar modificación durante iteración
			{
				fadeTasks.Add(FadeCoroutine(block));
			}

			await Task.WhenAll(fadeTasks);
		}

		private async Task StartFadeToWhiteThenReturn(ColorRect block)
		{
			await FadeCoroutine(block);
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

			block.GetParent()?.RemoveChild(block); // Eliminar parent antes de meterlo al poool
			activeVisualBlocks.Remove(block);
			blockPool.Return(block); // Visible = false
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


		public void SetPivot(Vector2 pivot)
		{
			pivotOverride = pivot;
		}

		public void Rotate(InputActionState state, bool clockwise)
		{
			if (!Godot.GodotObject.IsInstanceValid(currentPiece))
				return;

			if (state.state != InputActionState.PressState.JustPressed || currentPiece == null)
				return;

			List<ColorRect> blocks = currentPiece.GetChildren().OfType<ColorRect>().ToList();
			List<Vector2> originalLocal = blocks.Select(b => b.Position).ToList();

			Vector2 pivot = currentPivot;

			// Seleccionamos la función de rotación sin usar if
			Func<Vector2, Vector2> rotateFunc = clockwise
				? (Vector2 d) => new Vector2(-d.Y, d.X)     // Derecha (90°)
				: (Vector2 d) => new Vector2(d.Y, -d.X);    // Izquierda (-90°)

			List<Vector2> rotatedLocal = originalLocal
				.Select(pos => rotateFunc(pos - pivot) + pivot)
				.ToList();

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
				Vector2 global = currentPiece.Position.Round() + rotatedLocal[i];
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
			if (!GetTree().Paused)
			{
				Vector2 rawDirection = (Vector2)state.strength;
				direction = Mathf.Abs(rawDirection.X) > Mathf.Abs(rawDirection.Y)
					? new Vector2(Mathf.Sign(rawDirection.X), 0)
					: new Vector2(0, Mathf.Sign(rawDirection.Y));

				bool canMove = true;

				if (!Godot.GodotObject.IsInstanceValid(currentPiece))
					return;

				foreach (Node child in currentPiece.GetChildren())
				{
					if (child is ColorRect colorRect)
					{
						Vector2 nextPos = colorRect.GlobalPosition + direction;
						int x = Mathf.RoundToInt(nextPos.X);
						int y = Mathf.RoundToInt(nextPos.Y);

						if (y >= 0 && x >= 0 && x < board.GetLength(0) && y < board.GetLength(1) - TopY)
						{
							if (x < 0 || x >= BackGround.Size.X || board[x, y].IsOccupied)
							{
								canMove = false;
								break;
							}
						}
						else if (y >= board.GetLength(1) - TopY || x < 0 || x >= BackGround.Size.X)
						{
							canMove = false;
							break;
						}


					}
				}

				if (canMove)
				{
					currentPiece.Position += direction;
				}

				verifyPiece();
			}

		}

		public void SpawnPiece()
		{
			var random = new Random();
			PieceData randomPiece;

			// Elegir una pieza distinta a la anterior
			do
			{
				randomPiece = Piece[random.Next(Piece.Count)];
			} while (Piece.Count > 1 && randomPiece == lastPieceData); // Evita bucle infinito si solo hay 1 pieza

			lastPieceData = randomPiece;

			currentPiece = new Node2D();
			AddChild(currentPiece);

			foreach (var pos in randomPiece.Pos)
			{
				ColorRect block = blockPool != null ? blockPool.Get() : null;

				block.Color = randomPiece.col;
				block.Size = new Vector2(1, 1);
				block.Position = pos;
				currentPiece.AddChild(block);
			}

			Vector2 SizePiece = NodeUtils.GetSize<ColorRect>(currentPiece);
			int maxX = (int)(BackGround.Size.X - SizePiece.X);
			int randomX = (int)GD.RandRange(0, Math.Max(0, maxX));

			currentPiece.Position = new Vector2(randomX, -SizePiece.Y);
			currentPivot = randomPiece.Pivot;

			// Corrección extra si algún bloque se sale
			float rightLimit = BackGround.Position.X + BackGround.Size.X;
			float leftLimit = BackGround.Position.X;

			float correctionX = 0f;
			foreach (ColorRect block in currentPiece.GetChildren().OfType<ColorRect>())
			{
				float blockRight = block.GlobalPosition.X + block.Size.X;
				float blockLeft = block.GlobalPosition.X;

				if (blockRight > rightLimit)
					correctionX = Math.Max(correctionX, blockRight - rightLimit);

				if (blockLeft < leftLimit)
					correctionX = -Math.Max(correctionX, leftLimit - blockLeft);
			}

			currentPiece.Position -= new Vector2(correctionX, 0);

			verifyPiece();
		}
		void verifyPiece()
		{


			foreach (ColorRect child in currentPiece.GetChildren())
			{
				if (child is ColorRect block)
				{
					Vector2 globalPos = currentPiece.Position + block.Position;

					if (board[(int)globalPos.X, 0].IsOccupied)
					{
						GameOver();
					}
				}
			}
		}

		private void GameOver()
		{
			isGameOver = true;
			if (firstGame)
			{
				firstGame = false; // Marca como ya no primera partida
				_ = ClearBoardWithFade(); // Espera a que se limpien los bloques con fade
				SaveIsFirstGame(firstGame);
			}
			GD.Print("Game Over");
			GetTree().Paused = true;
			// Puedes también mostrar una UI de derrota aquí
		}

		private void SaveIsFirstGame(bool isFirstGame)
		{
			OptionsSavesHandler.Current.SetValue(SaveString, isFirstGame);
		}

		private void LoadFirstGameValue()
		{
			firstGame = OptionsSavesHandler.Current.GetValue(SaveString)?.As<bool>() ?? firstGame;

		}

	}
}
