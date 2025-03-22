using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Pieces : Node2D
{
    
    [Export]
    private float dropSpeed = 1f; // Velocidad de caída en píxeles por segundo (ajustado a 100 píxeles por segundo)
    private float timeElapsed = 0f; // Tiempo transcurrido desde la última actualización

    private int gridSize = 1; // La pieza cae 1 píxel por frame

    [Export]
    private int maxY = 32; // Límite del suelo en píxeles

	ColorRect color;

    private bool isDown = false;
    // Llamado cuando el nodo entra en el árbol de nodos por primera vez.

    // Llamado cada frame. 'delta' es el tiempo transcurrido desde el frame anterior.
    public override void _PhysicsProcess(double delta)
    {
        if (isDown == false)
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

                Area2D area = this.GetChild<Area2D>(0);   
                // Verifica que la pieza no se haya desplazado más allá del límite inferior
                if (newPosition.Y + NodeUtils.GetSize<ColorRect>(area).Y <= maxY && CanMoveCollide())
                {
                    Position = newPosition; // Baja la pieza en la cuadrícula
                }
                else
                {
                    TetrisManager Manager = this.GetParent<TetrisManager>();
                    Manager.SpawnPiece();
                    isDown = true;
                    //GD.Print("Pieza fijada al suelo"); // Fijar la pieza al suelo
                    //QueueFree(); // Eliminar la pieza, o agregarla al tablero (dependiendo de tu lógica)
                }
            }
        }

    }

    private bool CanMoveCollide()
    {
        // Obtener el área de colisión de la pieza
        Area2D area = GetNode<Area2D>("Area2D");
        Godot.Collections.Array<Area2D> overlappingAreas = area.GetOverlappingAreas();
        Area2D Thisarea = this.GetChild<Area2D>(0);
        // Recorremos las áreas con las que colisiona la pieza
        foreach (Area2D other in overlappingAreas)
        {
            // Obtener el nodo CollisionShape2D de la otra pieza
            CollisionShape2D otherCollision = other.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

            if (otherCollision == null) continue;

            // Recorremos los bloques de la otra pieza
            foreach (Node otherChild in other.GetChildren())
            {
                if (otherChild is ColorRect otherBlock)
                {
                    Vector2 otherBlockPos = otherBlock.GlobalPosition;


                    // Recorremos los bloques de la pieza actual
                    foreach (Node child in Thisarea.GetChildren())
                    {
                        if (child is ColorRect block)
                        {
                            Vector2 blockPos = block.GlobalPosition;


                            // Verificamos si este bloque de la pieza actual colisiona con el bloque específico de la otra pieza
                            bool sameColumn = Mathf.IsEqualApprox(blockPos.X, otherBlockPos.X);
                            bool directlyBelow = Mathf.IsEqualApprox(blockPos.Y + 1, otherBlockPos.Y);

                            // Si el bloque está colisionando justo debajo de este bloque de la otra pieza
                            if (sameColumn && directlyBelow)
                            {
                                return false; // La pieza no puede moverse hacia abajo
                            }
                        }
                    }
                }
            }
        }

        return true; // No hay colisión, la pieza puede moverse hacia abajo
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
