using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class TetrisManager : ColorRect
{
	[Export] 
	public PackedScene[] pieces;

 	private List<PackedScene> Blocks = new List<PackedScene>();

    private float fallSpeed = 1.0f;  // Velocidad de caída de las piezas
    private float timeSinceLastFall = 0.0f;  // Tiempo acumulado desde la última caída
    // Llamado cuando el nodo entra en el árbol de nodos por primera vez.

    [Export]
	Array<Color> colorPalette = new Array<Color>() {new Color(1,1,1)};
    public override void _Ready()
    {
        // Generar la primera pieza aleatoria
        SpawnPiece();
    }

    // Llamado cada frame. 'delta' es el tiempo transcurrido desde el frame anterior.
    public override void _Process(double delta)
    {
        /*timeSinceLastFall += (float)delta;

        // Si ha pasado el tiempo suficiente, hacemos caer la pieza
        if (timeSinceLastFall >= fallSpeed)
        {
            timeSinceLastFall = 0.0f;

            // Mover todas las piezas hacia abajo
            foreach (var block in Blocks)
            {
                if (block is PackedScene piece)
                {
                    piece += new Vector2(0, 1);  // Mueve la pieza hacia abajo
                }
            }

            // Aquí deberías agregar más lógica para manejar la colisión con otras piezas o el fondo
        }*/
    }

    // Método para generar una pieza aleatoria y colocarla en la parte superior del contenedor
    private void SpawnPiece()
    {
        // Seleccionar una pieza aleatoria del array 'pieces'
        int randomIndex = (int)GD.RandRange(0, pieces.Length - 1);
        
        PackedScene randomPieceScene = pieces[randomIndex];

        // Instanciar la pieza
        Pieces pieceInstance = (Pieces)randomPieceScene.Instantiate();

        // Obtener el tamaño de la pieza (solo el valor en X para verificar que encaje en la pantalla)
        float pieceWidth = NodeUtils.GetSize<ColorRect>(pieceInstance).X;

        // Generar una posición aleatoria en el eje X (en el rango de la anchura de la pantalla)
        int randomX = (int)GD.RandRange(0, this.Size.X);

        // Ajustar la posición X solo si la pieza sobresale del límite derecho
        if (randomX + pieceWidth > this.Size.X)
        {
            randomX = (int)this.Size.X - (int)pieceWidth;  // Ajustamos para que la pieza no se salga por la derecha
        }

        // Coloca la pieza en la parte superior de la pantalla, pero con la posición X ajustada
        pieceInstance.Position = new Vector2(randomX, 0);  // Coloca la pieza en la parte superior

        // Asignar un color aleatorio
        pieceInstance.setColor(colorPalette.PickRandom());

        // Agregar la pieza al contenedor para que se vea en pantalla
        AddChild(pieceInstance);

        // Agregar la pieza a la lista de bloques
        Blocks.Add(randomPieceScene);
    }
	
}
