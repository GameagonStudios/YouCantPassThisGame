using Godot;
using Godot.Collections;
using InputSystem;
using System;

[GlobalClass, GodotClassName("EventActuator"), Tool]
public partial class EventActuator : Node
{
	[Signal]
	public delegate void onInvokeEventHandler(Variant args);

	public void Invoke(params Variant[] args)
	{
		EmitSignal(SignalName.onInvoke, args);
	}
}
