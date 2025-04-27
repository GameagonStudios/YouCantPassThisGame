using Godot;
using System;

namespace InputSystem;

[GlobalClass, GodotClassName("InputEventSubHandler"), Tool]
public partial class InputEventSubHandler : EventActuator
{
	[Export]
	public InputAction inputAction;

	[Export(PropertyHint.Flags)]
	InputActionState.PressState state = InputActionState.PressState.All;

	EventActuator lastEA = null;

	void InvokeVar(Variant a)
	{
		if (state.HasFlag(((InputActionState)a).state)) Invoke(a);
	}

	public override void _EnterTree()
	{
		base._EnterTree();

		InputEventHandler.CurrentChanged += CurrentInputEventHandlerChanged;

		CurrentInputEventHandlerChanged();
	}

	public void CurrentInputEventHandlerChanged()
	{
		if (InputEventHandler.Current?.Events.TryGetValue(inputAction, out EventActuator value) ?? false)
		{
			value.onInvoke += InvokeVar;

			if (lastEA != null) lastEA.onInvoke -= InvokeVar;

			lastEA = value;
		}
	}
}