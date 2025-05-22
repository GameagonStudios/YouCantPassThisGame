using System;
using Godot;

namespace InputSystem
{
    public partial class InputActionState : GodotObject
    {
        public InputActionState()
        {
            state = PressState.None;
            timeHeld = 0;
            strength = 0f;
            inputEvent = null;
        }

        public InputActionState(PressState _state, ulong _timeHeld, object _strength, InputEvent _inputEvent)
        {
            state = _state;
            timeHeld = _timeHeld;
            strength = _strength;
            inputEvent = _inputEvent;
        }

        public InputActionState SetValues(PressState _state, ulong _timeHeld, object _strength, InputEvent _inputEvent)
        {
            state = _state;
            timeHeld = _timeHeld;
            strength = _strength;
            inputEvent = _inputEvent;
            return this;
        }

        public PressState state;

        public ulong timeHeld = 0;

        public object strength;

        public InputEvent inputEvent;

		[Flags]
        public enum PressState
		{
			All = -1, None = 0, Released = 1, JustPressed = 2, Holding = 3
		}

        public static PressState AddState(PressState a, PressState b)
        {
            return a > b ? a : b;
        }
    }
}

