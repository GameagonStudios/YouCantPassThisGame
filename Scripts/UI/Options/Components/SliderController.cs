using Godot;
using System.Diagnostics;
using static Godot.Slider;

namespace Options
{
    public partial class SliderController : Control
    {
        [Export]
        public StringName key;

        [Export]
        public float defaultValue;

        [Export]
        public SpinBox spinBox;

        [Export]
        public Slider slider;

        [Export]
        public float multyplier;

        public override void _EnterTree()
        {
            base._EnterTree();

            double value = OptionsSavesHandler.Current.GetValue(key)?.As<double>() ?? defaultValue;

            slider.SetValueNoSignal(value);
            if (spinBox != null)
            {
                spinBox.ValueChanged += SetValue;
                spinBox.Value = value * multyplier;
            }

            slider.ValueChanged += HandleValueChanged;
        }



        private void HandleValueChanged(double value)
        {
            OptionsSavesHandler.Current.SetValue(key, slider.Value);
            spinBox?.SetValueNoSignal(value * multyplier);
        }

        private void SetValue(double value)
        {

            slider.Value = value / multyplier;
            GD.Print(value);
            OptionsSavesHandler.Current.SetValue(key, slider.Value);


        }
    }
}

