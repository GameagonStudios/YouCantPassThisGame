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
				var newBrick = GetBrick();
				newBrick.Position = new Vector2((x * brickWidth) + (oddNumber == 0 ? 0 : MathF.Floor(brickWidth / 2)), y);
			}
		}

	}

	public void RestartLine()
	{
		int BricksThisLine = 0;
		int oddNumber = 0; 
		int firstInvisible = Bricks.FindIndex(b => !b.Visible);

		

		int lineIndex =(int)Math.Round((double)firstInvisible / bricksPerLine, 0, MidpointRounding.AwayFromZero);;
		oddNumber = lineIndex % 2;
		BricksThisLine = bricksPerLine - (lineFits ? oddNumber : 0);
		lineIndex =(int)Math.Round((double)firstInvisible / BricksThisLine, 0, MidpointRounding.AwayFromZero);;

		// Calcular el rango de índices de la línea
		int endIndex = Math.Min((lineIndex + 1) * BricksThisLine, Bricks.Count);
		GD.Print("linea " + lineIndex);
		GD.Print(lineFits);
		GD.Print(BricksThisLine);
		GD.Print(firstInvisible);
		GD.Print(endIndex);
		GD.Print(Bricks.Count);

		for (int i = firstInvisible; i < endIndex; i++)
		{
			Bricks[i].Visible = true;
			Bricks[i].ProcessMode = ProcessModeEnum.Always;
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

	public override void _Ready() 
	{
		RestartBrick();
	}
}
