using Godot;
using System.Diagnostics;
using static Godot.Slider;

namespace Options
{
    public partial class SliderController : Control
    {

		[Signal]
		public delegate void ValueChangedEventHandler(StringName key, double value);

        [Export]
        public StringName key;

        [Export]
        public float defaultValue = 1;

        [Export]
        public SpinBox spinBox;

        [Export]
        public Slider slider;

		[Export]
        public float step = 0.05f;

        [Export]
        public float minValue = 0;

		[Export]
        public float maxValue = 1;

		[Export]
		public float visualMultiplier = 1;

		public override void _EnterTree()
		{
			base._EnterTree();

			double value = OptionsSavesHandler.Current.GetValue(key)?.As<double>() ?? defaultValue;

			if (spinBox != null)
			{
				spinBox.ValueChanged += (value) => HandleValueChanged(value / visualMultiplier);

				spinBox.MinValue = minValue * visualMultiplier;
				spinBox.MaxValue = maxValue * visualMultiplier;
				spinBox.Step = step * visualMultiplier;
			}

			slider.ValueChanged += HandleValueChanged;

			slider.MinValue = minValue;
			slider.MaxValue = maxValue;
			slider.Step = step;

			HandleValueChanged(value);
		}

		public override void _ExitTree()
		{
			if (spinBox != null) spinBox.ValueChanged -= (value) => HandleValueChanged(value / visualMultiplier);
			slider.ValueChanged -= HandleValueChanged;
		}

		protected virtual void HandleValueChanged(double value)
		{
			spinBox?.SetValueNoSignal(value * visualMultiplier);
			slider?.SetValueNoSignal(value);
			
			EmitSignal(SignalName.ValueChanged, key, value);
			OptionsSavesHandler.Current.SetValue(key, value);
		}
    }
}

