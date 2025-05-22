using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;
using static InputSystem.InputActionState;

namespace InputSystem
{
	[GlobalClass, GodotClassName("InputAction"), Tool]
	public partial class InputAction : Resource
	{
		[Export]
		public string Name { get { return ResourceName; } set { ResourceName = value; } }

		[Export]
		public float DeadZone = 0.25f;

		[Export]
		public bool Sleeping
		{
			get
			{
				return _Sleeping;
			}
			set
			{
				if (value)
				{
					Sleep();
				}
				else
				{
					_Sleeping = false;
				}
			}
		}
		public bool _Sleeping = false;

		public MouseMotion MouseInput = MouseMotion.None;

		protected ulong TimeSinceLastPress;

        public virtual void Sleep()
		{
			throw new NotImplementedException();
		}

        public virtual InputActionState UpdateAndGetState(InputEvent @event, int subActionIndex)
        {
            throw new NotImplementedException();
        }

        public virtual List<List<Enum>> GetEvents()
		{
			throw new NotImplementedException();
		}

        protected PressState BiggerState(PressState a, PressState b)
        {
            return a > b ? a : b;
        }

        public virtual List<string> GetEventNames()
        {
            throw new NotImplementedException();
        }
    }
}