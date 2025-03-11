using Godot;
using Godot.Collections;

namespace Options
{
	public partial class SoftShadowsModeSelector : OptionButtonController
	{
		[Export]
		Array<RenderingServer.ShadowQuality> availableOptions = new Array<RenderingServer.ShadowQuality>();

		public override void CreateOptions()
		{
			Clear();

			for (int i = 0; i < availableOptions.Count; i++)
			{
				AddItem(availableOptions[i].ToString().Replace("Soft", "").ToUpper(), (int)availableOptions[i]);
				SetItemMetadata(i, (int)availableOptions[i]);
			}

			int ID = OptionsSavesHandler.Current.GetValue(key)?.As<int>() ?? GetItemId(Selected);

			Select(GetItemIndex(ID));
			OnItemSelected(ID);
		}

		public override void OnItemSelected(Variant data)
		{
			base.OnItemSelected(data);

			RenderingServer.ShadowQuality sq = (RenderingServer.ShadowQuality)(int)data;

			RenderingServer.PositionalSoftShadowFilterSetQuality(sq);
			RenderingServer.DirectionalSoftShadowFilterSetQuality(sq);
		}
	}
}