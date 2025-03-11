using Godot;
namespace Options
{
	public partial class CheckboxController : CheckBox
	{
		[Export]
		public StringName key;
		public override void _EnterTree()
		{
			Toggled += OnToggled;
			LoadOptions();
		}

		public override void _ExitTree()
		{
			Toggled -= OnToggled;
		}

		public virtual void LoadOptions()
		{
			bool value = OptionsSavesHandler.Current.GetValue(key)?.As<bool>() ?? ButtonPressed;

			SetPressedNoSignal(value);
			OnToggled(value);
		}

		public virtual void OnToggled(bool value)
		{
			OptionsSavesHandler.Current.SetValue(key, value);
		}
	}
}





