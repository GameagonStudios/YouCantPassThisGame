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
	public ShapeCast3D floorDetector;

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

	Vector2 MovementDir;

	bool Running;

	float Speed => Running ? runSpeed : walkSpeed;

	Vector2 LookDelta;
	Vector2 lastLookDelta;

	bool isPersistentLookDelta = false;

	bool Jumping = false;

	Rect2 rect;

	GodotObject lastFloor;

	float CurrentFriction = 0;

	PhysicsMaterial floorMaterial = null;

	Vector3 FlorNormal = Vector3.Up;

	bool OnFloor = false;

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
			LookDelta = (Vector2)state.strength;

			LookDelta = new(LookDelta.X, -LookDelta.Y);

			isPersistentLookDelta = false;
		}
		else
		{
			LookDelta = (Vector2)state.strength;

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

	public override void _Process(double delta)
	{
		if (isPersistentLookDelta)
		{
			if (LookDelta != Vector2.Zero) UpdateRotation(LookDelta * RotationSensitivity * 360, delta);

			lastLookDelta = LookDelta;
		}
		else
		{
			LookDelta = lastLookDelta.Lerp(LookDelta, (float)delta*20);

			if (LookDelta != Vector2.Zero) UpdateRotation(LookDelta * RotationSensitivity * (1 / (float)delta / 2), delta);

			lastLookDelta = LookDelta;

			LookDelta = Vector2.Zero;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		DetectFloor();
		Updatemovement(delta);
	}

	void UpdateRotation(Vector2 degrees, double delta)
	{
		head.RotationDegrees = new(Math.Clamp(head.RotationDegrees.X - degrees.Y * (float)delta, -maxY, minY), 0, 0);

		RotateY(Mathf.DegToRad(-degrees.X * (float)delta));
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

				CurrentFriction = MathF.Max(floorMaterial != null ? floorMaterial.Friction : AirFrictioin, AirFrictioin);
			}
		}
	}

	void Updatemovement(double delta)
	{
		Vector3 dir3d = Basis.Z * MovementDir.Y + Basis.X * -MovementDir.X;
		bool snap = true;

		Vector3 gravity = GetGravity();
		Vector3 gravityDir = gravity.Abs().Normalized();

		float maxVel = CurrentFriction > 1 ? Speed * CurrentFriction : Speed;

		if (OnFloor)
		{
			Vector3 newVel = Velocity.CalulateVelocity(dir3d.RotateFromToNormal(FlorNormal, Basis.Y), Acceleration, (float)delta, maxVel, CurrentFriction);

			if (Jumping)
			{
				newVel += JumpVelocity * FlorNormal;
				Jumping = false;
				snap = false;
			}

			Velocity = newVel;
		}
		else
		{
			Vector3 normalVel = (Velocity * (Vector3.One - gravityDir)).CalulateVelocity(dir3d, Acceleration, (float)delta, maxVel, CurrentFriction);

			Vector3 gravPerpVel = Velocity * gravityDir + (gravity * (float)delta);

			Velocity = normalVel + gravPerpVel;

			snap = gravPerpVel.Normalized().DistanceSquaredTo(gravity.Normalized()) < 1;
		}

		MoveAndSlide();

		if(snap) ApplyFloorSnap();

		if (-gravity != Vector3.Zero && Basis.Y != -gravity.Normalized())
		{
			ChangeUp(-gravity, (float)delta);
		}
	}

	public void ChangeUp(Vector3 newUp, float weight)
	{
		Vector3 newUpNormalized = newUp.Normalized();

		Basis basis = new Basis(Basis.Column0, Basis.Column1, Basis.Column2);

		basis.Y = basis.Y.Lerp(newUpNormalized, weight);

		Basis = basis.Orthonormalized();

		UpDirection = basis.Y;
	}
}
