using Godot;
using InputSystem;
using System;

public partial class WalkingController : CharacterBody3D
{
	[ExportGroup("Head")]
	[Export]
	public Node3D head;

	[Export(PropertyHint.Range, "0,180")]
	public float minY = 90, maxY = 90;

	[ExportGroup("Movement")]
	[Export]
	public float walkSpeed = 5.0f;

	[Export]
	public float runSpeed = 10.0f;

	[Export]
	public float Acceleration = 10f;

	[Export]
	public float JumpVelocity = 4.5f;

	public float RotationSensitivity = 1;

	[Export(PropertyHint.Range, "0,1")]
	public float AirFrictioin = 0.1f;

	[Export]
	private StringName sensivilityKey;

	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	Vector2 MovementDir;

	bool Running;

	float Speed => Running ? runSpeed : walkSpeed;

	Vector2 LookDelta;

	bool Jumping = false;

	Rect2 rect;

	PhysicsBody3D lastFloor;
	PhysicsMaterial floorMaterial = null;

	public override void _EnterTree()
	{
		base._EnterTree();

		RotationSensitivity = OptionsSavesHandler.Current.GetValue(sensivilityKey)?.As<float>() ?? 1;
		OptionsSavesHandler.Current.onOptionsChanged += SetSensivility;
		rect = GetViewport().GetVisibleRect();
	}

	public void SetSensivility(StringName key, Variant value)
	{
		if (key == sensivilityKey)
			RotationSensitivity = value.As<float>();
	}
	public void Move(InputActionState state)
	{
		MovementDir = ((Vector2)state.strength).Normalized();
	}
	public void Run(InputActionState state)
	{
		if (state.state == InputActionState.PressState.JustPressed)
		{
			Running = true;
		}
		else if (state.state == InputActionState.PressState.Released)
		{
			Running = false;
		}
	}

	public void Look(InputActionState state)
	{
		if (state.inputEvent is InputEventMouseMotion)
		{
			LookDelta = (Vector2)state.strength * RotationSensitivity * 360;
			LookDelta = new(LookDelta.X, -LookDelta.Y);

			UpdateRotation(GetProcessDeltaTime());

			LookDelta = Vector2.Zero;
		}
		else
		{
			LookDelta = (Vector2)state.strength * RotationSensitivity * 360;
		}
	}

	public void Jump(InputActionState state)
	{
		if (state.state == InputActionState.PressState.JustPressed)
		{
			Jumping = true;
		}
		else if (state.state == InputActionState.PressState.Released)
		{
			Jumping = false;
		}
	}

	public override void _Process(double delta)
	{
		if(LookDelta != Vector2.Zero) UpdateRotation(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		Updatemovement(delta);
	}

	void UpdateRotation(double delta)
	{
		head.RotationDegrees = new (Math.Clamp(head.RotationDegrees.X - LookDelta.Y * (float)delta, -maxY, minY),0,0);
		RotationDegrees = new (0,RotationDegrees.Y - LookDelta.X * (float)delta,0);
	}

	PhysicsBody3D GetLastMoveAndSlideFloor() 
	{
		KinematicCollision3D collision = GetLastSlideCollision();
		PhysicsBody3D body = collision?.GetCollider() as PhysicsBody3D;

		//collision.Dispose();

		return body;
	}

	void Updatemovement(double delta)
	{
		Vector3 velocity = Velocity;

		bool onFloor = IsOnFloor();

		Vector3 dir3d = Basis.Z * MovementDir.Y + Basis.X * -MovementDir.X;

		if (!onFloor)
		{
			floorMaterial = null;
			lastFloor = null;

			Vector3 ungravidVel = new Vector3(velocity.X, 0, velocity.Z).CalulateVelocity(dir3d, Acceleration, Speed, AirFrictioin);

			Velocity = new(ungravidVel.X, velocity.Y - gravity * (float)delta, ungravidVel.Z);
		}
		else
		{
			PhysicsBody3D currentFloor = GetLastMoveAndSlideFloor();

			Vector3 florNormal = GetFloorNormal();

			if (currentFloor != null && currentFloor != lastFloor)
			{
				floorMaterial = (PhysicsMaterial)currentFloor?.GetType().GetProperty("PhysicsMaterialOverride")?.GetValue(currentFloor);

				lastFloor = currentFloor;
			}

			float friction = MathF.Max(floorMaterial != null ? floorMaterial.Friction : AirFrictioin, AirFrictioin);

			float maxVel = friction > 1 ? Speed * friction : Speed;

			velocity = velocity.CalulateVelocity(dir3d.ProjectToNormal(florNormal), Acceleration, maxVel, friction);

			if (Jumping)
			{
				velocity.Y = JumpVelocity;
			}

			Velocity = velocity;
		}

		MoveAndSlide();
	}
}
