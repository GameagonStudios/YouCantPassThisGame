using Godot;
using System;
using System.Diagnostics;

public partial class SoftShadows : OptionButton
{
	// Called when the node enters the scene tree for the first time.
	StringName key = "Softshadows";
	// Called when the node enters the scene tree for the first time.
	public override void _EnterTree()
	{
		ItemSelected += OnItemSelected;
		QualitySoftShadows();
	}
	public void QualitySoftShadows()
	{
		int SoftshadowsQuality;
		SoftshadowsQuality = OptionsSavesHandler.Current.GetValue(key)?.As<int>() ?? 4;
		for (int i = 0; i < ItemCount; i++)
		{
			//AddItem(GetItemText(i), i);
			SetItemMetadata(i, GetItemId(i));
			int Index = GetItemIndex(i);


			if (SoftshadowsQuality == GetItemId(i))
			{
				Select(i);
			}

		}
		RenderingServer.PositionalSoftShadowFilterSetQuality((RenderingServer.ShadowQuality)SoftshadowsQuality);
		RenderingServer.DirectionalSoftShadowFilterSetQuality((RenderingServer.ShadowQuality)SoftshadowsQuality);

	}

	public void OnItemSelected(long index)
	{
		var l = (int)GetSelectedMetadata();
		OptionsSavesHandler.Current.SetValue(key, l);
		QualitySoftShadows();
	}


}
