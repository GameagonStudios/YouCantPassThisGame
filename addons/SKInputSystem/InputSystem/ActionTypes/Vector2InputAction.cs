using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using static InputSystem.InputActionState;

namespace InputSystem
{
    [GlobalClass, GodotClassName("Vector2InputAction"), Tool]
    public partial class Vector2InputAction : InputAction
    {
        [Export] public Array<string> AxisXNegative
        {
            get
            {
                return new Array<string>(enumAxisXNegative.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumAxisXNegative = value.Select(e =>
                {
                    var a = e.Split(" ");
                    return (Enum)Enum.Parse(InputEventHandler.GetInputEnumType(a[0]), a[1]);
                }).ToList();
            }
        }
        [Export] public Array<string> AxisXPositive
        {
            get
            {
                return new Array<string>(enumAxisXPositive.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumAxisXPositive = value.Select(e =>
                {
                    var a = e.Split(" ");
                    return (Enum)Enum.Parse(InputEventHandler.GetInputEnumType(a[0]), a[1]);
                }).ToList();
            }
        }
        [Export] public Array<string> AxisYNegative
        {
            get
            {
                return new Array<string>(enumAxisYNegative.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumAxisYNegative = value.Select(e =>
                {
                    var a = e.Split(" ");
                    return (Enum)Enum.Parse(InputEventHandler.GetInputEnumType(a[0]), a[1]);
                }).ToList();
            }
        }
        [Export] public Array<string> AxisYPositive
        {
            get
            {
                return new Array<string>(enumAxisYPositive.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumAxisYPositive = value.Select(e =>
                {
                    var a = e.Split(" ");
                    return (Enum)Enum.Parse(InputEventHandler.GetInputEnumType(a[0]), a[1]);
                }).ToList();
            }
        }

        [Export] public string XNegativeName = "XNegative";
        [Export] public string XPositiveName = "XPositive";
        [Export] public string YNegativeName = "YNegative";
        [Export] public string YPositiveName = "YPositive";

        public List<Enum> enumAxisXNegative = new();
        public List<Enum> enumAxisXPositive = new();
        public List<Enum> enumAxisYNegative = new();
        public List<Enum> enumAxisYPositive = new();

        InputActionState _state = new(PressState.None, 0, new Vector2(0, 0), null);
        public float[] separatedStrengths { get; private set; } = new float[4] { 0, 0, 0, 0 };

        public Vector2InputAction()
        {
            Name = "";
            enumAxisXNegative = new();
            enumAxisXPositive = new();
            enumAxisYNegative = new();
            enumAxisYPositive = new();
        }

        public Vector2InputAction(string name)
        {
            Name = name;
            enumAxisXNegative = new();
            enumAxisXPositive = new();
            enumAxisYNegative = new();
            enumAxisYPositive = new();
        }

        public Vector2InputAction(string name, (List<Enum>, List<Enum>, List<Enum>, List<Enum>) _events)
        {
            Name = name;
            enumAxisXNegative = new(_events.Item1);
            enumAxisXPositive = new(_events.Item2);
            enumAxisYNegative = new(_events.Item3);
            enumAxisYPositive = new(_events.Item4);
        }

        public override InputActionState UpdateAndGetState(InputEvent @event, int subActionIndex)
        {

            if (_Sleeping)
            {
                return _state = new(PressState.None, 0, new Vector2(0, 0), null);
            }

            _state.inputEvent = @event;

            if (@event is InputEventMouseMotion me)
            {
                _state.strength = (Vector2)InputEventHandler.MouseMotionMapping(me, MouseInput);
            }
            else
            {
                separatedStrengths[subActionIndex] = (float)InputEventHandler.EventGetStrenth(@event);

                _state.strength = new Vector2(-separatedStrengths[0] + separatedStrengths[1], -separatedStrengths[2] + separatedStrengths[3]);
            }

            return CalculateTotalState();
        }

        InputActionState CalculateTotalState()
        {
            if (Math.Abs(((Vector2)_state.strength).Length()) > DeadZone || _state.inputEvent is InputEventMouseMotion)
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

                _state.strength = Vector2.Zero;
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
            return new List<List<Enum>>() { enumAxisXNegative, enumAxisXPositive, enumAxisYNegative, enumAxisYPositive };
        }

        public override List<string> GetEventNames()
        {
            return new List<string>() { XNegativeName, XPositiveName, YNegativeName, YPositiveName };
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
                    { "hint_string", "None, PixelVelocity, ScreenVelocity, PixelDelta, ScreenDelta, SmoothScreenDelta, Position, Tilt" }
                }
            };

            return properties;
        }
    }
}
