using Godot;
using System;
using System.Collections.Generic;

namespace Options
{
	public partial class SoundControls : GridContainer
	{
		[Export]
		PackedScene sliderControllerPrefab;

		List<SliderController> soundControllers = new List<SliderController>();

		public override void _EnterTree()
		{
			for (int i = 0; i < AudioServer.BusCount; i++)
			{
				SliderController c = GenerateOption(AudioServer.GetBusName(i));

				int index = i;
				c.ValueChanged += (k, v) => HandleValueChanged(index, k, v);

				AddChild(c);
			}
		}

		SliderController GenerateOption(string oName)
		{
			oName = oName.ToUpper();

			Label oLabel = new Label();

			AddChild(oLabel);

			oLabel.Name = oName;
			oLabel.Text = oName;

			SliderController oSliderController = sliderControllerPrefab.Instantiate<SliderController>();

			oSliderController.key = oName;
			oSliderController.defaultValue = 0.5f;
			oSliderController.maxValue = 1;
			oSliderController.step = 0.01f;
			oSliderController.visualMultiplier = 100;

			soundControllers.Add(oSliderController);

			return oSliderController;
		}

		void HandleValueChanged(int index, StringName key, double value)
		{
			AudioServer.SetBusVolumeLinear(index, Mathf.Pow((float)value, 2));
		}
	}
}