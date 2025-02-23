using Godot;
using System;

public partial class Pieces : Node2D
{
    
    [Export]
    private float dropSpeed = 1f; // Velocidad de caída en píxeles por segundo (ajustado a 100 píxeles por segundo)
    private float timeElapsed = 0f; // Tiempo transcurrido desde la última actualización

    private int gridSize = 1; // La pieza cae 1 píxel por frame

    [Export]
    private int maxY = 32; // Límite del suelo en píxeles

	ColorRect color;
    // Llamado cuando el nodo entra en el árbol de nodos por primera vez.
    public override void _Ready()
    {

    }

    // Llamado cada frame. 'delta' es el tiempo transcurrido desde el frame anterior.
    public override void _PhysicsProcess(double delta)
    {
        // Acumulamos el tiempo de cada frame
        timeElapsed += (float)delta;

        // Si ha pasado suficiente tiempo (en términos de píxeles)
        if (timeElapsed >= 1f / dropSpeed)  // La pieza se mueve de 1 píxel por vez
        {
            // Restablecer el contador de tiempo
            timeElapsed = 0f;

            // Mover la pieza un píxel hacia abajo
            Vector2 newPosition = Position + new Vector2(0, gridSize);
            GD.Print(NodeUtils.GetSize<ColorRect>(this).Y);

            // Verifica que la pieza no se haya desplazado más allá del límite inferior
            if (newPosition.Y + NodeUtils.GetSize<ColorRect>(this).Y <= maxY)
            {
                Position = newPosition; // Baja la pieza en la cuadrícula
            }
            else
            {
                //GD.Print("Pieza fijada al suelo"); // Fijar la pieza al suelo
                //QueueFree(); // Eliminar la pieza, o agregarla al tablero (dependiendo de tu lógica)
            }
        }
    }

    public void setColor(Color col)
	{
        foreach (Node child in GetChildren())
        {
            if (child is ColorRect colorRect) // Verifica si el hijo es un ColorRect
            {
                ColorRect rect = (ColorRect)child;
                rect.Color = col;  // Aplica el color
            }
        }
    }
}
