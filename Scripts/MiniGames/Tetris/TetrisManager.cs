using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using InputSystem;

namespace Tetris
{
    public partial class TetrisManager : ColorRect
    {

        [Export]
        public Godot.Collections.Array<PieceData> Piece = new();

        [Export]
        public ColorRect BackGround;
        
        [Export]
        private float fallSpeed = 1.0f;  // Velocidad de caída de las piezas

        [Export]
        private float minFallSpeed = 0.1f;        // Límite mínimo

        [Export]
        private float speedDecreaseStep = 0.05f;  // Cuánto se reduce cada vez

        [Export]
        private float speedIncreaseInterval = 10f; // Cada cuántos segundos se acelera
        private float totalGameTime = 0.0f;
        private float timeSinceLastFall = 0.0f;  // Tiempo acumulado desde la última caída
                                                 // Llamado cuando el nodo entra en el árbol de nodos por primera vez.
        private Node2D currentPiece;  // Nodo que contiene las partes de la pieza actual
        private Vector2 currentPivot;
        Vector2 direction;
        private Vector2? pivotOverride = null; // <- Nuevo

        private ColorRect[,] board;




        public override void _Ready()
        {
            int width = (int)BackGround.Size.X;
            int height = (int)BackGround.Size.Y;
            board = new ColorRect[width, height];

            // Inicializa todos los valores a 0 explícitamente (opcional, pero claro)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    board[x, y] = null;
                }
            }
            // Generar la primera pieza aleatoria
            SpawnPiece();
        }

        // Llamado cada frame. 'delta' es el tiempo transcurrido desde el frame anterior.
        public override void _Process(double delta)
        {
            timeSinceLastFall += (float)delta;
            totalGameTime += (float)delta;

            // Aumentar la velocidad con el tiempo, con un límite
            if (totalGameTime >= speedIncreaseInterval)
            {
                if (fallSpeed > minFallSpeed)
                    fallSpeed = Math.Max(fallSpeed - speedDecreaseStep, minFallSpeed);

                totalGameTime = 0f;
            }

            if (timeSinceLastFall >= fallSpeed)
            {
                timeSinceLastFall = 0.0f;

                // Intentar mover hacia abajo
                bool canMoveDown = true;
                Vector2 direction = new Vector2(0, 1);

                foreach (Node child in currentPiece.GetChildren())
                {
                    if (child is ColorRect colorRect)
                    {
                        Vector2 nextGlobalPos = colorRect.GlobalPosition + direction;
                        int x = Mathf.RoundToInt(nextGlobalPos.X);
                        int y = Mathf.RoundToInt(nextGlobalPos.Y);

                        // Verificar si está fuera del fondo o colisiona con una celda ocupada
                        if (y >= (int)BackGround.Size.Y || x < 0 || x >= (int)BackGround.Size.X || board[x, y] != null)
                        {
                            canMoveDown = false;
                            break;
                        }
                    }
                }

                if (canMoveDown)
                {
                    currentPiece.Position += direction;
                }
                else
                {
                    // Marcar bloques como ocupados en el board
                    foreach (Node child in currentPiece.GetChildren())
                    {
                        if (child is ColorRect colorRect)
                        {
                            int x = (int)colorRect.GlobalPosition.X;
                            int y = (int)colorRect.GlobalPosition.Y;

                            // Solo asignamos si está dentro del área del fondo para evitar errores
                            if (x >= 0 && x < board.GetLength(0) && y >= 0 && y < board.GetLength(1))
                            {
                                board[x, y] = colorRect;
                            }
                        }
                    }

                    // Generar nueva pieza
                    SpawnPiece();
                }
            }

            // Aquí puedes añadir controles de izquierda/derecha si quieres
        }

        public void SetPivot(Vector2 pivot)
        {
            pivotOverride = pivot;
        }

        public void Rotate(InputSystem.InputActionState state)
        {
            if (state.state != InputSystem.InputActionState.PressState.JustPressed || currentPiece == null)
                return;

            // 1) Recoger bloques y sus posiciones locales originales
            List<ColorRect> blocks = currentPiece.GetChildren().OfType<ColorRect>().ToList();
            List<Vector2> originalLocal = blocks.Select(b => b.Position).ToList();

            // Redondeamos el centro al punto más cercano sobre la cuadrícula
            Vector2 pivot = currentPivot;
            GD.Print(pivot);

            // 3) Simular rotación local 90° horario
            List<Vector2> rotatedLocal = new List<Vector2>(blocks.Count);
            foreach (Vector2 pos in originalLocal)
            {
                Vector2 d = pos - pivot;
                Vector2 r = new Vector2(-d.Y, d.X) + pivot;
                rotatedLocal.Add(new Vector2(r.X, r.Y));
            }

            // 4) Corregir si se sale del tablero por izquierda o derecha
            float rotatedMinX = rotatedLocal.Min(p => p.X);
            float rotatedMaxX = rotatedLocal.Max(p => p.X);
            float correctionX = 0;
            if (currentPiece.Position.X + rotatedMinX < 0)
                correctionX = -(currentPiece.Position.X + rotatedMinX);
            else if (currentPiece.Position.X + rotatedMaxX >= board.GetLength(0))
                correctionX = (board.GetLength(0) - 1) - (currentPiece.Position.X + rotatedMaxX);

            for (int i = 0; i < rotatedLocal.Count; i++)
                rotatedLocal[i] += new Vector2(correctionX, 0);

            // 5) Verificar colisiones
            bool valid = true;
            HashSet<Vector2> uniquePositions = new HashSet<Vector2>();
            for (int i = 0; i < blocks.Count; i++)
            {
                Vector2 global = currentPiece.Position + rotatedLocal[i];
                int x = Mathf.RoundToInt(global.X);
                int y = Mathf.RoundToInt(global.Y);

                if (!uniquePositions.Add(global) ||
                    x < 0 || x >= board.GetLength(0) ||
                    y < 0 || y >= board.GetLength(1) ||
                    board[x, y] != null)
                {
                    valid = false;
                    break;
                }
            }

            // 6) Aplicar si es válido
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
            // Solo permitir movimientos en un eje a la vez (no diagonal)
            Vector2 rawDirection = (Vector2)state.strength;

            if (Mathf.Abs(rawDirection.X) > Mathf.Abs(rawDirection.Y))
                direction = new Vector2(Mathf.Sign(rawDirection.X), 0); // Movimiento horizontal
            else
                direction = new Vector2(0, Mathf.Sign(rawDirection.Y)); // Movimiento vertical

            bool canMove = true;

            foreach (Node child in currentPiece.GetChildren())
            {
                if (child is ColorRect colorRect)
                {
                    Vector2 nextGlobalPos = colorRect.GlobalPosition + direction;
                    int x = Mathf.RoundToInt(nextGlobalPos.X);
                    int y = Mathf.RoundToInt(nextGlobalPos.Y);

                    // Comprobación de límites
                    if (x < 0 || x >= BackGround.Size.X || y < 0 || y >= BackGround.Size.Y)
                    {
                        canMove = false;
                        break;
                    }

                    // Verificar colisión con otras piezas
                    if (board[x, y] != null)
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
        }

        // Método para generar una pieza aleatoria y colocarla en la parte superior del contenedor
        public void SpawnPiece()
        {
            // Escoger una pieza aleatoria desde el array de PieceData
            var random = new Random();

            PieceData randomPieceData = Piece[random.Next(Piece.Count)];


            // Crear un nodo contenedor para las partes de la pieza
            currentPiece = new Node2D();
            AddChild(currentPiece);
             
            int randomX = (int)GD.RandRange(0, BackGround.Size.X -1); // Asegura que la pieza no se salga del fondo
            //GD.Print(randomX);
           currentPiece.Position = new Vector2(randomX, 0);

            // Crear las partes de la pieza usando los datos de PieceData
            for (int i = 0; i < randomPieceData.Pos.Count; i++)
            {
               // GD.Print(randomPieceData.Pos.Count);
                
                ColorRect colorRect = new ColorRect
                {
                    Color = randomPieceData.col,
                    Size = new Vector2(1, 1),  // Tamaño de cada bloque (puedes ajustar)
                    Position = randomPieceData.Pos[i],  // Ajustar la posición de las partes según el Vector2 de PieceData
                };
                currentPiece.AddChild(colorRect);
            }
            currentPivot = randomPieceData.Pivot;

            HashSet<int> posicionesXSobresalientes = new();

            foreach (Node child in currentPiece.GetChildren())
            {
                if (child is ColorRect colorRect)
                {

                    if (colorRect.GlobalPosition.X >= BackGround.Position.X + BackGround.Size.X)
                    {
                        // Convertimos a int la posición X sobresaliente
                        int x = (int)colorRect.GlobalPosition.X;

                        posicionesXSobresalientes.Add(x); // HashSet evita duplicados automáticamente
                        

                    }
                }
            }


            currentPiece.Position -= new Vector2((float)posicionesXSobresalientes.Count, 0);
            

        }

    }
}

