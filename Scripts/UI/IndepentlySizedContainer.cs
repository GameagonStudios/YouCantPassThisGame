using Godot;
using System;

public partial class IndepentlySizedContainer : Control
{
	public override void _EnterTree()
	{
		base._EnterTree();

		SetAnchorsPreset(LayoutPreset.Center);
		SetOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.KeepSize);
		PivotOffset = Size / 2;

		GetViewport().SizeChanged += OnSizeChanged;
	}

	public void OnSizeChanged()
	{
		var windowSize = GetWindow().ContentScaleSize;
		Scale = Vector2.One * MathF.Min(windowSize.X / Size.X, windowSize.Y / Size.Y);
	}
}
