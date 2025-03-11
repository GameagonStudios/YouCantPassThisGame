#if TOOLS

using Godot;
using System;
using System.Reflection;
using System.Linq;
using Godot.Collections;
using Godot.NativeInterop;
using InputSystem;
using System.Collections.Generic;

public partial class ListOfInputsInpector : EditorInspectorPlugin
{
    Texture2D editIcon;
    Texture2D trashIcon;
    Texture2D addIcon;

    System.Collections.Generic.Dictionary<TreeItem, (Resource, List<Enum>)> properties = new();

    public ListOfInputsInpector()
    {
        EditorInterface editor = EditorInterface.Singleton;
        editIcon = editor.GetBaseControl().GetThemeIcon("Edit", "EditorIcons");
        trashIcon = editor.GetBaseControl().GetThemeIcon("Remove", "EditorIcons");
        addIcon = editor.GetBaseControl().GetThemeIcon("Add", "EditorIcons");
    }

    public override bool _CanHandle(GodotObject @object)
    {
        return @object is InputAction;
    }

    public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (name == "") return false;

        Type t = @object.GetType();
        PropertyInfo stringP = t.GetProperty(name);

        if (stringP != null && stringP.GetValue(@object) is Array<string> stringList)
        {
            FieldInfo enumP = t.GetField("enum" + name);

            if (enumP != null && enumP.GetValue(@object) is List<Enum> enumList)
            {
                Tree tree = new();

                tree.Columns = 1;
                tree.ScrollVerticalEnabled = false;
                tree.ButtonClicked += HandleTreeButtonClick;
                tree.ItemCollapsed += t => tree.UpdateMinimumSize();
                //tree.HideRoot = true;
                //tree.CustomMinimumSize += new Vector2(0,40);
                AddCustomControl(tree);
                TreeItem root = tree.CreateItem();
                root.SetText(0, stringP.Name);
                root.AddButton(0, addIcon, id: 0, tooltipText: "Add a input");

                if (!properties.ContainsKey(root)) properties.Add(root, (@object as Resource, enumList));

                foreach (string e in stringList)
                {
                    ParseKey(root, e);
                }

                return true;
            }
        }

        return false;
    }

    void ParseKey(TreeItem treeItem, string key)
    {
        TreeItem item = treeItem.GetTree().CreateItem(treeItem);
        item.SetText(0, key);
        item.AddButton(0, trashIcon, id: 1, tooltipText: "Add a input");
    }

    public void AddKey(TreeItem treeItem, Enum key)
    {
        var prop = properties[treeItem];

        if (prop.Item2.Contains(key)) return;

        prop.Item2.Add(key);

        ResourceSaver.Save(prop.Item1);

        ParseKey(treeItem, InputEventHandler.ParseEnum(key));
        treeItem.Collapsed = false;
        treeItem.GetTree().UpdateMinimumSize();
    }

    private void HandleTreeButtonClick(TreeItem item, long column, long id, long mouseButtonIndex)
    {
        switch (id)
        {
            case 0:
                HandleAdd(item);
                return;
            case 1:
                HandleRemove(item);
                return;
        }
    }

    private void HandleAdd(TreeItem item)
    {
        SKInputSystem.ShowInputMappingPanel((key) => AddKey(item, key));
    }

    private void HandleRemove(TreeItem item)
    {
        var prop = properties[item.GetParent()];

        prop.Item2.RemoveAt(item.GetIndex());

        ResourceSaver.Save(prop.Item1);

        var tree = item.GetTree();
        item.Free();
        tree.UpdateMinimumSize();

    }
}

#endif