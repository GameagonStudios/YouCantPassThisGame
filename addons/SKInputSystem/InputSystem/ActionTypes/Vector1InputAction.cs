using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using static InputSystem.InputActionState;

namespace InputSystem
{
    [GlobalClass, Tool]
    public partial class Vector1InputAction : InputAction
    {
        [Export] public Array<string> Negative
        {
            get
            {
                return new Array<string>(enumNegative.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumNegative = value.Select(e =>
                {
                    var a = e.Split(" ");
                    return (Enum)Enum.Parse(InputEventHandler.GetInputEnumType(a[0]), a[1]);
                }).ToList();
            }
        }
        [Export] public Array<string> Positive
        {
            get
            {
                return new Array<string>(enumPositive.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumPositive = value.Select(e =>
                {
                    var a = e.Split(" ");
                    return (Enum)Enum.Parse(InputEventHandler.GetInputEnumType(a[0]), a[1]);
                }).ToList();
            }
        }

        [Export] public string NegativeName = "Negative";
        [Export] public string PositiveName = "Positive";

        public List<Enum> enumNegative = new();
        public List<Enum> enumPositive = new();

        InputActionState _state = new(PressState.None, 0, 0f, null);
        public float[] separatedStrengths { get; private set; } = new float[2] { 0, 0 };

        public Vector1InputAction()
        {
            Name = "";
            enumNegative = new();
            enumPositive = new();
        }

        public Vector1InputAction(string name)
        {
            Name = name;
            enumNegative = new();
            enumPositive = new();
        }

        public Vector1InputAction(string name, (List<Enum>, List<Enum>) _events)
        {
            Name = name;
            enumNegative = new(_events.Item1);
            enumPositive = new(_events.Item2);
        }

        public override InputActionState UpdateAndGetState(InputEvent @event, int subActionIndex)
        {
            if (_Sleeping)
            {
                return _state = new(PressState.None, 0, 0f, null);
            }

            _state.inputEvent = @event;

            if (@event.IsPressed())
            {
                separatedStrengths[subActionIndex] = @event is InputEventMouseMotion me ?
                (float)InputEventHandler.MouseMotionMapping(me, MouseInput) : (float)InputEventHandler.EventGetStrenth(@event);
            }
            else
            {
                separatedStrengths[subActionIndex] = 0f;
            }

            _state.strength = -separatedStrengths[0] + separatedStrengths[1];

            return CalculateTotalState();
        }

        InputActionState CalculateTotalState()
        {
            if (Math.Abs((float)_state.strength) > DeadZone)
            {
                if (_state.state < PressState.JustPressed)
                {
                    _state.state = PressState.JustPressed;
                    TimeSinceLastPress = Time.GetTicksMsec();
                }
                else
                {
                    _state.state = PressState.Holding;
                }

                _state.timeHeld = Time.GetTicksMsec() - TimeSinceLastPress;
            }
            else
            {
                if (_state.state <= PressState.Released)
                {
                    _state.state = PressState.None;
                    _state.timeHeld = 0;
                }
                else
                {
                    _state.state = PressState.Released;
                    _state.timeHeld = Time.GetTicksMsec() - TimeSinceLastPress;
                }

                _state.strength = 0f;
            }

            //if (_state.state != PressState.None) GD.Print(_state.state + " with strength " + _state.strength);

            return _state;
        }

        public override void Sleep()
        {
            _Sleeping = true;
            _state.state = PressState.None;
            _state.strength = 0;
            _state.timeHeld = 0;
        }

        public override List<List<Enum>> GetEvents()
        {
            return new List<List<Enum>>() { enumNegative, enumPositive };
        }

        public override List<string> GetEventNames()
        {
            return new List<string>() { NegativeName, PositiveName};
        }

        public override Array<Dictionary> _GetPropertyList()
        {
            var properties = new Array<Dictionary>
            {
                new Dictionary()
                {
                    { "name", "MouseInput" },
                    { "type", (int)Variant.Type.Int },
                    { "usage", (int)PropertyUsageFlags.Default }, // See above assignment.
                    { "hint", (int)PropertyHint.Enum },
                    { "hint_string", "None, Pressure" }
                }
            };

            return properties;
        }
    }
}
