using Godot;
using System;

public partial class PlayerDummyAnimationController : AnimationTree
{
	[Export]
	public WalkingController PlayerDummy;

	public override void _Process(double delta)
	{
		base._Process(delta);

		this.Set("parameters/WalkSpeed/blend_amount", PlayerDummy.MovementDir.Normalized().Length());
	}
}
