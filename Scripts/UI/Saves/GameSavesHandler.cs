using Godot;
using System;

public partial class GameSavesHandler : Control
{
    public static GameSavesHandler Current;

    [Signal]
    public delegate void OnValueChangedEventHandler(string section, StringName key, Variant value);

    [Export] public string SlotName = "slot1";                      // slot actual
    [Export(PropertyHint.Dir)] public string SavesDir = "user://saves";
    [Export] public string FilePattern = "save_{0}.res";            // {0}=slot

    public GameSave Save;

    public override void _EnterTree()
    {
        Current = this;
        LoadSlot(SlotName);
    }

    public override void _ExitTree()
    {
        if (Current == this) Current = null;
        base._ExitTree();
    }

    private string GetPathForSlot(string slot)
        => System.IO.Path.Combine(SavesDir, string.Format(FilePattern, slot)).Replace('\\','/');

    public bool LoadSlot(string slot)
    {
        SlotName = slot;
        DirAccess.MakeDirRecursiveAbsolute(SavesDir);
        var path = GetPathForSlot(slot);

        if (ResourceLoader.Exists(path))
        {
            Save = ResourceLoader.Load<GameSave>(path) ?? new GameSave();
        }
        else
        {
            Save = new GameSave();
        }
        return true;
    }

    public void NewSlot(string slot)
    {
        SlotName = slot;
        Save = new GameSave();
        SaveNow();
    }

    public void SaveNow()
    {
        var path = GetPathForSlot(SlotName);
        var err = ResourceSaver.Save(Save, path);
        if (err != Error.Ok)
            GD.PushError($"Error al guardar en {path}: {err}");
    }

    public void SetValue(string section, StringName key, Variant value, bool autoSave = true)
    {
        var sec = Save.GetSection(section);
        sec.SetValue(key, value);
        EmitSignal(SignalName.OnValueChanged, section, key, value);
        if (autoSave) SaveNow();
    }

    public Variant? GetValue(string section, StringName key)
        => Save.GetSection(section).GetValue(key);

    public bool HasKey(string section, StringName key)
        => Save.GetSection(section).HasKey(key);

    public bool RemoveKey(string section, StringName key, bool autoSave = true)
    {
        var removed = Save.GetSection(section).Remove(key);
        if (removed && autoSave) SaveNow();
        return removed;
    }

    public void ClearSection(string section, bool autoSave = true)
    {
        Save.Sections[section] = new SaveSection();
        if (autoSave) SaveNow();
    }
}
