using Godot;
using System;

public partial class WoldEnvironmentManager : WorldEnvironment
{
	public static WoldEnvironmentManager Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}
}
