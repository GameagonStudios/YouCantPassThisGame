using Godot;
using System;

public static class SKMath
{
	public static Vector3 CalulateVelocity(this Vector3 currentVel, Vector3 direction, float acceleration, float delta,
		float maxSpeed = float.PositiveInfinity, float friction = 1, Vector3? maxVelIgnorantAxis = null)
	{
		float trueAcceleration = friction * acceleration * delta;
		if (trueAcceleration == 0) return currentVel;

		direction = direction.Normalized();

		Vector3 desiredVel = direction * maxSpeed;

		if (maxVelIgnorantAxis != null)
		{
			Vector3 maxVelIgnorantAxisN = ((Vector3)maxVelIgnorantAxis).Clamp(Vector3.Zero, Vector3.One);
			desiredVel = desiredVel + (currentVel * maxVelIgnorantAxisN);
		}

		return currentVel.MoveToward(desiredVel, trueAcceleration);
	}

	public static float UnsignedMax(this float a, float b)
	{
		return MathF.Abs(a) > MathF.Abs(b) ? a : b;
	}

	public static float UnsignedMin(this float a, float b)
	{
		return MathF.Abs(a) < MathF.Abs(b) ? a : b;
	}

	public static Vector3 ProjectToNormal(this Vector3 a, Vector3 b)
	{
		return (a - (b * a.Dot(b))).Normalized();
	}

	public static Vector3 RotateFromToNormal(this Vector3 dir, Vector3 up, Vector3 newUp)
	{
		return up.GetQuaternionTo(newUp) * dir;
	}

	public static Quaternion GetQuaternionTo(this Vector3 v1, Vector3 v2) => new Quaternion(v1, v2).Normalized();

	/// <summary>
	/// Returns the unsigned minimum angle to the given vector, in radians.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="cross"></param>
	/// <returns>The unsigned angle between the two vectors, in radians.</returns>
	public static float AngleTo(this Vector3 from, Vector3 to, out Vector3 cross)
	{
		return from.AngleTo(to, out cross, out float dot);
	}

	/// <summary>
	/// Returns the unsigned minimum angle to the given vector, in radians.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="dot"></param>
	/// <returns>The unsigned angle between the two vectors, in radians.</returns>
	public static float AngleTo(this Vector3 from, Vector3 to, out float dot)
	{
		return from.AngleTo(to, out Vector3 cross, out dot);
	}

	/// <summary>
	/// Returns the unsigned minimum angle to the given vector, in radians.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="cross"></param>
	/// <param name="dot"></param>
	/// <returns>The unsigned angle between the two vectors, in radians.</returns>
	public static float AngleTo(this Vector3 from, Vector3 to, out Vector3 cross, out float dot)
	{
		cross = from.Cross(to);
		dot = from.Dot(to);

		return Mathf.Atan2(cross.Length(), dot);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="basis"></param>
	/// <param name="axis"></param>
	/// <param name="at"></param>
	/// <param name="maxRotation"> clamps the maximum amount of rotation in radians</param>
	/// <returns>New basis rotated around the shortest path so the given "axis" now points to "at" direction</returns>
	public static Basis LookingAt(this Basis basis, Vector3 axis, Vector3 at, float maxRotation = float.Pi*2)
	{
		float angle = axis.Normalized().AngleTo(at.Normalized(), out Vector3 cross);

		return basis.Rotated(cross.Normalized(), Mathf.Min(angle, maxRotation));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="basis"></param>
	/// <param name="axis"></param>
	/// <param name="at"></param>
	/// <param name="weight"></param>
	/// <returns>New basis rotated around the shortest path so the given "axis" now points to "at" direction</returns>
	public static Basis LookingAtLerp(this Basis basis, Vector3 axis, Vector3 at, float weight)
	{
		float angle = axis.Normalized().AngleTo(at.Normalized(), out Vector3 cross);

		return basis.Rotated(cross.Normalized(), Mathf.Lerp(0, angle, weight));
	}
}
