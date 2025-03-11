using Godot;
using System;

namespace Options
{
	public partial class FpsLimitSelector : SliderController
	{
		protected override void HandleValueChanged(double value)
		{
			Engine.MaxFps = (int)value;

			base.HandleValueChanged(value);
        }
	}
}
