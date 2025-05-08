using Godot;

namespace Tetris
{
    [GlobalClass,  GodotClassName("PieceData") ]
    public partial class PieceData : Resource
    {
        [Export] public Color col; 
        [Export] public Godot.Collections.Array<Vector2> Pos = new(); 

        [Export]public Vector2 Pivot = new(); 
    }
}
     