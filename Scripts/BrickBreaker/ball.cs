using Godot;
using System;
using System.Drawing;

public partial class ball : RigidBody2D
{
	[Export]
	float multiplyVel;
	
	[Export]
	float limitVel;
	
	[Export]
	float Vel = 1f;
	Vector2 Direction = Vector2.Zero;
	Vector2 Start_Position;
	float floor;

	[Export]
	ColorRect Background;
	[Export]
	ColorRect Ball;


	public override void _Ready()
	{
		Start_Position = Position;
		start_ball();
		floor = Background.Position.Y + Background.Size.Y + Ball.Size.Y;

		//this.LinearVelocity = new Vector2(this.LinearVelocity.X, Vel);
	}

	public void start_ball()
	{
		Position = Start_Position;
		RandomNumberGenerator rand = new RandomNumberGenerator();
		LinearVelocity = new Vector2(rand.RandfRange(1,-1), rand.RandfRange(1,-1));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		KinematicCollision2D collission = MoveAndCollide(LinearVelocity.Normalized() * (float)(Vel * delta));
		if(collission != null)
		{
			LinearVelocity = LinearVelocity.Bounce(collission.GetNormal());
		}

		if(Position.Y > floor)
		{
			start_ball();
		}

		if(collission.GetCollider() is ColissionReciver b)
        {
            b.HandleCollision(this);
        }

	}

	//public override void 
	
}
