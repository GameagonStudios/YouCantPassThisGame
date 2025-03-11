using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static InputSystem.InputActionState;

namespace InputSystem
{
    [GlobalClass, Tool]
    public partial class InputEventHandler : Node
    {
        public static InputEventHandler Current { get; private set; }

        [Export]
        public InputSystem.InputMap inputMap;

        [Export]
        public Input.MouseModeEnum MouseMode { get; private set; }

        Dictionary<Enum, List<(InputAction, int)>> _inputs = new();

        Dictionary<InputAction, EventActuator> Events = new();

        void Remap()
        {
            if (inputMap == null) return;

            _inputs.Clear();
            Events.Clear();

            foreach (InputActionGroup group in inputMap.Groups)
            {

                if (!(FindChild(group.Name) is Node groupNode))
                {
                    groupNode = new() { Name = group.Name };
                    AddChild(groupNode, true);
                    groupNode.Owner = GetTree().EditedSceneRoot;
                }

                foreach (InputAction ac in group.Actions)
                {
                    List<List<Enum>> listOfListOfEnum = ac.GetEvents();

                    if (groupNode.FindChild(ac.Name) is EventActuator ea)
                    {
                        Events.Add(ac, ea);
                    }
                    else
                    {
                        ea = new() { Name = ac.Name };
                        groupNode.AddChild(ea, true);
                        ea.Owner = GetTree().EditedSceneRoot;

                        GD.Print(ea.Name, " was added to ", Name);
                        Events.Add(ac, ea);
                    }

                    if (ac.MouseInput != MouseMotion.None)
                    {
                        if (_inputs.TryGetValue(MouseMotion.None, out List<(InputAction, int)> actions))
                        {
                            actions.Add((ac, 0));
                        }
                        else
                            _inputs.Add(MouseMotion.None, new List<(InputAction, int)>() { (ac, 0) });
                    }

                    for (int i = 0; i < listOfListOfEnum.Count; i++)
                    {
                        List<Enum> items = listOfListOfEnum[i];


                        foreach (Enum item in items)
                        {
                            if (_inputs.TryGetValue(item, out List<(InputAction, int)> actions))
                            {
                                int index = actions.FindIndex(0, actions.Count, o => o.Item1 == ac);
                                if (index >= 0) actions.RemoveAt(index);

                                actions.Add((ac, i));
                            }
                            else
                                _inputs.Add(item, new List<(InputAction, int)>() { (ac, i) });
                        }
                    }
                }
            }
        }

        // void AddAction(,string name , Array<InputEvent> events = null)
        // {
        // 	Actions.Add(events == null ? new(name) : new(name, events));
        // }

        // void EraseAction()
        // {
        // 	Actions.Add
        // }

        // void ActionClearEvents()
        // {

        // }

        // void ActionAddEvent()
        // {

        // }

        // void ActionEraseEvent()
        // {

        // }

        public override void _UnhandledInput(InputEvent @event)
        {
            ulong testTimer = Time.GetTicksUsec();

            if (_inputs.TryGetValue(ParseInputEvent(@event), out List<(InputAction, int)> actions))
            {
                foreach ((InputAction, int) action in actions)
                {
                    InputActionState state = action.Item1.UpdateAndGetState(@event, action.Item2);
                    if (state.state != PressState.None)
                        Events[action.Item1].Invoke(state);
                }
            }
			//GetViewport().SetInputAsHandled();
            @event.Dispose();
        }

        public override void _Ready()
        {
            _EnterTree();
        }

        public override void _EnterTree()
        {
            Current = this;

            ProcessMode = Node.ProcessModeEnum.Always;

            if (!Engine.IsEditorHint())
                Input.MouseMode = MouseMode;

            Remap();
        }

        public override void _ExitTree()
        {
            if (Current == this) Current = null;

            base._ExitTree();
        }

        static public Enum ParseInputEvent(InputEvent ievent)
        {
            return ievent switch
            {
                InputEventKey i => i.Keycode,
                InputEventMouseButton i => i.ButtonIndex,
                InputEventJoypadButton i => i.ButtonIndex,
                InputEventJoypadMotion i => i.Axis,
                InputEventMidi i => i.Message,
                InputEventMouseMotion i => MouseMotion.None, //por favor cambia esto
                _ => JoyButton.Invalid,
            };
        }

        static public Type GetInputEnumType(string name)
        {
            return name switch
            {
                "Key" => typeof(Key),
                "MouseButton" => typeof(MouseButton),
                "JoyButton" => typeof(JoyButton),
                "JoyAxis" => typeof(JoyAxis),
                "MidiMessage" => typeof(MidiMessage),
                "MouseMotion" => typeof(MouseMotion),
                _ => null,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ievent"></param>
        /// <param name="positive"></param>
        /// <returns>The strength of the InputAction with the specified sign if it isn't already signed like AxisValues</returns>
        public static object EventGetStrenth(InputEvent ievent, bool positive = true)
        {
            float sign = positive ? 1 : -1;

            return ievent switch
            {
                InputEventKey i => (i.Pressed ? 1 : 0) * sign,
                InputEventMouseButton i => (i.Pressed ? i.Factor : 0) * sign,
                InputEventJoypadButton i => (i.Pressure == 0 ? i.Pressed ? 1 : 0 : i.Pressure) * sign,
                InputEventJoypadMotion i => JoyAxisMapping(i, sign),
                InputEventMidi i => i.Pitch,
                _ => null,
            };
        }

        public static object MouseMotionMapping(InputEventMouseMotion e, MouseMotion motion)
        {
            return motion switch
            {
                MouseMotion.PixelVelocity => e.Velocity,
                MouseMotion.ScreenVelocity => e.ScreenVelocity,
                MouseMotion.PixelDelta => e.Relative,
                MouseMotion.ScreenDelta => e.ScreenRelative,
                MouseMotion.Position => e.Position,
                MouseMotion.Tilt => e.Tilt,
                MouseMotion.Pressure => e.Pressure,
                _ => null,
            };
        }

        static float JoyAxisMapping(InputEventJoypadMotion e, float sign)
        {
            if (e.Axis == JoyAxis.LeftY || e.Axis == JoyAxis.RightY)
            {
                return -e.AxisValue;
            }
            else if (e.Axis == JoyAxis.TriggerLeft || e.Axis == JoyAxis.TriggerRight)
            {
                return e.AxisValue * sign;
            }

            return e.AxisValue;
        }

        public static string ParseEnum(Enum e)
        {
            return e.GetType().ToString().Split('.')[1] + " " + e.ToString();
        }
    }
}