using Godot;
using Godot.Collections;

namespace InputSystem
{
	[GlobalClass, GodotClassName("InputActionGroup"), Tool]
	public partial class InputActionGroup : Resource
	{
		[Export]
        public string Name { get{ return ResourceName; } set {ResourceName = value; } }

		[Export]
		public Array<InputAction> Actions { get; private set; } = new();

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
                    Awake();
                }
            }
        }
        public bool _Sleeping = false;

		public void Sleep()
        {
			foreach(InputAction a in Actions)
			{
                a.Sleep();
            }

			_Sleeping = true;
        }

		public void Awake()
        {
			foreach(InputAction a in Actions)
			{
                a.Sleeping = false;
            }

			_Sleeping = false;
        }
	}
}