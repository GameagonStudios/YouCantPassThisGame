using Godot;

namespace Options
{
	public partial class OptionButtonController : OptionButton
	{
		[Export]
		public StringName key;

		public override void _EnterTree()
		{
			ItemSelected += HandleItemSelected;
			CreateOptions();
		}

		public override void _ExitTree()
		{
			ItemSelected -= HandleItemSelected;
		}

		void HandleItemSelected(long index) => OnItemSelected(GetItemMetadata((int)index));

		public virtual void CreateOptions()
		{
			int ID = OptionsSavesHandler.Current.GetValue(key)?.As<int>() ?? GetItemId(Selected);

			for (int i = 0; i < ItemCount; i++)
			{
				SetItemMetadata(i, GetItemId(i));
			}

			Select(GetItemIndex(ID));
			OnItemSelected(ID);
		}

		public virtual void OnItemSelected(Variant data)
		{
			OptionsSavesHandler.Current.SetValue(key, data);
		}
	}
}