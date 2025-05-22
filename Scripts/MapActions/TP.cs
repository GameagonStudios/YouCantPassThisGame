using Godot;
using System;

[Tool]
public partial class TP : Node3D
{
	[Export]
	public Vector3 TPTo = Vector3.Zero;

	[Export]
	public bool local = false;

	public void TPBody(Node3D body)
	{
		body.GlobalPosition = local ? GlobalTransform.Inverse() * TPTo : TPTo;
	}
}
