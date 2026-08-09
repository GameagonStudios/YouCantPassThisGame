using Godot;
using Godot.Collections;
using System;

[GlobalClass, GodotClassName("BoneWheight"), Tool]
public partial class BoneWeight : Resource
{
	public int Bone { get => _Bone;
		set {
			ResourceName = Skeleton?.GetBoneName(value) ?? ResourceName;
			_Bone = value;
		} }
	public int _Bone = 0;

	[Export(PropertyHint.Range, "0, 1")]
	public float weight = 1;

	public Skeleton3D Skeleton = null;

	public BoneWeight() : this(0, 1, null) { }
	public BoneWeight(int _bone, float _weight, Skeleton3D _skeleton)
	{
		Bone = _bone;
		weight = _weight;
		Skeleton = _skeleton;
	}

	public override Array<Dictionary> _GetPropertyList()
	{
		var properties = new Array<Dictionary>
			{
				new Dictionary()
				{
					{ "name", "Bone" },
					{ "type", (int)Variant.Type.Int },
					{ "usage", (int)PropertyUsageFlags.Default }, // See above assignment.
                    { "hint", (int)PropertyHint.Enum },
					{ "hint_string", string.Join(",", Skeleton?.GetConcatenatedBoneNames() ??  "<null>") }
				}
			};

		return properties;
	}
}