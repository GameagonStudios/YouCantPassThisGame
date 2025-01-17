using Godot;
using System;
using System.Diagnostics;
using System.IO;
public partial class SetWindows : OptionButton
{
	public StringName key = "WindowsMode";
	// Called when the node enters the scene tree for the first time.
	public override void _EnterTree()
	{
		ItemSelected += OnItemSelected;
		SetWindow();
	}
	public void SetWindow()
	{
		int currentWindow;
		currentWindow = OptionsSavesHandler.Current.GetValue(key)?.As<int>() ?? 3;
		for (int i = 0; i < ItemCount; i++)
		{
			//AddItem(GetItemText(i), i);
			SetItemMetadata(i, GetItemId(i));
			int Index = GetItemIndex(i);


			if (currentWindow == GetItemId(i))
			{
				Select(i);
				Debug.WriteLine(Index);
				if(Index == 2)
				{
					currentWindow = 0;
					DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
				}
				else
				{
					DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
				}
			}

		}

		DisplayServer.WindowSetMode((DisplayServer.WindowMode)currentWindow);
	
	}

	public void OnItemSelected(long index)
	{
		var l = (int)GetSelectedMetadata();
		OptionsSavesHandler.Current.SetValue(key, l);
		SetWindow();
	}


}
