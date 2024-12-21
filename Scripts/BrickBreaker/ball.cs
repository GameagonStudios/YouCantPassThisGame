using Godot;
using System;
using System.Drawing;
using System.Threading;
using InputSystem;
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

	[Export]
	ColorRect Paddle;
	public float CurrentVelocity;

	bool isLaunched = false;


	float floor => Background.Position.Y + Background.Size.Y + rect.Size.Y;
	float ceiling => Background.Position.Y;
	float MaxWall => Background.Position.X + Background.Size.X - rect.Size.X;
	float MinWall => Background.Position.X;
	Vector2 InitialPosition => new Vector2(Paddle.GlobalPosition.X + Paddle.Size.X / 2 - rect.Size.X / 2, Paddle.GlobalPosition.Y - rect.Size.Y);
	public override void _Ready()
	{
		
		Position = InitialPosition;  // Asegúrate de que la pelota esté en la pala al inicio
    	LinearVelocity = Vector2.Zero;
	}

	public void LaunchBall(InputActionState state = null)
	{
		if(state?.state == InputActionState.PressState.JustPressed && isLaunched == false)
		{
			Position = InitialPosition;
			GD.Print("Si");
			isLaunched = true;
			CurrentVelocity = minVelocity;
			//Position = Start_Position;
			RandomNumberGenerator rand = new RandomNumberGenerator();
			LinearVelocity = new Vector2(rand.RandfRange(-1, 1), -1).Normalized() * CurrentVelocity;

		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (isLaunched == true)
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
			if (Position.Y >= floor)
			{
				GD.Print(Position.Y);
				GD.Print(floor);
				GD.Print("No");
				isLaunched = false;
			}
		}
		else
		{
			//LinearVelocity = Vector2.Zero;
			Position = InitialPosition;
			LinearVelocity = Vector2.Zero;
			GD.Print("velocidad" + LinearVelocity);
		}


	}
	//public override void 

}
