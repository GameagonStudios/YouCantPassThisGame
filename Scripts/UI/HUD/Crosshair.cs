using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
public partial class Crosshair : AnimatedSprite2D
{
	public StringName DefaultAnimation;
	public StringName DefaultSelectedAnimation;

	public override void _Ready()
	{
		base._Ready();

		Play(DefaultAnimation);
	}

	public void NodeSelected(Interactable3D interactable3D)
	{

		if (interactable3D is IDInteractable3D idi3d)
		{

			if (SpriteFrames.HasAnimation(idi3d.ID))
			{
				Play(idi3d.ID);
				return;
			}
		}

		Play(DefaultSelectedAnimation);
	}

	public void SetDefault()
	{
		Play(DefaultAnimation);
	}

	public override Array<Dictionary> _GetPropertyList()
	{
		var properties = new Array<Dictionary>
			{
				new Dictionary()
				{
					{ "name", "DefaultAnimation" },
					{ "type", (int)Variant.Type.StringName },
					{ "usage", (int)PropertyUsageFlags.Default }, // See above assignment.
                    { "hint", (int)PropertyHint.Enum },
					{ "hint_string", string.Join(",", SpriteFrames?.GetAnimationNames()) }
				},
				new Dictionary()
				{
					{ "name", "DefaultSelectedAnimation" },
					{ "type", (int)Variant.Type.StringName },
					{ "usage", (int)PropertyUsageFlags.Default }, // See above assignment.
                    { "hint", (int)PropertyHint.Enum },
					{ "hint_string", string.Join(",", SpriteFrames?.GetAnimationNames()) }
				}
			};

		return properties;
	}
}
