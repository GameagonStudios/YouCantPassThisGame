using Godot;
using Godot.Collections;

namespace Options
{
	public partial class WindowModeSelector : OptionButtonController
	{
		[Export]
		Array<DisplayServer.WindowMode> availableOptions = new Array<DisplayServer.WindowMode>();

		public override void CreateOptions()
		{
			Clear();

			GetWindow().MoveToCenter();

			for (int i = 0; i < availableOptions.Count; i++)
			{
				AddItem(availableOptions[i].ToString().ToUpper(), (int)availableOptions[i]);
				SetItemMetadata(i, (int)availableOptions[i]);
			}

			int ID = OptionsSavesHandler.Current.GetValue(key)?.As<int>() ?? GetItemId(Selected);

			Select(GetItemIndex(ID));
			OnItemSelected(ID);
		}

		public override void OnItemSelected(Variant data)
		{
			base.OnItemSelected(data);

			DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, (int)data == (int)DisplayServer.WindowMode.Maximized);
			DisplayServer.WindowSetMode((DisplayServer.WindowMode)(int)data);
		}
	}
}