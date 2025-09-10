using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public partial class Score : Node2D
{
	[Export] public PixelFont Font;                 // Recurso con FONT_NUMBERS
	[Export] public Color DigitColor = Colors.White;
	[Export] public int PixelSize = 8;              // Tamaño de cada “pixel”
	[Export] public int DigitSpacing = 2;           // Espacio (en “pixeles”) entre dígitos
	[Export] public Vector2 Origin = Vector2.Zero;  // Offset de dibujo
	[Export] public bool CenterX = true;            // Centrar horizontalmente el número

	[Export] public long StartScore = 0;
	[Export] public int ZIndexForBackground = -100; // Para que quede al fondo

	[Signal] public delegate void ScoreChangedEventHandler(long newScore, int lastGain);

	private long _score;
	private bool _lastWasTetris = false; // Back-to-back Tetris
	private Node2D _holder;              // Contenedor de ColorRects
	private readonly Dictionary<char, int[,]> _digits = new();

	public override void _Ready()
	{
		ZIndex = ZIndexForBackground;
		ZAsRelative = false;

		if (Font == null)
		{
			GD.PushError("[Score] Falta asignar 'Font'.");
			return;
		}

		foreach (var kv in Font.FONT_NUMBERS)
			_digits[kv.Key[0]] = kv.Value;

		_holder = new Node2D { Name = "DigitsHolder" };
		AddChild(_holder);

		SetScore(StartScore, true);
	}

	// ---------- API PÚBLICA ----------
	public void ResetScore()
	{
		_lastWasTetris = false;
		SetScore(0, true);
	}

	public void SetScore(long value, bool forceRedraw = false)
	{
		if (!forceRedraw && value == _score) return;
		_score = Math.Max(0, value);
		RedrawNumber();
		EmitSignal(SignalName.ScoreChanged, _score, 0);
	}

	/// <summary>
	/// Slot para conectar desde el manager. 'lines' = líneas limpiadas a la vez (1..4).
	/// </summary>
	public void OnLinesCleared(int lines)
	{
		if (lines <= 0) return;

		int baseGain = lines switch
		{
			1 => 100,
			2 => 300,
			3 => 500,
			4 => 800,
			_ => 0
		};

		int gain = baseGain;

		// Back-to-back si es Tetris y el anterior también lo fue
		if (lines == 4 && _lastWasTetris)
			gain = (int)Math.Round(baseGain * 1.5f); // 1200

		_lastWasTetris = (lines == 4);

		_score += gain;
		RedrawNumber();
		EmitSignal(SignalName.ScoreChanged, _score, gain);
	}

	// ---------- RENDER DEL NÚMERO CON COLORRECTS ----------
	private void RedrawNumber()
	{
		if (_holder == null) return;

		foreach (var c in _holder.GetChildren().ToList())
			c.QueueFree();

		string text = _score.ToString();

		int digitW = 0;
		int digitH = 0;
		if (_digits.TryGetValue('0', out var m0))
		{
			digitH = m0.GetLength(0);
			digitW = m0.GetLength(1);
		}

		int spacingPx = DigitSpacing * PixelSize;
		int totalWidthPx = 0;
		for (int i = 0; i < text.Length; i++)
		{
			totalWidthPx += digitW * PixelSize;
			if (i < text.Length - 1) totalWidthPx += spacingPx;
		}

		Vector2 start = Origin;
		if (CenterX) start.X -= totalWidthPx / 2f;

		int cursorX = 0;
		foreach (char ch in text)
		{
			if (!_digits.TryGetValue(ch, out var mat))
			{
				cursorX += digitW * PixelSize + spacingPx;
				continue;
			}

			int rows = mat.GetLength(0);
			int cols = mat.GetLength(1);

			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < cols; x++)
				{
					if (mat[y, x] == 1)
					{
						var rect = new ColorRect
						{
							Color = DigitColor,
							Size = new Vector2(PixelSize, PixelSize),
							Position = start + new Vector2(cursorX + x * PixelSize, y * PixelSize),
						};
						_holder.AddChild(rect);
					}
				}
			}

			cursorX += cols * PixelSize + spacingPx;
		}
	}
}
