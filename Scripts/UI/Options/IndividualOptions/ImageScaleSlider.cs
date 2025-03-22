using Godot;

namespace Options
{
	public partial class ImageScaleSlider : SliderController
	{
		protected override void HandleValueChanged(double value)
		{
			RenderingServer.ViewportSetScaling3DScale(GetViewport().GetViewportRid(), (float)value);

			base.HandleValueChanged(value);
        }
	}
}
