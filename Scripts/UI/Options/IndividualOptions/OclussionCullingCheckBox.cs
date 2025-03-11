using Godot;
using System;

namespace Options
{
	public partial class OclussionCullingCheckBox : CheckboxController
	{
		public override void OnToggled(bool value)
		{
			base.OnToggled(value);

			GetWindow().UseOcclusionCulling = value;
		}
	}
}