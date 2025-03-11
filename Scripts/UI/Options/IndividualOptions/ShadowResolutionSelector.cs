using Godot;
using System;
using System.ComponentModel.DataAnnotations;

namespace Options
{
	public partial class ShadowResolutionSelector : OptionButtonController
	{
		[Export]
		int baseRes = 256;

		[Export]
		public int defaultExponent = 1;

		[Export]
		int maxExponent = 4;

		public override void CreateOptions()
		{
			Clear();

			defaultExponent = Math.Clamp(defaultExponent, 0, maxExponent);

			int currentRes = OptionsSavesHandler.Current.GetValue(key)?.As<int>() ?? (baseRes * (int)Math.Pow(2, defaultExponent));

			int index = defaultExponent;

			for (int i = 0; i < maxExponent; i++)
			{
				int tempRes = baseRes * (int)Math.Pow(2, i);

				if (tempRes == currentRes) index = i;

				AddItem($"{tempRes}X{tempRes}", i);
				SetItemMetadata(i, tempRes);
			}

			Select(index);
			OnItemSelected(currentRes);
		}

		public override void OnItemSelected(Variant data)
		{
			base.OnItemSelected(data);

			RenderingServer.DirectionalShadowAtlasSetSize((int)data, true);
		}
	}
}