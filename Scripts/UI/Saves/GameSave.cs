using Godot;
using System;

public partial class GameSave : Resource
{
    [Export] public Godot.Collections.Dictionary<string, SaveSection> Sections = new();
    [Export] public int Version = 1;

    public SaveSection GetSection(string name)
    {
        if (!Sections.TryGetValue(name, out var sec) || sec == null)
        {
            sec = new SaveSection();
            Sections[name] = sec;
        }
        return sec;
    }
}
