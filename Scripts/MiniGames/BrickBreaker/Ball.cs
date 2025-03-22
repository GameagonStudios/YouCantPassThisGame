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
	float maxVelocity = 12;

	[Export]
	ColorRect Background;
	
	[Export]
	ColorRect rect;

	[Export]
	ColorRect Paddle;

	Area2D ControllerPaddle;

	[Export]
	BrickBreakerGenerator BrickBreakerController;

	float diirectionPaddle;
	public float CurrentVelocity;

	bool isLaunched = false;


	float floor => Background.Position.Y + Background.Size.Y + rect.Size.Y;
	float ceiling => Background.Position.Y;
	float MaxWall => Background.Position.X + Background.Size.X - rect.Size.X;
	float MinWall => Background.Position.X;
	Vector2 InitialPosition => new Vector2(Paddle.GlobalPosition.X + Paddle.Size.X / 2 - rect.Size.X / 2, Paddle.GlobalPosition.Y - rect.Size.Y);
	public override void _Ready()
	{
		ControllerPaddle = Paddle.GetParent() as Area2D;
		Position = InitialPosition;  // Asegúrate de que la pelota esté en la pala al inicio
    	LinearVelocity = Vector2.Zero;
	}

	public void LaunchBall(InputActionState state = null)
	{
		if(state?.state == InputActionState.PressState.JustPressed && isLaunched == false)
		{
			Position = InitialPosition;
			isLaunched = true;
			CurrentVelocity = minVelocity;
			RandomNumberGenerator rand = new RandomNumberGenerator();
			LinearVelocity = new Vector2(diirectionPaddle, -1).Normalized() * CurrentVelocity;

		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (ControllerPaddle is PaddleController controllerP)
		{
			diirectionPaddle = controllerP.Direction;
		}
		Vector2 lv = LinearVelocity;
		KinematicCollision2D collission = MoveAndCollide(LinearVelocity * (float)delta, false, 0, true);
		if (isLaunched == true)
		{
			if (collission != null)
			{
				LinearVelocity = lv.Bounce(collission.GetNormal());


				if (collission.GetCollider() is ColissionReciver b)
				{
					b.HandleCollision(this);
				}
			}

			LinearVelocity = new(Position.X <= MinWall || Position.X >= MaxWall ? -LinearVelocity.X : LinearVelocity.X,
				Position.Y <= ceiling ? -LinearVelocity.Y : LinearVelocity.Y);
			if (Position.Y >= floor)
			{
				isLaunched = false;
				BrickBreakerController.RestartLine();
			}

		}
		else
		{
			//LinearVelocity = Vector2.Zero;
			Position = InitialPosition;
		}
 		float speedLength = LinearVelocity.Length(); // Magnitud actual de la velocidad
        float speedFactor = Mathf.Min(1, maxVelocity / speedLength); // Si la velocidad es mayor que maxVelocity, reducimos la magnitud

        // Aplicamos el factor de reducción si es necesario
        LinearVelocity = LinearVelocity * speedFactor;
		if(BrickBreakerController.WinText.Visible == true)
		{
			this.QueueFree();
		}
	}
	//public override void 

}
