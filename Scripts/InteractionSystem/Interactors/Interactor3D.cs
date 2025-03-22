using Godot;
using InputSystem;
using System;

public partial class Interactor3D : Node3D 
{
	[Signal]
	public delegate void TargetFoundEventHandler(Interactable3D target);
	[Signal]
	public delegate void TargetLostEventHandler(Interactable3D target);

	public virtual Interactable3D Target { get; protected set; }

    protected bool Active = false;

    public virtual void Select(Interactable3D _target)
	{
		if(Target != null) UnSelect();

        Target = _target;
		
        _target.Select(this);

		EmitSignal(SignalName.TargetFound, _target);
    }

	public virtual void UnSelect()
	{
		if(Target == null) return;

		if(Active) Release();

        Target.UnSelect(this);

		EmitSignal(SignalName.TargetLost, Target);

		Target = null;
	}

	public virtual void Activate(Interactable3D _target =  null)
	{
        Active = true;

        _target ??= Target;

        _target?.Activate(this);
    }

	public virtual void Release(Interactable3D _target =  null)
	{
		Active = false;

		_target ??= Target;

		_target?.Release(this);
	}

	public virtual void Interact(InputActionState state)
	{
		if (state.state == InputActionState.PressState.JustPressed)
        {
            Activate();
        }
        else if (state.state == InputActionState.PressState.Released)
        {
            Release();
        }
    }
}
