using Godot;
using System;

public partial class CheckboxController : CheckBox
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public StringName VolumeKey;

    [Signal]
    public delegate void OnOnEventHandler();

	[Signal]
    public delegate void OnOffEventHandler();
	public override void _EnterTree()
	{
		base._EnterTree();
		bool isOn = OptionsSavesHandler.Current.GetValue(VolumeKey)?.As<bool>() ?? true;
		SetFunction(isOn);
	}
	
	public void Onclick()
	{
		SetFunction(this.ButtonPressed);
	}
	private void SetFunction(bool isOn)
	{
		this.ButtonPressed = isOn;
		if(isOn == true)
		{
			EmitSignal(SignalName.OnOn);
		}
		else
		{
			EmitSignal(SignalName.OnOff);
		}
		OptionsSavesHandler.Current.SetValue(VolumeKey, this.ButtonPressed);
	}
}





