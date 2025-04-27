using Godot;
using Godot.Collections;
using InputSystem;
using System;

public partial class WalkingController : CharacterBody3D
{
	[ExportGroup("Head")]
	[Export]
	public Node3D head;

	[Export(PropertyHint.Range, "0,180")]
	public float MinY
	{
		get => Mathf.RadToDeg(minYrad);
		set => minYrad = Mathf.DegToRad(value);
	}

	[Export(PropertyHint.Range, "0,180")]
	public float MaxY
	{
		get => Mathf.RadToDeg(maxYrad);
		set => maxYrad = Mathf.DegToRad(value);
	}

	public float minYrad = Mathf.Pi, maxYrad = Mathf.Pi;

	[Export]
	ulong RotationSmoothness = 10;

	Vector2 LookDelta;
	Vector2 rotationVelocity;

	[ExportGroup("Movement")]
	[Export]
	public ShapeCast3D floorDetector;

	[Export]
	public float walkSpeed = 5.0f;

	[Export]
	public float runSpeed = 10.0f;

	[Export]
	public float Acceleration = 10f;

	[Export]
	public float JumpVelocity = 4.5f;

	[Export]
	public bool resetJump = true;

	public float RotationSensitivity = 1;

	[Export(PropertyHint.Range, "0,1")]
	public float AirFrictioin = 0.1f;

	[Export]
	ulong terminalVelocity = 50;

	[Export]
	private StringName sensivilityKey;

	public Vector2 MovementDir { get; private set; }

	bool Running;

	float Speed => Running ? runSpeed : walkSpeed;

	bool isPersistentLookDelta = false;

	bool Jumping = false;

	Rect2 rect;

	GodotObject lastFloor;

	float CurrentFriction = 0;

	PhysicsMaterial floorMaterial = null;

	Vector3 FlorNormal = Vector3.Up;

	bool OnFloor = false;

	Vector3 lastVel = Vector3.Zero;

	public override void _EnterTree()
	{
		base._EnterTree();

		RotationSensitivity = OptionsSavesHandler.Current.GetValue(sensivilityKey)?.As<float>() ?? 1;

		GD.Print("Sensitivity " + RotationSensitivity);

		OptionsSavesHandler.Current.onOptionsChanged += (key, value) =>
		{
			if (key == sensivilityKey) SetSensivility((float)value);
		};

		rect = GetViewport().GetVisibleRect();
	}

	public void SetSensivility(float value)
	{
		RotationSensitivity = value;
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
			var unhandledDelta = (Vector2)state.strength;

			LookDelta += new Vector2(unhandledDelta.X, -unhandledDelta.Y);

			isPersistentLookDelta = false;
		}
		else
		{
			LookDelta = (Vector2)state.strength * 10;

			isPersistentLookDelta = true;
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

	Vector2 lastMousePos = Vector2.Zero;

	public override void _Process(double delta)
	{
		if (LookDelta != Vector2.Zero)
		{
			UpdateRotation(LookDelta * RotationSensitivity * MathF.PI, 0.001f);
			if (!isPersistentLookDelta) LookDelta = Vector2.Zero;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		DetectFloor();
		Updatemovement((float)delta);
	}

	void UpdateRotation(Vector2 radians, float delta)
	{
		head.Rotation = new(Math.Clamp(head.Rotation.X - radians.Y * delta, -maxYrad, minYrad), 0, 0);
		GlobalBasis = GlobalBasis.Rotated(GlobalBasis.Y, -radians.X * delta);
	}

	void DetectFloor()
	{
		OnFloor = IsOnFloor();

		if (!OnFloor)
		{
			floorMaterial = null;
			lastFloor = null;
			CurrentFriction = AirFrictioin;
			FlorNormal = -GetGravity().Normalized();
		}
		else
		{
			FlorNormal = GetFloorNormal();

			GodotObject currentFloor = floorDetector.GetCollider(0);

			if (currentFloor != null && currentFloor != lastFloor)
			{
				floorMaterial = (PhysicsMaterial)currentFloor?.GetType().GetProperty("PhysicsMaterialOverride")?.GetValue(currentFloor);

				lastFloor = currentFloor; 

				CurrentFriction = MathF.Max(floorMaterial?.Friction ?? AirFrictioin, AirFrictioin);
			}
		}
	}

	void Updatemovement(float delta)
	{
		Vector3 dir3d = Basis.Z * MovementDir.Y + Basis.X * -MovementDir.X;
		bool snap = true;

		Vector3 gravity = GetGravity();
		Vector3 gravityDir = gravity.Abs().Normalized();

		float maxVel = CurrentFriction > 1 ? Speed * CurrentFriction : Speed;

		Vector3? avoidStop = null;

		if (OnFloor)
		{
			dir3d = dir3d.RotateFromToNormal(Basis.Y, FlorNormal);


			Velocity = lastVel.Slide(FlorNormal).Lerp(lastVel.Bounce(FlorNormal), floorMaterial.Bounce);

			snap = floorMaterial.Bounce <= 0;

			if (Jumping)
			{
				Velocity = Velocity + JumpVelocity * FlorNormal;
				Jumping = !resetJump && Jumping;
				snap = false;
				avoidStop = Vector3.One;
			}
		}
		else
		{
			snap = (Velocity * gravityDir).Normalized().Dot(gravity.Normalized()) < 0;

			avoidStop = gravityDir;
		}
		
		Velocity += gravity * delta;

		Velocity = Velocity.CalulateVelocity(dir3d, Acceleration, delta, maxVel, CurrentFriction, avoidStop).LimitLength(terminalVelocity);

		lastVel = Velocity;

		MoveAndSlide();

		if (snap) ApplyFloorSnap();

		if (-gravity != Vector3.Zero && Basis.Y != -gravity.Normalized())
		{
			ChangeUp(-gravity, (float)delta);
		}
	}

	public void ChangeUp(Vector3 newUp, float weight)
	{
		GlobalBasis = GlobalBasis.LookingAt(GlobalBasis.Y, newUp, weight);

		UpDirection = newUp;
	}
}
