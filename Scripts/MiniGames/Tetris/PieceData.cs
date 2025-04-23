using Godot;

namespace Tetris
{
    [GlobalClass]
    public partial class PieceData : Resource
    {
        [Export] public Color col; 
        [Export] public Godot.Collections.Array<Vector2> Pos = new(); 
    }
}
     