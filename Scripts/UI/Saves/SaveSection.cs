using Godot;
using System;

public partial class SaveSection : Resource
{
    [Export] public Godot.Collections.Dictionary<StringName, Variant> Data
    = new();

    public void SetValue(StringName key, Variant value) => Data[key] = value;

    public Variant? GetValue(StringName key)
        => Data.TryGetValue(key, out var v) ? v : null;

    public bool HasKey(StringName key) => Data.ContainsKey(key);

    public bool Remove(StringName key) => Data.Remove(key);
}
