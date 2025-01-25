using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BrickBreakerGenerator : Container
{
	[Export]
	PackedScene BrickPrefab;
	[Export]
	ColorRect ball;
	
	[Export]
	Node2D TextWin;

	[Export]
	int initialLines = 10;
	[Export]
	int brickWidth = 2;
	[Export]
	Array<Color> colorPalette = new Array<Color>() {new Color(1,1,1)};

    List<BrickBreakerBrick> Bricks = new List<BrickBreakerBrick>();

	
	float width => this.Size.X;
	float height => this.Size.Y;
	bool lineFits => width % brickWidth == 0;
	int bricksPerLine => (int)MathF.Floor(width / brickWidth);

	[Signal]
	public delegate void WinGameEventHandler();
	
	bool winGame;
	private void RestartBrick()
	{
		int BricksThisLine = 0;
		int oddNumber = 0; 

		for(int y = 0; y < MathF.Min(initialLines, height); y++)
		{
			oddNumber = y % 2;
			BricksThisLine = bricksPerLine - (lineFits ? oddNumber : 0);

			for(int x = 0; x < BricksThisLine; x++)
			{
				BrickBreakerBrick newBrick = GetBrick();
				newBrick.Position = new Vector2((x * brickWidth) + (oddNumber == 0 ? 0 : MathF.Floor(brickWidth / 2)), y);
			}
		}

	}

	public void Win()
	{
		if(Bricks.All(b => !b.Visible))
		{
			ball.QueueFree();
			TextWin.Visible = true;
			
		}
	}

	public void RestartLine()
	{
		// Paso 1: Encuentra el primer bloque invisible
		int firstInvisible = Bricks.FindIndex(b => !b.Visible);


		// Paso 2: Variables de cálculo 
		int accumulatedBlocks = 0;
		int lineIndex = 0;
		int bricksInThisLine = 0;

		// Paso 3: Encontrar la fila que contiene el primer bloque invisible
		while (true)
		{
			// Calculamos cuántos bloques hay en la fila actual (considerando filas impares)
			bricksInThisLine = bricksPerLine - ((lineIndex % 2 == 1) ? 1 : 0);

			// Si el primer bloque invisible cae dentro de esta fila, la encontramos
			if (accumulatedBlocks + bricksInThisLine > firstInvisible)
			{
				break;
			}

			// Continuamos acumulando bloques hasta llegar a la fila donde está el primer bloque invisible
			accumulatedBlocks += bricksInThisLine;
			lineIndex++;
		}


		// Paso 4: Calcular el rango de índices de la fila
		int startIndex = accumulatedBlocks;
		int endIndex = startIndex + bricksInThisLine;

		// Asegurarse de no superar el número de bloques disponibles
		endIndex = Mathf.Min(endIndex, Bricks.Count);

		// Paso 5: Restaurar los bloques visibles en esa fila
		for (int i = startIndex; i < endIndex; i++)
		{
			// Si encontramos un bloque invisible, lo hacemos visible
			if (!Bricks[i].Visible)
			{
				Bricks[i].Visible = true;
				Bricks[i].ProcessMode = ProcessModeEnum.Always; // Asegura que los bloques sean procesados constantemente
			}
		}


	}

	public BrickBreakerBrick GetBrick()
	{
		BrickBreakerBrick newBrick = Bricks.FirstOrDefault(b => !b.Visible);
		
		if(newBrick == null)
		{
			newBrick = BrickPrefab.Instantiate<BrickBreakerBrick>();
			this.AddChild(newBrick);
			Bricks.Add(newBrick);
		}

		newBrick.Scale = new Vector2(brickWidth, 1);
		newBrick.setHealth(1);
		newBrick.setColor(colorPalette.PickRandom());
		newBrick.Visible = true;

		return newBrick;
	}

	public void OnBrickVisibilityChanged()
	{
		EmitSignal(nameof(WinGame)); // Emitimos la señal de victoria
		
	}

	public override void _Ready() 
	{
		RestartBrick();
	}
}
