using Godot;
using System;
using System.Drawing;

public partial class Ball : RigidBody2D
{
	[Export]
	float minVelocity = 5;

	[Export]
	float maxVelocity = 15;

	[Export]
	ColorRect Background;
	
	[Export]
	ColorRect rect;
	public float CurrentVelocity;

	bool Start = false;
	Vector2 Start_Position;

	float floor => Background.Position.Y + Background.Size.Y + rect.Size.Y;
	float ceiling => Background.Position.Y;
	float MaxWall => Background.Position.X + Background.Size.X - rect.Size.X;
	float MinWall => Background.Position.X;
	public override void _Ready()
	{
		Start_Position = Position;
		start_ball();
		//this.LinearVelocity = new Vector2(this.LinearVelocity.X, Vel);
	}

	public void start_ball()
	{
		//CurrentVelocity = minVelocity;
		//Position = Start_Position;
		/*RandomNumberGenerator rand = new RandomNumberGenerator();
		LinearVelocity = new Vector2(rand.RandfRange(-1, 1), -1).Normalized() * CurrentVelocity;*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (Start == true)
		{
			Vector2 lv = LinearVelocity;
			KinematicCollision2D collission = MoveAndCollide(LinearVelocity * (float)delta, false, 0, true);
			if (collission != null)
			{
				GD.Print(collission.GetCollider());
				LinearVelocity = lv.Bounce(collission.GetNormal());


				if (collission.GetCollider() is ColissionReciver b)
				{
					GD.Print(collission.GetCollider() + " - 2");
					b.HandleCollision(this);
				}
			}

			LinearVelocity = new(Position.X <= MinWall || Position.X >= MaxWall ? -LinearVelocity.X : LinearVelocity.X,
				Position.Y <= ceiling ? -LinearVelocity.Y : LinearVelocity.Y);
			if (Position.Y > floor)
			{
				start_ball();
			}
		}




	}

	//public override void 

}
