using Godot;
using System;

public partial class BrickBreakerBrick : StaticBody2D, ColissionReciver
{
	[Export]
	int livePoints;

	[Export]
	ColorRect color;

	public void setColor(Color col)
	{
		color.Color = col;
	}

	public void setHealth(int lives)
	{
		livePoints = lives;
	}
    public void HandleCollision(Node node)
    {
		if(livePoints > 0)
		{
			livePoints--;
		}
		if(livePoints <= 0)
		{
			this.Visible = false;
			this.ProcessMode = ProcessModeEnum.Disabled;

		}
    }

}
