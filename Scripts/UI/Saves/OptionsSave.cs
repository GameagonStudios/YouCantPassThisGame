using Godot;
using Godot.Collections;
using System;

public partial class OptionsSave : Resource
{
	[Export]
	public Dictionary<StringName, Variant> Options = new Dictionary<StringName, Variant>();

	public void SetValue(StringName key, Variant value)
	{
		if (Options.ContainsKey(key))
			Options[key] = value;
		else
			Options.Add(key, value);
	}

	public Variant? GetValue(StringName key)
	{
		if (key != null && Options.ContainsKey(key))
			return Options[key];
		else
			return null;
	}
}
