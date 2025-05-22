using System.Linq;
using Godot;
using Godot.Collections;

namespace InputSystem
{
	[GlobalClass, GodotClassName("InputMap"), Tool]
	public partial class InputMap : Resource
	{
		[Export]
		public string Name { get{ return ResourceName; } set {ResourceName = value; } }

		[Export]
		public Array<InputActionGroup> Groups { get; private set; } = new();
	}
}