// ============================================
// OBJECT POOLING & FACTORY
// ============================================

/// <summary>
/// Generic object pool for efficient spawning/despawning.
/// Reduces garbage allocation during heavy combat.
/// </summary>
using UnityEngine;

using System.Collections.Generic;


public class ObjectPool<T> where T : Component
{
    private Queue<T> availableObjects;
    private HashSet<T> activeObjects;
    private T prefab;
    private Transform poolParent;
    private int initialSize;

    public ObjectPool(T prefab, int initialSize = 10)
    {
        this.prefab = prefab;
        this.initialSize = initialSize;
        this.availableObjects = new Queue<T>(initialSize);
        this.activeObjects = new HashSet<T>();
        
        poolParent = new GameObject($"{prefab.name}_Pool").transform;
        
        for (int i = 0; i < initialSize; i++)
        {
            CreateObject();
        }
    }

    private void CreateObject()
    {
        T obj = Object.Instantiate(prefab, poolParent);
        obj.gameObject.SetActive(false);
        availableObjects.Enqueue(obj);
    }

    public T Get()
    {
        T obj;
        
        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        else
        {
            CreateObject();
            obj = availableObjects.Dequeue();
        }
        
        activeObjects.Add(obj);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (!activeObjects.Contains(obj)) return;
        
        activeObjects.Remove(obj);
        obj.gameObject.SetActive(false);
        availableObjects.Enqueue(obj);
    }

    public void ClearAll()
    {
        foreach (var obj in activeObjects)
        {
            Object.Destroy(obj.gameObject);
        }
        activeObjects.Clear();
        availableObjects.Clear();
    }
}
