using Godot;
using Godot.Collections;
using System;

[GlobalClass, GodotClassName("CopyBoneRotation"), Tool]
public partial class CopyBoneRotation : BoneAttachment3D
{
	[Export]
	public Array<BoneWeight> BoneWeights;

	public override void _Process(double delta)
	{
		base._Process(delta);

		var sk = GetSkeleton();
		
		foreach (var item in BoneWeights)
		{
			sk?.SetBonePoseRotation(item.Bone, sk.GetBoneGlobalRest(item.Bone).Basis.GetRotationQuaternion().Slerp(Basis.GetRotationQuaternion(), item.weight));
		}
	}
	
	public override bool _Set(StringName property, Variant value)
	{
		GD.Print(property);
		if (property == "external_skeleton" || property == "BoneWeights")
		{
			foreach (var item in BoneWeights)
			{
				item.Skeleton = GetSkeleton();
			}
		}

		return base._Set(property, value);
	}
}