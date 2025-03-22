using Godot;
using Godot.Collections;
using System;

namespace Options
{
	public partial class ResolutionSelector : OptionButtonController
	{
		[Export]
		public Vector2I defaultIndex = new(0, 0);

		[Export]
		Array<int> heights = new();

		[Export]
		Array<Vector2I> aspectRatios = new();

		int GetIndex(int x, int y) => x * aspectRatios.Count + y;
		int GetIndex(Vector2I v) => GetIndex(v.X, v.Y);

		public override void CreateOptions()
		{
			Clear();

			Vector2I? currentRes = OptionsSavesHandler.Current.GetValue(key)?.As<Vector2I>();
			

			for (int i = 0; i < heights.Count; i++)
			{
				for (int e = 0; e < aspectRatios.Count; e++)
				{
					int index = GetIndex(i, e);

					var asp = aspectRatios[e];

					Vector2I vec = new Vector2I(heights[i] * asp.X / asp.Y, heights[i]);

					AddItem($"{vec.X}X{vec.Y} {asp.X}:{asp.Y}", index);
					SetItemMetadata(index, vec);

					if (currentRes != null ? currentRes == vec : (i == defaultIndex.X && e == defaultIndex.Y))
					{
						Select(index);
						OnItemSelected(vec);
					}
				}
			}
		}

		public override void OnItemSelected(Variant data)
		{
			base.OnItemSelected(data);

			Vector2I res = (Vector2I)data;
			
			DisplayServer.WindowSetSize(res,0);
			GetWindow().ContentScaleSize = res;
		}
	}
}