using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Stack<T> _pool;
    private readonly int _maxSize;

    public ObjectPool(T prefab, int initialSize, int maxSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
        _maxSize = maxSize;
        _pool = new Stack<T>(initialSize);

        for (int i = 0; i < initialSize; i++)
        {
            T instance = CreateNew();
            instance.gameObject.SetActive(false);
            _pool.Push(instance);
        }
    }

    private T CreateNew()
    {
        T instance = Object.Instantiate(_prefab, _parent);
        return instance;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        T obj;
        if (_pool.Count > 0)
        {
            obj = _pool.Pop();
        }
        else
        {
            obj = CreateNew();
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (_pool.Count >= _maxSize)
        {
            Object.Destroy(obj.gameObject);
            return;
        }

        obj.gameObject.SetActive(false);
        if (_parent != null)
            obj.transform.SetParent(_parent);
        _pool.Push(obj);
    }
}

