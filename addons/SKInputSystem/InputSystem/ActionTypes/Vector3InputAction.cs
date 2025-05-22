using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using static InputSystem.InputActionState;

namespace InputSystem
{
    [GlobalClass, GodotClassName("Vector3InputAction"), Tool]
    public partial class Vector3InputAction : InputAction
    {
        [Export]
        public Array<string> AxisXNegative
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
        [Export]
        public Array<string> AxisXPositive
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
        [Export]
        public Array<string> AxisYNegative
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
        [Export]
        public Array<string> AxisYPositive
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
        [Export]
        public Array<string> AxisZNegative
        {
            get
            {
                return new Array<string>(enumAxisZNegative.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumAxisZNegative = value.Select(e =>
                {
                    var a = e.Split(" ");
                    return (Enum)Enum.Parse(InputEventHandler.GetInputEnumType(a[0]), a[1]);
                }).ToList();
            }
        }
        [Export]
        public Array<string> AxisZPositive
        {
            get
            {
                return new Array<string>(enumAxisZPositive.Select(e => InputEventHandler.ParseEnum(e)));
            }
            set
            {
                enumAxisZPositive = value.Select(e =>
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
        [Export] public string ZNegativeName = "YNegative";
        [Export] public string ZPositiveName = "YPositive";

        public List<Enum> enumAxisXNegative = new();
        public List<Enum> enumAxisXPositive = new();
        public List<Enum> enumAxisYNegative = new();
        public List<Enum> enumAxisYPositive = new();
        public List<Enum> enumAxisZNegative = new();
        public List<Enum> enumAxisZPositive = new();

        InputActionState _state = new(PressState.None, 0, new Vector3(0, 0, 0), null);
        public float[] separatedStrengths { get; private set; } = new float[6] { 0, 0, 0, 0, 0, 0 };

        public Vector3InputAction()
        {
            Name = "";
            enumAxisXNegative = new();
            enumAxisXPositive = new();
            enumAxisYNegative = new();
            enumAxisYPositive = new();
            enumAxisZNegative = new();
            enumAxisZPositive = new();
        }

        public Vector3InputAction(string name)
        {
            Name = name;
            enumAxisXNegative = new();
            enumAxisXPositive = new();
            enumAxisYNegative = new();
            enumAxisYPositive = new();
            enumAxisZNegative = new();
            enumAxisZPositive = new();
        }

        public Vector3InputAction(string name, (List<Enum>, List<Enum>, List<Enum>, List<Enum>, List<Enum>, List<Enum>) _events)
        {
            Name = name;
            enumAxisXNegative = new(_events.Item1);
            enumAxisXPositive = new(_events.Item2);
            enumAxisYNegative = new(_events.Item3);
            enumAxisYPositive = new(_events.Item4);
            enumAxisZNegative = new(_events.Item5);
            enumAxisZPositive = new(_events.Item6);
        }

        public override InputActionState UpdateAndGetState(InputEvent @event, int subActionIndex)
        {
            if (_Sleeping)
            {
                return _state = new(PressState.None, 0, new Vector3(0, 0, 0), null);
            }

            _state.inputEvent = @event;

            object stg = InputEventHandler.EventGetStrenth(@event);
            if (stg is Vector2 vector)
            {
                separatedStrengths[subActionIndex - 2] = vector.X;
                separatedStrengths[subActionIndex] = vector.Y;
            }
            else
            {
                if (@event.IsPressed())
                {
                    separatedStrengths[subActionIndex] = (float)InputEventHandler.EventGetStrenth(@event);
                }
                else
                {
                    separatedStrengths[subActionIndex] = 0f;
                }
            }
            _state.strength = new Vector3(-separatedStrengths[0] + separatedStrengths[1], -separatedStrengths[2] + separatedStrengths[3], -separatedStrengths[4] + separatedStrengths[5]);

            return CalculateTotalState();
        }

        InputActionState CalculateTotalState()
        {
            if(_Sleeping) return _state;

            if (Math.Abs(((Vector3)_state.strength).Length()) > DeadZone)
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

                _state.strength = Vector3.Zero;
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
            return new List<List<Enum>>() { enumAxisXNegative, enumAxisXPositive, enumAxisYNegative, enumAxisYPositive, enumAxisZNegative, enumAxisZPositive };
        }

        public override List<string> GetEventNames()
        {
            return new List<string>() { XNegativeName, XPositiveName, YNegativeName, YPositiveName, ZNegativeName, ZPositiveName };
        }
    }
}
