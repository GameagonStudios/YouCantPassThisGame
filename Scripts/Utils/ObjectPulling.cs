using Godot;
using System;
using System.Collections.Generic;

public class ObjectPulling<T> where T : Node
{
    private readonly Func<T> createFunc;
    private readonly Queue<T> pool;

    public ObjectPulling(Func<T> createFunc, int initialSize = 10)
    {
        this.createFunc = createFunc;
        pool = new Queue<T>();

        for (int i = 0; i < initialSize; i++)
        {
            T obj = createFunc();
            SetVisibleIfPossible(obj, false);
            pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : createFunc();
        SetVisibleIfPossible(obj, true);
        return obj;
    }

    public void Return(T obj)
    {
        SetVisibleIfPossible(obj, false);
        pool.Enqueue(obj);
    }

    private void SetVisibleIfPossible(T obj, bool visible)
    {
        if (obj is CanvasItem canvasItem)
        {
            canvasItem.Visible = visible;
        }
    }
}