using Godot;
using System;
using System.Linq;

public partial class NodeUtils : Node
{
	public static void diable_and_hide_nide(Node node)
	{
		//node.Visible

	}

    public static Vector2 GetSize<T>(Node2D Obj) where T : CanvasItem
    {
        // Inicializamos min y max con valores extremos
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (T node in Obj.GetChildren().OfType<T>())
        {
            Vector2 pos;
            Vector2 size;

            // Intentamos obtener la posición y el tamaño de cualquier tipo de nodo derivado de Node2D
            if (node is Node2D node2D)
            {
                pos = node2D.Position;  // Si es Node2D, usamos la propiedad Position
                size = Vector2.Zero;     // Si es Node2D, no asumimos un tamaño adicional
            }
            else if (node is Control control)
            {
                pos = control.Position;  // Si es Control, usamos RectPosition
                size = control.Size;         // Y usamos el tamaño del Control (como ColorRect)
            }
            else
            {
                continue;  // Si el nodo no es ni Node2D ni Control, lo ignoramos
            }

            // Calculamos los límites del rectángulo que cubre todos los nodos
            min = new Vector2(Mathf.Min(min.X, pos.X), Mathf.Min(min.Y, pos.Y));
            max = new Vector2(Mathf.Max(max.X, pos.X + size.X), Mathf.Max(max.Y, pos.Y + size.Y));
        }

        return max - min;  // Esto te da el tamaño total que cubre todo el 
    }
}
