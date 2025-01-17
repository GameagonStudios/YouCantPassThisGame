using Godot;
using Godot.Collections;
using InputSystem;
using System;

public partial class AudioController : AudioStreamPlayer
{
    [Export]
    public string BusName;

    [Export]
    public StringName VolumeKey;

    [Export]
    public int DefaultVolume;

    int AudioIndex;

    public override void _EnterTree()
    {
        base._EnterTree();
        AudioIndex = AudioServer.GetBusIndex(BusName);
        AudioServer.SetBusVolumeDb(AudioIndex, Mathf.LinearToDb(OptionsSavesHandler.Current.GetValue(VolumeKey)?.As<float>() ?? Mathf.DbToLinear(DefaultVolume)));
        OptionsSavesHandler.Current.onOptionsChanged += SetVolume;
    }
    public void SetVolume(StringName key, Variant value)
    {
        if (key == VolumeKey)
        {
            AudioServer.SetBusVolumeDb(AudioIndex, Mathf.LinearToDb(value.As<float>()));
        }
    }
}
