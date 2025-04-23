using Godot;
using System;
using System.Collections.Generic;
public partial class ObjectPulling <T> where T : Node
{
    private readonly PackedScene scene;
    private readonly Node parent;
    private readonly Queue<T> pool;

    public ObjectPulling(PackedScene scene, Node parent, int initialSize = 10)
    {
        this.scene = scene;
        this.parent = parent;
        pool = new Queue<T>();

        // Crear los objetos iniciales
        for (int i = 0; i < initialSize; i++)
        {
            T obj = scene.Instantiate<T>();
            obj.Name = $"{typeof(T).Name}_Pooled_{i}";
            pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = scene.Instantiate<T>();
            parent.AddChild(obj);
        }

        // Reiniciar el estado del objeto si es necesario (como un Timer)
        if (obj is Timer timer)
        {
            timer.Stop();
            timer.Start();  // Reiniciar el timer
        }

        return obj;
    }

    public void Return(T obj)
    {
        // Si el objeto es un Timer, detenerlo antes de devolverlo
        if (obj is Timer timer)
        {
            timer.Stop();
        }

        pool.Enqueue(obj);
    }
}
