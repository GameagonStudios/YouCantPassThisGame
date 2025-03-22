using Godot;
using System;

namespace Options
{
	public partial class BrightnessSlider : SliderController
	{
		protected override void HandleValueChanged(double value)
		{
			WoldEnvironmentManager.Instance.Environment.AdjustmentBrightness = (float)value;

			base.HandleValueChanged(value);
		}
	}
}