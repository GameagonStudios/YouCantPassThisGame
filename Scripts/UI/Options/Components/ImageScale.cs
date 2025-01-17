using Godot;
using System;

public partial class ImageScale : Node
{

	[Export]
	private StringName sensivilityKey; 
	public override void _EnterTree()
	{
		base._EnterTree();

		RenderingServer.ViewportSetScaling3DScale(GetViewport().GetViewportRid(), OptionsSavesHandler.Current.GetValue(sensivilityKey)?.As<float>() ?? 1);
		OptionsSavesHandler.Current.onOptionsChanged += OnChangeScale;
	}
	public void OnChangeScale(StringName key, Variant value)
	{
		if(key == sensivilityKey)
			RenderingServer.ViewportSetScaling3DScale(GetViewport().GetViewportRid(), value.As<float>());
	}
}
